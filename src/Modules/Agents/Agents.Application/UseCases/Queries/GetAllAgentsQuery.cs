using Agents.Application.Dtos;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;

public record GetAllAgentsQuery(int PageNumber, int PageSize, bool IncluirInativos = false) : IQuery<IEnumerable<AgenteDto>>;


