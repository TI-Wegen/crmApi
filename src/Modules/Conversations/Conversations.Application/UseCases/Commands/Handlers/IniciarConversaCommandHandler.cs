using Conversations.Application.Abstractions;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

  public class IniciarConversaCommandHandler : ICommandHandler<IniciarConversaCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _notifier;
    private readonly IConversationReadService _readService;
    private readonly IFileStorageService _fileStorageService;
    public IniciarConversaCommandHandler(
          IConversationRepository conversationRepository,
          IUnitOfWork unitOfWork,
          IRealtimeNotifier notifier,
          IFileStorageService fileStorageService,
          IConversationReadService readService) 
    {
       
        _conversationRepository = conversationRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _readService = readService;
        _notifier = notifier;
        _readService = readService;
    }
    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        var existingConversation =
            await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);

        var remetente = Remetente.Cliente();
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
        var novaMensagem = new Mensagem(command.TextoDaPrimeiraMensagem, remetente, anexoUrl);

        if (existingConversation is not null)
        {
            try
            {
                existingConversation.AdicionarMensagem(novaMensagem);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao processar a conversa existente.", ex);
            }

            await _notifier.NotificarNovaMensagemAsync(existingConversation.Id.ToString(), novaMensagem.ToDto());

            return existingConversation.Id;
        }
        else
        {
            // 3. SE NÃO EXISTE: Cria uma nova conversa (comportamento antigo)
            var novaConversa = Conversa.Iniciar(command.ContatoId, novaMensagem);
            await _conversationRepository.AddAsync(novaConversa, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notifica o frontend sobre a NOVA CONVERSA na fila
            var summaryDto = await _readService.GetSummaryByIdAsync(novaConversa.Id, cancellationToken);
            if (summaryDto is not null)
            {
                await _notifier.NotificarNovaConversaNaFilaAsync(summaryDto);
            }

            return novaConversa.Id;
        }
    }
}