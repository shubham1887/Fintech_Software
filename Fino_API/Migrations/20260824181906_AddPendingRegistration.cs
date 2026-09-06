using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingRegistrations",
                columns: table => new
                {
                    PendingRegistrationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonName = table.Column<string>(type: "varchar(150)", nullable: false),
                    FirmName = table.Column<string>(type: "varchar(150)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(150)", nullable: false),
                    PANNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    AadharNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    GSTNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    Address = table.Column<string>(type: "varchar(300)", nullable: false),
                    CityName = table.Column<string>(type: "varchar(100)", nullable: false),
                    District = table.Column<string>(type: "varchar(100)", nullable: false),
                    Pincode = table.Column<string>(type: "varchar(10)", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: true),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    ReferralCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    RejectReason = table.Column<string>(type: "varchar(300)", nullable: false),
                    IsMobileVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUserID = table.Column<int>(type: "int", nullable: true),
                    ActionByUserID = table.Column<int>(type: "int", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingRegistrations", x => x.PendingRegistrationID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingRegistrations");
        }
    }
}
