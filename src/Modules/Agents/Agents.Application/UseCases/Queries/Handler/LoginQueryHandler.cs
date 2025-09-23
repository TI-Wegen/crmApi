using Agents.Application.Repository;
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
        var agente = await _agentRepository.GetByEmailAsync(query.Email);
        if (agente == null || !agente.VerificarSenha(query.Password))
        {
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
        }

        var token = await _tokenService.GerarToken(agente);
        return token;
    }
}