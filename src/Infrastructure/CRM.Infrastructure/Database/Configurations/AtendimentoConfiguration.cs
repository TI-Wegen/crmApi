using Conversations.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Database.Configurations;

public class AtendimentoConfiguration : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> builder)
    {
        builder.ToTable("Atendimentos");
        builder.HasKey(a => a.Id);

        builder.HasOne<Conversa>()
            .WithMany()
            .HasForeignKey(a => a.ConversaId)
            .IsRequired();
        
        builder.HasOne(a => a.Tag)
            .WithMany()
            .HasForeignKey(a => a.TagsId)
            .IsRequired(false); 

        builder.Property(a => a.Status).HasConversion<string>().IsRequired();
        builder.Property(a => a.BotStatus).HasConversion<string>().IsRequired();
        builder.Property(a => a.DataFinalizacao).HasColumnName("DataFinalizacao");

        builder.OwnsOne(a => a.Avaliacao, avaliacaoBuilder =>
        {
            avaliacaoBuilder.Property(av => av.Nota).HasColumnName("AvaliacaoNota");
            avaliacaoBuilder.Property(av => av.Comentario).HasColumnName("AvaliacaoComentario").HasMaxLength(500);
        });

        builder.Property(a => a.Version).IsConcurrencyToken();
        builder.Ignore(a => a.DomainEvents);
    }
}