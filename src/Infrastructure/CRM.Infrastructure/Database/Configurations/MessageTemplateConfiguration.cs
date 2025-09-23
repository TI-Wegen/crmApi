using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Templates.Domain.Entities;
using Templates.Domain.Enuns;

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


        builder.HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0009-000000000001"),
                Name = "template",
                Language = "pt_BR",
                Body = "Olá {{1}}, Tudo bem?",
                Description = "Template de boas-vindas para novos usuários.",
                Status = TemplateStatus.Aprovado,
                MotivoRejeicao = (string?)null,
                Version = Guid.Parse("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a"),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 01, 01, 0, 0, 0), DateTimeKind.Utc)
            }
        );
    }
}