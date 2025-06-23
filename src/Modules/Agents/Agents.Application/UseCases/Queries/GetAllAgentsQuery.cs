using Agents.Application.Dtos;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;

public record GetAllAgentsQuery(int PageNumber, int PageSize) : IQuery<IEnumerable<AgenteDto>>;

