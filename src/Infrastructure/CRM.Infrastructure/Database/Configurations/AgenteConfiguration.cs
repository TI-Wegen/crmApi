using Agents.Domain.Aggregates;
using Agents.Domain.Entities;
using Agents.Domain.Enuns;
using CRM.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CRM.Infrastructure.Database.Configurations;

public class AgenteConfiguration : IEntityTypeConfiguration<Agente>
{
    public void Configure(EntityTypeBuilder<Agente> builder)
    {
        builder.ToTable("Agentes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(a => a.Email).IsUnique();
        builder.Property(a => a.PasswordHash).IsRequired();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<Setor>()
            .WithMany()
            .HasForeignKey(a => a.SetorId)
            .IsRequired();
        
        builder.Property(a => a.Version).IsConcurrencyToken();

        builder.Ignore(a => a.DomainEvents);
    }
}