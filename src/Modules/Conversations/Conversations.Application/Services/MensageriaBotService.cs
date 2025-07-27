namespace Conversations.Application.Services;

using Conversations.Application.Mappers;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using global::Conversations.Application.Abstractions;
using global::Conversations.Domain.Entities;
using global::Conversations.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Domain.Repositories;

public class MensageriaBotService : IMensageriaBotService
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaMessageSender _metaSender;

    private readonly ITemplateRepository _templateRepository; // NOVO

    private readonly IRealtimeNotifier _notifier;

    public MensageriaBotService(
        IAtendimentoRepository atendimentoRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IMetaMessageSender metaSender,
        ITemplateRepository templateRepository,
        IRealtimeNotifier notifier)
    {
        _atendimentoRepository = atendimentoRepository;
        _conversationRepository = conversationRepository;
        _templateRepository = templateRepository; // NOVO
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

    public async Task<string> EnviarETemplateAsync(Guid atendimentoId, string telefoneDestino, string templateName, List<string> bodyParameters)
    {
        // 1. Envia o template através da API da Meta primeiro.
        var wamid = await _metaSender.EnviarTemplateAsync(telefoneDestino, templateName, bodyParameters);
        if (string.IsNullOrEmpty(wamid))
        {
            throw new Exception("Falha ao enviar template: não foi possível obter o ID da mensagem da Meta.");
        }

        // 2. Busca o atendimento e a conversa para registrar a mensagem.
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return wamid; // Retorna o ID mesmo se não conseguir salvar

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return wamid;

        // 3. Cria a entidade Mensagem para o template enviado.
        var remetente = Remetente.Agente(atendimento.AgenteId ?? SystemGuids.SystemAgentId);
        var template = await _templateRepository.GetByNameAsync(templateName);
        var textoParaHistorico = $"Template '{templateName}' enviado.";

        if (template is not null)
        {
            // 3. Substitui os placeholders (ex: {{1}}, {{2}}) pelos parâmetros.
            textoParaHistorico = ConstruirTextoDoTemplate(template.Body, bodyParameters);
        }
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, textoParaHistorico, remetente, DateTime.UtcNow, null);

        // 4. Adiciona ao histórico e salva no banco.
        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);
        await _unitOfWork.SaveChangesAsync();

        // 5. Notifica o frontend em tempo real.
        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());

        return wamid;
    }

    private string ConstruirTextoDoTemplate(string templateBody, List<string> parameters)
    {
        var result = templateBody;
        for (int i = 0; i < parameters.Count; i++)
        {
            // Substitui {{1}} pelo primeiro parâmetro, {{2}} pelo segundo, e assim por diante.
            result = result.Replace($"{{{{{i + 1}}}}}", parameters[i]);
        }
        return result;
    }
}