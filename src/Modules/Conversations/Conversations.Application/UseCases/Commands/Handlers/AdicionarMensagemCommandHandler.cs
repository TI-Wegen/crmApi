namespace Conversations.Application.UseCases.Commands.Handlers;

using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AdicionarMensagemCommandHandler : ICommandHandler<AdicionarMensagemCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRealtimeNotifier _notifier;
    private readonly IMetaMessageSender _metaSender; // NOVO
    private readonly IContactRepository _contactRepository; // NOVO
    private readonly IUserContext _userContext;
    private readonly IBotSessionCache _botSessionCache;
    private readonly IAtendimentoRepository _atendimentoRepository;



    public AdicionarMensagemCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IRealtimeNotifier notifier,
        IMetaMessageSender metaSender,
        IContactRepository contactRepository,
        IBotSessionCache botSessionCache,
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
        _botSessionCache = botSessionCache;
        _atendimentoRepository = atendimentoRepository;
    }

    public async Task<MessageDto> HandleAsync(AdicionarMensagemCommand command, CancellationToken cancellationToken)
    {
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");
        var atendimento = await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);
        if (atendimento is null)
            throw new InvalidOperationException("Não há um atendimento ativo para adicionar esta mensagem.");
        string? anexoUrl = null;
        if (command.AnexoStream is not null)
        {
            var nomeUnicoAnexo = $"{Guid.NewGuid()}-{command.AnexoNome}";
            anexoUrl = await _fileStorageService.UploadAsync(command.AnexoStream, nomeUnicoAnexo, command.AnexoContentType!);
        }

        Guid? agenteId = null;
        if (command.RemetenteTipo == RemetenteTipo.Agente)
        {
            agenteId = _userContext.GetCurrentUserId();
            if (agenteId is null)
            {
                throw new UnauthorizedAccessException("Não foi possível identificar o agente autenticado.");
            }

            if (atendimento.Status == ConversationStatus.AguardandoNaFila)
            {
                atendimento.AtribuirAgente(agenteId.Value);
            }
        }

        var remetente = command.RemetenteTipo == RemetenteTipo.Agente
                    ? Remetente.Agente(agenteId.Value)
                    : Remetente.Cliente();

        var novaMensagem = new Mensagem(conversa.Id,atendimento.Id ,command.Texto, remetente, anexoUrl);

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        if (command.RemetenteTipo == RemetenteTipo.Agente)
        {
            var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
            if (contato is not null)
            {
                await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, command.Texto);
            }
        }

        try
        {

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.ReloadEntityAsync(conversa, cancellationToken);

            conversa.AdicionarMensagem(novaMensagem, atendimento.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var messageDto = novaMensagem.ToDto();
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), messageDto);

        return messageDto;
    }
}