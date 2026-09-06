using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnboradingUseraddedcolumnAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "supportID",
                table: "OnboardingUser",
                newName: "supportUserID");

            migrationBuilder.RenameColumn(
                name: "Verify",
                table: "OnboardingUser",
                newName: "supportUserVerify");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OnboardingUser",
                newName: "SupportStatus");

            migrationBuilder.RenameColumn(
                name: "ApproveVerify",
                table: "OnboardingUser",
                newName: "AdminVerify");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "OnboardingUser",
                newName: "Residential_Address");

            migrationBuilder.AddColumn<string>(
                name: "AdminRemark",
                table: "OnboardingUser",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminStatus",
                table: "OnboardingUser",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Business_Address",
                table: "OnboardingUser",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "OnboardingUser",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportRemark",
                table: "OnboardingUser",
                type: "varchar(50)",
                nullable: true);



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "AdminRemark",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "AdminStatus",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Business_Address",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "SupportRemark",
                table: "OnboardingUser");

            migrationBuilder.RenameColumn(
                name: "supportUserVerify",
                table: "OnboardingUser",
                newName: "Verify");

            migrationBuilder.RenameColumn(
                name: "supportUserID",
                table: "OnboardingUser",
                newName: "supportID");

            migrationBuilder.RenameColumn(
                name: "SupportStatus",
                table: "OnboardingUser",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Residential_Address",
                table: "OnboardingUser",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "AdminVerify",
                table: "OnboardingUser",
                newName: "ApproveVerify");
        }
    }
}
