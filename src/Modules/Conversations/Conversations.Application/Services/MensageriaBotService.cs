using Conversations.Application.Mappers;
using Conversations.Application.Repository;
using Conversations.Domain.Entities;
using Conversations.Domain.ValueObjects;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using Templates.Domain.Repositories;

namespace Conversations.Application.Services;

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

    public async Task EnviarEDocumentoAsync(Guid atendimentoId, string telefoneDestino, string urlDoDocumento,
        string nomeDoArquivo, string? legenda)
    {
        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return;

        var remetente = Remetente.Agente(SystemGuids.SystemAgentId);
        var textoParaHistorico = legenda ?? nomeDoArquivo;
        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, textoParaHistorico, remetente, DateTime.UtcNow,
            urlDoDocumento);

        conversa.AdicionarMensagem(novaMensagem, atendimento.Id);

        await _unitOfWork.SaveChangesAsync();

        await _metaSender.EnviarDocumentoAsync(telefoneDestino, urlDoDocumento, nomeDoArquivo, legenda);

        await _notifier.NotificarNovaMensagemAsync(conversa.Id.ToString(), novaMensagem.ToDto());
    }

    public async Task<string> EnviarETemplateAsync(Guid atendimentoId, string telefoneDestino, string templateName,
        List<string> bodyParameters)
    {
        var wamid = await _metaSender.EnviarTemplateAsync(telefoneDestino, templateName, bodyParameters);
        if (string.IsNullOrEmpty(wamid))
        {
            throw new Exception("Falha ao enviar template: não foi possível obter o ID da mensagem da Meta.");
        }

        var atendimento = await _atendimentoRepository.GetByIdAsync(atendimentoId);
        if (atendimento is null) return wamid;

        var conversa = await _conversationRepository.GetByIdAsync(atendimento.ConversaId);
        if (conversa is null) return wamid;

        var remetente = Remetente.Agente(atendimento.AgenteId ?? SystemGuids.SystemAgentId);
        var template = await _templateRepository.GetByNameAsync(templateName);
        var textoParaHistorico = $"Template '{templateName}' enviado.";

        if (template is not null)
        {
            textoParaHistorico = ConstruirTextoDoTemplate(template.Body, bodyParameters);
        }

        var novaMensagem = new Mensagem(conversa.Id, atendimento.Id, textoParaHistorico, remetente, DateTime.UtcNow,
            null);

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