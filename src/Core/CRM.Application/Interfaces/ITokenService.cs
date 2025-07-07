using Agents.Domain.Aggregates;

namespace CRM.Application.Interfaces;

public interface ITokenService
{
    Task<string> GerarToken(Agente agente);
}
