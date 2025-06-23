namespace CRM.Infrastructure.Database.Configurations;


using Agents.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SetorConfiguration : IEntityTypeConfiguration<Setor>
{
    public void Configure(EntityTypeBuilder<Setor> builder)
    {
        builder.ToTable("Setores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nome).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Descricao).HasMaxLength(255);
        builder.Ignore(s => s.DomainEvents);
    }
}