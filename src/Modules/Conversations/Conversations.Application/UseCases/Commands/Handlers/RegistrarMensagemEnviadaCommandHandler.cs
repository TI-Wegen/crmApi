using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using Conversations.Application.Mappers;
using Conversations.Application.Repository;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using CRM.Domain.Common;

namespace Conversations.Application.UseCases.Commands.Handlers;



public class RegistrarMensagemEnviadaCommandHandler : ICommandHandler<RegistrarMensagemEnviadaCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _notifier;

    public RegistrarMensagemEnviadaCommandHandler(
        IContactRepository contactRepository,
        IConversationRepository conversationRepository,
        IAtendimentoRepository atendimentoRepository,
        IUnitOfWork unitOfWork,
        IRealtimeNotifier notifier)
    {
        _contactRepository = contactRepository;
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task HandleAsync(RegistrarMensagemEnviadaCommand command, CancellationToken cancellationToken)
    {
        var timestamp =  DateTime.UtcNow;
        var contato = await _contactRepository.GetByTelefoneAsync(command.ContatoTelefone, cancellationToken);
        if (contato is null)
        {
            contato = Contato.Criar(command.NomeContato, command.ContatoTelefone,"");
            await _contactRepository.AddAsync(contato);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var conversa = await _conversationRepository.FindActiveByContactIdAsync(contato.Id, cancellationToken);

        var novoAtendimento = Atendimento.Iniciar(conversa?.Id ?? Guid.Empty);
        novoAtendimento.AtribuirAgente(SystemGuids.SystemAgentId);
        novoAtendimento.Resolver(SystemGuids.SystemAgentId);

        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);

        if (conversa is null)
        {
            var mensagemParaNovaConversa = new Mensagem(Guid.NewGuid(), novoAtendimento.Id, command.TextoDaMensagem, remetente, timestamp: timestamp, null);
            conversa = Conversa.Iniciar(contato.Id, contato.Nome);
            conversa.SetConversaId( mensagemParaNovaConversa.ConversaId); 

            await _conversationRepository.AddAsync(conversa, cancellationToken);
        }
        else
        {
            var novaMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaMensagem, remetente, timestamp: timestamp, null);
            conversa.AdicionarMensagem(novaMensagem, novoAtendimento.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ultimaMensagem = conversa.Mensagens.Last();
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), ultimaMensagem.ToDto());
    }
}