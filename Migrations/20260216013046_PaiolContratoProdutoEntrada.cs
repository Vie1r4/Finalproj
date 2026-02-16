using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class PaiolContratoProdutoEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PerfilRisco",
                table: "Paiol",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "1.3G");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Paiol",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ativo");

            migrationBuilder.Sql("UPDATE Paiol SET Estado = N'Ativo', PerfilRisco = N'1.3G' WHERE Estado = '' OR PerfilRisco = '';");

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NEMPorUnidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FamiliaRisco = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntradasPaiol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaiolId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasPaiol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradasPaiol_Paiol_PaiolId",
                        column: x => x.PaiolId,
                        principalTable: "Paiol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasPaiol_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntradasPaiol_PaiolId",
                table: "EntradasPaiol",
                column: "PaiolId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasPaiol_ProdutoId",
                table: "EntradasPaiol",
                column: "ProdutoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntradasPaiol");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Paiol");

            migrationBuilder.DropColumn(
                name: "PerfilRisco",
                table: "Paiol");
        }
    }
}
