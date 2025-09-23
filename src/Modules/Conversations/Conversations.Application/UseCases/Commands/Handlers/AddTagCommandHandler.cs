using Conversations.Application.Repositories;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AddTagCommandHandler : ICommandHandler<AddTagCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConversationRepository _conversationRepository;

    public AddTagCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _conversationRepository = conversationRepository;
    }
    public Task HandleAsync(AddTagCommand command, CancellationToken cancellationToken = default)
    {
        return _conversationRepository.AddTagAtendimento(command.ContactId, command.TagId, cancellationToken);
    }
}