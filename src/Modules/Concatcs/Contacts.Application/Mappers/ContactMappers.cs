namespace Contacts.Application.Mappers;

// Em Modules/Contacts/Application/Mappers/
using Contacts.Application.Dtos;
using Contacts.Domain.Aggregates;

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
            // Mapeia a lista de Value Objects Tag para uma lista de strings
            Tags = contato.Tags.Select(tag => tag.Texto).ToList()
        };
    }
}