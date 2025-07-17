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
    private readonly ICommandHandler<RegistrarMensagemEnviadaCommand> _registrarMensagemHandler;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public EnviarTemplateCommandHandler(
        IContactRepository contactRepository,
        IMetaMessageSender metaSender,
        ICommandHandler<RegistrarMensagemEnviadaCommand> registrarMensagemHandler,
        IAtendimentoRepository atendimentoRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IConversationRepository conversationRepository
        )
    {
        _contactRepository = contactRepository;
        _metaSender = metaSender;
        _registrarMensagemHandler = registrarMensagemHandler;
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _conversationRepository = conversationRepository;
    }

    public async Task HandleAsync(EnviarTemplateCommand command, CancellationToken cancellationToken)
    {
        var agenteId = _userContext.GetCurrentUserId();
        if (agenteId is null)
            throw new UnauthorizedAccessException("Não foi possível identificar o agente autenticado para enviar o template.");


        var contato = await _contactRepository.GetByIdAsync(command.ContatoId);
        if (contato is null)
            throw new NotFoundException("Contato não encontrado.");

        var conversa = await _conversationRepository.FindActiveByContactIdAsync(contato.Id, cancellationToken);
        if (conversa is null)
        {
            conversa = Conversa.Iniciar(contato.Id);
            await _conversationRepository.AddAsync(conversa);
        }

        var novoAtendimento = Atendimento.IniciarProativamente(conversa.Id, agenteId.Value);
        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        var wamid = await _metaSender.EnviarTemplateAsync(contato.Telefone, command.TemplateName, command.BodyParameters);

        if (string.IsNullOrEmpty(wamid))
            throw new Exception("Não foi possível obter o ID da mensagem da Meta após o envio do template.");


        await _unitOfWork.SaveChangesAsync(cancellationToken);

        novoAtendimento.AddDomainEvent(new TemplateEnviadoEvent(
         novoAtendimento.Id,
         agenteId.Value,
         command.TemplateName,
         DateTime.UtcNow));


        var textoRegistrado = $"Template '{command.TemplateName}' enviado.";
        var registrarCommand = new RegistrarMensagemEnviadaCommand(
            contato.Telefone, 
            contato.Nome, 
            textoRegistrado, 
            wamid,
             novoAtendimento.Id);

        await _registrarMensagemHandler.HandleAsync(registrarCommand, cancellationToken);
    }
}