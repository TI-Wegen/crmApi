using Agents.Application.Repositories;

namespace Agents.Application.UseCases.Commands.Handlers;

using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class InativarAgenteCommandHandler : ICommandHandler<InativarAgenteCommand>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InativarAgenteCommandHandler(IAgentRepository agentRepository, IUnitOfWork unitOfWork)
    {
        _agentRepository = agentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(InativarAgenteCommand command, CancellationToken cancellationToken)
    {
        var agente = await _agentRepository.GetByIdAsync(command.AgenteId, cancellationToken);
        if (agente is null)
            throw new NotFoundException($"Agente com o Id '{command.AgenteId}' não encontrado.");

        agente.Inativar();

        await _agentRepository.UpdateAsync(agente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}