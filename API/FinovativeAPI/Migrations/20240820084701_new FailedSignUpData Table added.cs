using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class newFailedSignUpDataTableadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedSignUpData",
                columns: table => new
                {
                    FailedSignUpDataID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    PANNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    ReferralCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    EmployeeID = table.Column<string>(type: "varchar(50)", nullable: true),
                    OrderID = table.Column<string>(type: "varchar(50)", nullable: true),
                    RedirectURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedSignUpData", x => x.FailedSignUpDataID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedSignUpData");
        }
    }
}
