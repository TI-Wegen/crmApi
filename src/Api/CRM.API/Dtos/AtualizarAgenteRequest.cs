namespace CRM.API.Dtos;

public record AtualizarAgenteRequest(string Nome, List<Guid> SetorIds);
