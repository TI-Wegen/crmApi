using Contacts.Application.Dtos;
using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands.Queries;

public record GetAllContactsQuery(int PageNumber, int PageSize, bool IncluirInativos = false) : IQuery<IEnumerable<ContatoDto>>;
