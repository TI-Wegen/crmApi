namespace CRM.Infrastructure.Database.Configurations;

using Contacts.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContatoConfiguration : IEntityTypeConfiguration<Contato>
{
    public void Configure(EntityTypeBuilder<Contato> builder)
    {
        builder.ToTable("Contatos");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.Telefone).IsUnique();

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(50);

        // Configura a coleção de Value Objects 'Tag'
        // O EF Core criará uma tabela 'ContatoTags' para armazenar esta coleção.
        builder.OwnsMany(c => c.Tags, tagsBuilder =>
        {
            tagsBuilder.ToTable("ContatoTags");
            tagsBuilder.WithOwner().HasForeignKey("ContatoId");
            tagsBuilder.HasKey("Id"); // Chave primária para a tabela de tags
            tagsBuilder.Property(t => t.Texto).IsRequired().HasMaxLength(50);
        });

        // Configura a coleção de Entidades Internas 'HistoricoStatus'
        var historicoNavigation = builder.Navigation(c => c.HistoricoStatus);
        historicoNavigation.UsePropertyAccessMode(PropertyAccessMode.Field);
        historicoNavigation.HasField("_historicoStatus");

        builder.HasMany(c => c.HistoricoStatus)
            .WithOne()
            .HasForeignKey("ContatoId")
            .OnDelete(DeleteBehavior.Cascade); // Se o contato for deletado, seu histórico também será.

        builder.Ignore(c => c.DomainEvents);
    }
}