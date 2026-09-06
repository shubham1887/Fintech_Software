using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class MPinHistorytableadded1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MPinHistory",
                columns: table => new
                {
                    MPinHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mpin = table.Column<string>(type: "varchar(6)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MPinHistory", x => x.MPinHistoryID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MPinHistory");
        }
    }
}
