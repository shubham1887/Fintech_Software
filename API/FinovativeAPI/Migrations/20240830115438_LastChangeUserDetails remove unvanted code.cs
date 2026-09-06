using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class LastChangeUserDetailsremoveunvantedcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LastChangeUserDetails_UserMaster_UserID",
                table: "LastChangeUserDetails");

            migrationBuilder.DropIndex(
                name: "IX_LastChangeUserDetails_UserID",
                table: "LastChangeUserDetails");

            migrationBuilder.DropColumn(
                name: "ChangesByID",
                table: "LastChangeUserDetails");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LastChangeUserDetails");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "LastChangeUserDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChangesByID",
                table: "LastChangeUserDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LastChangeUserDetails",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "LastChangeUserDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LastChangeUserDetails_UserID",
                table: "LastChangeUserDetails",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_LastChangeUserDetails_UserMaster_UserID",
                table: "LastChangeUserDetails",
                column: "UserID",
                principalTable: "UserMaster",
                principalColumn: "UserID");
        }
    }
}
