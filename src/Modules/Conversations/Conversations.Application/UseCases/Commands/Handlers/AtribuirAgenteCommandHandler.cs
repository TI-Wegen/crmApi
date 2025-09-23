using Conversations.Application.Repository;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AtribuirAgenteCommandHandler : ICommandHandler<AtribuirAgenteCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtribuirAgenteCommandHandler(IAtendimentoRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtribuirAgenteCommand command, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);

        if (atendimento is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        atendimento.AtribuirAgente(command.AgenteId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}