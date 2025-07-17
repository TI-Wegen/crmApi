using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class seederTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"));

            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"));

            migrationBuilder.InsertData(
                table: "MessageTemplates",
                columns: new[] { "Id", "Body", "CreatedAt", "Description", "Language", "MotivoRejeicao", "Name", "Status", "Version" },
                values: new object[] { new Guid("00000000-0000-0000-0009-000000000001"), "Olá {{1}}, Tudo bem?", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Template de boas-vindas para novos usuários.", "pt_BR", null, "template", 1, new Guid("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a") });

            migrationBuilder.InsertData(
                table: "Setores",
                columns: new[] { "Id", "CreatedAt", "Descricao", "Nome", "Version" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Setor responsável por questões financeiras e boletos.", "Financeiro", new Guid("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a") },
                    { new Guid("00000000-0000-0000-0001-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Setor responsável por vendas e novas oportunidades.", "Comercial", new Guid("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MessageTemplates",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0009-000000000001"));

            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"));

            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000002"));

            migrationBuilder.InsertData(
                table: "Setores",
                columns: new[] { "Id", "CreatedAt", "Descricao", "Nome", "Version" },
                values: new object[,]
                {
                    { new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Setor responsável por vendas e novas oportunidades.", "Comercial", new Guid("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d") },
                    { new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Setor responsável por questões financeiras e boletos.", "Financeiro", new Guid("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a") }
                });
        }
    }
}
