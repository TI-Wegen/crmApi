namespace Templates.Application.UseCases.Commands.Handler;

using CRM.Application.Interfaces;
using Templates.Domain.Repositories;

public class AtualizarStatusTemplateCommandHandler : ICommandHandler<AtualizarStatusTemplateCommand>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarStatusTemplateCommandHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtualizarStatusTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByNameAsync(command.TemplateName, cancellationToken);
        if (template is null)
        {
            Console.WriteLine($"Webhook de status recebido para o template '{command.TemplateName}', mas ele não foi encontrado no CRM.");
            return;
        }

        template.AtualizarStatus(command.NovoStatus, command.MotivoRejeicao);

        // O Change Tracker detecta a alteração no status.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"Status do template '{template.Name}' atualizado para '{template.Status}'.");
    }
}