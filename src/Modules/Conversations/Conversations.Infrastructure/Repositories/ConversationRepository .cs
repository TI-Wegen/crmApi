namespace Conversations.Infrastructure.Repositories;

using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Enuns;
using CRM.Infrastructure.Database;
// Em Infrastructure/Repositories/
using Microsoft.EntityFrameworkCore;
using System;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    // O DbContext é injetado via construtor. O repositório não o cria.
    public ConversationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Adiciona uma nova conversa ao DbContext. A operação de INSERT só ocorrerá
    /// no banco quando o SaveChangesAsync do UnitOfWork for chamado.
    /// </summary>
    public async Task AddAsync(Conversa conversa, CancellationToken cancellationToken = default)
    {
        await _context.Conversas.AddAsync(conversa, cancellationToken);
    }

    /// <summary>
    /// Busca uma conversa pelo seu ID, sem carregar as mensagens.
    /// Útil para operações de escrita onde apenas o agregado raiz é necessário.
    /// </summary>
    public Task<Conversa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Conversas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Busca uma conversa pelo seu ID, carregando também a lista de mensagens associadas.
    /// Essencial para as queries que precisam exibir o histórico da conversa.
    /// </summary>
    public Task<Conversa?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // O método .Include() instrui o EF Core a carregar a propriedade de navegação 'Mensagens'.
        // Isso é o que chamamos de "Eager Loading" (carregamento ansioso).
        return _context.Conversas
            .Include(c => c.Mensagens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Marca uma entidade 'Conversa' como modificada no Change Tracker do EF Core.
    /// Para fluxos onde a entidade é buscada e modificada no mesmo contexto,
    /// esta chamada pode ser redundante, mas é uma boa prática para ser explícito
    /// e para cenários "desconectados".
    /// </summary>
    public Task UpdateAsync(Conversa conversa, CancellationToken cancellationToken = default)
    {
        _context.Conversas.Update(conversa);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Conversa>> GetConversasAtivasCriadasAntesDeAsync(DateTime limite, CancellationToken cancellationToken = default)
    {
        return await _context.Conversas
         .Where(c => c.Status == ConversationStatus.AguardandoNaFila || c.Status == ConversationStatus.EmAtendimento)
         .ToListAsync(cancellationToken);
    }

    public Task<Conversa?> FindActiveByContactIdAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        // A MUDANÇA ESTÁ AQUI: Adicionamos .Include(c => c.Mensagens)
        // Isso garante que a coleção de mensagens seja carregada e rastreada pelo EF Core.
        return _context.Conversas
            .Include(c => c.Mensagens)
            .FirstOrDefaultAsync(c =>
                c.ContatoId == contactId &&
                (c.Status == ConversationStatus.AguardandoNaFila || c.Status == ConversationStatus.EmAtendimento),
                cancellationToken);
    }
}