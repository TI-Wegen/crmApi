namespace Templates.Application.UseCases.Commands.Handler;


using CRM.Application.Interfaces;
using CRM.Domain.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using Templates.Application.Abstractions;
using Templates.Application.Dtos;
using Templates.Application.Mappers;
using Templates.Domain.Aggregates;
using Templates.Domain.Repositories;

public class CriarTemplateCommandHandler : ICommandHandler<CriarTemplateCommand, TemplateDto>
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaTemplateManager _metaTemplateManager;
    public CriarTemplateCommandHandler(ITemplateRepository templateRepository, 
        IUnitOfWork unitOfWork,
        IMetaTemplateManager metaTemplateManager)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _metaTemplateManager = metaTemplateManager;
    }

    public async Task<TemplateDto> HandleAsync(CriarTemplateCommand command, CancellationToken cancellationToken)
    {
        var existingTemplate = await _templateRepository.GetByNameAsync(command.Name, cancellationToken);
        if (existingTemplate is not null)
        {
            throw new DomainException($"Um template com o nome '{command.Name}' já existe.");
        }

        var template = MessageTemplate.Criar(command.Name, command.Language, command.Body, command.Description);

        await _templateRepository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _metaTemplateManager.CriarTemplateNaMetaAsync(template);
        }
        catch (Exception ex)
        {
           
            Console.WriteLine($"--> O template '{template.Name}' foi salvo no CRM, mas falhou ao ser enviado para a Meta. Erro: {ex.Message}");
        }


        return template.ToDto();
    }
}