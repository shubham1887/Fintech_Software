using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class OnBorading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         
            migrationBuilder.CreateTable(
                name: "OnboardingUser",
                columns: table => new
                {
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileID = table.Column<string>(type: "varchar(500)", nullable: true),
                    ReferenceID = table.Column<string>(type: "varchar(500)", nullable: false),
                    DocumentID = table.Column<string>(type: "varchar(500)", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOB = table.Column<string>(type: "varchar(50)", nullable: true),
                    PanNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    AadhaarNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    PinCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", nullable: true),
                    EmailID = table.Column<string>(type: "varchar(50)", nullable: true),
                    MobileNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    FirmName = table.Column<string>(type: "varchar(500)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Profile_URL = table.Column<string>(type: "varchar(500)", nullable: true),
                    supportID = table.Column<int>(type: "int", nullable: false),
                    Verify = table.Column<bool>(type: "bit", nullable: false),
                    ApproveVerify = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUser", x => x.OnboardingUserID);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingUserDocument",
                columns: table => new
                {
                    OnboardingUserDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Ref_ID = table.Column<string>(type: "varchar(500)", nullable: true),
                    Type = table.Column<string>(type: "varchar(500)", nullable: false),
                    OldImage = table.Column<string>(type: "varchar(500)", nullable: true),
                    NewImage = table.Column<string>(type: "varchar(500)", nullable: true),
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUserDocument", x => x.OnboardingUserDocumentID);
                    table.ForeignKey(
                        name: "FK_OnboardingUserDocument_OnboardingUser_OnboardingUserID",
                        column: x => x.OnboardingUserID,
                        principalTable: "OnboardingUser",
                        principalColumn: "OnboardingUserID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_OnboardingUserDocument_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingUserDocument_OnboardingUserID",
                table: "OnboardingUserDocument",
                column: "OnboardingUserID");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingUserDocument_UserID",
                table: "OnboardingUserDocument",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingUserDocument");

            migrationBuilder.DropTable(
                name: "OnboardingUser");
        }
    }
}
