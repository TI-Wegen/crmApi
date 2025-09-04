using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AddTagCommand(
    Guid ContactId,
    Guid TagId
    ) : ICommand;