using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CRM.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedSetores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "Tags",
                table: "Conversas",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "UltimoAgenteId",
                table: "Conversas",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Setores",
                columns: new[] { "Id", "Descricao", "Nome", "Version" },
                values: new object[,]
                {
                    { new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"), "Setor responsável por vendas e novas oportunidades.", "Comercial", new Guid("f49b23b6-a53e-4b4b-ac8f-c5319ba6d765") },
                    { new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"), "Setor responsável por questões financeiras e boletos.", "Financeiro", new Guid("228a1086-316e-488e-af5f-5a0b61616f83") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Setores_Nome",
                table: "Setores",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Setores_Nome",
                table: "Setores");

            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"));

            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"));

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Conversas");

            migrationBuilder.DropColumn(
                name: "UltimoAgenteId",
                table: "Conversas");
        }
    }
}
