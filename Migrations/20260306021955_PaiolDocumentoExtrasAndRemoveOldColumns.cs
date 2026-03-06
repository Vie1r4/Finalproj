using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class PaiolDocumentoExtrasAndRemoveOldColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Paiol");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "Paiol");

            migrationBuilder.DropColumn(
                name: "DivisoesAutorizadas",
                table: "Paiol");

            migrationBuilder.DropColumn(
                name: "GruposAutorizados",
                table: "Paiol");

            migrationBuilder.DropColumn(
                name: "TipoPaiol",
                table: "Paiol");

            migrationBuilder.CreateTable(
                name: "PaiolDocumentoExtras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaiolId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Caminho = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaiolDocumentoExtras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaiolDocumentoExtras_Paiol_PaiolId",
                        column: x => x.PaiolId,
                        principalTable: "Paiol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaiolDocumentoExtras_PaiolId",
                table: "PaiolDocumentoExtras",
                column: "PaiolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaiolDocumentoExtras");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Paiol",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "Paiol",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DivisoesAutorizadas",
                table: "Paiol",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GruposAutorizados",
                table: "Paiol",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPaiol",
                table: "Paiol",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }
    }
}
