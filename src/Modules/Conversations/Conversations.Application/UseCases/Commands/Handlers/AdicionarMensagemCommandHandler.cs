using Contacts.Application.Repositories;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using Conversations.Application.Repositories;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AdicionarMensagemCommandHandler : ICommandHandler<AdicionarMensagemCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRealtimeNotifier _notifier;
    private readonly IMetaMessageSender _metaSender;
    private readonly IContactRepository _contactRepository;
    private readonly IUserContext _userContext;
    private readonly IAtendimentoRepository _atendimentoRepository;

    public AdicionarMensagemCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IRealtimeNotifier notifier,
        IMetaMessageSender metaSender,
        IContactRepository contactRepository,
        IUserContext userContext,
        IAtendimentoRepository atendimentoRepository)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _notifier = notifier;
        _metaSender = metaSender;
        _contactRepository = contactRepository;
        _userContext = userContext;
        _atendimentoRepository = atendimentoRepository;
    }

    public async Task<MessageDto> HandleAsync(AdicionarMensagemCommand command, CancellationToken cancellationToken)
    {
        var timestamp = command.Timestamp ?? DateTime.UtcNow;
        var timestampUtc = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        var fusoHorarioBrasil = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        var timestampBrasilia = TimeZoneInfo.ConvertTimeFromUtc(timestampUtc, fusoHorarioBrasil);

        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        var atendimento = await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);
        if (atendimento is null)
        {
            atendimento = Atendimento.Iniciar(conversa.Id);
            await _atendimentoRepository.AddAsync(atendimento, cancellationToken);
        }

        if (command.RemetenteTipo != RemetenteTipo.Agente)
            throw new DomainException("Remetente inválido. Deve ser 'Agente'");

        string? anexoUrl = null;
        if (conversa.SessaoAtiva is null || !conversa.SessaoAtiva.EstaAtiva(conversa.SessaoAtiva.DataInicio))
            throw new DomainException(
                "A janela está fechada. Use um Template de Mensagem para iniciar uma nova conversa.");

        var agenteId = _userContext.GetCurrentUserId();
        if (agenteId is null)
            throw new UnauthorizedAccessException("Não foi possível identificar o agente autenticado.");

        if (atendimento.Status == ConversationStatus.AguardandoNaFila) atendimento.AtribuirAgente(agenteId.Value);

        if (command.AnexoStream is not null)
        {
            var nomeUnicoAnexo = $"{Guid.NewGuid()}-{command.AnexoNome}";
            anexoUrl = await _fileStorageService.UploadAsync(command.AnexoStream, nomeUnicoAnexo,
                command.AnexoContentType!);
        }

        var remetente = Remetente.Agente(agenteId.Value);
        var timestampCompatível = DateTime.SpecifyKind(timestampBrasilia, DateTimeKind.Utc);

        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, command.Texto, remetente,
            timestamp: timestampCompatível, anexoUrl);

        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is not null)
        {
            if (novaMensagem.AnexoUrl is not null)
            {
                await _metaSender.EnviarDocumentoAsync(contato.Telefone, anexoUrl, command.AnexoNome,
                    novaMensagem.Texto);
            }
            else
            {
                novaMensagem.ExternalId = await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, command.Texto);
            }
        }

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var messageDto = novaMensagem.ToDto();
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), messageDto);

        var summaryDto = new ConversationSummaryDto
        {
            Id = conversa.Id,
            AtendimentoId = novaMensagem.Id,
            ContatoNome = contato.Nome,
            ContatoTelefone = contato.Telefone,

            AgenteNome = null,
            TagId = conversa.TagsId,
            TagName = conversa.Tag?.Nome ?? "",
            Status = atendimento.Status.ToString(),

            UltimaMensagemTimestamp = novaMensagem.Timestamp,
            UltimaMensagemPreview = novaMensagem.Texto,

            SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva(conversa.SessaoAtiva.DataInicio) ?? true,
            SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
        };
        await _notifier.NotificarNovaConversaNaFilaAsync(summaryDto);

        return messageDto;
    }
}