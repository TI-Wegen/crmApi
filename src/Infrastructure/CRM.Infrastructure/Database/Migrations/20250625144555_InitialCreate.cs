using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CargaDeTrabalho = table.Column<int>(type: "integer", nullable: false),
                    SetorIds = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contatos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conversas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SetorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContatoTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContatoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContatoTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContatoTags_Contatos_ContatoId",
                        column: x => x.ContatoId,
                        principalTable: "Contatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataDeAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContatoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoStatus_Contatos_ContatoId",
                        column: x => x.ContatoId,
                        principalTable: "Contatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    MessageId = table.Column<string>(type: "text", nullable: true),
                    AnexoUrl = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RemetenteTipo = table.Column<string>(type: "text", nullable: false),
                    RemetenteAgenteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agentes_Email",
                table: "Agentes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contatos_Telefone",
                table: "Contatos",
                column: "Telefone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContatoTags_ContatoId",
                table: "ContatoTags",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoStatus_ContatoId",
                table: "HistoricoStatus",
                column: "ContatoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ConversaId",
                table: "Mensagens",
                column: "ConversaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agentes");

            migrationBuilder.DropTable(
                name: "ContatoTags");

            migrationBuilder.DropTable(
                name: "HistoricoStatus");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "Setores");

            migrationBuilder.DropTable(
                name: "Contatos");

            migrationBuilder.DropTable(
                name: "Conversas");
        }
    }
}
