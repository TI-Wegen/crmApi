namespace CRM.API.Dtos
{
    public record AtualizarContatoRequest(
        string Nome,
        string Telefone,
        Guid? Tags
    );
}