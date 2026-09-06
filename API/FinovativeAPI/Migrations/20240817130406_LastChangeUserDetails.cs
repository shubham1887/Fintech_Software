using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class LastChangeUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LastChangeUserDetails",
                columns: table => new
                {
                    LastChangeUserDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    Change = table.Column<string>(type: "varchar(500)", nullable: true),
                    OldChanges = table.Column<string>(type: "varchar(500)", nullable: false),
                    NewChanges = table.Column<string>(type: "varchar(500)", nullable: true),
                    ChangesByID = table.Column<string>(type: "varchar(50)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastChangeUserDetails", x => x.LastChangeUserDetailsID);
                    table.ForeignKey(
                        name: "FK_LastChangeUserDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastChangeUserDetails_UserID",
                table: "LastChangeUserDetails",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastChangeUserDetails");
        }
    }
}
