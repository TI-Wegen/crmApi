using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatetagIdfieldtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atendimentos_Tags_TagsId",
                table: "Atendimentos");

            migrationBuilder.DropIndex(
                name: "IX_Atendimentos_TagsId",
                table: "Atendimentos");

            migrationBuilder.DropColumn(
                name: "TagsId",
                table: "Atendimentos");

            migrationBuilder.AddColumn<Guid>(
                name: "TagsId",
                table: "Conversas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_TagsId",
                table: "Conversas",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversas_Tags_TagsId",
                table: "Conversas",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversas_Tags_TagsId",
                table: "Conversas");

            migrationBuilder.DropIndex(
                name: "IX_Conversas_TagsId",
                table: "Conversas");

            migrationBuilder.DropColumn(
                name: "TagsId",
                table: "Conversas");

            migrationBuilder.AddColumn<Guid>(
                name: "TagsId",
                table: "Atendimentos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atendimentos_TagsId",
                table: "Atendimentos",
                column: "TagsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atendimentos_Tags_TagsId",
                table: "Atendimentos",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
