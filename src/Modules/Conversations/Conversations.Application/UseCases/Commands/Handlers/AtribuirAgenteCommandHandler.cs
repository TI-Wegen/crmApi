using Conversations.Application.Abstractions;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class AtribuirAgenteCommandHandler : ICommandHandler<AtribuirAgenteCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtribuirAgenteCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AtribuirAgenteCommand command, CancellationToken cancellationToken)
    {
        // 1. Buscar o agregado do repositório.
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);

        // 2. Validar se o agregado existe.
        if (conversa is null)
        {
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");
        }

        // 3. Executar a lógica de negócio, que está PROTEGIDA dentro do agregado.
        // A camada de aplicação não sabe as regras, ela apenas invoca o método.
        conversa.AtribuirAgente(command.AgenteId);

        // 4. Persistir a mudança no banco de dados.
        // O UpdateAsync pode ser necessário dependendo da implementação do EF Core (Change Tracker).
        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
