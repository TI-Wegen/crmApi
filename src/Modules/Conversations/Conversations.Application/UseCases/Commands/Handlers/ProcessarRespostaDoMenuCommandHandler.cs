using Agents.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class ProcessarRespostaDoMenuCommandHandler : ICommandHandler<ProcessarRespostaDoMenuCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IBotSessionCache _botSessionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaMessageSender _metaMessageSender;
    private readonly IBoletoService _boletoService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAgentRepository _agentRepository;
    private readonly ILogger<ProcessarRespostaDoMenuCommandHandler> _logger;

    public ProcessarRespostaDoMenuCommandHandler(
        IConversationRepository conversationRepository,
        IBotSessionCache botSessionCache,
        IUnitOfWork unitOfWork,
        IMetaMessageSender metaMessageSender,
        IBoletoService boletoService,
        IFileStorageService fileStorageService,
        IAgentRepository agentRepository,
        ILogger<ProcessarRespostaDoMenuCommandHandler> logger

        )
    {
        _conversationRepository = conversationRepository;
        _botSessionCache = botSessionCache;
        _unitOfWork = unitOfWork;
        _metaMessageSender = metaMessageSender;
        _boletoService = boletoService;
        _fileStorageService = fileStorageService;
        _agentRepository = agentRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ProcessarRespostaDoMenuCommand command, CancellationToken cancellationToken)
    {
        // 1. Busca o estado atual da sessão do bot no Redis usando o telefone.
        var sessionState = await _botSessionCache.GetStateAsync(command.ContatoTelefone);
        if (sessionState is null)
        {
            _logger.LogWarning("Sessão do bot não encontrada para o telefone {Telefone}.", command.ContatoTelefone);
            return;
        }

        // 2. Busca a conversa correspondente no banco de dados.
        var conversa = await _conversationRepository.GetByIdAsync(sessionState.ConversationId, cancellationToken);
        if (conversa is null || conversa.Status != ConversationStatus.EmAutoAtendimento)
        {
            _logger.LogWarning("Conversa não encontrada ou não está em autoatendimento para o telefone {Telefone}.", command.ContatoTelefone);
            return;
        }

        // 3. Máquina de Estados: decide o que fazer com base no estado ATUAL do bot.
        switch (sessionState.Status)
        {
            case BotStatus.AguardandoOpcaoMenuPrincipal:
                await ProcessarMenuPrincipal(conversa, command.ContatoTelefone, command.TextoDaResposta);
                break;

            case BotStatus.AguardandoCpfParaBoleto:
                await ProcessarPedidoDeBoleto(conversa, command.ContatoTelefone, command.TextoDaResposta);
                break;
            case BotStatus.AguardandoEscolhaDeBoleto:
                if (sessionState.BoletosDisponiveis is null || !sessionState.BoletosDisponiveis.Any())
                {
                    await _metaMessageSender.EnviarMensagemTextoAsync(command.ContatoTelefone, "Ocorreu um erro, vamos recomeçar. Por favor, digite seu CPF novamente.");
                    conversa.AguardarCpf();
                    await _botSessionCache.SetStateAsync(command.ContatoTelefone, new BotSessionState(conversa.Id, conversa.BotStatus, null), TimeSpan.FromMinutes(30));
                }
                else
                {
                    await ProcessarEscolhaDeBoleto(conversa, command.ContatoTelefone, command.TextoDaResposta, sessionState.BoletosDisponiveis);
                }
                break;

        }

        // 4. Se o bot ainda estiver ativo na conversa, atualiza a sessão no Redis e reseta o timer de 2h.


        // 5. Salva todas as alterações feitas na conversa no banco de dados.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessarMenuPrincipal(Conversa conversa, string contatoTelefone, string resposta)
    {
        // Define o setor do Comercial. Em uma aplicação real, isso viria de uma configuração ou do banco.
        //var setorComercialId = Guid.Parse("SEU-GUID-DO-SETOR-COMERCIAL-AQUI");

        switch (resposta.Trim())
        {
            case "1": // Segunda via de boleto
                // Aqui você mudaria o estado do bot e pediria o CPF.
                conversa.AguardarCpf(); // Exemplo de um novo método de domínio
                await _botSessionCache.SetStateAsync(contatoTelefone, new BotSessionState(conversa.Id, conversa.BotStatus), TimeSpan.FromMinutes(30));
                await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Por favor, digite seu CPF para gerarmos a segunda via.");
                break;

            case "2": // Falar com o Comercial
                var setorComercial = await _agentRepository.GetSetorByNomeAsync(SetorNome.Comercial.ToDbValue());
                conversa.IniciarTransferenciaParaFila(setorComercial.Id);
                // Remove a sessão do bot, pois agora um humano vai assumir.
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Ok, estou te transferindo para um de nossos especialistas do setor comercial.");
                break;

            case "3": // Falar com o Financeiro
                var setorFinanceiro = await _agentRepository.GetSetorByNomeAsync(SetorNome.Financeiro.ToDbValue());
                conversa.IniciarTransferenciaParaFila(setorFinanceiro.Id);
                // Remove a sessão do bot, pois agora um humano vai assumir.
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Ok, estou te transferindo para um de nossos especialistas do setor Financeiro.");
                break;

            case "4": // Encerrar atendimento
                conversa.Resolver();
                // Remove a sessão do bot.
                await _botSessionCache.DeleteStateAsync(contatoTelefone);
                await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Seu atendimento foi encerrado. Obrigado pelo contato!");
                break;

            default: // Opção inválida
                await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Opção inválida. Por favor, digite um dos números do menu.");
                break;
        }
    }



    private async Task ProcessarPedidoDeBoleto(Conversa conversa, string contatoTelefone, string cpf)
    {
        var boletos = (await _boletoService.GetBoletosAbertosAsync(cpf.Trim())).ToList();
        bool temBoletoVencido = boletos.Any(b => b.StatusBoleto.Equals("Vencida", StringComparison.OrdinalIgnoreCase));
        if (temBoletoVencido)
        {
            var nomeSetor = SetorNome.Financeiro.ToDbValue();
            var setorFinanceiro = await _agentRepository.GetSetorByNomeAsync(nomeSetor);
            if (setorFinanceiro is null)
            {

                Console.WriteLine($"ERRO CRÍTICO: O setor semeado '{nomeSetor}' não foi encontrado no banco.");
                return;
            }

            conversa.AdicionarTag(ConversaTag.Inadimplente); // Adiciona a tag à conversa
            conversa.IniciarTransferenciaParaFila(setorFinanceiro.Id); // Muda o status e direciona

            // Remove a sessão do bot, pois o fluxo foi transferido para um humano.
            await _botSessionCache.DeleteStateAsync(contatoTelefone);

            // Envia uma mensagem informando o cliente da transferência.
            await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Identifiquei que há pendências em seu nome. Para te ajudar melhor, estou te transferindo para nosso setor Financeiro.");

            // Encerra a execução deste método.
            return;
        }

        if (!boletos.Any())
        {
            await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Não encontrei um boleto em aberto para o CPF informado. Deseja tentar novamente?");
            conversa.AguardarCpf(); // Mantém o estado para nova tentativa
            return;
        }

        var gruposDeInstalacao = boletos.GroupBy(b => b.Numinstalacao).ToList();

        if (gruposDeInstalacao.Count == 1)
        {
            // CENÁRIO 1: Todas as faturas são da MESMA instalação.
            // Pegamos o boleto com o vencimento mais próximo (o primeiro, pois ordenamos por data).
            var boletoParaEnviar = boletos.First();

            var boletoCompleto = await _boletoService.GetBoletoAsync(boletoParaEnviar.IdFatura);
            if (boletoCompleto is not null) await EnviarBoleto(contatoTelefone, boletoCompleto);

            conversa.Resolver();
            await _botSessionCache.DeleteStateAsync(contatoTelefone);
        }
        else
        {
            // CENÁRIO 2: Faturas de instalações DIFERENTES. Apresentamos o menu de escolha.
            conversa.AguardarEscolhaDeBoleto();

            var sb = new StringBuilder("Identifiquei boletos para mais de uma instalação em seu nome. Digite o número do boleto que você deseja:\n");
            for (int i = 0; i < boletos.Count; i++)
            {
                var boleto = boletos[i];
                // No menu, deixamos claro de qual instalação é cada boleto.
                sb.AppendLine($"{i + 1}- Instalação {boleto.Numinstalacao}: Ref. a {boleto.Referente}, venc. {boleto.DataVencimento:dd/MM/yyyy}");
            }

            // Armazena a lista de boletos na sessão do Redis para o próximo passo.
            var newSessionState = new BotSessionState(conversa.Id, conversa.BotStatus, boletos);
            await _botSessionCache.SetStateAsync(contatoTelefone, newSessionState, TimeSpan.FromMinutes(30));

            await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, sb.ToString());
        }
    }

    private async Task ProcessarEscolhaDeBoleto(Conversa conversa, string contatoTelefone, string escolha, List<BoletoDto> boletosDisponiveis)
    {
        if (!int.TryParse(escolha, out var indice) || indice < 1 || indice > boletosDisponiveis.Count)
        {
            await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Opção inválida. Por favor, digite um dos números da lista.");
            return;
        }

        // 1. Pega o "resumo" do boleto que o usuário escolheu.
        var boletoEscolhidoSummary = boletosDisponiveis[indice - 1];

        // 2. Usa o IdFatura da escolha para buscar o boleto COMPLETO (com o PDF).
        var boletoCompleto = await _boletoService.GetBoletoAsync(boletoEscolhidoSummary.IdFatura);

        if (boletoCompleto is null)
        {
            await _metaMessageSender.EnviarMensagemTextoAsync(contatoTelefone, "Ocorreu um erro ao buscar os detalhes do boleto selecionado. Por favor, tente novamente.");
            return;
        }

        // 3. Envia o boleto completo.
        await EnviarBoleto(contatoTelefone, boletoCompleto);

        // 4. Resolve a conversa e limpa a sessão.
        conversa.Resolver();
        await _botSessionCache.DeleteStateAsync(contatoTelefone);
    }

    // NOVO MÉTODO auxiliar para não repetir o código de envio
    private async Task EnviarBoleto(string contatoTelefone, BoletoDto boleto)
    {
        var boletoEmBytes = Convert.FromBase64String(boleto.PdfBoleto);
        var memoryStream = new MemoryStream(boletoEmBytes);
        var nomeArquivo = $"boleto-{contatoTelefone}-{DateTime.UtcNow:yyyyMMdd}.pdf";
        var legenda = $"Pronto! Segue seu boleto referente a conta '{boleto.IdFatura}' com vencimento em {boleto.DataVencimento:dd/MM/yyyy}.O Atendimento foi encerrado";
        var urlDoBoleto = await _fileStorageService.UploadAsync(memoryStream, nomeArquivo, "application/pdf");
        await _metaMessageSender.EnviarDocumentoAsync(contatoTelefone, urlDoBoleto, nomeArquivo, legenda);
    }
}
