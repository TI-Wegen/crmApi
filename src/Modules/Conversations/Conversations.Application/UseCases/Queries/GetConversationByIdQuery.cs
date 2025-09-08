using Conversations.Application.Dtos;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries;

public record GetConversationByIdQuery(
    Guid ConversaId, 
    int PageNumber = 1, 
    int PageSize = 20) : IQuery<ConversationDetailsDto>;
