namespace CRM.Infrastructure.Database.Configurations;

using Agents.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AgenteConfiguration : IEntityTypeConfiguration<Agente>
{
    public void Configure(EntityTypeBuilder<Agente> builder)
    {
        builder.ToTable("Agentes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(a => a.Email).IsUnique(); // Garante unicidade do e-mail no banco
        builder.Property(a => a.PasswordHash).IsRequired();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(a => a.CargaDeTrabalho, cargaBuilder =>
        {
            cargaBuilder.Property(c => c.Valor).HasColumnName("CargaDeTrabalho");
        });

        // EF Core não sabe mapear List<Guid> diretamente, então usamos uma conversão simples.
        // Isso salvará os Ids dos setores como uma string de texto separada por vírgulas.
        // Para relações muitos-para-muitos mais complexas, usaríamos uma tabela de junção.
        builder.Property(a => a.SetorIds).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());

        builder.Ignore(a => a.DomainEvents);
    }
}