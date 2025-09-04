using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;
using CRM.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Conversations.Infrastructure.Jobs;

public class CleanExpiredBotSessionsJob
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IBotSessionCache _botSessionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanExpiredBotSessionsJob> _logger;

    public CleanExpiredBotSessionsJob(
        IAtendimentoRepository atendimentoRepository,
        IConversationRepository conversationRepository,
        IContactRepository contactRepository,
        IBotSessionCache botSessionCache,
        IUnitOfWork unitOfWork,
        ILogger<CleanExpiredBotSessionsJob> logger)
    {
        _atendimentoRepository = atendimentoRepository;
        _conversationRepository = conversationRepository;
        _contactRepository = contactRepository;
        _botSessionCache = botSessionCache;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Executar()
    {
        _logger.LogInformation("Iniciando Job de limpeza de sessões de bot expiradas...");

        int sessoesExpiradas = 0;
        

        if (sessoesExpiradas > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Job de limpeza de sessões finalizado. {Count} atendimentos foram resolvidos.",
            sessoesExpiradas);
    }
}