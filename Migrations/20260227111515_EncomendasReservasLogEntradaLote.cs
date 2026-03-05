using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class EncomendasReservasLogEntradaLote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EncomendaId",
                table: "SaidasPaiol",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntradaPaiolId",
                table: "SaidasPaiol",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFabrico",
                table: "EntradasPaiol",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidade",
                table: "EntradasPaiol",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLote",
                table: "EntradasPaiol",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Clientes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Encomendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FuncionarioAceiteUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FuncionarioPreparouUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encomendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encomendas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogSistema",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Acao = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    JsonDados = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncomendaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncomendaId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    QuantidadePedida = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncomendaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncomendaItems_Encomendas_EncomendaId",
                        column: x => x.EncomendaId,
                        principalTable: "Encomendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncomendaItems_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncomendaId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Encomendas_EncomendaId",
                        column: x => x.EncomendaId,
                        principalTable: "Encomendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaidasPaiol_EntradaPaiolId",
                table: "SaidasPaiol",
                column: "EntradaPaiolId");

            migrationBuilder.CreateIndex(
                name: "IX_EncomendaItems_EncomendaId",
                table: "EncomendaItems",
                column: "EncomendaId");

            migrationBuilder.CreateIndex(
                name: "IX_EncomendaItems_ProdutoId",
                table: "EncomendaItems",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_ClienteId",
                table: "Encomendas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_EncomendaId_ProdutoId",
                table: "Reservas",
                columns: new[] { "EncomendaId", "ProdutoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ProdutoId",
                table: "Reservas",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaidasPaiol_EntradasPaiol_EntradaPaiolId",
                table: "SaidasPaiol",
                column: "EntradaPaiolId",
                principalTable: "EntradasPaiol",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaidasPaiol_EntradasPaiol_EntradaPaiolId",
                table: "SaidasPaiol");

            migrationBuilder.DropTable(
                name: "EncomendaItems");

            migrationBuilder.DropTable(
                name: "LogSistema");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Encomendas");

            migrationBuilder.DropIndex(
                name: "IX_SaidasPaiol_EntradaPaiolId",
                table: "SaidasPaiol");

            migrationBuilder.DropColumn(
                name: "EncomendaId",
                table: "SaidasPaiol");

            migrationBuilder.DropColumn(
                name: "EntradaPaiolId",
                table: "SaidasPaiol");

            migrationBuilder.DropColumn(
                name: "DataFabrico",
                table: "EntradasPaiol");

            migrationBuilder.DropColumn(
                name: "DataValidade",
                table: "EntradasPaiol");

            migrationBuilder.DropColumn(
                name: "NumeroLote",
                table: "EntradasPaiol");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Clientes");
        }
    }
}
