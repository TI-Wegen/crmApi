using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addsetorid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SetorIds",
                table: "Agentes");

            migrationBuilder.AddColumn<Guid>(
                name: "SetorId",
                table: "Agentes",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Agentes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "SetorId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Agentes_SetorId",
                table: "Agentes",
                column: "SetorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes");

            migrationBuilder.DropIndex(
                name: "IX_Agentes_SetorId",
                table: "Agentes");

            migrationBuilder.DropColumn(
                name: "SetorId",
                table: "Agentes");

            migrationBuilder.AddColumn<string>(
                name: "SetorIds",
                table: "Agentes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Agentes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "SetorIds",
                value: "");
        }
    }
}
