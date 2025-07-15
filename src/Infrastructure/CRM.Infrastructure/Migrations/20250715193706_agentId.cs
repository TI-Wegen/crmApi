using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class agentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mensagens_Conversas_ConversaId",
                table: "Mensagens");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversaId",
                table: "Mensagens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AtendimentoId",
                table: "Mensagens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Mensagens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Mensagens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Mensagens_Conversas_ConversaId",
                table: "Mensagens",
                column: "ConversaId",
                principalTable: "Conversas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mensagens_Conversas_ConversaId",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "AtendimentoId",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Mensagens");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Mensagens");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversaId",
                table: "Mensagens",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensagens_Conversas_ConversaId",
                table: "Mensagens",
                column: "ConversaId",
                principalTable: "Conversas",
                principalColumn: "Id");
        }
    }
}
