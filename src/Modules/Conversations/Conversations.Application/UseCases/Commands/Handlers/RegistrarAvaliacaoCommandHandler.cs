using Conversations.Application.Abstractions;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class RegistrarAvaliacaoCommandHandler : ICommandHandler<RegistrarAvaliacaoCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarAvaliacaoCommandHandler(IAtendimentoRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(RegistrarAvaliacaoCommand command, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimento is null) return; 

        var avaliacao = Avaliacao.Criar(command.Nota);
        atendimento.AdicionarAvaliacao(avaliacao);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
