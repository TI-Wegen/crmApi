using Agents.Domain.Enuns;
using Agents.Domain.Repository;
using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class IniciarConversaCommandHandler : ICommandHandler<IniciarConversaCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBotSessionCache _botSessionCache;
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
        _readService = realtimeNotifier;
        _agentRepository = agentRepository;
        _mensageriaBotService = mensageriaBotService;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.UtcNow;
        var timestampUtc = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        
        try {
            var conversa = await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);
        
            if (conversa is not null)
            {
                var atendimentoAtivo = await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);
                if (atendimentoAtivo is not null)
                {
                    conversa.IniciarOuRenovarSessao(timestamp);

                    var mensagemEmAndamento = new Mensagem(conversa.Id, atendimentoAtivo.Id, command.TextoDaMensagem, Remetente.Cliente(), timestampUtc, command.AnexoUrl, command.Wamid);
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
            conversa.IniciarOuRenovarSessao(timestampUtc);
            
            var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId);
            if (contato is null) return conversa.Id;

            var fusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var dataAtualLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoHorarioBrasil).Date;
            var timestampBrasilia = TimeZoneInfo.ConvertTimeFromUtc(timestampUtc, fusoHorarioBrasil).Date;

            bool deveIniciarBot = command.IniciarComBot && (dataAtualLocal == timestampBrasilia);

            if (deveIniciarBot)
            {
                _logger.LogInformation("Mensagem atual recebida. Iniciando fluxo de bot para a conversa {ConversaId}.", conversa.Id);

                var novoAtendimento = Atendimento.Iniciar(conversa.Id);
                var primeiraMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaMensagem, Remetente.Cliente(), timestampUtc, command.AnexoUrl);

                primeiraMensagem.SetAtendimentoId(novoAtendimento.Id);
                conversa.AdicionarMensagem(primeiraMensagem, novoAtendimento.Id);
                await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";
                await _mensageriaBotService.EnviarEMensagemTextoAsync(novoAtendimento.Id, contato.Telefone, menuText);

                var summaryDto = new ConversationSummaryDto
                {
                    Id = conversa.Id,
                    AtendimentoId = novoAtendimento.Id,
                    ContatoNome = contato.Nome,
                    ContatoTelefone = contato.Telefone,
                    AgenteNome = null,
                    TagId = conversa.TagsId,
                    TagName = conversa?.Tag?.Nome ?? "",
                    UltimaMensagemTimestamp = primeiraMensagem.Timestamp,
                    UltimaMensagemPreview = primeiraMensagem.Texto,
                    SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva() ?? true,
                    SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
                };
                await _readService.NotificarNovaConversaNaFilaAsync(summaryDto);

            }
            else
            {
                var setorAdmin = await _agentRepository.GetSetorByNomeAsync(SetorNomeExtensions.ToDbValue(SetorNome.Admin));
                var novoAtendimento = Atendimento.IniciarEmFila(conversa.Id, setorAdmin.Id);
                var primeiraMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaMensagem, Remetente.Cliente(), timestampUtc, command.AnexoUrl);
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
                    TagId = conversa.TagsId,
                    TagName = conversa.Tag?.Nome,

                    UltimaMensagemTimestamp = primeiraMensagem.Timestamp,
                    UltimaMensagemPreview = primeiraMensagem.Texto,

                    SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva() ?? true,
                    SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
                };
                await _readService.NotificarNovaConversaNaFilaAsync(summaryDto);
            }
            return conversa.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar conversa: {ContatoNome}", command.ContatoNome);
            throw;
        }

    
    }
}