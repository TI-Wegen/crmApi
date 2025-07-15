using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record EnviarTemplateCommand(
    Guid ContatoId,
    string TemplateName,
    List<string> BodyParameters
) : ICommand;