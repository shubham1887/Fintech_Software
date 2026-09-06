using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class newFailedSignUpDataTableaddedbaseentityaddded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DOC",
                table: "FailedSignUpData",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DOCInt",
                table: "FailedSignUpData",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDOC",
                table: "FailedSignUpData",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOC",
                table: "FailedSignUpData");

            migrationBuilder.DropColumn(
                name: "DOCInt",
                table: "FailedSignUpData");

            migrationBuilder.DropColumn(
                name: "UpdatedDOC",
                table: "FailedSignUpData");
        }
    }
}
