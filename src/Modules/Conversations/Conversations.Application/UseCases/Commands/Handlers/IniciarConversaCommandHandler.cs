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
        bool isNewConversation = conversa is null;

        // 2. Procura por um Atendimento ATIVO associado a esta Conversa.
        // Se a conversa for nula, o atendimento ativo também será.
        var atendimentoAtivo = isNewConversation
            ? null
            : await _atendimentoRepository.FindActiveByConversaIdAsync(conversa.Id, cancellationToken);

        // Se já existe um atendimento ativo, a mensagem deve ser tratada pelo fluxo do bot.
        // Este handler só lida com o início de NOVAS interações.
        if (atendimentoAtivo is not null)
        {
            // Apenas adicionamos a mensagem ao histórico do atendimento existente.
            var mensagemParaAtendimentoExistente = new Mensagem(conversa.Id, atendimentoAtivo.Id, command.TextoDaPrimeiraMensagem, Remetente.Cliente(), null);
            conversa.AdicionarMensagem(mensagemParaAtendimentoExistente, atendimentoAtivo.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return conversa.Id;
        }

        // 3. Se NÃO há atendimento ativo, um NOVO atendimento se inicia.
        // Criamos os objetos em memória primeiro.
        var novoAtendimento = Atendimento.Iniciar(conversa?.Id ?? Guid.Empty); // Passa o Id da conversa se ela já existe

        // Criamos a primeira mensagem, agora com o ID do novo atendimento.
        var primeiraMensagem = new Mensagem(conversa?.Id ?? Guid.Empty, novoAtendimento.Id, command.TextoDaPrimeiraMensagem, Remetente.Cliente(), null);

        if (isNewConversation)
        {
            // Se a conversa é nova, usamos o método Iniciar, que já adiciona a mensagem.
            conversa = Conversa.Iniciar(command.ContatoId, novoAtendimento.Id, primeiraMensagem);
            await _conversationRepository.AddAsync(conversa, cancellationToken);
        }
        else
        {
            // Se a conversa já existia, apenas adicionamos a nova mensagem.
            conversa.AdicionarMensagem(primeiraMensagem, novoAtendimento.Id);
        }

        // Adiciona o novo atendimento ao repositório.
        await _atendimentoRepository.AddAsync(novoAtendimento, cancellationToken);

        // 4. Inicia o fluxo do bot para ESTE novo atendimento.
        var contato = await _contactRepository.GetByIdAsync(conversa.ContatoId, cancellationToken);
        if (contato is null)
            throw new InvalidOperationException($"Contato com ID {conversa.ContatoId} não encontrado.");

        var sessionState = new BotSessionState(novoAtendimento.Id, novoAtendimento.BotStatus);
        await _botSessionCache.SetStateAsync(contato.Telefone, sessionState, TimeSpan.FromHours(2));

        var menuText = "Olá! Bem-vindo ao nosso atendimento. Digite o número da opção desejada:\n1- Segunda via de boleto\n2- Falar com o Comercial\n3- Falar com o Financeiro\n4- Encerrar atendimento";
        await _metaSender.EnviarMensagemTextoAsync(contato.Telefone, menuText);

        // 5. Persiste tudo no banco.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return conversa.Id;
    }

}