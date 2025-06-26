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

// Implementa a interface que retorna um resultado, neste caso, o DTO da mensagem criada.
public class AdicionarMensagemCommandHandler : ICommandHandler<AdicionarMensagemCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRealtimeNotifier _notifier;
    private readonly IMetaMessageSender _metaSender; // NOVO
    private readonly IContactRepository _contactRepository; // NOVO




    public AdicionarMensagemCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IRealtimeNotifier notifier,
        IMetaMessageSender metaSender,
        IContactRepository contactRepository)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _notifier = notifier;
        _metaSender = metaSender;
        _contactRepository = contactRepository;
    }

    public async Task<MessageDto> HandleAsync(AdicionarMensagemCommand command, CancellationToken cancellationToken)
    {
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        string? anexoUrl = null;
        if (command.AnexoStream is not null)
        {
            var nomeUnicoAnexo = $"{Guid.NewGuid()}-{command.AnexoNome}";
            anexoUrl = await _fileStorageService.UploadAsync(command.AnexoStream, nomeUnicoAnexo, command.AnexoContentType!);
        }

        var remetente = command.RemetenteTipo == RemetenteTipo.Agente
            ? Remetente.Agente(command.AgenteId ?? Guid.Empty)
            : Remetente.Cliente();

        var novaMensagem = new Mensagem(command.Texto, remetente, anexoUrl);

        conversa.AdicionarMensagem(novaMensagem);

        if (command.RemetenteTipo == RemetenteTipo.Agente)
        {
            // Precisamos do número do contato para enviar a mensagem
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

            conversa.AdicionarMensagem(novaMensagem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var messageDto = novaMensagem.ToDto();
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), messageDto);

        return messageDto;
    }
}