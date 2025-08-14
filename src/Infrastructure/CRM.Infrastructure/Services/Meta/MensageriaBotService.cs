namespace CRM.Infrastructure.Services.Meta;

using Conversations.Application.Abstractions;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using CRM.Application.ValueObject;
using CRM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Templates.Domain.Repositories;
using CRM.Application.Mappers;
using Conversations.Domain.Aggregates;

public class MensageriaBotService : IMensageriaBotService
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetaMessageSender _metaSender;

    private readonly ITemplateRepository _templateRepository; 

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
        _templateRepository = templateRepository; 
        _unitOfWork = unitOfWork;
        _metaSender = metaSender;
        _notifier = notifier;
    }

    public async Task EnviarEMensagemTextoAsync(Guid atendimentoId, string telefoneDestino, string texto)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return;

        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, texto, remetente, DateTime.UtcNow, null);

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        await _unitOfWork.SaveChangesAsync();

        await _metaSender.EnviarMensagemTextoAsync(telefoneDestino, texto);

        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
    }

    public async Task EnviarEDocumentoAsync(Guid atendimentoId, string telefoneDestino, string urlDoDocumento, string nomeDoArquivo, string? legenda)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return;

        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);
        var textoParaHistorico = legenda ?? nomeDoArquivo;
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, textoParaHistorico, remetente, DateTime.UtcNow, urlDoDocumento);

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        await _unitOfWork.SaveChangesAsync();

        await _metaSender.EnviarDocumentoAsync(telefoneDestino, urlDoDocumento, nomeDoArquivo, legenda);

        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
    }

    public async Task<string?> EnviarETemplateAsync(Guid atendimentoId, SendTemplateInput sendTemplateInput)

    {
        var wamid = await _metaSender.EnviarTemplateAsync(sendTemplateInput);
        if (string.IsNullOrEmpty(wamid))
        {
            throw new Exception("Falha ao enviar template: não foi possível obter o ID da mensagem da Meta.");
        }

        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId) ??
            throw new Exception("Atendimento não encontrado.");

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId) ??
            throw new Exception("Atendimento não encontrado.");

        var remetente = Remetente.Agente(atendimento.AgenteId ?? SystemGuids.SystemAgentId);
        var template = await _templateRepository.GetByNameAsync(sendTemplateInput.TemplateName);
        var textoParaHistorico = $"Template '{sendTemplateInput.TemplateName}enviado.";

        if (template is not null)
        {
            textoParaHistorico = ConstruirTextoDoTemplate(template.Body, sendTemplateInput.Parameters);
        }

        var novaMensagem = new Mensagem(
            conversa.Id, 
            atendimento.Id, textoParaHistorico, 
            remetente, DateTime.UtcNow, 
            sendTemplateInput.DocumentUrl);

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);
        await _unitOfWork.SaveChangesAsync();

        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());

        return wamid;
    }

    private string ConstruirTextoDoTemplate(string templateBody, List<string> parameters)
    {
        var result = templateBody;
        for (int i = 0; i < parameters.Count; i++)
        {
            result = result.Replace($"{{{{{i + 1}}}}}", parameters[i]);
        }
        return result;
    }
}