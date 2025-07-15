namespace Templates.Application.UseCases.Queries.Handler;

using CRM.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Templates.Application.Dtos;
using Templates.Application.Mappers;
using Templates.Domain.Repositories;

public class GetAllTemplatesQueryHandler : IQueryHandler<GetAllTemplatesQuery, IEnumerable<TemplateDto>>
{
    private readonly ITemplateRepository _templateRepository;

    public GetAllTemplatesQueryHandler(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<IEnumerable<TemplateDto>> HandleAsync(GetAllTemplatesQuery query, CancellationToken cancellationToken)
    {
        var templates = await _templateRepository.GetAllAsync(cancellationToken);
        return templates.Select(t => t.ToDto());
    }
}