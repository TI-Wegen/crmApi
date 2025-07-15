namespace Conversations.Application.UseCases.Commands.Handlers;

using Conversations.Application.Abstractions;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

public class ResolverAtendimentoCommandHandler : ICommandHandler<ResolverAtendimentoCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public ResolverAtendimentoCommandHandler(IAtendimentoRepository atendimentoRepository, 
        IUnitOfWork unitOfWork,
        IUserContext userContext
        )
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }
      public async Task HandleAsync(ResolverAtendimentoCommand command, CancellationToken cancellationToken)
    {
        // 1. Carrega o agregado de Atendimento.
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimento is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        var agenteIdLogado = _userContext.GetCurrentUserId();
        if (agenteIdLogado is null)
        {
            throw new UnauthorizedAccessException("Usuário não está autenticado ou não possui um agente associado.");
        }


        atendimento.Resolver(agenteIdLogado);

        // 3. Persiste a alteração de estado no banco de dados.
        // Não é necessário chamar UpdateAsync, pois o Change Tracker já sabe da mudança.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}