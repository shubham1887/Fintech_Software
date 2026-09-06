using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class AddViewProviderMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ViewProviderMappings",
                columns: table => new
                {
                    ViewProviderMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewProviderMappings", x => x.ViewProviderMappingID);
                    table.ForeignKey(
                        name: "FK_ViewProviderMappings_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViewProviderMappings_OperatorID",
                table: "ViewProviderMappings",
                column: "OperatorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewProviderMappings");
        }
    }
}
