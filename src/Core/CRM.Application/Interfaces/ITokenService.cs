using Agents.Domain.Aggregates;

namespace CRM.Application.Interfaces;

public interface ITokenService
{
    string GerarToken(Agente agente);
}
