using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletDisableServiceGSTVirtualBalanceUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "VirtualBalances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGSTInclusive",
                table: "ServiceMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "VirtualBalances");

            migrationBuilder.DropColumn(
                name: "IsGSTInclusive",
                table: "ServiceMasters");
        }
    }
}
