using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFundModeSubMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModeID",
                table: "FundTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubModeID",
                table: "FundTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Modes",
                columns: table => new
                {
                    ModeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modes", x => x.ModeID);
                });

            migrationBuilder.CreateTable(
                name: "SubModes",
                columns: table => new
                {
                    SubModeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModeID = table.Column<int>(type: "int", nullable: false),
                    SubModeName = table.Column<string>(type: "varchar(150)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubModes", x => x.SubModeID);
                    table.ForeignKey(
                        name: "FK_SubModes_Modes_ModeID",
                        column: x => x.ModeID,
                        principalTable: "Modes",
                        principalColumn: "ModeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_ModeID",
                table: "FundTransactions",
                column: "ModeID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_SubModeID",
                table: "FundTransactions",
                column: "SubModeID");

            migrationBuilder.CreateIndex(
                name: "IX_SubModes_ModeID",
                table: "SubModes",
                column: "ModeID");

            migrationBuilder.AddForeignKey(
                name: "FK_FundTransactions_Modes_ModeID",
                table: "FundTransactions",
                column: "ModeID",
                principalTable: "Modes",
                principalColumn: "ModeID");

            migrationBuilder.AddForeignKey(
                name: "FK_FundTransactions_SubModes_SubModeID",
                table: "FundTransactions",
                column: "SubModeID",
                principalTable: "SubModes",
                principalColumn: "SubModeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundTransactions_Modes_ModeID",
                table: "FundTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FundTransactions_SubModes_SubModeID",
                table: "FundTransactions");

            migrationBuilder.DropTable(
                name: "SubModes");

            migrationBuilder.DropTable(
                name: "Modes");

            migrationBuilder.DropIndex(
                name: "IX_FundTransactions_ModeID",
                table: "FundTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FundTransactions_SubModeID",
                table: "FundTransactions");

            migrationBuilder.DropColumn(
                name: "ModeID",
                table: "FundTransactions");

            migrationBuilder.DropColumn(
                name: "SubModeID",
                table: "FundTransactions");
        }
    }
}
