using Agents.Domain.Aggregates;
using Agents.Domain.Entities;

namespace CRM.Application.Interfaces;

public interface ITokenService
{
    Task<string> GerarToken(Agente agente);
}