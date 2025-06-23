using Agents.Application.Dtos;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;


public record GetAgentByIdQuery(Guid AgentId) : IQuery<AgenteDto>;
