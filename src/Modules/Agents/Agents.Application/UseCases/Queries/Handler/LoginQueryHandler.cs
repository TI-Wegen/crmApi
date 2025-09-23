using Agents.Application.Repositories;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries.Handler;

 public class LoginQueryHandler : IQueryHandler<LoginQuery, string>
{
    private readonly IAgentRepository _agentRepository;
    private readonly ITokenService _tokenService;

    public LoginQueryHandler(IAgentRepository agentRepository, ITokenService tokenService)
    {
        _agentRepository = agentRepository;
        _tokenService = tokenService;
    }

    public async Task<string> HandleAsync(LoginQuery query, CancellationToken cancellationToken)
    {
        var agente = await _agentRepository.FilterAsync(1, 1, false, x => x.Email == query.Email);
        
        if (agente.Any() || !agente.First().VerificarSenha(query.Password))
        {
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
        }

        var token = await _tokenService.GerarToken(agente.First());
        return token;
    }
}