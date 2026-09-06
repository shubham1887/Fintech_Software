using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeviceWiseFailLoginHistorytableChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttempt",
                table: "DeviceWiseFailLoginHistory");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DeviceWiseFailLoginHistory",
                newName: "DeviceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceID",
                table: "DeviceWiseFailLoginHistory",
                newName: "Key");

            migrationBuilder.AddColumn<int>(
                name: "FailedAttempt",
                table: "DeviceWiseFailLoginHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
