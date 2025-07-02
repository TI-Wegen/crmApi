namespace Agents.Application.UseCases.Queries.Handler;


// Em Modules/Agents/Application/UseCases/Queries/Handlers/
using Agents.Application.Dtos;
using Agents.Application.Mappers;
using Agents.Application.UseCases.Queries;
using Agents.Domain.Repository;
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
        // 1. Usa o repositório para buscar o agente
        var agente = await _agentRepository.GetByIdAsync(query.AgentId, cancellationToken);

        // 2. Se não encontrar, lança uma exceção que a API saberá como tratar
        if (agente is null)
            throw new NotFoundException($"Agente com o Id '{query.AgentId}' não encontrado.");

        // 3. Mapeia a entidade de domínio para o DTO de resposta e retorna
        return agente.ToDto();
    }
}