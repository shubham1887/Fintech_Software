using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnBoardingUserDocument1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingUserDocument_UserMaster_UserID",
                table: "OnboardingUserDocument");

            migrationBuilder.DropIndex(
                name: "IX_OnboardingUserDocument_UserID",
                table: "OnboardingUserDocument");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "OnboardingUserDocument");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "OnboardingUserDocument",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingUserDocument_UserID",
                table: "OnboardingUserDocument",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingUserDocument_UserMaster_UserID",
                table: "OnboardingUserDocument",
                column: "UserID",
                principalTable: "UserMaster",
                principalColumn: "UserID",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
