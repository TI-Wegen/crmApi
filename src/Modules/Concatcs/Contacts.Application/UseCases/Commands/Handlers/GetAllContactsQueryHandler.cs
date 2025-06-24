namespace Contacts.Application.UseCases.Commands.Handlers;


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
        // 1. Usa o repositório para buscar os contatos com paginação
        var contatos = await _contactRepository.GetAllAsync(query.PageNumber, query.PageSize, false, cancellationToken);

        // 2. Mapeia a lista de entidades para uma lista de DTOs
        return contatos.Select(contato => contato.ToDto());
    }
}