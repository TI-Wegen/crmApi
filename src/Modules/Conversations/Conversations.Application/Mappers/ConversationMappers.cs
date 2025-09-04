using Conversations.Application.Dtos;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;

namespace Conversations.Application.Mappers;

public static class ConversationMappers
{
    public static ConversationDetailsDto ToDetailsDto(this Conversa conversa, Atendimento? atendimentoAtivo)
    {
        return new ConversationDetailsDto
        {
            Id = conversa.Id,
            ContatoId = conversa.ContatoId,
            Mensagens = conversa.Mensagens?.Select(m => m.ToDto()).ToList() ?? new List<MessageDto>(),

            AtendimentoId = atendimentoAtivo?.Id,
            AgenteId = atendimentoAtivo?.AgenteId,
            SetorId = atendimentoAtivo?.SetorId,
            TagId = atendimentoAtivo?.TagsId,
            Status = atendimentoAtivo?.Status.ToString() ?? "N/A",
            BotStatus = atendimentoAtivo?.BotStatus.ToString() ?? "N/A",
            SessaoWhatsappAtiva = conversa.SessaoAtiva?.EstaAtiva() ?? false,
            SessaoWhatsappExpiraEm = conversa.SessaoAtiva?.DataFim
        };
    }

    public static MessageDto ToDto(this Mensagem mensagem)
    {
        return new MessageDto
        {
            Id = mensagem.Id,
            Texto = mensagem.Texto,
            AnexoUrl = mensagem.AnexoUrl,
            Timestamp = mensagem.Timestamp,
            RemetenteTipo = mensagem.Remetente.Tipo.ToString(),
            RemetenteAgenteId = mensagem.Remetente.AgenteId,
            Wamid = mensagem.ExternalId
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
            TagId = atendimento.TagsId,
            Status = atendimento.Status.ToString(),
            BotStatus = atendimento.BotStatus.ToString(),
            DataFinalizacao = atendimento.DataFinalizacao,
            Avaliacao = atendimento.Avaliacao != null
                ? new AvaliacaoDto { Nota = atendimento.Avaliacao.Nota, Comentario = atendimento.Avaliacao.Comentario }
                : null
        };
    }
}