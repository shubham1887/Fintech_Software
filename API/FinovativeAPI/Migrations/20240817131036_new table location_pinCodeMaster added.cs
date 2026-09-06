using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class newtablelocation_pinCodeMasteradded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location_PinCodeMaster",
                columns: table => new
                {
                    SR_NO = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PINCODE = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_DISTRICT = table.Column<string>(type: "varchar(100)", nullable: false),
                    HYP_STATE = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_TALUKA_CITY = table.Column<string>(type: "varchar(100)", nullable: false),
                    REGIONNAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_REGION = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_ZONE = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_AREA = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_NSM = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_RH = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_ZSM = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_SH = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_CM = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_ASM = table.Column<string>(type: "varchar(100)", nullable: false),
                    HPY_FLS = table.Column<string>(type: "varchar(100)", nullable: false),
                    ACTIVATED = table.Column<string>(type: "varchar(100)", nullable: false),
                    ACTIVATION_DATE = table.Column<string>(type: "varchar(100)", nullable: false),
                    LOCATION = table.Column<string>(type: "varchar(100)", nullable: false),
                    WITHIN_CITY_LIMITS = table.Column<string>(type: "varchar(100)", nullable: false),
                    DISTANCE_FROM_CBD = table.Column<string>(type: "varchar(100)", nullable: false),
                    INDIAPOST_OFFICE_NAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    DIVISIONNAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    LONGITUDE = table.Column<string>(type: "varchar(100)", nullable: false),
                    LATITUDE = table.Column<string>(type: "varchar(100)", nullable: false),
                    EMPLOYEE_CODE = table.Column<string>(type: "varchar(100)", nullable: true),
                    EMPLOYEE_NAME = table.Column<string>(type: "varchar(100)", nullable: true),
                    EMPLOYEE_MOBILE = table.Column<string>(type: "varchar(100)", nullable: true),
                    EMPLOYEE_EMAIL = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_5 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_6 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_7 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_8 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_9 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_10 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_11 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_12 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_13 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_14 = table.Column<string>(type: "varchar(100)", nullable: true),
                    RESERVEFIELD_15 = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location_PinCodeMaster", x => x.SR_NO);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Location_PinCodeMaster");
        }
    }
}
