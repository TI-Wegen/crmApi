namespace CRM.Infrastructure.Database.Configurations;


using Agents.Domain.Aggregates;
using Agents.Domain.Enuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SetorConfiguration : IEntityTypeConfiguration<Setor>
{
    public void Configure(EntityTypeBuilder<Setor> builder)
    {
        builder.ToTable("Setores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nome).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Nome).IsUnique(); // Garante que não teremos setores com o mesmo nome
        builder.Property(s => s.Descricao).HasMaxLength(255);
        builder.Ignore(s => s.DomainEvents);

        builder.HasData(
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000001"), // ID Fixo para o Financeiro
                Nome = SetorNome.Financeiro.ToDbValue(),
                Descricao = "Setor responsável por questões financeiras e boletos.",
                Version = Guid.Parse("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a"),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 01, 01, 0, 0, 0), DateTimeKind.Utc)
            },
            new
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000002"), // ID Fixo para o Comercial
                Nome = SetorNome.Comercial.ToDbValue(),
                Descricao = "Setor responsável por vendas e novas oportunidades.",
                Version = Guid.Parse("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"),
                CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 01, 01, 0, 0, 0), DateTimeKind.Utc)

            },
             new
             {
                 Id = Guid.Parse("00000000-0000-0000-0001-000000000003"), // ID Fixo para o Admin
                 Nome = SetorNome.Admin.ToDbValue(),
                 Descricao = "Setor responsável pela administração geral",
                 Version = Guid.Parse("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7a"),
                 CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 01, 01, 0, 0, 0), DateTimeKind.Utc)

             }
        );
    }
}