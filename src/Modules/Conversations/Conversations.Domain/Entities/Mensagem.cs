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
    public string? ExternalId { get; set; } 
    public string? AnexoUrl { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Remetente Remetente { get; private set; }
    public string? ReacaoMensagem { get; private set; }
    
    public Mensagem(Guid conversaId, Guid atendimentoId, string? texto, Remetente remetente, DateTime timestamp,
        string? anexoUrl = null, string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(texto) && string.IsNullOrWhiteSpace(anexoUrl))
            throw new DomainException("A mensagem precisa ter um texto ou um anexo.");

        ConversaId = conversaId;
        AtendimentoId = atendimentoId;
        Texto = texto;
        Remetente = remetente;
        AnexoUrl = anexoUrl;
        Timestamp = timestamp;
        ExternalId = externalId;
    }

    private Mensagem()
    {
    }

    public void SetConversaId(Guid conversaId)
    {
        ConversaId = conversaId;
    }

    public void SetAtendimentoId(Guid atendimentoId)
    {
        AtendimentoId = atendimentoId;
    }
    
    public void SetReacaoMensagem(string reacaoMensagem)
    {
        ReacaoMensagem = reacaoMensagem;
    }
}