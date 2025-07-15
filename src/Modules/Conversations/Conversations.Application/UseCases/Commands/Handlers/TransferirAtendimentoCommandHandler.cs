namespace Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
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
        // 1. Busca o atendimento ATUAL que será transferido.
        var atendimentoAtual = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);
        if (atendimentoAtual is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        // 2. Resolve o atendimento ATUAL, finalizando esta etapa do trabalho.
        // O evento AtendimentoResolvidoEvent será disparado.

        var agenteIdLogado = _userContext.GetCurrentUserId();
        if (agenteIdLogado is null)
        {
            throw new UnauthorizedAccessException("Nenhum agente autenticado para realizar esta ação.");
        }

        // Passa o ID do agente que está resolvendo para o método de domínio.
        atendimentoAtual.Resolver(agenteIdLogado);

        // 3. Cria um NOVO atendimento, ligado à MESMA conversa.
        var novoAtendimento = Atendimento.Iniciar(atendimentoAtual.ConversaId);

        // 4. Imediatamente transfere o novo atendimento para o setor e agente de destino.
        novoAtendimento.IniciarTransferenciaParaFila(command.NovoSetorId);
        novoAtendimento.AtribuirAgente(command.NovoAgenteId); // O status muda para EmAtendimento

        // 5. Adiciona o novo atendimento ao repositório.
        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        // 6. Salva todas as alterações (o atendimento antigo resolvido e o novo criado)
        // em uma única transação.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Opcional: Notificar o frontend sobre a transferência bem-sucedida.
    }
}