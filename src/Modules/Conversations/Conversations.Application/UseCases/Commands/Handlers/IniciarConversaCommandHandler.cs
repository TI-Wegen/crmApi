using Conversations.Application.Abstractions;
using Conversations.Application.Mappers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
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
       
        var existingConversation = await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);

        var remetente = Remetente.Cliente();
        string? anexoUrl = null; // Lógica do anexo
        var novaMensagem = new Mensagem(command.TextoDaPrimeiraMensagem, remetente, anexoUrl);


        if (existingConversation is not null)
        {
            existingConversation.AdicionarMensagem(novaMensagem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (existingConversation.Status == ConversationStatus.AguardandoNaFila)
            {
                var summaryDto = await _readService.GetSummaryByIdAsync(existingConversation.Id, cancellationToken);
                if (summaryDto is not null)
                {
                    await _notifier.NotificarNovaConversaNaFilaAsync(summaryDto);
                }
            }
            else // O status é EmAtendimento
            {
                
                await _notifier.NotificarNovaMensagemAsync(existingConversation.Id.ToString(), novaMensagem.ToDto());
            }

            return existingConversation.Id;
        }
        else
        {
            var novaConversa = Conversa.Iniciar(command.ContatoId, novaMensagem);
            await _conversationRepository.AddAsync(novaConversa, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var summaryDto = await _readService.GetSummaryByIdAsync(novaConversa.Id, cancellationToken);
            if (summaryDto is not null)
            {
                await _notifier.NotificarNovaConversaNaFilaAsync(summaryDto);
            }

            return novaConversa.Id;
        }
    }
}