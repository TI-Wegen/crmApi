namespace CRM.API.Dtos;

public record AdicionarMensagemRequest(string Texto, string? AnexoUrl, string RemetenteTipo, Guid? AgenteId);
