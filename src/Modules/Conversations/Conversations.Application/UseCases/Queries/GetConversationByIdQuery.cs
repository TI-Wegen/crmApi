using CRM.Application.Interfaces;
using CRM.Application.Mappers;

namespace Conversations.Application.UseCases.Queries;

public record GetConversationByIdQuery(Guid ConversaId) : IQuery<ConversationDetailsDto>;
