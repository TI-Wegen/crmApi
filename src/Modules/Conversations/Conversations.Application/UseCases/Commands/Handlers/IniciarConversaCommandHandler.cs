using Agents.Domain.Enuns;
using Agents.Domain.Repository;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class IniciarConversaCommandHandler : ICommandHandler<IniciarConversaCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBotSessionCache _botSessionCache;
    private readonly IConversationReadService _notifier;
    private readonly IRealtimeNotifier _readService;
    private readonly IAgentRepository _agentRepository;
    private readonly IMensageriaBotService _mensageriaBotService;

    private readonly ILogger<IniciarConversaCommandHandler> _logger;

    public IniciarConversaCommandHandler(
        IConversationRepository conversationRepository,
        IAtendimentoRepository atendimentoRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        IBotSessionCache botSessionCache,
        IConversationReadService notifier,
        IRealtimeNotifier realtimeNotifier,
        IAgentRepository agentRepository,
        IMensageriaBotService mensageriaBotService,
        ILogger<IniciarConversaCommandHandler> logger

        )
    {
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _botSessionCache = botSessionCache;
        _notifier = notifier;
        _readService = realtimeNotifier;
        _agentRepository = agentRepository;
        _mensageriaBotService = mensageriaBotService;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        var timestamp = command.Timestamp ?? DateTime.UtcNow;

        var conversa = await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);
        if (conversa is not null)
        {
            var atendimentoAtivo = await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);
            if (atendimentoAtivo is not null)
            {
                conversa.IniciarOuRenovarSessao(timestamp);

                if (atendimentoAtivo.Status == Domain.Enuns.ConversationStatus.AguardandoRespostaCliente)
                {
                    atendimentoAtivo.RegistrarRespostaDoCliente();
                    _logger.LogInformation("Cliente respondeu ao template. Atendimento {AtendimentoId} movido para EmAtendimento.", atendimentoAtivo.Id);

                }

                var mensagemEmAndamento = new Mensagem(conversa.Id, atendimentoAtivo.Id, command.TextoDaMensagem, Remetente.Cliente(), timestamp, command.AnexoUrl);
                conversa.AdicionarMensagem(mensagemEmAndamento, atendimentoAtivo.Id);
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                var messageDto = mensagemEmAndamento.ToDto();
                await _readService.NotificarNovaMensagemAsync(conversa.Id.ToString(), messageDto);

                return conversa.Id;
            }
        }

        if (conversa is null)
        {
            conversa = Conversa.Iniciar(command.ContatoId, command.ContatoNome);
            await _conversationRepository.AddAsync(conversa, cancellationToken);
        }
        conversa.IniciarOuRenovarSessao(timestamp);


        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId);
        if (contato is null) return conversa.Id; // Ou logar erro
        // 4. Cria a primeira Mensagem, agora que temos todos os IDs.
        var fusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        var dataAtualLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoHorarioBrasil).Date;
        var dataMensagemLocal = TimeZoneInfo.ConvertTimeFromUtc(timestamp, fusoHorarioBrasil).Date;

        // A decisão de usar o bot agora depende do flag E da data da mensagem.
        bool deveIniciarBot = command.IniciarComBot && (dataAtualLocal == dataMensagemLocal);

        if (deveIniciarBot)
        {
            _logger.LogInformation("Mensagem atual recebida. Iniciando fluxo de bot para a conversa {ConversaId}.", conversa.Id);

            var novoAtendimento = Atendimento.Iniciar(conversa.Id);
            var primeiraMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaMensagem, Remetente.Cliente(), timestamp, command.AnexoUrl);

            primeiraMensagem.SetAtendimentoId(novoAtendimento.Id);
            conversa.AdicionarMensagem(primeiraMensagem, novoAtendimento.Id);
            await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Inicia o fluxo externo do bot (Redis + Meta)
            var sessionState = new BotSessionState(novoAtendimento.Id, novoAtendimento.BotStatus, DateTime.UtcNow);
            await _botSessionCache.SetStateAsync(contato!.Telefone, sessionState, TimeSpan.FromHours(2));
            var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";
            await _mensageriaBotService.EnviarEMensagemTextoAsync(novoAtendimento.Id, contato.Telefone, menuText);

            var summaryDto = new ConversationSummaryDto
            {
                Id = conversa.Id,
                AtendimentoId = novoAtendimento.Id,
                ContatoNome = contato.Nome,
                ContatoTelefone = contato.Telefone,

                AgenteNome = null,
                Status = novoAtendimento.Status.ToString(),

                UltimaMensagemTimestamp = primeiraMensagem.Timestamp,
                UltimaMensagemPreview = primeiraMensagem.Texto,

                SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva(DateTime.UtcNow) ?? false,
                SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
            };
            await _readService.NotificarNovaConversaNaFilaAsync(summaryDto);
            
        }
        else
        {
            var setorAdmin = await _agentRepository.GetSetorByNomeAsync(SetorNomeExtensions.ToDbValue(SetorNome.Admin));
            var novoAtendimento = Atendimento.IniciarEmFila(conversa.Id, setorAdmin.Id);
            var primeiraMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaMensagem, Remetente.Cliente(), timestamp, command.AnexoUrl);
            primeiraMensagem.SetAtendimentoId(novoAtendimento.Id);
            conversa.AdicionarMensagem(primeiraMensagem, novoAtendimento.Id);
            await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var summaryDto = new ConversationSummaryDto
            {
                Id = conversa.Id,
                AtendimentoId = novoAtendimento.Id,
                ContatoNome = contato.Nome,
                ContatoTelefone = contato.Telefone,

                AgenteNome = null,
                Status = novoAtendimento.Status.ToString(),

                UltimaMensagemTimestamp = primeiraMensagem.Timestamp,
                UltimaMensagemPreview = primeiraMensagem.Texto,

                SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva(DateTime.UtcNow) ?? false,
                SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
            };
                await _readService.NotificarNovaConversaNaFilaAsync(summaryDto);
        }
        return conversa.Id;

    }


}