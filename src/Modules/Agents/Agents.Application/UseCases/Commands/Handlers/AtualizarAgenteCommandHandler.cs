using Agents.Application.Repository;

namespace Agents.Application.UseCases.Commands.Handlers;

using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class AtualizarAgenteCommandHandler : ICommandHandler<AtualizarAgenteCommand>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarAgenteCommandHandler(IAgentRepository agentRepository, IUnitOfWork unitOfWork)
    {
        _agentRepository = agentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtualizarAgenteCommand command, CancellationToken cancellationToken)
    {
        var agente = await _agentRepository.GetByIdAsync(command.AgenteId, cancellationToken);
        if (agente is null)
            throw new NotFoundException($"Agente com o Id '{command.AgenteId}' não encontrado.");

        agente.Atualizar(command.NovoNome, command.NovosSetorIds);

        await _agentRepository.UpdateAsync(agente, cancellationToken); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}