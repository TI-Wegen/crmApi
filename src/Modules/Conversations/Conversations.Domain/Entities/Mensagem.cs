using Conversations.Domain.Exceptions;
using Conversations.Domain.ValueObjects;

namespace Conversations.Domain.Entities;

    public class Mensagem
{
    public Guid Id { get; private set; }
    public string Texto { get; private set; }
    public string MessageId {get; private set; }
    public string? AnexoUrl { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Remetente Remetente { get; private set; }

    internal Mensagem(string texto, Remetente remetente, string? anexoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(texto) && string.IsNullOrWhiteSpace(anexoUrl))
            throw new DomainException("A mensagem precisa ter um texto ou um anexo.");

        Id = Guid.NewGuid();
        Texto = texto;
        Remetente = remetente;
        AnexoUrl = anexoUrl;
        Timestamp = DateTime.UtcNow;
    }

    public void AdicionarMessageId(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            throw new DomainException("O MessageId não pode ser nulo ou vazio.");
        MessageId = messageId;
    }
}

