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
    public IniciarConversaCommandHandler(
          IConversationRepository conversationRepository,
          IUnitOfWork unitOfWork,
          IRealtimeNotifier notifier, 
          IConversationReadService readService) 
    {
       
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _readService = readService;
        _notifier = notifier;
        _readService = readService;
    }
    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        // 1. Tenta encontrar uma conversa ativa para este contato
        var existingConversation =
            await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);
        // 2. Criamos os objetos de domínio a partir dos dados do comando
        var remetente = Remetente.Cliente();
        var novaMensagem = new Mensagem(command.TextoDaPrimeiraMensagem, remetente, command.AnexoUrl);

        if (existingConversation is not null)
        {
            // 2. SE EXISTE: Adiciona a mensagem à conversa existente
            existingConversation.AdicionarMensagem(novaMensagem);
            //await _conversationRepository.UpdateAsync(existingConversation, cancellationToken); 
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Notifica o frontend sobre a NOVA MENSAGEM em uma conversa existente
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