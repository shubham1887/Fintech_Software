using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class Lob_Details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Business_Locality",
                table: "OnboardingUser",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LOB_MasterID",
                table: "OnboardingUser",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineOfBusiness",
                table: "OnboardingUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Residential_Locality",
                table: "OnboardingUser",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopType",
                table: "OnboardingUser",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LOB_Details",
                columns: table => new
                {
                    LOB_MasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeOfShop = table.Column<string>(type: "varchar(500)", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    LineOfBusiness = table.Column<string>(type: "varchar(500)", nullable: true),
                    LOBCode = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOB_Details", x => x.LOB_MasterID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LOB_Details");

            migrationBuilder.DropColumn(
                name: "Business_Locality",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "LOB_MasterID",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "LineOfBusiness",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "Residential_Locality",
                table: "OnboardingUser");

            migrationBuilder.DropColumn(
                name: "ShopType",
                table: "OnboardingUser");
        }
    }
}
