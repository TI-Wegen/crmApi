using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatesetoridsetrequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes");

            migrationBuilder.AlterColumn<Guid>(
                name: "SetorId",
                table: "Agentes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes");

            migrationBuilder.AlterColumn<Guid>(
                name: "SetorId",
                table: "Agentes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.UpdateData(
                table: "Agentes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "CreatedAt", "Email", "Nome", "PasswordHash", "SetorId", "Status", "Version" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sistema@crm.local", "Sistema", "$2a$11$fH.d2sB7aY.s.1b2a3c4d5e6f7g8h9i0j", null, "Offline", new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.AddForeignKey(
                name: "FK_Agentes_Setores_SetorId",
                table: "Agentes",
                column: "SetorId",
                principalTable: "Setores",
                principalColumn: "Id");
        }
    }
}
