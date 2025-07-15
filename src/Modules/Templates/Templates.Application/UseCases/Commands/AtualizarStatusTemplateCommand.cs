using CRM.Application.Interfaces;
using Templates.Domain.Enuns;

namespace Templates.Application.UseCases.Commands;

public record AtualizarStatusTemplateCommand(
    string TemplateName,
    TemplateStatus NovoStatus,
    string? MotivoRejeicao
) : ICommand;

