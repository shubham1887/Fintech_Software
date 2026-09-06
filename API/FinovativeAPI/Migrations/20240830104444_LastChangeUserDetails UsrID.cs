using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class LastChangeUserDetailsUsrID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ID",
                table: "LastChangeUserDetails");

            migrationBuilder.AlterColumn<string>(
                name: "OldChanges",
                table: "LastChangeUserDetails",
                type: "varchar(500)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)");

            migrationBuilder.AlterColumn<int>(
                name: "ChangesByID",
                table: "LastChangeUserDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingUserID",
                table: "LastChangeUserDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingUserID",
                table: "LastChangeUserDetails");

            migrationBuilder.AlterColumn<string>(
                name: "OldChanges",
                table: "LastChangeUserDetails",
                type: "varchar(500)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChangesByID",
                table: "LastChangeUserDetails",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ID",
                table: "LastChangeUserDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
