using Conversations.Application.Dtos;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries;

public record GetConversationByContactQuery(Guid ContactId, int PageNumber, int PageSize)
    : IQuery<ConversationDetailsDto>;