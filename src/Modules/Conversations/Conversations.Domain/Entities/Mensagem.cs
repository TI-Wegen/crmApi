using Conversations.Domain.ValueObjects;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;

namespace Conversations.Domain.Entities;

public class Mensagem : Entity
{
    public Guid ConversaId { get; private set; }
    public Guid AtendimentoId { get; private set; }
    public string? Texto { get; private set; }
    public string? MessageId { get; private set; }
    public string? AnexoUrl { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Remetente Remetente { get; private set; }

    public Mensagem(Guid conversaId, Guid atendimentoId, string? texto, Remetente remetente, DateTime timestamp,
        string? anexoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(texto) && string.IsNullOrWhiteSpace(anexoUrl))
            throw new DomainException("A mensagem precisa ter um texto ou um anexo.");

        ConversaId = conversaId;
        AtendimentoId = atendimentoId;
        Texto = texto;
        Remetente = remetente;
        AnexoUrl = anexoUrl;
        Timestamp = timestamp;
    }

    private Mensagem()
    {
    }

    public void AdicionarMessageId(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new DomainException("O MessageId não pode ser nulo ou vazio.");
        MessageId = messageId;
    }

    public void SetConversaId(Guid conversaId)
    {
        ConversaId = conversaId;
    }

    public void SetAtendimentoId(Guid atendimentoId)
    {
        AtendimentoId = atendimentoId;
    }
}