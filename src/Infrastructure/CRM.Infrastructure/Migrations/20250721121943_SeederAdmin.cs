using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeederAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Setores",
                columns: new[] { "Id", "CreatedAt", "Descricao", "Nome", "Version" },
                values: new object[] { new Guid("00000000-0000-0000-0001-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Setor responsável pela administração geral", "Administração", new Guid("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7a") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000003"));
        }
    }
}
