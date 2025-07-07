using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedSetor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"),
                column: "Version",
                value: new Guid("d4a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"));

            migrationBuilder.UpdateData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"),
                column: "Version",
                value: new Guid("f6b3a2a8-8e6a-4b2a-8b8d-9b8e1f0c3b1a"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("c2a3b4d5-6e7f-8a9b-0c1d-2e3f4a5b6c7d"),
                column: "Version",
                value: new Guid("f49b23b6-a53e-4b4b-ac8f-c5319ba6d765"));

            migrationBuilder.UpdateData(
                table: "Setores",
                keyColumn: "Id",
                keyValue: new Guid("f4d4a8e2-8e6a-4b2a-8b8d-9b8e1f0c3b1a"),
                column: "Version",
                value: new Guid("228a1086-316e-488e-af5f-5a0b61616f83"));
        }
    }
}
