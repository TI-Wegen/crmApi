namespace Contacts.Application.UseCases.Commands.Handlers;


// Em Modules/Contacts/Application/UseCases/Queries/Handlers/
using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Application.UseCases.Commands.Queries;
using Contacts.Domain.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class GetContactByIdQueryHandler : IQueryHandler<GetContactByIdQuery, ContatoDto>
{
    private readonly IContactRepository _contactRepository;

    public GetContactByIdQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<ContatoDto> HandleAsync(GetContactByIdQuery query, CancellationToken cancellationToken)
    {
        // Para popular o DTO, precisamos das tags e do histórico.
        // Vamos usar um método de repositório que inclua esses detalhes.
        var contato = await _contactRepository.GetByIdWithDetailsAsync(query.ContactId, cancellationToken);

        if (contato is null)
            throw new NotFoundException($"Contato com o Id '{query.ContactId}' não encontrado.");

        return contato.ToDto();
    }
}
