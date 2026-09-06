using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class onboradingGstDetailsShow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "account_exists",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pan_number",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trade_name",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_exists",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "pan_number",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "trade_name",
                table: "OnboardingUser");
        }
    }
}
