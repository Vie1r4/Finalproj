using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class PaiolProdutoMotorValidacaoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoPaiol",
                table: "Paiol",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidadeLicenca",
                table: "Paiol",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroLicenca",
                table: "Paiol",
                type: "nvarchar(50)",
                maxLength: 50,
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
                name: "DivisaoDominante",
                table: "Paiol",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "Paiol",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Paiol",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrupoCompatibilidade",
                table: "Produtos",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidade",
                table: "Produtos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TipoPaiol", table: "Paiol");
            migrationBuilder.DropColumn(name: "DataValidadeLicenca", table: "Paiol");
            migrationBuilder.DropColumn(name: "NumeroLicenca", table: "Paiol");
            migrationBuilder.DropColumn(name: "DivisoesAutorizadas", table: "Paiol");
            migrationBuilder.DropColumn(name: "GruposAutorizados", table: "Paiol");
            migrationBuilder.DropColumn(name: "DivisaoDominante", table: "Paiol");
            migrationBuilder.DropColumn(name: "DataInicio", table: "Paiol");
            migrationBuilder.DropColumn(name: "DataFim", table: "Paiol");
            migrationBuilder.DropColumn(name: "GrupoCompatibilidade", table: "Produtos");
            migrationBuilder.DropColumn(name: "DataValidade", table: "Produtos");
        }
    }
}
