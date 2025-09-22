using Agents.Application.Dtos;
using Agents.Domain.Aggregates;

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
            CargaDeTrabalho = agente.CargaDeTrabalho.Valor,
            SetorId = agente.SetorId
        };
    }
}