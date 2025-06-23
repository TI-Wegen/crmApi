using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;

namespace Conversations.Application.Jobs;

public class ExpirarSessoesJob
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExpirarSessoesJob(IConversationRepository conversationRepository, IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
    }

    // O Hangfire irá chamar este método publico.
    public async Task Executar()
    {
        Console.WriteLine("Executando Job de expiração de sessões...");

        // A regra de negócio é expirar após 24 horas.
        var dataLimite = DateTime.UtcNow.AddHours(-24);

        var conversasParaExpirar = await _conversationRepository.GetConversasAtivasCriadasAntesDeAsync(dataLimite);

        foreach (var conversa in conversasParaExpirar)
        {
            // Precisamos filtrar aqui também pois o tempo passou entre a query e agora
            if (conversa.DataCriacao < dataLimite)
            {
                conversa.MarcarComoExpirada();
                await _conversationRepository.UpdateAsync(conversa);
            }
        }

        // Salva todas as alterações de uma vez só.
        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine("Job de expiração de sessões finalizado.");
    }
}