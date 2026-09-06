using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class OperatorDenominationFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDenominationApplicable",
                table: "OperatorMasters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDenominationApplicable",
                table: "OperatorMasters");
        }
    }
}
