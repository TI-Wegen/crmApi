namespace Conversations.Application.UseCases.Commands.Handlers;

using Conversations.Application.Abstractions;
using Conversations.Application.Exceptions;
using CRM.Application.Interfaces;

public class ReabrirConversaCommandHandler : ICommandHandler<ReabrirConversaCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReabrirConversaCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ReabrirConversaCommand command, CancellationToken cancellationToken)
    {
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        conversa.Reabrir();

        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}