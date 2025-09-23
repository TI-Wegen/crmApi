using Contacts.Application.Repositories;
using Contacts.Domain.Entities;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Infrastructure.Adapters;

public class ContactAdapter : IContactRepository
{
    private readonly AppDbContext _context;

    public ContactAdapter(AppDbContext context)
    {
        _context = context;
    }

    public async Task <Contato> AddAsync(Contato contato, CancellationToken cancellationToken = default)
    {
        await _context.Contatos.AddAsync(contato, cancellationToken);

        return contato;
    }

    public Task<Contato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Contatos.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Contato?> GetByTelefoneAsync(string telefone, CancellationToken cancellationToken = default)
    {
        return _context.Contatos.FirstOrDefaultAsync(c => c.Telefone == telefone, cancellationToken);
    }

    public Task<Contato?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Contatos
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    public async Task<IEnumerable<Contato>> GetAllAsync(int pageNumber, int pageSize, bool incluirInativos, CancellationToken cancellationToken = default)
    {
        var query = _context.Contatos.AsQueryable();

        if (!incluirInativos)
        {
            query = query.Where(c => c.Status != Contacts.Domain.Enums.ContatoStatus.Inativo);
        }

        return await query
            .OrderBy(c => c.Nome)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Contato contato, CancellationToken cancellationToken = default)
    {
        _context.Contatos.Update(contato);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Contato?> GetByWaIdAsync(string waId, CancellationToken cancellationToken = default)
    {
        return await _context.Contatos.FirstOrDefaultAsync(c => c.WaId == waId, cancellationToken);

    }
}