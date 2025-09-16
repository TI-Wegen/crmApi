using Agents.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class ProcessarRespostaDoMenuCommandHandler : ICommandHandler<ProcessarRespostaDoMenuCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IBotSessionCache _botSessionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBoletoService _boletoService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<ProcessarRespostaDoMenuCommandHandler> _logger;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IMensageriaBotService _mensageriaBot; 


    public ProcessarRespostaDoMenuCommandHandler(
        IAtendimentoRepository atendimentoRepository,
        IConversationRepository conversationRepository,
        IBotSessionCache botSessionCache,
        IUnitOfWork unitOfWork,
        IMetaMessageSender metaMessageSender,
        IBoletoService boletoService,
        IFileStorageService fileStorageService,
        IAgentRepository agentRepository,
        IMensageriaBotService mensageriaBot, 
        ILogger<ProcessarRespostaDoMenuCommandHandler> logger

        )
    {
        _conversationRepository = conversationRepository;
        _botSessionCache = botSessionCache;
        _unitOfWork = unitOfWork;
        _boletoService = boletoService;
        _fileStorageService = fileStorageService;
        _agentRepository = agentRepository;
        _logger = logger;
        _atendimentoRepository = atendimentoRepository;
        _mensageriaBot = mensageriaBot; 
    }

    public async Task HandleAsync(ProcessarRespostaDoMenuCommand command, CancellationToken cancellationToken)
    {
        var timestamp = command.Timestamp ?? DateTime.UtcNow;

        var sessionState = await _botSessionCache.GetStateAsync(command.ContatoTelefone);
        if (sessionState is null)
        {
            _logger.LogWarning("Sessão do bot não encontrada para o telefone {Telefone}.", command.ContatoTelefone);
            return;
        }

        var atendimento = await _atendimentoRepository.GetByIdAsync(sessionState.AtendimentoId, cancellationToken);
        if (atendimento is null || atendimento.Status != ConversationStatus.EmAutoAtendimento)
        {
            _logger.LogWarning("Inconsistência de estado detectada: Sessão de bot existe no Redis para o telefone {Telefone}, mas o atendimento correspondente ({AtendimentoId}) não foi encontrado ou não está em autoatendimento.", command.ContatoTelefone, sessionState.AtendimentoId);

            await _botSessionCache.DeleteStateAsync(command.ContatoTelefone);

            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id  ,command.ContatoTelefone, "Sua sessão anterior expirou ou não pôde ser encontrada. Por favor, envie sua mensagem novamente para recomeçar.");

            return;
        }

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId, cancellationToken);
        var mensagemDoCliente = new Mensagem(conversa.Id, atendimento.Id, command.TextoDaResposta, Remetente.Cliente(), command.Timestamp ?? DateTime.UtcNow, null);
        conversa.AdicionarMensagem(mensagemDoCliente, atendimento.Id);

        switch (sessionState.Status)
        {
            case BotStatus.AguardandoOpcaoMenuPrincipal:
                await ProcessarMenuPrincipal(atendimento, command.ContatoTelefone, command.TextoDaResposta);
                break;

            case BotStatus.AguardandoCpfParaBoleto:
                await ProcessarPedidoDeBoleto(atendimento, command.ContatoTelefone, command.TextoDaResposta);
                break;
            case BotStatus.AguardandoEscolhaDeBoleto:
                if (sessionState.BoletosDisponiveis is null || !sessionState.BoletosDisponiveis.Any())
                {
                    await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, command.ContatoTelefone, "Ocorreu um erro, vamos recomeçar. Por favor, digite seu CPF novamente.");
                    atendimento.AguardarCpf();
                    await _botSessionCache.SetStateAsync(command.ContatoTelefone, new BotSessionState(atendimento.Id, atendimento.BotStatus, DateTime.UtcNow,null), TimeSpan.FromMinutes(30));
                }
                else
                {
                    await ProcessarEscolhaDeBoleto(atendimento, command.ContatoTelefone, command.TextoDaResposta, sessionState.BoletosDisponiveis);
                }
                break;

        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessarMenuPrincipal(Atendimento atendimento, string contatoTelefone, string resposta)
    {


        switch (resposta.Trim())
        {
            case "1": // Segunda via de boleto
                atendimento.AguardarCpf(); 
                await _botSessionCache.SetStateAsync(contatoTelefone, new BotSessionState(atendimento.Id, atendimento.BotStatus, DateTime.UtcNow), TimeSpan.FromMinutes(30));
                await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Por favor, digite seu CPF para gerarmos a segunda via.");
                await _unitOfWork.SaveChangesAsync();
                break;

            case "2": // Falar com o Comercial
                var setorComercial = await _agentRepository.GetSetorByNomeAsync(SetorNome.Comercial.ToDbValue());
                atendimento.IniciarTransferenciaParaFila(setorComercial.Id);
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Ok, estou te transferindo para um de nossos especialistas do setor comercial.");
                await _unitOfWork.SaveChangesAsync();
                break;

            case "3": // Falar com o Financeiro
                var setorFinanceiro = await _agentRepository.GetSetorByNomeAsync(SetorNome.Financeiro.ToDbValue());
                atendimento.IniciarTransferenciaParaFila(setorFinanceiro.Id);
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Ok, estou te transferindo para um de nossos especialistas do setor Financeiro.");
                await _unitOfWork.SaveChangesAsync();
                break;

            case "4": 
                atendimento.Resolver(SystemGuids.SystemAgentId);
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Seu atendimento foi encerrado. Obrigado pelo contato!");
                await _unitOfWork.SaveChangesAsync();
                break;

            default: 
                await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Opção inválida. Por favor, digite um dos números do menu.");
                break;
        }
    }



    private async Task ProcessarPedidoDeBoleto(Atendimento atendimento, string contatoTelefone, string cpf)
    {
        var boletos = (await _boletoService.GetBoletosAbertosAsync(cpf.Trim())).ToList();
        var boletosVencidos = boletos.Where(b => b.StatusBoleto.Equals("Vencida", StringComparison.OrdinalIgnoreCase)).ToList();
        if (boletosVencidos.Any())
        {
            atendimento.AguardarEscolhaDeBoleto(); 

            var sb = new StringBuilder("Identifiquei que há pendências em seu nome. Por favor, selecione uma opção:\n");
            sb.AppendLine("0 - Enviar todos os boletos"); 
            for (int i = 0; i < boletosVencidos.Count; i++)
            {
                var boleto = boletosVencidos[i];
                sb.AppendLine($"{i + 1}- Boleto da Instalação {boleto.Numinstalacao}, venc. {boleto.DataVencimento:dd/MM/yyyy}");
            }

            var newSessionState = new BotSessionState(atendimento.Id, atendimento.BotStatus, DateTime.UtcNow, boletosVencidos);
            await _botSessionCache.SetStateAsync(contatoTelefone, newSessionState, TimeSpan.FromMinutes(30));

            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id,contatoTelefone, sb.ToString());
            return; 
        }

        if (!boletos.Any())
        {
            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Não encontrei um boleto em aberto para o CPF informado.Obrigado pelo contato!!");
            atendimento.Resolver(SystemGuids.SystemAgentId);
            await _botSessionCache.DeleteStateAsync(contatoTelefone);
            return;
        }

        var gruposDeInstalacao = boletos.GroupBy(b => b.Numinstalacao).ToList();

        if (gruposDeInstalacao.Count == 1)
        {
            var boletoParaEnviar = boletos.First();

            var boletoCompleto = await _boletoService.GetBoletoAsync(boletoParaEnviar.IdFatura);
            if (boletoCompleto is not null) await EnviarBoleto(atendimento.Id,contatoTelefone, boletoCompleto);

            atendimento.Resolver(SystemGuids.SystemAgentId);
            await _botSessionCache.DeleteStateAsync(contatoTelefone);
        }
        else
        {
            atendimento.AguardarEscolhaDeBoleto();

            var sb = new StringBuilder("Identifiquei boletos para mais de uma instalação em seu nome. Digite o número do boleto que você deseja:\n");
            sb.AppendLine("0 - Enviar todos os boletos");
            for (int i = 0; i < boletos.Count; i++)
            {
                var boleto = boletos[i];
                sb.AppendLine($"{i + 1}- Instalação {boleto.Numinstalacao}: Ref. a {boleto.Referente}, venc. {boleto.DataVencimento:dd/MM/yyyy}");
            }

            var newSessionState = new BotSessionState(atendimento.Id, atendimento.BotStatus, DateTime.UtcNow, boletos);
            await _botSessionCache.SetStateAsync(contatoTelefone, newSessionState, TimeSpan.FromMinutes(30));

            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, sb.ToString());
        }
    }

    private async Task ProcessarEscolhaDeBoleto(Atendimento atendimento, string contatoTelefone, string escolha, List<BoletoDto> boletosDisponiveis)
    {
        if (escolha.Trim() == "0")
        {
            await EnviarMultiplosBoletosAsync(atendimento.Id, contatoTelefone, boletosDisponiveis);
            atendimento.Resolver(SystemGuids.SystemAgentId);
            await _botSessionCache.DeleteStateAsync(contatoTelefone);
            return;
        }

        if (!int.TryParse(escolha, out var indice) || indice < 1 || indice > boletosDisponiveis.Count)
        {
            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Opção inválida. Por favor, digite um dos números da lista.");
            return;
        }

        var boletoEscolhidoSummary = boletosDisponiveis[indice - 1];

        var boletoCompleto = await _boletoService.GetBoletoAsync(boletoEscolhidoSummary.IdFatura);

        if (boletoCompleto is null)
        {
            await _mensageriaBot.EnviarEMensagemTextoAsync(atendimento.Id, contatoTelefone, "Ocorreu um erro ao buscar os detalhes do boleto selecionado. Por favor, tente novamente.");
            return;
        }

        await EnviarBoleto(atendimento.Id, contatoTelefone, boletoCompleto);

        atendimento.Resolver(SystemGuids.SystemAgentId);
        await _botSessionCache.DeleteStateAsync(contatoTelefone);
    }

    private async Task EnviarBoleto(Guid atendimentoId, string contatoTelefone, BoletoDto boleto)
    {
        var boletoEmBytes = Convert.FromBase64String(boleto.PdfBoleto);
        var memoryStream = new MemoryStream(boletoEmBytes);
        var nomeArquivo = $"boleto-{contatoTelefone}-{DateTime.UtcNow:yyyyMMdd}.pdf";
        var urlDoBoleto = await _fileStorageService.UploadAsync(memoryStream, nomeArquivo, "application/pdf");

        var legenda = $"Pronto! Segue seu boleto referente a conta '{boleto.IdFatura}' com vencimento em {boleto.DataVencimento:dd/MM/yyyy}.";

        await _mensageriaBot.EnviarEDocumentoAsync(atendimentoId, contatoTelefone, urlDoBoleto, nomeArquivo, legenda);
    }

    private async Task EnviarMultiplosBoletosAsync(Guid atendimentoId, string contatoTelefone, List<BoletoDto> boletosResumo)
    {
        await _mensageriaBot.EnviarEMensagemTextoAsync(atendimentoId, contatoTelefone, "Certo! Preparando o envio de todos os boletos. Isso pode levar um instante...");

        foreach (var resumo in boletosResumo)
        {
            var boletoCompleto = await _boletoService.GetBoletoAsync(resumo.IdFatura);
            if (boletoCompleto is not null)
            {
                await EnviarBoleto(atendimentoId, contatoTelefone, boletoCompleto);
                await Task.Delay(500);
            }
        }

        await _mensageriaBot.EnviarEMensagemTextoAsync(atendimentoId, contatoTelefone, "Pronto! Enviei todos os boletos solicitados. O atendimento foi encerrado.");
    }
}
