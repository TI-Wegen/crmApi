using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;
using CRM.Domain.DomainEvents;
using static Conversations.Domain.Events.AtendimentoEvent;

namespace Conversations.Application.UseCases.Events;

public class AtendimentoResolvidoEventHandler : IDomainEventHandler<AtendimentoResolvidoEvent>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IMetaMessageSender _metaSender;

    public AtendimentoResolvidoEventHandler(
        IAtendimentoRepository atendimentoRepository,
        IConversationRepository conversationRepository,
        IContactRepository contactRepository,
        IMetaMessageSender metaSender)
    {
        _atendimentoRepository = atendimentoRepository;
        _conversationRepository = conversationRepository;
        _contactRepository = contactRepository;
        _metaSender = metaSender;
    }

    public async Task Handle(AtendimentoResolvidoEvent domainEvent, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(domainEvent.AtendimentoId, cancellationToken);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId, cancellationToken);
        if (conversa is null) return;

        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is null) return;

        await _metaSender.EnviarPesquisaDeSatisfacaoAsync(contato.Telefone, atendimento.Id);
    }
}