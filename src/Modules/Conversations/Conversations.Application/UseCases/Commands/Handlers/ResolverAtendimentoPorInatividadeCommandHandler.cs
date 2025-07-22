using Conversations.Application.Abstractions;
using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class ResolverAtendimentoPorInatividadeCommandHandler : ICommandHandler<ResolverAtendimentoPorInatividadeCommand>
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResolverAtendimentoPorInatividadeCommandHandler> _logger;

    public ResolverAtendimentoPorInatividadeCommandHandler(
        IAtendimentoRepository atendimentoRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResolverAtendimentoPorInatividadeCommandHandler> logger)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(ResolverAtendimentoPorInatividadeCommand command, CancellationToken cancellationToken)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimento is null) return;

        // REGRA DE NEGÓCIO CRÍTICA:
        // Só resolvemos se o atendimento AINDA estiver no bot. Se um agente já o pegou
        // ou se ele já foi resolvido, não fazemos nada.
        if (atendimento.Status == ConversationStatus.EmAutoAtendimento)
        {
            _logger.LogInformation("Resolvendo atendimento {AtendimentoId} por inatividade do bot.", atendimento.Id);
            atendimento.Resolver(SystemGuids.SystemAgentId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("Atendimento {AtendimentoId} já não estava mais com o bot. Nenhuma ação tomada.", atendimento.Id);
        }
    }
}
