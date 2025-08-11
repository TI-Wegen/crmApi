using Conversations.Application.Dtos;
using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries;

public record GetAllConversationsQuery(
    int PageNumber = 1,
    int PageSize = 1000,
    ConversationStatus? Status = null,
    Guid? AgenteId = null,
    Guid? SetorId = null
) : IQuery<IEnumerable<ConversationSummaryDto>>;

