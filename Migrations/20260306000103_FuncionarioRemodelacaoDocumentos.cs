using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finalproj.Migrations
{
    /// <inheritdoc />
    public partial class FuncionarioRemodelacaoDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataAdmissao",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DataSaida",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Localidade",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Morada",
                table: "Funcionarios");

            migrationBuilder.AddColumn<string>(
                name: "CartaoCidadaoCaminho",
                table: "Funcionarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoADDRCaminho",
                table: "Funcionarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TemADDR",
                table: "Funcionarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartaoCidadaoCaminho",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "DocumentoADDRCaminho",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "TemADDR",
                table: "Funcionarios");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Funcionarios",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAdmissao",
                table: "Funcionarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Funcionarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSaida",
                table: "Funcionarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localidade",
                table: "Funcionarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Morada",
                table: "Funcionarios",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
