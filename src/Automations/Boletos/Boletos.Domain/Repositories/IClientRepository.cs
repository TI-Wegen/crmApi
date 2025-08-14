using Boletos.Domain.Entities;

namespace Boletos.Domain.Repositories;

public interface IClientRepository
{
    Task<List<Client>> GetAllInvoicesGenerate();
    Task<List<Client>> GetAllInvoices3Days();
    Task<List<Client>> GetAllInvoicesToDueDate();
    Task<string> GetBoletoBase64ById(int id);
    Task Update3DaysAsync(Client client);
    Task UpdateGenerateAsync(Client client);
    Task UpdateDueDateAsync(Client client);
    Task CreateWpp(Client client);
    Task UpdateAlert(Client client);
}
