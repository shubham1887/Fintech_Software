using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class Devicecolumnadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeviceID",
                table: "MPinHistory",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "MPinHistory",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "MPinHistory");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceID",
                table: "MPinHistory",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)");
        }
    }
}
