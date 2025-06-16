using Conversations.Application.Dtos;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries;

public record GetConversationByIdQuery(Guid ConversaId) : IQuery<ConversationDetailsDto>;
