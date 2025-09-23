using Agents.Application.Dtos;
using Agents.Application.Mappers;
using Agents.Application.Repositories;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries.Handler;

public class GetAllAgentsQueryHandler : IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>>
{
    private readonly IAgentRepository _agentRepository;

    public GetAllAgentsQueryHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<IEnumerable<AgenteDto>> HandleAsync(GetAllAgentsQuery query, CancellationToken cancellationToken)
    {
        var agentes =
            await _agentRepository.FilterAsync(query.PageNumber, query.PageSize, false, null, cancellationToken);

        return agentes.Select(agente => agente.ToDto());
    }
}