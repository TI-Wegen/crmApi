using Contacts.Domain.Entities;

namespace Contacts.Application.Repositories
{
    public interface IContactRepository
    {
        Task <Contato> AddAsync(Contato contato, CancellationToken cancellationToken = default);
        Task<Contato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Contato?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken = default);
        Task<Contato?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Contato>> 
            GetAllAsync(int pageNumber, int pageSize, bool incluirInativos, CancellationToken cancellationToken = default);
        Task UpdateAsync(Contato contato, CancellationToken cancellationToken = default);
        Task<Contato?> GetByWaIdAsync(string waId, CancellationToken cancellationToken = default);


    }

}
