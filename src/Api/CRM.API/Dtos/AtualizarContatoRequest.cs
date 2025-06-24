namespace CRM.API.Dtos
{
    public record AtualizarContatoRequest(string Nome, string Telefone, List<string> Tags);

}
