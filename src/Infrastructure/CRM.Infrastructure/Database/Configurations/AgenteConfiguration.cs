using Agents.Domain.Aggregates;
using Agents.Domain.Enuns;
using CRM.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Infrastructure.Database.Configurations;

public class AgenteConfiguration : IEntityTypeConfiguration<Agente>
{
    public void Configure(EntityTypeBuilder<Agente> builder)
    {
        builder.ToTable("Agentes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(a => a.Email).IsUnique();
        builder.Property(a => a.PasswordHash).IsRequired();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(a => a.CargaDeTrabalho, cargaBuilder =>
        {
            cargaBuilder.Property(c => c.Valor).HasColumnName("CargaDeTrabalho");

            cargaBuilder.HasData(new
            {
                AgenteId = SystemGuids.SystemAgentId,
                Valor = 0
            });
        });
        var converter = new ValueConverter<List<Guid>, string>(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());

        var comparer = new ValueComparer<List<Guid>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property<List<Guid>>("_setorIds")
               .HasColumnName("SetorIds")
               .HasConversion(converter)
               .Metadata.SetValueComparer(comparer);



        builder.Property(a => a.Version).IsConcurrencyToken();

        builder.Ignore(a => a.DomainEvents);

        builder.HasData(
           new
           {
               Id = SystemGuids.SystemAgentId,
               Nome = "Sistema",
               Email = "sistema@crm.local", 
               Status = AgenteStatus.Offline,
               PasswordHash = "$2a$11$fH.d2sB7aY.s.1b2a3c4d5e6f7g8h9i0j",
               Version = Guid.Parse("00000000-0000-0000-0000-000000000001"),
               _setorIds = new List<Guid>(),
               CreatedAt = DateTime.SpecifyKind(new DateTime(2024, 01, 01, 0, 0, 0), DateTimeKind.Utc)

           }
       );
    }
}