using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class eKycmasteraddedcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KYCToken",
                table: "eKYCMaster",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OTPToken",
                table: "eKYCMaster",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wadh",
                table: "eKYCMaster",
                type: "varchar(200)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KYCToken",
                table: "eKYCMaster");

            migrationBuilder.DropColumn(
                name: "OTPToken",
                table: "eKYCMaster");

            migrationBuilder.DropColumn(
                name: "wadh",
                table: "eKYCMaster");
        }
    }
}
