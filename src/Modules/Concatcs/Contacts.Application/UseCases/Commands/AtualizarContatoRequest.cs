namespace Contacts.Application.UseCases.Commands
{
    public record AtualizarContatoRequest(
        string Nome,
        string Telefone,
        Guid? Tags
    );
}