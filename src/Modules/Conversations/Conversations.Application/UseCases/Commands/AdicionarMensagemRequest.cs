using Microsoft.AspNetCore.Http;

namespace Conversations.Application.UseCases.Commands;

public class AdicionarMensagemRequest
{
    public string Texto { get; set; }
    public string RemetenteTipo { get; set; }
    public string? AnexoUrl { get; set; }
    public Guid? AgenteId { get; set; }
    public IFormFile? Anexo { get; set; }
}