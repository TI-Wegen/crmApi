using Agents.Domain.Aggregates;
using Agents.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Database.Configurations;

public class TagsConfiguration : IEntityTypeConfiguration<Tags.Domain.Entities.Tags>
{
    public void Configure(EntityTypeBuilder<Tags.Domain.Entities.Tags> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Nome).IsRequired().HasMaxLength(150);
        builder.HasIndex(a => a.Nome).IsUnique();
        
        builder.HasOne<Agente>()
            .WithMany()
            .HasForeignKey(a => a.AgenteId)
            .IsRequired();

        builder.Property(a => a.Descricao).HasMaxLength(250);
        
        builder.Property(a => a.Version).IsConcurrencyToken();
        builder.Ignore(a => a.DomainEvents);
    }
}