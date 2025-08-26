using Agents.Domain.Aggregates;
using Contacts.Domain.Aggregates;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using CRM.Domain.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Templates.Domain.Aggregates;

namespace CRM.Infrastructure.Database;

public class AppDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<Conversa> Conversas { get; set; }
    public DbSet<Mensagem> Mensagens { get; set; }
    public DbSet<Agente> Agentes { get; set; }
    public DbSet<Setor> Setores { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<MessageTemplate> MessageTemplates { get; set; }
    public DbSet<Atendimento> Atendimentos { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Property("Version").CurrentValue = Guid.NewGuid();
        }


        var result = await base.SaveChangesAsync(cancellationToken);

        if (_dispatcher is not null)
        {
            await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}