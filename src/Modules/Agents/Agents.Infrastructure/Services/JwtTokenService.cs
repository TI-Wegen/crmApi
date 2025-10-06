using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Agents.Application.Repositories;
using Agents.Domain.Aggregates;
using Agents.Domain.Entities;
using CRM.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Agents.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;
    private readonly IAgentRepository _agentRepository;

    public JwtTokenService(IConfiguration config, IAgentRepository agentRepository)
    {
        _config = config;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]));
        _agentRepository = agentRepository;
    }

    public async Task<string> GerarToken(Agente agente)
    {
        var setor = await _agentRepository.GetSetorByIdAsync(agente.SetorId, CancellationToken.None);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, agente.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, agente.Email),
            new Claim(JwtRegisteredClaimNames.Name, agente.Nome),
            new Claim("setorId", setor?.Id.ToString() ?? string.Empty),
            new Claim("setorNome", setor?.Nome ?? string.Empty),
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}