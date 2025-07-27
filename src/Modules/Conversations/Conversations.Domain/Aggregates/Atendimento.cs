using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;

using CRM.Domain.DomainEvents;
using CRM.Domain.Exceptions;
using static Conversations.Domain.Events.AtendimentoEvent;

namespace Conversations.Domain.Aggregates;

public class Atendimento : Entity
{
    public Guid ConversaId { get; private set; }
    public Guid? AgenteId { get; private set; }
    public Guid? SetorId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public BotStatus BotStatus { get; private set; }
    public Avaliacao? Avaliacao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }



    private Atendimento() { }

    public static Atendimento Iniciar(Guid conversaId)
    {
        var atendimento = new Atendimento
        {
            ConversaId = conversaId,
            Status = ConversationStatus.EmAutoAtendimento,
            BotStatus = BotStatus.AguardandoOpcaoMenuPrincipal
        };
        // Disparar evento: AtendimentoIniciadoEvent(atendimento.Id, conversaId)
        atendimento.AddDomainEvent(new AtendimentoIniciadoEvent(atendimento.Id, conversaId));
        return atendimento;
    }

    public static Atendimento IniciarEmFila(Guid conversaId, Guid setorId)
    {
        var atendimento = new Atendimento
        {
            ConversaId = conversaId,
            Status = ConversationStatus.AguardandoNaFila,
            BotStatus = BotStatus.Nenhum, 
            SetorId = setorId
        };
        atendimento.AddDomainEvent(new AtendimentoIniciadoEvent(atendimento.Id, conversaId));
        return atendimento;
    }

    public void IniciarTransferenciaParaFila(Guid setorId)
    {
        if (Status != ConversationStatus.EmAutoAtendimento)
            throw new DomainException("Apenas um atendimento no bot pode ser transferido para a fila.");

        Status = ConversationStatus.AguardandoNaFila;
        BotStatus = BotStatus.Nenhum;
        SetorId = setorId;
    }

    public void AtribuirAgente(Guid? agenteId)
    {
        if ( Status != ConversationStatus.AguardandoRespostaCliente && Status != ConversationStatus.AguardandoNaFila)
            throw new DomainException("Apenas atendimentos na fila podem ser atribuídos.");

        Status = ConversationStatus.EmAtendimento;
        AgenteId = agenteId;
        // Disparar evento: AtendimentoAtribuidoEvent
    }

    public void Resolver(Guid? agenteIdResolvedor)
    {
        if (Status is ConversationStatus.Resolvida)
            throw new DomainException("Este atendimento já foi finalizado.");

        Status = ConversationStatus.Resolvida;
        BotStatus = BotStatus.Nenhum;
        DataFinalizacao = DateTime.UtcNow;
        AgenteId = agenteIdResolvedor;
        AddDomainEvent(new AtendimentoResolvidoEvent(this.Id, this.AgenteId, DateTime.UtcNow));
    }

    public void AdicionarAvaliacao(Avaliacao novaAvaliacao)
    {
        if (Status != ConversationStatus.Resolvida)
            throw new DomainException("Apenas atendimentos resolvidos podem ser avaliados.");
        Avaliacao = novaAvaliacao;
    }

    public void AguardarCpfParaBoleto()
    {
        if (Status != ConversationStatus.EmAutoAtendimento)
            throw new DomainException("A conversa deve estar em autoatendimento para aguardar CPF.");
        BotStatus = BotStatus.AguardandoCpfParaBoleto;
    }

    public void AguardarEscolhaDeBoleto()
    {
        if (Status != ConversationStatus.EmAutoAtendimento || BotStatus != BotStatus.AguardandoCpfParaBoleto)
            throw new DomainException("Não é possível aguardar a escolha de um boleto neste estado.");
        BotStatus = BotStatus.AguardandoEscolhaDeBoleto;
    }


    public void AguardarCpf()
    {
        if (Status != ConversationStatus.EmAutoAtendimento)
            throw new DomainException("A conversa deve estar em autoatendimento para aguardar CPF.");
        BotStatus = BotStatus.AguardandoCpfParaBoleto;
    }

    public void SetAtendimentoId(Guid atendimentoId)
    {
        if (Id != atendimentoId)
            throw new DomainException("O ID do atendimento não pode ser alterado.");
        Id = atendimentoId;
    }
    public static Atendimento IniciarProativamente(Guid conversaId, Guid agenteId)
    {
        var atendimento = new Atendimento
        {
            ConversaId = conversaId,
            AgenteId = agenteId, 
            Status = ConversationStatus.AguardandoRespostaCliente, 
            BotStatus = BotStatus.Nenhum 
        };
        return atendimento;
    }
    public void RegistrarRespostaDoCliente()
    {
        if (Status != ConversationStatus.AguardandoRespostaCliente)
            throw new DomainException("Este atendimento não estava aguardando uma resposta do cliente.");

        Status = ConversationStatus.EmAtendimento;
        // Disparar evento: ClienteRespondeuTemplateEvent
    }

    public void MarcarComoSemResposta()
    {
        if (Status != ConversationStatus.AguardandoRespostaCliente) return; // Ação idempotente

        Status = ConversationStatus.FechadoSemResposta;
    }
}