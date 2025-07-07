//using Contacts.Domain.Aggregates;
//using Contacts.Domain.Repository;
//using Conversations.Application.Abstractions;
//using Conversations.Domain.Aggregates;
//using Conversations.Domain.Entities;
//using Conversations.Domain.ValueObjects;
//using CRM.Application.Interfaces;

//namespace Conversations.Application.UseCases.Commands.Handlers;

//public class RegistrarMensagemEnviadaCommandHandler : ICommandHandler<RegistrarMensagemEnviadaCommand>
//{
//    private readonly IContactRepository _contactRepository;
//    private readonly IConversationRepository _conversationRepository;
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly IRealtimeNotifier _notifier;

//    public RegistrarMensagemEnviadaCommandHandler(IContactRepository contactRepository, 
//        IConversationRepository conversationRepository, 
//        IUnitOfWork unitOfWork, 
//        IRealtimeNotifier notifier)
//    {
//        _contactRepository = contactRepository;
//        _conversationRepository = conversationRepository;
//        _unitOfWork = unitOfWork;
//        _notifier = notifier;
//    }

//    public async Task HandleAsync(RegistrarMensagemEnviadaCommand command, CancellationToken cancellationToken)
//    {
//        // 1. Encontra ou cria o Contato
//        var contato = await _contactRepository.GetByTelefoneAsync(command.ContatoTelefone);
//        if (contato is null)
//        {
//            contato = Contato.Criar(command.NomeContato, command.ContatoTelefone);
//            await _contactRepository.AddAsync(contato);
//        }

//        // 2. Encontra uma conversa ativa ou cria uma nova
//        var conversa = await _conversationRepository.FindActiveByContactIdAsync(contato.Id, cancellationToken);
//        var remetente = Remetente.Agente(null); // Mensagem enviada pelo "Sistema" ou App Externo
//        var novaMensagem = new Mensagem(command.TextoDaMensagem, remetente, null);

//        if (conversa is not null)
//        {
//            conversa.AdicionarMensagem(novaMensagem);
//        }
//        else
//        {
//            // Se não há conversa ativa, o template INICIOU uma nova conversa.
//            conversa = Conversa.Iniciar(contato.Id, novaMensagem);
//            // Imediatamente atribui e marca como em atendimento, pois foi uma ação da empresa.
//            conversa.IniciarTransferenciaParaFila(Guid.Empty); // Ou um setor padrão
//            conversa.AtribuirAgente(null); // Atribuído ao "Sistema"
//        }

//        await _unitOfWork.SaveChangesAsync(cancellationToken);

//        // Notifica o frontend em tempo real para que a conversa apareça/seja atualizada
//        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
//    }
//}