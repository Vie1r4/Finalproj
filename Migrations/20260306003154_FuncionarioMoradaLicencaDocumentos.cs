using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class FuncionarioMoradaLicencaDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemADDR",
                table: "Funcionarios");

            migrationBuilder.AddColumn<string>(
                name: "LicencaOperadorCaminho",
                table: "Funcionarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "Funcionarios",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicencaOperadorCaminho",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "Funcionarios");

            migrationBuilder.AddColumn<bool>(
                name: "TemADDR",
                table: "Funcionarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
