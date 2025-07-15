namespace Templates.Application.Dtos;

public record TemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Language { get; init; }
    public string Body { get; init; }
    public string? Description { get; init; }
}
