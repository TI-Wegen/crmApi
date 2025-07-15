namespace CRM.Infrastructure.Database.Configurations;

using Conversations.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AtendimentoConfiguration : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> builder)
    {
        builder.ToTable("Atendimentos");
        builder.HasKey(a => a.Id);

        // Relacionamento com a Conversa
        builder.HasOne<Conversa>()
            .WithMany() // Uma conversa pode ter muitos atendimentos
            .HasForeignKey(a => a.ConversaId)
            .IsRequired();

        builder.Property(a => a.Status).HasConversion<string>().IsRequired();
        builder.Property(a => a.BotStatus).HasConversion<string>().IsRequired();
        builder.Property(a => a.DataFinalizacao).HasColumnName("DataFinalizacao");

        // Mapeamento do Value Object Avaliacao
        builder.OwnsOne(a => a.Avaliacao, avaliacaoBuilder =>
        {
            avaliacaoBuilder.Property(av => av.Nota).HasColumnName("AvaliacaoNota");
            avaliacaoBuilder.Property(av => av.Comentario).HasColumnName("AvaliacaoComentario").HasMaxLength(500);
        });

        builder.Property(a => a.Version).IsConcurrencyToken();
        builder.Ignore(a => a.DomainEvents);
    }
}