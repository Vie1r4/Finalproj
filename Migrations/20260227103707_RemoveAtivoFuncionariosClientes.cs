using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAtivoFuncionariosClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Clientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Funcionarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
