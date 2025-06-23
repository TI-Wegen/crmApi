namespace CRM.Infrastructure.Database.Configurations;

// Em Infrastructure/Database/Configurations/
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Conversations.Domain.Aggregates;

public class ConversaConfiguration : IEntityTypeConfiguration<Conversa>
{
    public void Configure(EntityTypeBuilder<Conversa> builder)
    {
        builder.ToTable("Conversas"); // Nome da tabela no banco
        builder.HasKey(c => c.Id); // Chave primária

        // Mapeia o Enum 'Status' para ser salvo como uma string no banco
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);

        // Configura o relacionamento "um-para-muitos" com Mensagem
        var navigation = builder.Navigation(c => c.Mensagens);
        navigation.UsePropertyAccessMode(PropertyAccessMode.Field); // Diz ao EF para usar o campo privado _mensagens
        navigation.HasField("_mensagens");

        // Ignora a propriedade de eventos de domínio, para não ser persistida
        builder.Ignore(c => c.DomainEvents);
    }
}