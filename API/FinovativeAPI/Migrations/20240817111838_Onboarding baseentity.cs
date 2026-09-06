using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class Onboardingbaseentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DOC",
                table: "OnboardingUserDocument",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DOCInt",
                table: "OnboardingUserDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDOC",
                table: "OnboardingUserDocument",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DOC",
                table: "OnboardingUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DOCInt",
                table: "OnboardingUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDOC",
                table: "OnboardingUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOC",
                table: "OnboardingUserDocument");

            migrationBuilder.DropColumn(
                name: "DOCInt",
                table: "OnboardingUserDocument");

            migrationBuilder.DropColumn(
                name: "UpdatedDOC",
                table: "OnboardingUserDocument");

            migrationBuilder.DropColumn(
                name: "DOC",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "DOCInt",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "UpdatedDOC",
                table: "OnboardingUser");
        }
    }
}
