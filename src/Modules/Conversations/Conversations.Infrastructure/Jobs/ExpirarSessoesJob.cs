using Conversations.Application.Abstractions;
using CRM.Application.Interfaces;

namespace Conversations.Infrastructure.Jobs;

public class ExpirarSessoesJob
{
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExpirarSessoesJob(IAtendimentoRepository atendimentoRepository, IUnitOfWork unitOfWork)
    {
        _atendimentoRepository = atendimentoRepository;
        _unitOfWork = unitOfWork;
    }



    // O Hangfire irá chamar este método publico.
    public async Task Executar()
    {
        //Console.WriteLine("Executando Job de expiração de sessões...");

        //// A regra de negócio é expirar após 24 horas.
        //var dataLimite = DateTime.UtcNow.AddMinutes(3);

        //var atendimentosParaExpirar = await _atendimentoRepository.GetAtendimentosAtivosCriadosAntesDeAsync(dataLimite);

        //foreach (var atendimento in atendimentosParaExpirar)
        //{
        //    atendimento.MarcarComoExpirada();

        //}
        //await _unitOfWork.SaveChangesAsync();
        //Console.WriteLine("Job de expiração de sessões finalizado.");
    }
}