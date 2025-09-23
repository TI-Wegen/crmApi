using Conversations.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Database.Configurations;

public class ConversaConfiguration : IEntityTypeConfiguration<Conversa>
{
    public void Configure(EntityTypeBuilder<Conversa> builder)
    {
        builder.ToTable("Conversas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Version).IsConcurrencyToken();

        var navigation = builder.Navigation(c => c.Mensagens);
        navigation.UsePropertyAccessMode(PropertyAccessMode.Field);
        navigation.HasField("_mensagens");
        
        builder.HasOne(a => a.Tag)
            .WithMany()
            .HasForeignKey(a => a.TagsId)
            .IsRequired(false); 

        builder.Ignore(c => c.DomainEvents);

        builder.OwnsOne(c => c.SessaoAtiva, sessaoBuilder =>
        {
            sessaoBuilder.Property(s => s.DataInicio).HasColumnName("SessaoInicio");
            sessaoBuilder.Property(s => s.DataFim).HasColumnName("SessaoFim");
        });
    }
}