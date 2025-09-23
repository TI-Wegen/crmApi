using Templates.Application.Dtos;
using Templates.Domain.Entities;

namespace Templates.Application.Mappers;

public static class TemplateMappers
{
    public static TemplateDto ToDto(this MessageTemplate template)
    {
        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Language = template.Language,
            Body = template.Body,
            Description = template.Description
        };
    }
}