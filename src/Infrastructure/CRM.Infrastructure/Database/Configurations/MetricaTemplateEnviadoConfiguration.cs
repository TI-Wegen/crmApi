using Metrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Database.Configurations;

public class MetricaTemplateEnviadoConfiguration : IEntityTypeConfiguration<MetricaTemplateEnviado>
{
    public void Configure(EntityTypeBuilder<MetricaTemplateEnviado> builder)
    {
        builder.ToTable("MetricasTemplatesEnviados");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(m => m.AtendimentoId)
            .IsRequired();
        builder.Property(m => m.AgenteId)
            .IsRequired();
        builder.Property(m => m.TemplateName)
            .IsRequired()
            .HasMaxLength(100); 
        builder.Property(m => m.SentAt)
            .IsRequired();
    }
}

