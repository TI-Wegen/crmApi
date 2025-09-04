namespace Tags.Application.Dtos;

public record TagDto(
    Guid Id,
    string Nome, 
    string Cor,
    string Descricao
    );