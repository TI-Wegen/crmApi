using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands.Handlers;

public class IniciarConversaCommandHandler : ICommandHandler<IniciarConversaCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository; // NOVO: Repositório para Atendimento
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaMessageSender _metaSender;
    private readonly IBotSessionCache _botSessionCache;

    public IniciarConversaCommandHandler(
        IConversationRepository conversationRepository,
        IAtendimentoRepository atendimentoRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        IMetaMessageSender metaSender,
        IBotSessionCache botSessionCache)
    {
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _metaSender = metaSender;
        _botSessionCache = botSessionCache;
    }

    public async Task<Guid> HandleAsync(IniciarConversaCommand command, CancellationToken cancellationToken)
    {
        // 1. Encontra a Conversa (o histórico) ou a prepara para criação.
        var conversa = await _conversationRepository.FindActiveByContactIdAsync(command.ContatoId, cancellationToken);
        if (conversa is not null)
        {
            var atendimentoAtivo = await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);
            if (atendimentoAtivo is not null)
            {
           
                var mensagemEmAndamento = new Mensagem(conversa.Id, atendimentoAtivo.Id, command.TextoDaPrimeiraMensagem, Remetente.Cliente(), null);
                conversa.AdicionarMensagem(mensagemEmAndamento, atendimentoAtivo.Id);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return conversa.Id;
            }
        }

        if (conversa is null)
        {
            conversa = Conversa.Iniciar(command.ContatoId);
            await _conversationRepository.AddAsync(conversa, cancellationToken);
        }

        // 3. Cria o novo Atendimento para esta interação.
        var novoAtendimento = Atendimento.Iniciar(conversa.Id);
        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        // 4. Cria a primeira Mensagem, agora que temos todos os IDs.
        var primeiraMensagem = new Mensagem(conversa.Id, novoAtendimento.Id, command.TextoDaPrimeiraMensagem, Remetente.Cliente(), null);
        conversa.AdicionarMensagem(primeiraMensagem, novoAtendimento.Id);

        // 5. Salva tudo no banco de dados ANTES de operações externas.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Com os dados persistidos, inicia o fluxo do bot.
        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is null)
            throw new InvalidOperationException($"Contato com ID {conversa.ContatoId} não encontrado.");

        var sessionState = new BotSessionState(novoAtendimento.Id, novoAtendimento.BotStatus);
        await _botSessionCache.SetStateAsync(contato.Telefone, sessionState, TimeSpan.FromHours(2));

        var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";
        await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, menuText);

        return conversa.Id;
    }
}