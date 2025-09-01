using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using CRM.Domain.DomainEvents;

namespace Conversations.Application.UseCases.Events;

public class ConversaResolvidaEventHandler : IDomainEventHandler<ConversaResolvidaEvent>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IMetaMessageSender _metaSender;

    public ConversaResolvidaEventHandler(
        IConversationRepository conversationRepository,
        IContactRepository contactRepository,
        IMetaMessageSender metaSender)
    {
        _conversationRepository = conversationRepository;
        _contactRepository = contactRepository;
        _metaSender = metaSender;
    }

    public async Task Handle(ConversaResolvidaEvent domainEvent, CancellationToken cancellationToken)
    {
        var conversa = await _conversationRepository.GetByIdAsync(domainEvent.ConversaId, cancellationToken);
        if (conversa is null) return;

        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is null) return;

        Console.WriteLine($"--> Enviando pesquisa de satisfação para a conversa {conversa.Id}");

        await _metaSender.EnviarPesquisaDeSatisfacaoAsync(contato.Telefone, conversa.Id);
    }
}