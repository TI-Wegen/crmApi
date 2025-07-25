using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SesaoWpp24H : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SessaoFim",
                table: "Conversas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessaoInicio",
                table: "Conversas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSessoesIniciadas",
                table: "Conversas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessaoFim",
                table: "Conversas");

            migrationBuilder.DropColumn(
                name: "SessaoInicio",
                table: "Conversas");

            migrationBuilder.DropColumn(
                name: "TotalSessoesIniciadas",
                table: "Conversas");
        }
    }
}
