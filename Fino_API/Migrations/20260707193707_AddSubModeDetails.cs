using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSubModeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "SubModes",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "SubModes",
                type: "varchar(150)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryName",
                table: "SubModes",
                type: "varchar(150)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IFSC",
                table: "SubModes",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "SubModes",
                type: "varchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QRImage",
                table: "SubModes",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "SubModes");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "SubModes");

            migrationBuilder.DropColumn(
                name: "BeneficiaryName",
                table: "SubModes");

            migrationBuilder.DropColumn(
                name: "IFSC",
                table: "SubModes");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "SubModes");

            migrationBuilder.DropColumn(
                name: "QRImage",
                table: "SubModes");
        }
    }
}
