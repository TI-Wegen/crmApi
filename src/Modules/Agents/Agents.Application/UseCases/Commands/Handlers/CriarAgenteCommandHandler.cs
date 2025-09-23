using Agents.Application.Repository;

namespace Agents.Application.UseCases.Commands.Handlers;

using Agents.Application.Dtos;
using Agents.Application.Mappers; 
using Agents.Domain.Aggregates;
using CRM.Application.Interfaces;

public class CriarAgenteCommandHandler : ICommandHandler<CriarAgenteCommand, AgenteDto>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarAgenteCommandHandler(IAgentRepository agentRepository, IUnitOfWork unitOfWork)
    {
        _agentRepository = agentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AgenteDto> HandleAsync(CriarAgenteCommand command, CancellationToken cancellationToken)
    {
        var existingAgent = await _agentRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingAgent is not null)
        {
            throw new Exception($"Já existe um agente com o e-mail '{command.Email}'.");
        }

        var agente = Agente.Criar(command.Nome, command.Email);
        agente.DefinirSenha(command.Senha);
        await _agentRepository.AddAsync(agente, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return agente.ToDto();
    }
}