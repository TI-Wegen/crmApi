using Agents.Domain.Aggregates;
using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Events;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class EnviarTemplateCommandHandler : ICommandHandler<EnviarTemplateCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMetaMessageSender _metaSender;
    private readonly IMensageriaBotService _mensageriaBotService;
    private readonly ICommandHandler<RegistrarMensagemEnviadaCommand> _registrarMensagemHandler;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public EnviarTemplateCommandHandler(
        IContactRepository contactRepository,
        IMetaMessageSender metaSender,
        ICommandHandler<RegistrarMensagemEnviadaCommand> registrarMensagemHandler,
        IAtendimentoRepository atendimentoRepository,
        IMensageriaBotService mensageriaBotService,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IConversationRepository conversationRepository
        )
    {
        _contactRepository = contactRepository;
        _metaSender = metaSender;
        _mensageriaBotService = mensageriaBotService;
        _registrarMensagemHandler = registrarMensagemHandler;
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _conversationRepository = conversationRepository;
    }
    public async Task HandleAsync(EnviarTemplateCommand command, CancellationToken cancellationToken)
    {
        var agenteId = _userContext.GetCurrentUserId() ??
            throw new UnauthorizedAccessException("Agente não autenticado.");

        var contato = await _contactRepository.GetByIdAsync(command.ContatoId, cancellationToken) ??
            throw new NotFoundException($"Contato com o ID {command.ContatoId} não encontrado.");

        var conversa = await _conversationRepository.FindActiveByContactIdAsync(contato.Id, cancellationToken);
        if (conversa is null)
        {
            conversa = Conversa.Iniciar(contato.Id);
            await _conversationRepository.AddAsync(conversa);
        }

        var novoAtendimento = Atendimento.IniciarProativamente(conversa.Id, agenteId);
        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        // Salva a criação do novo atendimento no banco PRIMEIRO.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Chama o nosso novo serviço centralizado que faz tudo: envia, registra e notifica.
        await _mensageriaBotService.EnviarETemplateAsync(
            novoAtendimento.Id,
            contato.Telefone,
            command.TemplateName,
            command.BodyParameters);

        // O evento de domínio pode ser disparado aqui se ainda for necessário para outras métricas.
        novoAtendimento.AddDomainEvent(new TemplateEnviadoEvent(
            novoAtendimento.Id, agenteId, command.TemplateName, DateTime.UtcNow));

        // Salva o evento de domínio.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}