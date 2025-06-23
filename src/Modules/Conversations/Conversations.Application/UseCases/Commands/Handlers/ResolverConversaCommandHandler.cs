namespace Conversations.Application.UseCases.Commands.Handlers;

using Conversations.Application.Abstractions;
using Conversations.Application.Exceptions;
using Conversations.Domain.Exceptions;
using CRM.Application.Interfaces;

public class ResolverConversaCommandHandler : ICommandHandler<ResolverConversaCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolverConversaCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ResolverConversaCommand command, CancellationToken cancellationToken)
    {
        // 1. Carregamos o agregado.
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        // 2. Invocamos o método de domínio. A regra de negócio é validada aqui dentro.
        conversa.Resolver();

        // 3. Persistimos a alteração de estado.
        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}