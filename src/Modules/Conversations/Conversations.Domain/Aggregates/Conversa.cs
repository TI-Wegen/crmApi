using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using CRM.Domain.Exceptions;
using CRM.Domain.DomainEvents;

namespace Conversations.Domain.Aggregates;

public class Conversa: Entity
{
    public Guid Id { get; private set; }
    public Guid ContatoId { get; private set; }
    public Guid? AgenteId { get; private set; }
    public Guid? SetorId { get; private set; } 
    public ConversationStatus Status { get; private set; }
    private readonly List<Mensagem> _mensagens = new();
    public IReadOnlyCollection<Mensagem> Mensagens => _mensagens.AsReadOnly();
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
    public BotStatus BotStatus { get; private set; }
    private Conversa() { }

    public static Conversa Iniciar(Guid contatoId, Mensagem primeiraMensagem)
    {
        // Regra de Negócio (Invariante): Validações antes da criação.
        if (contatoId == Guid.Empty)
            throw new DomainException("Uma conversa precisa estar associada a um contato.");

        if (primeiraMensagem == null)
            throw new DomainException("Uma conversa não pode ser iniciada sem uma primeira mensagem.");

        var conversa = new Conversa
        {
            Id = Guid.NewGuid(),
            ContatoId = contatoId,
            Status = ConversationStatus.AguardandoNaFila, // O estado inicial definido 
            BotStatus =  BotStatus.AguardandoOpcaoMenuPrincipal, // Estado inicial do bot
        };

        // A primeira mensagem é adicionada ao estado interno da conversa.
        conversa._mensagens.Add(primeiraMensagem);

        var evento = new ConversaIniciadaEvent(conversa.Id, conversa.ContatoId, DateTime.UtcNow);
        conversa.AddDomainEvent(evento);
        return conversa;
    }

    public void AtribuirAgente(Guid agenteId)
    {
        // Não se pode atribuir uma conversa que não está aguardando na fila.
        if (Status != ConversationStatus.AguardandoNaFila)
            throw new DomainException("A conversa não pode ser atribuída, pois não está aguardando na fila.");

        if (agenteId == Guid.Empty)
            throw new DomainException("O AgenteId fornecido é inválido.");

        AgenteId = agenteId;
        Status = ConversationStatus.EmAtendimento; // Atinge o novo estado 

        var evento = new ConversaAtribuidaEvent(this.Id, agenteId, DateTime.UtcNow);
        this.AddDomainEvent(evento);

    }

    public void AdicionarMensagem(Mensagem novaMensagem)
    {
        // Regra 1: Validações de estado terminal (não pode adicionar msg em conversa resolvida/expirada)
        if (Status is ConversationStatus.Resolvida or ConversationStatus.SessaoExpirada)
        {
            throw new DomainException($"Não é possível adicionar mensagens a uma conversa com status '{Status}'.");
        }

        var remetenteIsAgente = novaMensagem.Remetente.Tipo == RemetenteTipo.Agente;

        // Regra 2: A nova lógica de atribuição implícita
        if (Status == ConversationStatus.AguardandoNaFila && remetenteIsAgente)
        {
            // Se a conversa está na fila e um agente responde, ele se torna o dono.
            Status = ConversationStatus.EmAtendimento;
            AgenteId = novaMensagem.Remetente.AgenteId;

            // Disparamos o evento para que outros sistemas saibam da atribuição
            AddDomainEvent(new ConversaAtribuidaEvent(this.Id, AgenteId.Value, DateTime.UtcNow));
        }
        // Regra 3: Tratamento de "Race Condition"
        else if (Status == ConversationStatus.EmAtendimento && remetenteIsAgente && AgenteId != novaMensagem.Remetente.AgenteId)
        {
            // Se outro agente tentar responder uma conversa que já está em atendimento por outra pessoa.
            throw new DomainException("Esta conversa já está sendo atendida por outro agente.");
        }

        // Regra 4: Validação da janela de 24h da Meta
        if (Status == ConversationStatus.SessaoExpirada && remetenteIsAgente)
        {
            throw new DomainException("A sessão de 24h expirou. O agente não pode mais enviar mensagens de formato livre.");
        }

        // Ação Principal: Adicionar a mensagem e disparar o evento
        _mensagens.Add(novaMensagem);
        AddDomainEvent(new MensagemAdicionadaEvent(this.Id, novaMensagem.Id, novaMensagem.Texto, DateTime.UtcNow));
    }

    public void Resolver()
    {
        if (Status != ConversationStatus.EmAtendimento)
            throw new DomainException("Apenas conversas em atendimento podem ser resolvidas.");

        Status = ConversationStatus.Resolvida;

        var evento = new ConversaResolvidaEvent(this.Id, this.AgenteId, DateTime.UtcNow);
        this.AddDomainEvent(evento);
    }

    // Dentro da classe Conversa
    public void Transferir(Guid novoAgenteId, Guid novoSetorId)
    {
        // Regra de negócio: só pode transferir conversas em andamento.
        if (Status != ConversationStatus.EmAtendimento)
            throw new DomainException("Apenas conversas em atendimento podem ser transferidas.");

        if (novoAgenteId == Guid.Empty)
            throw new DomainException("O novo AgenteId é inválido.");

        AgenteId = novoAgenteId;
         SetorId = novoSetorId; 

        var evento = new ConversaTransferidaEvent(this.Id, novoAgenteId, novoSetorId);
        this.AddDomainEvent(evento);

    }

    public void MarcarComoExpirada()
    {
        if (Status is not ConversationStatus.AguardandoNaFila and not ConversationStatus.EmAtendimento)
            throw new DomainException("A conversa não pode ser marcada como expirada, pois já foi resolvida.");
        Status = ConversationStatus.SessaoExpirada;

        var evento = new ConversaExpiradaEvent(this.Id, DateTime.UtcNow);
        this.AddDomainEvent(evento);
    }

    public void MarcarComoAguardandoNaFila()
    {
        if (Status != ConversationStatus.SessaoExpirada)
            throw new DomainException("A conversa só pode ser marcada como aguardando na fila se estiver expirada.");
        Status = ConversationStatus.AguardandoNaFila;
        var evento = new ConversaReabertaEvent(this.Id, DateTime.UtcNow);
        this.AddDomainEvent(evento);
    }

    public void Reabrir()
    {
        // A regra de negócio permite reabrir conversas que foram Resolvidas.
        // Podemos incluir SessaoExpirada para maior flexibilidade.
        if (Status is not ConversationStatus.Resolvida and not ConversationStatus.SessaoExpirada)
        {
            throw new DomainException($"Não é possível reabrir uma conversa com o status '{Status}'.");
        }

        // Ao reabrir, a conversa volta para a fila geral, sem agente atribuído.
        Status = ConversationStatus.AguardandoNaFila;
        AgenteId = null; // Importante: Limpa a atribuição anterior.

        var evento = new ConversaReabertaEvent(this.Id, DateTime.UtcNow);
        this.AddDomainEvent(evento);
    }
}

