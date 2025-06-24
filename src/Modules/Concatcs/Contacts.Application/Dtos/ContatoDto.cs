namespace Contacts.Application.Dtos;

public record ContatoDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; }
    public string Telefone { get; init; }
    public string Status { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; }
}