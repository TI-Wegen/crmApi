using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class EnviarTemplateCommandHandler : ICommandHandler<EnviarTemplateCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMetaMessageSender _metaSender;
    private readonly ICommandHandler<RegistrarMensagemEnviadaCommand> _registrarMensagemHandler;

    public EnviarTemplateCommandHandler(
        IContactRepository contactRepository,
        IMetaMessageSender metaSender,
        ICommandHandler<RegistrarMensagemEnviadaCommand> registrarMensagemHandler)
    {
        _contactRepository = contactRepository;
        _metaSender = metaSender;
        _registrarMensagemHandler = registrarMensagemHandler;
    }

    public async Task HandleAsync(EnviarTemplateCommand command, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetByIdAsync(command.ContatoId);
        if (contato is null)
            throw new NotFoundException("Contato não encontrado.");

        // 1. Envia o template através da API da Meta
        var wamid = await _metaSender.EnviarTemplateAsync(contato.Telefone, command.TemplateName, command.BodyParameters);

        if (string.IsNullOrEmpty(wamid))
            throw new Exception("Não foi possível obter o ID da mensagem da Meta após o envio do template.");

        // 2. REUTILIZA nosso outro caso de uso para registrar a mensagem enviada no nosso CRM
        // O texto pode ser uma representação do template enviado
        var textoRegistrado = $"Template '{command.TemplateName}' enviado.";
        var registrarCommand = new RegistrarMensagemEnviadaCommand(contato.Telefone, contato.Nome, textoRegistrado, wamid);

        await _registrarMensagemHandler.HandleAsync(registrarCommand, cancellationToken);
    }
}