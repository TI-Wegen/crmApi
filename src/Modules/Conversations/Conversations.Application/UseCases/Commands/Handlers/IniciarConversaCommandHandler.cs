using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
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
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _notifier;
    private readonly IConversationReadService _readService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMetaMessageSender _metaSender;
    private readonly IBotSessionCache _botSessionCache;
    public IniciarConversaCommandHandler(
          IConversationRepository conversationRepository,
            IContactRepository contactRepository,
          IUnitOfWork unitOfWork,
          IRealtimeNotifier notifier,
          IFileStorageService fileStorageService,
          IConversationReadService readService,
          IMetaMessageSender metaMessageSender,
            IBotSessionCache botSessionCache
          ) 
    {
       
        _conversationRepository = conversationRepository;
        _contactRepository = contactRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _readService = readService;
        _notifier = notifier;
        _readService = readService;
        _metaSender = metaMessageSender;
        _botSessionCache = botSessionCache;
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
            else 
            {
                var contato = await _contactRepository.GetByIdAsync(command.ContatoId, cancellationToken);
                var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";

                var botSession = await _botSessionCache.GetStateAsync(contato.Telefone);
                if (existingConversation.Status == ConversationStatus.EmAutoAtendimento)
                {
                   
                    if (botSession is null)
                    {
                        var sessionState = new BotSessionState(existingConversation.Id, existingConversation.BotStatus);

                        await _botSessionCache.SetStateAsync(contato.Telefone, sessionState, TimeSpan.FromMinutes(30));


                        await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, menuText);
                    }
                }

                await _notifier.NotificarNovaMensagemAsync(existingConversation.Id.ToString(), novaMensagem.ToDto());
            }

            return existingConversation.Id;
        }
        else
        {
            var novaConversa = Conversa.Iniciar(command.ContatoId, novaMensagem);
            await _conversationRepository.AddAsync(novaConversa, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var contato = await _contactRepository.GetByIdAsync(command.ContatoId, cancellationToken);
            if (contato is null)
            {
                // Lançar exceção ou logar, pois um contato válido é esperado aqui
                throw new InvalidOperationException("Contato não encontrado para enviar mensagem de menu.");
            }
            var sessionState = new BotSessionState(novaConversa.Id, novaConversa.BotStatus);

            await _botSessionCache.SetStateAsync(contato.Telefone, sessionState, TimeSpan.FromMinutes(30));


            var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";
            await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, menuText);

            var summaryDto = await _readService.GetSummaryByIdAsync(novaConversa.Id, cancellationToken);
            if (summaryDto is not null)
            {
                await _notifier.NotificarNovaConversaNaFilaAsync(summaryDto);
            }

            return novaConversa.Id;
        }
    }
}