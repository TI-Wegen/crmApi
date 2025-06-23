namespace Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.Abstractions;
using Conversations.Application.Exceptions;
using CRM.Application.Interfaces;

public class TransferirConversaCommandHandler : ICommandHandler<TransferirConversaCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferirConversaCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TransferirConversaCommand command, CancellationToken cancellationToken)
    {
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        // Nota de Arquitetura: Em um cenário real, poderíamos injetar aqui
        // IAgentRepository e ISetorRepository para verificar se os novos IDs
        // são válidos antes de prosseguir.

        conversa.Transferir(command.NovoAgenteId, command.NovoSetorId);

        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}