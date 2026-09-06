using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class ServiceSubscriptionMaster_and_servicedetailTableadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionCreatePlanMaster",
                columns: table => new
                {
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ImageURL = table.Column<string>(type: "varchar(800)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionCreatePlanMaster", x => x.ServiceSubscriptionCreatePlanMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionPlanDetails",
                columns: table => new
                {
                    ServiceSubscriptionPlanDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionPlanDetails", x => x.ServiceSubscriptionPlanDetailsID);
                    table.ForeignKey(
                        name: "FK_ServiceSubscriptionPlanDetails_ServiceSubscriptionCreatePlanMaster_ServiceSubscriptionCreatePlanMasterID",
                        column: x => x.ServiceSubscriptionCreatePlanMasterID,
                        principalTable: "ServiceSubscriptionCreatePlanMaster",
                        principalColumn: "ServiceSubscriptionCreatePlanMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSubscriptionPlanDetails_ServiceSubscriptionCreatePlanMasterID",
                table: "ServiceSubscriptionPlanDetails",
                column: "ServiceSubscriptionCreatePlanMasterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceSubscriptionPlanDetails");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionCreatePlanMaster");
        }
    }
}
