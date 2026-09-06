using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnboardingUserAddingnewcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_Proof_Document_Number",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentID",
                table: "OnboardingUser",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Alternate_Occupation",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Business_City",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Business_District",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Business_Pincode",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Business_State",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Handicapped",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Highest_Qualification",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Operating_Time",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referral_Code",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Residential_District",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shop_Address_Proof_Document",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Small_Merchant",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type_of_Shop",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Working_Days",
                table: "OnboardingUser",
                type: "varchar(200)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_Proof_Document_Number",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "AgentID",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Alternate_Occupation",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Business_City",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Business_District",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Business_Pincode",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Business_State",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Handicapped",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Highest_Qualification",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Operating_Time",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Referral_Code",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Residential_District",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Shop_Address_Proof_Document",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Small_Merchant",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Type_of_Shop",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Working_Days",
                table: "OnboardingUser");
        }
    }
}
