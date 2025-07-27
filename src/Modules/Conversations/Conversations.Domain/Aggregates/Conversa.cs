using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;


namespace Conversations.Domain.Aggregates;

public class Conversa : Entity
{
    public Guid ContatoId { get; private set; }
    public string ContatoNome { get; private set; }

    private readonly List<Mensagem> _mensagens = new();
    public IReadOnlyCollection<Mensagem> Mensagens => _mensagens.AsReadOnly();

    private readonly List<ConversaTag> _tags = new();
    public IReadOnlyCollection<ConversaTag> Tags => _tags.AsReadOnly();
    public SessaoWhatsapp? SessaoAtiva { get; private set; }
    public int TotalSessoesIniciadas { get; private set; }
    private Conversa() { }

    public static Conversa Iniciar(Guid contatoId, string contatoNome)
    {
        if (contatoId == Guid.Empty)
            throw new DomainException("Uma conversa precisa estar associada a um contato.");

        if (string.IsNullOrWhiteSpace(contatoNome))
            throw new DomainException("O nome do contato não pode ser vazio.");

        var conversa = new Conversa
        {
            ContatoId = contatoId,
            ContatoNome = contatoNome
        };
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

    public void IniciarOuRenovarSessao(DateTime dataMensagem)
    {
        if (SessaoAtiva is null || !SessaoAtiva.EstaAtiva(dataMensagem))
        {
            SessaoAtiva = SessaoWhatsapp.Iniciar(dataMensagem);
            TotalSessoesIniciadas++;
            // Disparar evento: NovaSessaoWhatsappIniciadaEvent
        }
    }
}