using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnboradingUserValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GST_Address",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IIBF_Certification",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IIBF_Certification_Number",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VerifiedPage",
                table: "OnboardingUser",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GST_Address",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "IIBF_Certification",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "IIBF_Certification_Number",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "VerifiedPage",
                table: "OnboardingUser");
        }
    }
}
