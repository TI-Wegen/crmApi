namespace CRM.Infrastructure.Database;

using Agents.Domain.Aggregates;
using Contacts.Domain.Aggregates;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using CRM.Domain.DomainEvents;
// Em Infrastructure/Database/
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets para os Agregados que precisam ser persistidos
    public DbSet<Conversa> Conversas { get; set; }
    public DbSet<Mensagem> Mensagens { get; set; }
    public DbSet<Agente> Agentes { get; set; }
    public DbSet<Setor> Setores { get; set; }
    public DbSet<Contato> Contatos { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Esta linha garante que, para qualquer UPDATE, o token de concorrência mude.
            entry.Property("Version").CurrentValue = Guid.NewGuid();
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidade definidas neste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}