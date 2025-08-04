namespace Contacts.Application.UseCases.Queries.Handlers;


using Contacts.Application.Dtos;
using Contacts.Application.Mappers;
using Contacts.Application.UseCases.Commands.Queries;
using Contacts.Domain.Repository;
using CRM.Application.Interfaces;

public class GetAllContactsQueryHandler : IQueryHandler<GetAllContactsQuery, IEnumerable<ContatoDto>>
{
    private readonly IContactRepository _contactRepository;

    public GetAllContactsQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<IEnumerable<ContatoDto>> HandleAsync(GetAllContactsQuery query, CancellationToken cancellationToken)
    {
        var contatos = await _contactRepository.GetAllAsync(query.PageNumber, query.PageSize, false, cancellationToken);

        return contatos.Select(contato => contato.ToDto());
    }
}