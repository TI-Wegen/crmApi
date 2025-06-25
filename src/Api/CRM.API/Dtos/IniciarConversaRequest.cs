namespace CRM.API.Dtos;

public record IniciarConversaRequest(Guid ContatoId, string Texto, IFormFile? Anexo);

