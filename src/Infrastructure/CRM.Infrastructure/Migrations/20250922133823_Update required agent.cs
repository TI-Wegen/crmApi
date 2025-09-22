using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updaterequiredagent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgenteId",
                table: "Tags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags",
                column: "AgenteId",
                principalTable: "Agentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags");

            migrationBuilder.AlterColumn<Guid>(
                name: "AgenteId",
                table: "Tags",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Agentes_AgenteId",
                table: "Tags",
                column: "AgenteId",
                principalTable: "Agentes",
                principalColumn: "Id");
        }
    }
}
