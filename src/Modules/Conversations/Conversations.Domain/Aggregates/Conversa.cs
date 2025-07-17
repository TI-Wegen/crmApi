using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;

using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;


namespace Conversations.Domain.Aggregates;

public class Conversa : Entity
{
    public Guid ContatoId { get; private set; }

    private readonly List<Mensagem> _mensagens = new();
    public IReadOnlyCollection<Mensagem> Mensagens => _mensagens.AsReadOnly();

    // As Tags podem pertencer à Conversa, pois são sobre o histórico geral do contato.
    private readonly List<ConversaTag> _tags = new();
    public IReadOnlyCollection<ConversaTag> Tags => _tags.AsReadOnly();

    private Conversa() { }

    public static Conversa Iniciar(Guid contatoId)
    {
        if (contatoId == Guid.Empty)
            throw new DomainException("Uma conversa precisa estar associada a um contato.");

        var conversa = new Conversa { ContatoId = contatoId };
        return conversa;
    }

    public void AdicionarMensagem(Mensagem novaMensagem, Guid atendimentoId)
    {
        if (novaMensagem.ConversaId != this.Id)
        {
            novaMensagem.SetConversaId(this.Id);
        }

        // Validação para garantir que o atendimentoId é o correto
        if (novaMensagem.AtendimentoId != atendimentoId)
            throw new DomainException("A mensagem não pertence a este atendimento.");
        _mensagens.Add(novaMensagem);
        AddDomainEvent(new MensagemAdicionadaEvent(this.Id, novaMensagem.Id, novaMensagem.Texto, DateTime.UtcNow));
    }

    public void AdicionarTag(ConversaTag tag)
    {
        if (!_tags.Contains(tag))
        {
            _tags.Add(tag);
        }
    }

    public void RemoverTag(ConversaTag tag)
    {
        if (_tags.Contains(tag))
        {
            _tags.Remove(tag);
        }
    }

    public void SetConversaId(Guid conversaId)
    {
        if (conversaId == Guid.Empty)
            throw new DomainException("O ID da conversa não pode ser vazio.");
        this.Id = conversaId;
    }
}