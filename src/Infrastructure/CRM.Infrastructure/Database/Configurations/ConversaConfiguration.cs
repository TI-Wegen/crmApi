namespace CRM.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Conversations.Domain.Aggregates;

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

        builder.Ignore(c => c.DomainEvents);

        builder.OwnsOne(c => c.SessaoAtiva, sessaoBuilder =>
        {
            sessaoBuilder.Property(s => s.DataInicio).HasColumnName("SessaoInicio");
            sessaoBuilder.Property(s => s.DataFim).HasColumnName("SessaoFim");
        });
    }
}