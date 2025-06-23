namespace Agents.Application.Dtos;

public record AgenteDto
{
    public Guid Id { get; init; }
    public string Nome { get; init; }
    public string Email { get; init; }
    public string Status { get; init; }
    public int CargaDeTrabalho { get; init; }
    public IReadOnlyCollection<Guid> SetorIds { get; init; }
}