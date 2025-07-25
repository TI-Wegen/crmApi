using Conversations.Application.Dtos;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;

namespace Conversations.Application.Mappers;

public static class ConversationMappers
{
    // Método de extensão que converte o agregado para o DTO detalhado
    public static ConversationDetailsDto ToDetailsDto(this Conversa conversa, Atendimento? atendimentoAtivo)
    {
        return new ConversationDetailsDto
        {
            Id = conversa.Id,
            ContatoId = conversa.ContatoId,
            Mensagens = conversa.Mensagens?.Select(m => m.ToDto()).ToList() ?? new List<MessageDto>(),


             // Dados do Atendimento Atual
            AtendimentoId = atendimentoAtivo?.Id,
            AgenteId = atendimentoAtivo?.AgenteId,
            SetorId = atendimentoAtivo?.SetorId,
            Status = atendimentoAtivo?.Status.ToString() ?? "N/A", // Se não houver atendimento ativo
            BotStatus = atendimentoAtivo?.BotStatus.ToString() ?? "N/A",
            SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva(DateTime.UtcNow) ?? false,
            SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
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

    public static AtendimentoDto ToDto(this Atendimento atendimento)
    {
        return new AtendimentoDto
        {
            Id = atendimento.Id,
            ConversaId = atendimento.ConversaId,
            AgenteId = atendimento.AgenteId,
            SetorId = atendimento.SetorId,
            Status = atendimento.Status.ToString(),
            BotStatus = atendimento.BotStatus.ToString(),
            DataFinalizacao = atendimento.DataFinalizacao,
            Avaliacao = atendimento.Avaliacao != null
                ? new AvaliacaoDto { Nota = atendimento.Avaliacao.Nota, Comentario = atendimento.Avaliacao.Comentario }
                : null
        };
    }
}