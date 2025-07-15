using Agents.Application.Dtos;
using Agents.Domain.Repository;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries.Handler;

public class GetSetoresQueryHandler : IQueryHandler<GetSetoresQuery, IEnumerable<SetorDto>>
{
    private readonly IAgentRepository _agentRepository;

    public GetSetoresQueryHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<IEnumerable<SetorDto>> HandleAsync(GetSetoresQuery query, CancellationToken cancellationToken = default)
    {
        var setores = await _agentRepository.GetSetoresAsync(cancellationToken);

        return setores.Select(setor => new SetorDto
        {
            Id = setor.Id,
            Nome = setor.Nome,
        });
    }
}

