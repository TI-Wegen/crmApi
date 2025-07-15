using CRM.Application.Interfaces;
using Templates.Application.Dtos;

namespace Templates.Application.UseCases.Queries;

public record GetAllTemplatesQuery() : IQuery<IEnumerable<TemplateDto>>;


