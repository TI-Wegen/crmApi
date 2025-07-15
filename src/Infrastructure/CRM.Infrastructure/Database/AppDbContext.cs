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
    // A nova dependência para nosso despachante de eventos.
    private readonly IDomainEventDispatcher _dispatcher;

    // O construtor agora recebe tanto as opções quanto o dispatcher.
    public AppDbContext(DbContextOptions<AppDbContext> options, IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    // Seus DbSets...
    public DbSet<Conversa> Conversas { get; set; }
    public DbSet<Mensagem> Mensagens { get; set; }
    public DbSet<Agente> Agentes { get; set; }
    public DbSet<Setor> Setores { get; set; }
    public DbSet<Contato> Contatos { get; set; }
    public DbSet<MessageTemplate> MessageTemplates { get; set; }
    public DbSet<Atendimento> Atendimentos { get; set; }

    // O método SaveChangesAsync agora orquestra as duas lógicas.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Encontra todas as entidades com eventos de domínio PENDENTES.
        // Guardamos em uma variável antes de salvar, pois o SaveChanges pode limpar o estado.
        var entitiesWithEvents = ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToArray();

        // 2. Lógica para atualizar o token de concorrência (Version).
        // Isso precisa acontecer ANTES de salvar no banco.
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Property("Version").CurrentValue = Guid.NewGuid();
        }

        // 3. Salva todas as alterações no banco de dados primeiro.
        // Isso inclui as novas versões e quaisquer outras mudanças de estado.
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. Se e somente se o salvamento foi bem-sucedido, despacha os eventos.
        // Isso garante que os "ouvintes" só sejam notificados se a transação for commitada.
        if (_dispatcher is not null)
        {
            await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidade do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}