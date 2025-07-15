using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Templates.Domain.Aggregates;

namespace CRM.Infrastructure.Database.Configurations;

public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.ToTable("MessageTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Name).IsUnique(); 
        builder.Property(t => t.Language).IsRequired().HasMaxLength(10);
        builder.Property(t => t.Body).IsRequired().HasMaxLength(1024);
        builder.Property(t => t.Version).IsConcurrencyToken();
        builder.Ignore(t => t.DomainEvents);
    }
}