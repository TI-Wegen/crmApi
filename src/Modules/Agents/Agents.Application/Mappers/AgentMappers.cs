using Agents.Application.Dtos;
using Agents.Domain.Aggregates;
using Agents.Domain.Entities;

namespace Agents.Application.Mappers;
public static class AgentMappers
{
    public static AgenteDto ToDto(this Agente agente)
    {
        return new AgenteDto
        {
            Id = agente.Id,
            Nome = agente.Nome,
            Email = agente.Email,
            Status = agente.Status.ToString(),
            SetorId = agente.SetorId
        };
    }
}