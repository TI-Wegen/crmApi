using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addagentidintag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgenteId",
                table: "Tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_AgenteId",
                table: "Tags",
                column: "AgenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags",
                column: "AgenteId",
                principalTable: "Agentes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_AgenteId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "AgenteId",
                table: "Tags");
        }
    }
}
