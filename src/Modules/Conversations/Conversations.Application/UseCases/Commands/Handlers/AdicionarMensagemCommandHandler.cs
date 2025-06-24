namespace Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

// Implementa a interface que retorna um resultado, neste caso, o DTO da mensagem criada.
public class AdicionarMensagemCommandHandler : ICommandHandler<AdicionarMensagemCommand, MessageDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRealtimeNotifier _notifier; 

    public AdicionarMensagemCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IRealtimeNotifier notifier)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _notifier = notifier;
    }

    public async Task<MessageDto> HandleAsync(AdicionarMensagemCommand command, CancellationToken cancellationToken)
    {
        // 1. Carregamos o agregado. Não precisamos das mensagens anteriores, então GetByIdAsync é suficiente.
        var conversa = await _conversationRepository.GetByIdAsync(command.ConversaId, cancellationToken);
        if (conversa is null)
            throw new NotFoundException($"Conversa com o Id '{command.ConversaId}' não encontrada.");

        string? anexoUrl = null;
        if (command.AnexoStream is not null)
        {
            // Gera um nome de arquivo único para evitar colisões
            var nomeUnicoAnexo = $"{Guid.NewGuid()}-{command.AnexoNome}";
            anexoUrl = await _fileStorageService.UploadAsync(
                command.AnexoStream,
                nomeUnicoAnexo,
                command.AnexoContentType!);
        }

        // 2. Criamos o Value Object 'Remetente' a partir dos dados do comando.
        var remetente = command.RemetenteTipo == RemetenteTipo.Agente
            ? Remetente.Agente(command.AgenteId ?? Guid.Empty)
            : Remetente.Cliente();

        // 3. Criamos a entidade 'Mensagem'.
        var novaMensagem = new Mensagem(command.Texto, remetente, anexoUrl);

        // 4. Invocamos o método de domínio.
        conversa.AdicionarMensagem(novaMensagem);

        // 5. Persistimos as alterações.
        await _conversationRepository.UpdateAsync(conversa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var messageDto = novaMensagem.ToDto();

        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), messageDto);

        return novaMensagem.ToDto();
    }
}