using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AssignPlan_Table_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignSubscriptionCharge",
                columns: table => new
                {
                    AssignSubscriptionChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignSubscriptionCharge", x => x.AssignSubscriptionChargeID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignSubscriptionCharge");
        }
    }
}
