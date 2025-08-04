namespace Agents.Application.UseCases.Queries.Handler;

// Em Modules/Agents/Application/UseCases/Queries/Handlers/
using Agents.Application.Dtos;
using Agents.Application.Mappers;
using Agents.Application.UseCases.Queries;
using Agents.Domain.Repository;
using CRM.Application.Interfaces;

public class GetAllAgentsQueryHandler : IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>>
{
    private readonly IAgentRepository _agentRepository;

    public GetAllAgentsQueryHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<IEnumerable<AgenteDto>> HandleAsync(GetAllAgentsQuery query, CancellationToken cancellationToken)
    {
        var agentes = await _agentRepository.GetAllAsync(query.PageNumber, query.PageSize, false, cancellationToken);

        return agentes.Select(agente => agente.ToDto());
    }
}