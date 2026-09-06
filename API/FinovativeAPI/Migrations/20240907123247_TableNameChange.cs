using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class TableNameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignSubscriptionCharge");

            migrationBuilder.CreateTable(
                name: "UserSubscriptionPlanMapping",
                columns: table => new
                {
                    UserSubscriptionPlanMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(500)", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionPlanMapping", x => x.UserSubscriptionPlanMappingID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSubscriptionPlanMapping");

            migrationBuilder.CreateTable(
                name: "AssignSubscriptionCharge",
                columns: table => new
                {
                    AssignSubscriptionChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignSubscriptionCharge", x => x.AssignSubscriptionChargeID);
                });
        }
    }
}
