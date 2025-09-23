using Agents.Application.Repository;

namespace Agents.Application.UseCases.Queries.Handler;


using Agents.Application.Dtos;
using Agents.Application.Mappers;
using Agents.Application.UseCases.Queries;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class GetAgentByIdQueryHandler : IQueryHandler<GetAgentByIdQuery, AgenteDto>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentByIdQueryHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgenteDto> HandleAsync(GetAgentByIdQuery query, CancellationToken cancellationToken)
    {
        var agente = await _agentRepository.GetByIdAsync(query.AgentId, cancellationToken);

        if (agente is null)
            throw new NotFoundException($"Agente com o Id '{query.AgentId}' não encontrado.");

        return agente.ToDto();
    }
}