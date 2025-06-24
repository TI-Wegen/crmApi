namespace CRM.Infrastructure.Database;

using Agents.Domain.Aggregates;
using Contacts.Domain.Aggregates;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de entidade definidas neste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}