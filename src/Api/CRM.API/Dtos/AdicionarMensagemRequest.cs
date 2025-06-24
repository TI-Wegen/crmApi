namespace CRM.API.Dtos;

public class AdicionarMensagemRequest
{
    public string Texto { get; set; }
    public string RemetenteTipo { get; set; }
    public Guid? AgenteId { get; set; }
    public IFormFile? Anexo { get; set; }
}