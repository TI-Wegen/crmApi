using Conversations.Application.Dtos;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;

namespace Conversations.Application.Mappers;

public static class ConversationMappers
{
    // Método de extensão que converte o agregado para o DTO detalhado
    public static ConversationDetailsDto ToDetailsDto(this Conversa conversa)
    {
        return new ConversationDetailsDto
        {
            Id = conversa.Id,
            ContatoId = conversa.ContatoId,
            AgenteId = conversa.AgenteId,
            SetorId = conversa.SetorId,
            Status = conversa.Status.ToString(), // Converte o Enum para string
            Mensagens = conversa.Mensagens?.Select(m => m.ToDto()).ToList() ?? new List<MessageDto>()
        };
    }

    // Método de extensão que converte a entidade Mensagem para seu DTO
    public static MessageDto ToDto(this Mensagem mensagem)
    {
        return new MessageDto
        {
            Id = mensagem.Id,
            Texto = mensagem.Texto,
            AnexoUrl = mensagem.AnexoUrl,
            Timestamp = mensagem.Timestamp,
            RemetenteTipo = mensagem.Remetente.Tipo.ToString(),
            RemetenteAgenteId = mensagem.Remetente.AgenteId
        };
    }
}