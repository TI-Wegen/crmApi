using Conversations.Application.Abstractions;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AtribuirAgenteCommandHandler : ICommandHandler<AtribuirAgenteCommand>
{
    // A dependência agora é no repositório de Atendimentos
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtribuirAgenteCommandHandler(IAtendimentoRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtribuirAgenteCommand command, CancellationToken cancellationToken)
    {
        // 1. Buscar o agregado de Atendimento pelo seu ID.
        var atendimento = await _atendimentoRepository.GetByIdAsync(command.AtendimentoId, cancellationToken);

        // 2. Validar se o atendimento existe.
        if (atendimento is null)
        {
            throw new NotFoundException($"Atendimento com o Id '{command.AtendimentoId}' não encontrado.");
        }

        // 3. Executar a lógica de negócio, que está protegida dentro do agregado Atendimento.
        // O handler não sabe as regras (ex: só pode atribuir se estiver na fila), ele apenas invoca.
        atendimento.AtribuirAgente(command.AgenteId);

        // 4. Persistir a mudança no banco de dados.
        // Não precisamos chamar um método UpdateAsync. O Change Tracker do EF Core já detectou
        // a mudança no estado do atendimento e o SaveChangesAsync cuidará disso.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}