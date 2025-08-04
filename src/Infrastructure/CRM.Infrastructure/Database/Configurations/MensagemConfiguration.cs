namespace CRM.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Conversations.Domain.Entities;

public class MensagemConfiguration : IEntityTypeConfiguration<Mensagem>
{
    public void Configure(EntityTypeBuilder<Mensagem> builder)
    {
        builder.ToTable("Mensagens");
        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.Remetente, remetenteBuilder =>
        {
            remetenteBuilder.Property(r => r.Tipo).HasConversion<string>().HasColumnName("RemetenteTipo");
            remetenteBuilder.Property(r => r.AgenteId).HasColumnName("RemetenteAgenteId");
        });
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.MessageId)
       .HasColumnName("MessageId")
       .IsRequired(false);
    }
}