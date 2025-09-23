using Conversations.Application.Repositories;
using Conversations.Domain.Entities;

namespace Conversations.Application.UseCases.Commands.Handlers;

using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class TransferirAtendimentoCommandHandler : ICommandHandler<TransferirAtendimentoCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public TransferirAtendimentoCommandHandler(IAtendimentoRepository atendimentoRepository, 
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task HandleAsync(TransferirAtendimentoCommand command, CancellationToken cancellationToken)
    {
        var atendimentoAtual = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimentoAtual is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        var agenteIdLogado = _userContext.GetCurrentUserId();
        if (agenteIdLogado is null)
        {
            throw new UnauthorizedAccessException("Nenhum agente autenticado para realizar esta ação.");
        }
        atendimentoAtual.Resolver(agenteIdLogado);
        var novoAtendimento = Atendimento.Iniciar(atendimentoAtual.ConversaId);

        novoAtendimento.IniciarTransferenciaParaFila(command.NovoSetorId);
        novoAtendimento.AtribuirAgente(command.NovoAgenteId); 

        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}