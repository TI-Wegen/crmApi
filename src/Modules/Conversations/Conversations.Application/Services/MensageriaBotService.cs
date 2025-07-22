namespace Conversations.Application.Services;

using Conversations.Application.Mappers;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using global::Conversations.Application.Abstractions;
using global::Conversations.Domain.Entities;
using global::Conversations.Domain.ValueObjects;
using System;
using System.Threading.Tasks;


public class MensageriaBotService : IMensageriaBotService
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaMessageSender _metaSender;
    private readonly IRealtimeNotifier _notifier;

    public MensageriaBotService(
        IAtendimentoRepository atendimentoRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IMetaMessageSender metaSender,
        IRealtimeNotifier notifier)
    {
        _atendimentoRepository = atendimentoRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _metaSender = metaSender;
        _notifier = notifier;
    }

    public async Task EnviarEMensagemTextoAsync(Guid atendimentoId, string telefoneDestino, string texto)
    {
        // 1. Busca o atendimento e a conversa associada.
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return;

        // 2. Cria a entidade Mensagem para a resposta do bot.
        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, texto, remetente, DateTime.UtcNow, null);

        // 3. Adiciona a mensagem ao histórico da conversa.
        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        // 4. Salva a nova mensagem no banco de dados.
        await _unitOfWork.SaveChangesAsync();

        // 5. Envia a mensagem para o cliente através da Meta.
        await _metaSender.EnviarMensagemTextoAsync(telefoneDestino, texto);

        // 6. Notifica o frontend em tempo real.
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
    }

    public async Task EnviarEDocumentoAsync(Guid atendimentoId, string telefoneDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return;

        // 1. Cria a entidade Mensagem para o documento do bot
        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);
        // O "texto" da mensagem pode ser a legenda ou o nome do arquivo.
        var textoParaHistorico = legenda ?? nomeDoArquivo;
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, textoParaHistorico, remetente, DateTime.UtcNow, urlDoDocumento);

        // 2. Adiciona ao histórico da conversa
        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        // 3. Salva a nova mensagem no banco
        await _unitOfWork.SaveChangesAsync();

        // 4. Envia o documento para o cliente através da Meta
        await _metaSender.EnviarDocumentoAsync(telefoneDestino, urlDoDocumento, nomeDoArquivo, legenda);

        // 5. Notifica o frontend em tempo real
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
    }
}