using CRM.Application.Interfaces;

namespace Templates.Application.UseCases.Commands;

public record CriarTemplateCommand(
    string Name,
    string Language,
    string Body,
    string? Description
) : ICommand;
