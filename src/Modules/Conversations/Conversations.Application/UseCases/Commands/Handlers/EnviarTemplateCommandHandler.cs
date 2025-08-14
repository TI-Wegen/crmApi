using Agents.Domain.Aggregates;
using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Events;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using CRM.Application.ValueObject;

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
            conversa = Conversa.Iniciar(contato.Id, contato.Nome);
            await _conversationRepository.AddAsync(conversa);
        }

        var novoAtendimento = Atendimento.IniciarProativamente(conversa.Id, agenteId);
        var templateInput = new SendTemplateInput(
            to: contato.Telefone,
            templateName: command.TemplateName,
            parameters: command.BodyParameters,
            type: TemplateType.Text
            );

        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mensageriaBotService.EnviarETemplateAsync(
            novoAtendimento.Id, templateInput);

        novoAtendimento.AddDomainEvent(new TemplateEnviadoEvent(
            novoAtendimento.Id, agenteId, command.TemplateName, DateTime.UtcNow));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}