namespace Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Exceptions;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;

// Implementa a interface que retorna um resultado, neste caso, o DTO da mensagem criada.
public class AdicionarMensagemCommandHandler : ICommandHandler<AdicionarMensagemCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdicionarMensagemCommandHandler(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageDto> HandleAsync(AdicionarMensagemCommand command, CancellationToken cancellationToken)
    {
        // 1. Carregamos o agregado. Não precisamos das mensagens anteriores, então GetByIdAsync é suficiente.
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        // 2. Criamos o Value Object 'Remetente' a partir dos dados do comando.
        var remetente = command.RemetenteTipo == RemetenteTipo.Agente
            ? Remetente.Agente(command.AgenteId ?? Guid.Empty)
            : Remetente.Cliente();

        // 3. Criamos a entidade 'Mensagem'.
        var novaMensagem = new Mensagem(command.Texto, remetente, command.AnexoUrl);

        // 4. Invocamos o método de domínio.
        conversa.AdicionarMensagem(novaMensagem);

        // 5. Persistimos as alterações.
        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Mapeamos a nova mensagem para um DTO e a retornamos.
        return novaMensagem.ToDto();
    }
}