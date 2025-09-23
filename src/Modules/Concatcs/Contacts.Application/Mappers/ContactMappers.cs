using Contacts.Application.Dtos;
using Contacts.Domain.Entities;

namespace Contacts.Application.Mappers;

public static class ContactMappers
{
    public static ContatoDto ToDto(this Contato contato)
    {
        return new ContatoDto
        {
            Id = contato.Id,
            Nome = contato.Nome,
            Telefone = contato.Telefone,
            Status = contato.Status.ToString(),
        };
    }
}