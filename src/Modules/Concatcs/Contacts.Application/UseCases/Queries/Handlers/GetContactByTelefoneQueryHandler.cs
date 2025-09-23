using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Application.Repositories;
using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands.Queries.Handlers;

public class GetContactByTelefoneQueryHandler : IQueryHandler<GetContactByTelefoneQuery, ContatoDto?>
{
    private readonly IContactRepository _contactRepository;

    public GetContactByTelefoneQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<ContatoDto?> HandleAsync(GetContactByTelefoneQuery query, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetByTelefoneAsync(query.Telefone, cancellationToken);

        // Se o contato não for encontrado, retorna nulo.
        if (contato is null)
        {
            return null;
        }

        return ContactMappers.ToDto(contato);
    }
}