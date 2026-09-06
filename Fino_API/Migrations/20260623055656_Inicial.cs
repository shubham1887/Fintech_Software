using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FintechApp.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountOpenSettings",
                columns: table => new
                {
                    AccountOpenSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "varchar(500)", nullable: true),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: true),
                    CallBackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountOpenSettings", x => x.AccountOpenSettingID);
                });

            migrationBuilder.CreateTable(
                name: "AEPSSettings",
                columns: table => new
                {
                    AEPSSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsMerchantVerify = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AEPSSettings", x => x.AEPSSettingID);
                });

            migrationBuilder.CreateTable(
                name: "AndroidMenuMasters",
                columns: table => new
                {
                    AndroidMenuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AndroidMenuMasters", x => x.AndroidMenuMasterID);
                });

            migrationBuilder.CreateTable(
                name: "AndroidSectionMasters",
                columns: table => new
                {
                    AndroidSectionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AndroidSectionMasters", x => x.AndroidSectionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(100)", nullable: false),
                    Module = table.Column<string>(type: "varchar(100)", nullable: false),
                    OldValue = table.Column<string>(type: "varchar(max)", nullable: false),
                    NewValue = table.Column<string>(type: "varchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogID);
                });

            migrationBuilder.CreateTable(
                name: "BBPSSettings",
                columns: table => new
                {
                    BBPSSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: true),
                    AgentID = table.Column<string>(type: "varchar(500)", nullable: true),
                    PartnerID = table.Column<string>(type: "varchar(500)", nullable: true),
                    MerchantID = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BBPSSettings", x => x.BBPSSettingID);
                });

            migrationBuilder.CreateTable(
                name: "BlackListBankMasters",
                columns: table => new
                {
                    BlackListBankMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListBankMasters", x => x.BlackListBankMasterID);
                });

            migrationBuilder.CreateTable(
                name: "BrowserUserAgentMasters",
                columns: table => new
                {
                    BrowserUserAgentMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAgent = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowserUserAgentMasters", x => x.BrowserUserAgentMasterID);
                });

            migrationBuilder.CreateTable(
                name: "BulkMessages",
                columns: table => new
                {
                    BulkMessageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsSMS = table.Column<bool>(type: "bit", nullable: false),
                    IsEmail = table.Column<bool>(type: "bit", nullable: false),
                    IsWhatsUp = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkMessages", x => x.BulkMessageID);
                });

            migrationBuilder.CreateTable(
                name: "BulkPaySettings",
                columns: table => new
                {
                    BulkPaySettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkPaySettings", x => x.BulkPaySettingID);
                });

            migrationBuilder.CreateTable(
                name: "CallMeCategoryMaster",
                columns: table => new
                {
                    CallMeCategoryMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallMeCategoryMaster", x => x.CallMeCategoryMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CashCollectionDetails",
                columns: table => new
                {
                    CashCollectionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    OutstandingOpening = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OutstandingClosing = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ParentRemark = table.Column<string>(type: "varchar(100)", nullable: false),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: false),
                    Credit = table.Column<string>(type: "varchar(50)", nullable: false),
                    Debit = table.Column<string>(type: "varchar(50)", nullable: false),
                    UTRNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCollectionDetails", x => x.CashCollectionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "CashDepositSettings",
                columns: table => new
                {
                    CashDepositSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashDepositSettings", x => x.CashDepositSettingID);
                });

            migrationBuilder.CreateTable(
                name: "Categorys",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorys", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ChannelCategorys",
                columns: table => new
                {
                    ChannelCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelCategorys", x => x.ChannelCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSlabMasters",
                columns: table => new
                {
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabName = table.Column<string>(type: "varchar(80)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSlabMasters", x => x.ChannelSlabMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CircleAPIMasters",
                columns: table => new
                {
                    CircleAPIMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CircleAPIURL = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleAPIMasters", x => x.CircleAPIMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CMSSettings",
                columns: table => new
                {
                    CMSSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CMSSettings", x => x.CMSSettingID);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMasters",
                columns: table => new
                {
                    CompanyMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMasters", x => x.CompanyMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerTypeMasters",
                columns: table => new
                {
                    ConsumerTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerTypeMasters", x => x.ConsumerTypeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardSettings",
                columns: table => new
                {
                    CreditCardSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardSettings", x => x.CreditCardSettingID);
                });

            migrationBuilder.CreateTable(
                name: "CreditInstitutionMasters",
                columns: table => new
                {
                    CreditInstitutionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptionKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditInstitutionMasters", x => x.CreditInstitutionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "DateFormatMasters",
                columns: table => new
                {
                    DateFormatMasterkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateFormat = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateFormatMasters", x => x.DateFormatMasterkID);
                });

            migrationBuilder.CreateTable(
                name: "DematSettings",
                columns: table => new
                {
                    DematSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UTMSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DematSettings", x => x.DematSettingID);
                });

            migrationBuilder.CreateTable(
                name: "DenominationMasters",
                columns: table => new
                {
                    DenominationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    DenominationRange = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsStop = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenominationMasters", x => x.DenominationMasterID);
                });

            migrationBuilder.CreateTable(
                name: "DeviceWiseFailLoginHistorys",
                columns: table => new
                {
                    DeviceWiseFailLoginHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceWiseFailLoginHistorys", x => x.DeviceWiseFailLoginHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "DMTBanks",
                columns: table => new
                {
                    DMTBankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    DefaultIFSC = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsIMPS = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTBanks", x => x.DMTBankID);
                });

            migrationBuilder.CreateTable(
                name: "DMTSettings",
                columns: table => new
                {
                    DMTSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: false),
                    WalletID = table.Column<string>(type: "varchar(500)", nullable: false),
                    CallBackURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(500)", nullable: false),
                    SplitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SenderLimit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsCCF = table.Column<bool>(type: "bit", nullable: false),
                    IsNeft = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsPayoutDMT = table.Column<bool>(type: "bit", nullable: false),
                    ProviderCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsExpressDMT = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTSettings", x => x.DMTSettingID);
                });

            migrationBuilder.CreateTable(
                name: "EcollectionSettings",
                columns: table => new
                {
                    EcollectionSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrivateKeyPath = table.Column<string>(type: "varchar(500)", nullable: false),
                    PrivateKeyPassword = table.Column<string>(type: "varchar(500)", nullable: false),
                    PublicKeyPath = table.Column<string>(type: "varchar(500)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "varchar(100)", nullable: false),
                    IFSCCode = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcollectionSettings", x => x.EcollectionSettingID);
                });

            migrationBuilder.CreateTable(
                name: "eKYCSettings",
                columns: table => new
                {
                    eKYCSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EncryptionKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCSettings", x => x.eKYCSettingID);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettingMasters",
                columns: table => new
                {
                    EmailSettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "varchar(100)", nullable: false),
                    Password = table.Column<string>(type: "varchar(100)", nullable: false),
                    HostName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    IsSSL = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettingMasters", x => x.EmailSettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryDetails",
                columns: table => new
                {
                    EnquiryDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonName = table.Column<string>(type: "varchar(50)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(50)", nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: false),
                    EnquiryType = table.Column<string>(type: "varchar(50)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(200)", nullable: false),
                    AdminStatus = table.Column<string>(type: "varchar(200)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(200)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryDetails", x => x.EnquiryDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "ExternalMedias",
                columns: table => new
                {
                    ExternalMediaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(100)", nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMedias", x => x.ExternalMediaID);
                });

            migrationBuilder.CreateTable(
                name: "FailedSignUpDatas",
                columns: table => new
                {
                    FailedSignUpDataID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PANNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PinCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedSignUpDatas", x => x.FailedSignUpDataID);
                });

            migrationBuilder.CreateTable(
                name: "FingerPrintDeviceTypes",
                columns: table => new
                {
                    FingerPrintDeviceTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "varchar(200)", nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FingerPrintDeviceTypes", x => x.FingerPrintDeviceTypeID);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSettings",
                columns: table => new
                {
                    InvoiceSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "varchar(500)", nullable: false),
                    LogoURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    CompanyPrefix = table.Column<string>(type: "varchar(10)", nullable: false),
                    Address = table.Column<string>(type: "varchar(max)", nullable: false),
                    ShortAddress = table.Column<string>(type: "varchar(max)", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(max)", nullable: false),
                    District = table.Column<string>(type: "varchar(max)", nullable: false),
                    Country = table.Column<string>(type: "varchar(max)", nullable: false),
                    GSTIN = table.Column<string>(type: "varchar(max)", nullable: false),
                    PAN = table.Column<string>(type: "varchar(10)", nullable: false),
                    CIN = table.Column<string>(type: "varchar(25)", nullable: false),
                    State = table.Column<string>(type: "varchar(50)", nullable: false),
                    AccountNo = table.Column<string>(type: "varchar(100)", nullable: false),
                    AccountType = table.Column<string>(type: "varchar(30)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(30)", nullable: false),
                    AccountHolderName = table.Column<string>(type: "varchar(100)", nullable: false),
                    BankName = table.Column<string>(type: "varchar(max)", nullable: false),
                    BranchName = table.Column<string>(type: "varchar(max)", nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", nullable: false),
                    WebsiteName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Note = table.Column<string>(type: "varchar(max)", nullable: false),
                    FullAddress = table.Column<string>(type: "varchar(max)", nullable: false),
                    SignatureURL = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSettings", x => x.InvoiceSettingID);
                });

            migrationBuilder.CreateTable(
                name: "KYCApprovedDetails",
                columns: table => new
                {
                    KYCApprovedDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ApporvedUserID = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "varchar(200)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KYCApprovedDetails", x => x.KYCApprovedDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "KycDocumentMasters",
                columns: table => new
                {
                    KycDocumentMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "varchar(100)", nullable: true),
                    DocumentType = table.Column<string>(type: "varchar(100)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(200)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    RegularRxpression = table.Column<string>(type: "varchar(500)", nullable: true),
                    SampleImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycDocumentMasters", x => x.KycDocumentMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LapuPartyMasters",
                columns: table => new
                {
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(70)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuPartyMasters", x => x.LapuPartyMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LapuTransactionDetails",
                columns: table => new
                {
                    LapuTransactionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProivderID = table.Column<int>(type: "int", nullable: false),
                    LapuMasterID = table.Column<int>(type: "int", nullable: false),
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false),
                    LapuNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuTransactionDetails", x => x.LapuTransactionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "LastChangeUserDetailss",
                columns: table => new
                {
                    LastChangeUserDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false),
                    Change = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldChanges = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewChanges = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastChangeUserDetailss", x => x.LastChangeUserDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "LeadGenerationMasters",
                columns: table => new
                {
                    LeadGenerationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FormID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PanNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadGenerationMasters", x => x.LeadGenerationMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LeadGenerationSettings",
                columns: table => new
                {
                    LeadGenerationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadGenerationSettings", x => x.LeadGenerationSettingID);
                });

            migrationBuilder.CreateTable(
                name: "LoanTypeMasters",
                columns: table => new
                {
                    LoanTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanTypeMasters", x => x.LoanTypeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LocationLookUps",
                columns: table => new
                {
                    LocationLookUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableID = table.Column<int>(type: "int", nullable: false),
                    SimCardDetails = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(150)", nullable: false),
                    Location = table.Column<string>(type: "varchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(max)", nullable: false),
                    Latitude = table.Column<string>(type: "varchar(max)", nullable: false),
                    Longitude = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationLookUps", x => x.LocationLookUpID);
                });

            migrationBuilder.CreateTable(
                name: "LocationPinCodeMasters",
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
                    EMPLOYEE_CODE = table.Column<string>(type: "varchar(100)", nullable: false),
                    EMPLOYEE_NAME = table.Column<string>(type: "varchar(100)", nullable: false),
                    EMPLOYEE_MOBILE = table.Column<string>(type: "varchar(100)", nullable: false),
                    EMPLOYEE_EMAIL = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_5 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_6 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_7 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_8 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_9 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_10 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_11 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_12 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_13 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_14 = table.Column<string>(type: "varchar(100)", nullable: false),
                    RESERVEFIELD_15 = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPinCodeMasters", x => x.SR_NO);
                });

            migrationBuilder.CreateTable(
                name: "MediaMasters",
                columns: table => new
                {
                    MediaMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MediaType = table.Column<string>(type: "varchar(100)", nullable: false),
                    MediaPath = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaMasters", x => x.MediaMasterID);
                });

            migrationBuilder.CreateTable(
                name: "MenuActionMappings",
                columns: table => new
                {
                    MenuActionMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuMasterGUID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuActionMappings", x => x.MenuActionMappingID);
                });

            migrationBuilder.CreateTable(
                name: "MenuItem",
                columns: table => new
                {
                    MenuItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MappingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItem", x => x.MenuItemID);
                });

            migrationBuilder.CreateTable(
                name: "MenuMasters",
                columns: table => new
                {
                    MenuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuSequence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Url = table.Column<string>(type: "varchar(100)", nullable: false),
                    Icon = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsSuperDistributer = table.Column<bool>(type: "bit", nullable: false),
                    IsDistributer = table.Column<bool>(type: "bit", nullable: false),
                    IsRetailer = table.Column<bool>(type: "bit", nullable: false),
                    IsAPIUser = table.Column<bool>(type: "bit", nullable: false),
                    IsSupport = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    GUID = table.Column<string>(type: "varchar(100)", nullable: false),
                    ParentMenuGUID = table.Column<string>(type: "varchar(100)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuMasters", x => x.MenuMasterID);
                });

            migrationBuilder.CreateTable(
                name: "MessageSettings",
                columns: table => new
                {
                    MessageSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsSMS = table.Column<bool>(type: "bit", nullable: false),
                    IsMail = table.Column<bool>(type: "bit", nullable: false),
                    IsWhatsUp = table.Column<bool>(type: "bit", nullable: false),
                    WhatsUpMessage = table.Column<string>(type: "varchar(max)", nullable: false),
                    SmsMessage = table.Column<string>(type: "varchar(500)", nullable: false),
                    EmailMessage = table.Column<string>(type: "varchar(max)", nullable: false),
                    TemplateID = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsOTP = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSettings", x => x.MessageSettingID);
                });

            migrationBuilder.CreateTable(
                name: "MicroATMSettings",
                columns: table => new
                {
                    MicroATMSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicroATMSettings", x => x.MicroATMSettingID);
                });

            migrationBuilder.CreateTable(
                name: "MPinHistorys",
                columns: table => new
                {
                    MPinHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mpin = table.Column<int>(type: "int", nullable: false),
                    DeviceID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MPinHistorys", x => x.MPinHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationText = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    NotificationImagePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                });

            migrationBuilder.CreateTable(
                name: "NPCIBanks",
                columns: table => new
                {
                    NPCIBankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    BankName = table.Column<string>(type: "varchar(200)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(50)", nullable: true),
                    IIN = table.Column<string>(type: "varchar(50)", nullable: true),
                    ParentBank = table.Column<string>(type: "varchar(200)", nullable: true),
                    BankType = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NPCIBanks", x => x.NPCIBankID);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingUsers",
                columns: table => new
                {
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PanNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AadhaarNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PinCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminRemark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportRemark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlabID = table.Column<int>(type: "int", nullable: false),
                    AgentID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUsers", x => x.OnboardingUserID);
                });

            migrationBuilder.CreateTable(
                name: "OperatorAmountWiseSettings",
                columns: table => new
                {
                    OperatorAmountWiseSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SerialCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorAmountWiseSettings", x => x.OperatorAmountWiseSettingID);
                });

            migrationBuilder.CreateTable(
                name: "OperatorwiseCallbackSettings",
                columns: table => new
                {
                    OperatorwiseCallbackSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(100)", nullable: true),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateFormat = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorwiseCallbackSettings", x => x.OperatorwiseCallbackSettingID);
                });

            migrationBuilder.CreateTable(
                name: "OTPMasters",
                columns: table => new
                {
                    OTPMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPMasters", x => x.OTPMasterID);
                });

            migrationBuilder.CreateTable(
                name: "OTTSettings",
                columns: table => new
                {
                    OTTSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTTSettings", x => x.OTTSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PanCardSettings",
                columns: table => new
                {
                    PanCardSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanCardSettings", x => x.PanCardSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGatewayLogs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    GatewayName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Request = table.Column<string>(type: "varchar(150)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: false),
                    OrderID = table.Column<string>(type: "varchar(150)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGatewayLogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGatewaySettings",
                columns: table => new
                {
                    PaymentGatewaySettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    SuccessURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    ActionURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    FailURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsUPI = table.Column<bool>(type: "bit", nullable: false),
                    IsManualStatusChange = table.Column<bool>(type: "bit", nullable: false),
                    SettlementHoldInMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGatewaySettings", x => x.PaymentGatewaySettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentModeMasters",
                columns: table => new
                {
                    PaymentModeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentModeMasters", x => x.PaymentModeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "PayoutBeneficiaries",
                columns: table => new
                {
                    PayoutBeneficaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountHolderName = table.Column<string>(type: "varchar(200)", nullable: false),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    AccountNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(20)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(max)", nullable: false),
                    Document = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBeneficiaries", x => x.PayoutBeneficaryID);
                });

            migrationBuilder.CreateTable(
                name: "PayoutSettings",
                columns: table => new
                {
                    PayoutSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    ActionURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutSettings", x => x.PayoutSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaySprintBanks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaySprintBanks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PlanSettings",
                columns: table => new
                {
                    PlanSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    UserID = table.Column<string>(type: "varchar(500)", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Circle = table.Column<string>(type: "varchar(200)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanSettings", x => x.PlanSettingID);
                });

            migrationBuilder.CreateTable(
                name: "POSSettingMasters",
                columns: table => new
                {
                    POSSettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSSettingMasters", x => x.POSSettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCallBackLogs",
                columns: table => new
                {
                    ProviderCallBackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderReply = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCallBackLogs", x => x.ProviderCallBackID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderMasters",
                columns: table => new
                {
                    ProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "varchar(150)", nullable: false),
                    ProviderUrl = table.Column<string>(type: "varchar(max)", nullable: false),
                    ProviderUniqueID = table.Column<string>(type: "varchar(150)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsHLR = table.Column<bool>(type: "bit", nullable: false),
                    RechargeBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMTBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ResponseGroupID = table.Column<int>(type: "int", nullable: false),
                    IsRealAPI = table.Column<bool>(type: "bit", nullable: false),
                    LoginID = table.Column<string>(type: "varchar(200)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    SupportNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    LowBalanceAmount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderMasters", x => x.ProviderID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorMasters",
                columns: table => new
                {
                    ProviderOperatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Operatorcode = table.Column<string>(type: "varchar(150)", nullable: true),
                    MarginType = table.Column<string>(type: "varchar(50)", nullable: true),
                    CommissionAdmin = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOperatorMasters", x => x.ProviderOperatorID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderResponseTypeMasters",
                columns: table => new
                {
                    ProviderResponseTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(200)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderResponseTypeMasters", x => x.ProviderResponseTypeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "PureDMTTransferBeneDetails",
                columns: table => new
                {
                    PureDMTTransferBeneDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    BeneID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerifyBeneName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerify = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PureDMTTransferBeneDetails", x => x.PureDMTTransferBeneDetailID);
                });

            migrationBuilder.CreateTable(
                name: "RedirectUrlMasters",
                columns: table => new
                {
                    RedirectUrlMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    RedirectUrl = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUrlMasters", x => x.RedirectUrlMasterID);
                });

            migrationBuilder.CreateTable(
                name: "RemainderOperatorAmountWises",
                columns: table => new
                {
                    RemainderOperatorAmountWiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemainderOperatorAmountWises", x => x.RemainderOperatorAmountWiseID);
                });

            migrationBuilder.CreateTable(
                name: "RichTextMedias",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: false),
                    Content = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RichTextMedias", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SDKSettings",
                columns: table => new
                {
                    SDKSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDKSettings", x => x.SDKSettingID);
                });

            migrationBuilder.CreateTable(
                name: "SentSMS",
                columns: table => new
                {
                    SentSMSID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", nullable: false),
                    Message = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: false),
                    SMSMasterID = table.Column<int>(type: "int", nullable: false),
                    TemplateID = table.Column<string>(type: "varchar(100)", nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    Request = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentSMS", x => x.SentSMSID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAndUserwiseSubscriptions",
                columns: table => new
                {
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    SubscriptionType = table.Column<string>(type: "varchar(10)", nullable: false),
                    OriginalCharge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "varchar(max)", nullable: false),
                    LoginID = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsAutoDebit = table.Column<bool>(type: "bit", nullable: false),
                    ServiceSubscriptionPlanMasterID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(max)", nullable: false),
                    DaysCount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAndUserwiseSubscriptions", x => x.ServiceAndUserwiseSubscriptionID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMasters",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    ServiceCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    HSNCode = table.Column<string>(type: "varchar(200)", nullable: false),
                    HSNCodeForSelfInvoice = table.Column<string>(type: "varchar(100)", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDisplay = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsOutside = table.Column<bool>(type: "bit", nullable: false),
                    IsBBPS = table.Column<bool>(type: "bit", nullable: true),
                    IsSubscriptionAllow = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMasters", x => x.ServiceID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionCreatePlanMasters",
                columns: table => new
                {
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionCreatePlanMasters", x => x.ServiceSubscriptionCreatePlanMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionPlanMasters",
                columns: table => new
                {
                    ServiceSubscriptionPlanMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionPlanMasters", x => x.ServiceSubscriptionPlanMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SettingMasters",
                columns: table => new
                {
                    SettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsOn = table.Column<bool>(type: "bit", nullable: false),
                    IsSecure = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsPassword = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingMasters", x => x.SettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SignUps",
                columns: table => new
                {
                    SignUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PANNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PinCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignUps", x => x.SignUpID);
                });

            migrationBuilder.CreateTable(
                name: "SlabMasters",
                columns: table => new
                {
                    SlabMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabName = table.Column<string>(type: "varchar(80)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlabMasters", x => x.SlabMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SliderMasters",
                columns: table => new
                {
                    SliderMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SliderPath = table.Column<string>(type: "varchar(max)", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderMasters", x => x.SliderMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SmsMasters",
                columns: table => new
                {
                    SMSMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SMSString = table.Column<string>(type: "varchar(max)", nullable: false),
                    BalanceURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    WhatsAppURL = table.Column<string>(type: "varchar(1000)", nullable: false),
                    APIName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    MethodTypeWhatsApp = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJsonWhatsApp = table.Column<string>(type: "varchar(max)", nullable: false),
                    MobileWithOrWithout91 = table.Column<string>(type: "varchar(30)", nullable: false),
                    SMSOTPURL = table.Column<string>(type: "varchar(1000)", nullable: false),
                    IsOTP = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMasters", x => x.SMSMasterID);
                });

            migrationBuilder.CreateTable(
                name: "StateMasters",
                columns: table => new
                {
                    StateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateName = table.Column<string>(type: "varchar(100)", nullable: false),
                    StateCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMasters", x => x.StateID);
                });

            migrationBuilder.CreateTable(
                name: "SubMenuItem",
                columns: table => new
                {
                    MenuItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MappingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubMappingCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubMenuItem", x => x.MenuItemID);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionMasters",
                columns: table => new
                {
                    SubscriptionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOCInt = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionMasters", x => x.SubscriptionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SupportUserAccessMasters",
                columns: table => new
                {
                    SupportUserAccessMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MenuGUID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportUserAccessMasters", x => x.SupportUserAccessMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyServiceProviders",
                columns: table => new
                {
                    ThirdPartyServiceProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedServices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyServiceProviders", x => x.ThirdPartyServiceProviderID);
                });

            migrationBuilder.CreateTable(
                name: "TicketCategories",
                columns: table => new
                {
                    TicketCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCategories", x => x.TicketCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionAPIUserRequestMasters",
                columns: table => new
                {
                    TransactionAPIUserRequestMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ApiUserRequestID = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAPIUserRequestMasters", x => x.TransactionAPIUserRequestMasterID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDetailss",
                columns: table => new
                {
                    TransactionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ReferenceID = table.Column<int>(type: "int", nullable: false),
                    AssignedUserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    DOCType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountCreditDebit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommissionCreditDebit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDetailss", x => x.TransactionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "UISettingMasters",
                columns: table => new
                {
                    UISettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(200)", nullable: false),
                    Value = table.Column<string>(type: "varchar(500)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UISettingMasters", x => x.UISettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "UnitMasters",
                columns: table => new
                {
                    UnitMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMasters", x => x.UnitMasterID);
                });

            migrationBuilder.CreateTable(
                name: "UPITransferSettings",
                columns: table => new
                {
                    UPITransferSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CallBackURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AuthCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UPITransferSettings", x => x.UPITransferSettingID);
                });

            migrationBuilder.CreateTable(
                name: "UrlTypeMasters",
                columns: table => new
                {
                    UrlTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlTypeMasters", x => x.UrlTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserBalanceHistories",
                columns: table => new
                {
                    UserBalanceHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalanceHistories", x => x.UserBalanceHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "UserPaymentCollectMappings",
                columns: table => new
                {
                    UserPaymentCollectMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentCollectMappings", x => x.UserPaymentCollectMappingID);
                });

            migrationBuilder.CreateTable(
                name: "UserPlanUtilizations",
                columns: table => new
                {
                    UserPlanUtilizationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CircleFetchCount = table.Column<int>(type: "int", nullable: false),
                    PlanFetchCount = table.Column<int>(type: "int", nullable: false),
                    BillFetchCount = table.Column<int>(type: "int", nullable: false),
                    TotalPlanCount = table.Column<int>(type: "int", nullable: false),
                    TotalCircleCount = table.Column<int>(type: "int", nullable: false),
                    TotalBillFetchCount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlanUtilizations", x => x.UserPlanUtilizationID);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceCharges",
                columns: table => new
                {
                    UserServiceChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceCharges", x => x.UserServiceChargeID);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceSubscriptionCharges",
                columns: table => new
                {
                    UserServiceSubscriptionChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsAdminApproved = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceSubscriptionCharges", x => x.UserServiceSubscriptionChargeID);
                });

            migrationBuilder.CreateTable(
                name: "UserTypeMasters",
                columns: table => new
                {
                    UserTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserType = table.Column<string>(type: "varchar(100)", nullable: false),
                    AliasName = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypeMasters", x => x.UserTypeID);
                });

            migrationBuilder.CreateTable(
                name: "VerificationSettings",
                columns: table => new
                {
                    VerificationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CallBackURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AuthCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationSettings", x => x.VerificationSettingID);
                });

            migrationBuilder.CreateTable(
                name: "VirtualAccountMasters",
                columns: table => new
                {
                    VirtualAccountMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualAccountMasters", x => x.VirtualAccountMasterID);
                });

            migrationBuilder.CreateTable(
                name: "VirtualBalances",
                columns: table => new
                {
                    VirtualBalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    DOCType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualBalances", x => x.VirtualBalanceID);
                });

            migrationBuilder.CreateTable(
                name: "VoucherTypes",
                columns: table => new
                {
                    VoucherTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeName = table.Column<string>(type: "varchar(30)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherTypes", x => x.VoucherTypeID);
                });

            migrationBuilder.CreateTable(
                name: "WalletMaster",
                columns: table => new
                {
                    WalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    UISequence = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletMaster", x => x.WalletID);
                });

            migrationBuilder.CreateTable(
                name: "WebHookLogs",
                columns: table => new
                {
                    WebHookLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookLogs", x => x.WebHookLogID);
                });

            migrationBuilder.CreateTable(
                name: "WebHookSettings",
                columns: table => new
                {
                    WebHookSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    URL = table.Column<string>(type: "varchar(max)", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookSettings", x => x.WebHookSettingID);
                });

            migrationBuilder.CreateTable(
                name: "WrongPasswordHistorys",
                columns: table => new
                {
                    WrongPasswordHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WrongPasswordHistorys", x => x.WrongPasswordHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "WrongPasswordLogs",
                columns: table => new
                {
                    WrongPasswordLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WrongPassword = table.Column<string>(type: "varchar(500)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: false),
                    LogAction = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WrongPasswordLogs", x => x.WrongPasswordLogID);
                });

            migrationBuilder.CreateTable(
                name: "ZoneMasters",
                columns: table => new
                {
                    ZoneMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(300)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneMasters", x => x.ZoneMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SubCategorys",
                columns: table => new
                {
                    SubCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LongDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategorys", x => x.SubCategoryID);
                    table.ForeignKey(
                        name: "FK_SubCategorys_Categorys_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categorys",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryChannelMappings",
                columns: table => new
                {
                    CategoryChannelMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelCategoryID = table.Column<int>(type: "int", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryChannelMappings", x => x.CategoryChannelMappingID);
                    table.ForeignKey(
                        name: "FK_CategoryChannelMappings_ChannelCategorys_ChannelCategoryID",
                        column: x => x.ChannelCategoryID,
                        principalTable: "ChannelCategorys",
                        principalColumn: "ChannelCategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingUserDocuments",
                columns: table => new
                {
                    OnboardingUserDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ref_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OldImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUserDocuments", x => x.OnboardingUserDocumentID);
                    table.ForeignKey(
                        name: "FK_OnboardingUserDocuments_OnboardingUsers_OnboardingUserID",
                        column: x => x.OnboardingUserID,
                        principalTable: "OnboardingUsers",
                        principalColumn: "OnboardingUserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutVendorBeneficiaryMappings",
                columns: table => new
                {
                    PayoutVendorBeneficiaryMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    PayoutVendorBeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: false),
                    PayoutBeneficaryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutVendorBeneficiaryMappings", x => x.PayoutVendorBeneficiaryMappingID);
                    table.ForeignKey(
                        name: "FK_PayoutVendorBeneficiaryMappings_PayoutBeneficiaries_PayoutBeneficaryID",
                        column: x => x.PayoutBeneficaryID,
                        principalTable: "PayoutBeneficiaries",
                        principalColumn: "PayoutBeneficaryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderResponseMasters",
                columns: table => new
                {
                    ProviderResponseMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    TextMustexits = table.Column<string>(type: "varchar(250)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    Response = table.Column<string>(type: "varchar(1000)", nullable: false),
                    IsReference = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderResponseMasters", x => x.ProviderResponseMasterID);
                    table.ForeignKey(
                        name: "FK_ProviderResponseMasters_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderStateConfigurations",
                columns: table => new
                {
                    ProviderStateConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    StateCode = table.Column<string>(type: "varchar(30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderStateConfigurations", x => x.ProviderStateConfigurationID);
                    table.ForeignKey(
                        name: "FK_ProviderStateConfigurations_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationSettings",
                columns: table => new
                {
                    ReconciliationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    Amount = table.Column<string>(type: "varchar(100)", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", nullable: false),
                    WorksheetName = table.Column<string>(type: "varchar(100)", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: false),
                    SuccessStatusAs = table.Column<string>(type: "varchar(100)", nullable: false),
                    FailureStatusAs = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationSettings", x => x.ReconciliationSettingID);
                    table.ForeignKey(
                        name: "FK_ReconciliationSettings_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorMasters",
                columns: table => new
                {
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MinimumRechargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumRechargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RepeatRechargeInMinute = table.Column<int>(type: "int", nullable: false),
                    BlockedTill = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatorImagePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    Opcode = table.Column<string>(type: "varchar(50)", nullable: false),
                    BusinessModel = table.Column<string>(type: "varchar(50)", nullable: false),
                    NumberLength = table.Column<int>(type: "int", nullable: false),
                    HSNCode = table.Column<string>(type: "varchar(max)", nullable: false),
                    Prefix = table.Column<string>(type: "varchar(max)", nullable: false),
                    AccountStartWith = table.Column<string>(type: "varchar(max)", nullable: false),
                    SequenceLogic = table.Column<string>(type: "varchar(50)", nullable: false),
                    MinLength = table.Column<int>(type: "int", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: false),
                    AccountName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: false),
                    TollFreeNumber = table.Column<string>(type: "varchar(max)", nullable: false),
                    UISequence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlockedAmount = table.Column<string>(type: "varchar(max)", nullable: false),
                    AllowedFor = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    IsTimelyBlocked = table.Column<bool>(type: "bit", nullable: true),
                    IsRandom = table.Column<bool>(type: "bit", nullable: true),
                    IsBBPS = table.Column<bool>(type: "bit", nullable: true),
                    IsBillFetch = table.Column<bool>(type: "bit", nullable: false),
                    IsPartialBill = table.Column<bool>(type: "bit", nullable: false),
                    IsViewRequired = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorMasters", x => x.OperatorID);
                    table.ForeignKey(
                        name: "FK_OperatorMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderDetails",
                columns: table => new
                {
                    ProviderDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    UrlType = table.Column<string>(type: "varchar(100)", nullable: false),
                    ProviderURL = table.Column<string>(type: "varchar(max)", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateFormat = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TransactionIDPrefix = table.Column<string>(type: "varchar(20)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderDetails", x => x.ProviderDetailID);
                    table.ForeignKey(
                        name: "FK_ProviderDetails_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderDetails_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceWiseSliderSettings",
                columns: table => new
                {
                    ServiceWiseSliderSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    ReferenceLink = table.Column<string>(type: "varchar(500)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceWiseSliderSettings", x => x.ServiceWiseSliderSettingID);
                    table.ForeignKey(
                        name: "FK_ServiceWiseSliderSettings_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionPlanDetailss",
                columns: table => new
                {
                    ServiceSubscriptionPlanDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionPlanDetailss", x => x.ServiceSubscriptionPlanDetailsID);
                    table.ForeignKey(
                        name: "FK_ServiceSubscriptionPlanDetailss_ServiceSubscriptionCreatePlanMasters_ServiceSubscriptionCreatePlanMasterID",
                        column: x => x.ServiceSubscriptionCreatePlanMasterID,
                        principalTable: "ServiceSubscriptionCreatePlanMasters",
                        principalColumn: "ServiceSubscriptionCreatePlanMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationMasters",
                columns: table => new
                {
                    AuthorizationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationMasters", x => x.AuthorizationMasterID);
                    table.ForeignKey(
                        name: "FK_AuthorizationMasters_UserTypeMasters_UserTypeID",
                        column: x => x.UserTypeID,
                        principalTable: "UserTypeMasters",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMasters",
                columns: table => new
                {
                    UserMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: true),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(200)", nullable: false),
                    FirmName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(200)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(70)", nullable: false),
                    GSTNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    PANNo = table.Column<string>(type: "varchar(10)", nullable: false),
                    CIN = table.Column<string>(type: "varchar(20)", nullable: false),
                    AadharNo = table.Column<string>(type: "varchar(16)", nullable: false),
                    Address = table.Column<string>(type: "varchar(max)", nullable: false),
                    Mpin = table.Column<int>(type: "int", nullable: false),
                    UniqueReferenceNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsKYCApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsLoginOTP = table.Column<bool>(type: "bit", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: true),
                    APIToken = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProfileImagePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    VirtualAccountNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsInvoiceApplicable = table.Column<bool>(type: "bit", nullable: false),
                    JoiningByUserID = table.Column<int>(type: "int", nullable: true),
                    RegistrationType = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsTwoFA = table.Column<bool>(type: "bit", nullable: false),
                    CityName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Pincode = table.Column<string>(type: "varchar(100)", nullable: false),
                    WalletIDforEcollection = table.Column<int>(type: "int", nullable: true),
                    StaticQRWalletID = table.Column<int>(type: "int", nullable: false),
                    AliasName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsPin = table.Column<bool>(type: "bit", nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: false),
                    ReferralCode = table.Column<string>(type: "varchar(5)", nullable: false),
                    ReferralAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ReferralBenefitMinAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IseKYC = table.Column<bool>(type: "bit", nullable: false),
                    IsCommissionHold = table.Column<bool>(type: "bit", nullable: false),
                    IsInitialPassword = table.Column<bool>(type: "bit", nullable: false),
                    ShopAddress = table.Column<string>(type: "varchar(max)", nullable: false),
                    BusinessGSTStateCode = table.Column<string>(type: "varchar(150)", nullable: false),
                    PartnerConfigJson = table.Column<string>(type: "varchar(200)", nullable: false),
                    AppFCMToken = table.Column<string>(type: "varchar(500)", nullable: false),
                    WebFCMToken = table.Column<string>(type: "varchar(500)", nullable: false),
                    NPCIAgentID = table.Column<string>(type: "varchar(100)", nullable: false),
                    PaySwiffMerchantId = table.Column<string>(type: "varchar(100)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMasters", x => x.UserMasterID);
                    table.ForeignKey(
                        name: "FK_UserMasters_StateMasters_StateID",
                        column: x => x.StateID,
                        principalTable: "StateMasters",
                        principalColumn: "StateID");
                    table.ForeignKey(
                        name: "FK_UserMasters_UserTypeMasters_UserTypeID",
                        column: x => x.UserTypeID,
                        principalTable: "UserTypeMasters",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoucherMasters",
                columns: table => new
                {
                    VoucherMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeID = table.Column<int>(type: "int", nullable: false),
                    SelectDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CrGroup = table.Column<string>(type: "varchar(70)", nullable: true),
                    CrName = table.Column<string>(type: "varchar(70)", nullable: true),
                    DrGroup = table.Column<string>(type: "varchar(70)", nullable: true),
                    DrName = table.Column<string>(type: "varchar(70)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(70)", nullable: true),
                    Status = table.Column<string>(type: "varchar(70)", nullable: true),
                    SubmitedBy = table.Column<int>(type: "int", nullable: false),
                    ApproveBy = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Is_Delete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherMasters", x => x.VoucherMasterID);
                    table.ForeignKey(
                        name: "FK_VoucherMasters_VoucherTypes_VoucherTypeID",
                        column: x => x.VoucherTypeID,
                        principalTable: "VoucherTypes",
                        principalColumn: "VoucherTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderPurchases",
                columns: table => new
                {
                    ProviderPurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseDOCInt = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderPurchases", x => x.ProviderPurchaseID);
                    table.ForeignKey(
                        name: "FK_ProviderPurchases_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderPurchases_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletServiceMappings",
                columns: table => new
                {
                    WalletServiceMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletServiceMappings", x => x.WalletServiceMappingID);
                    table.ForeignKey(
                        name: "FK_WalletServiceMappings_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletServiceMappings_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BBPSConfigurations",
                columns: table => new
                {
                    BBPSConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlaceHolder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMeeage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    ConfigList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDisplay = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BBPSConfigurations", x => x.BBPSConfigurationID);
                    table.ForeignKey(
                        name: "FK_BBPSConfigurations_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSlabDetails",
                columns: table => new
                {
                    ChannelSlabDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CommissionType = table.Column<string>(type: "varchar(100)", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    RealCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealCommissionType = table.Column<string>(type: "varchar(100)", nullable: false),
                    GeneralCommissionCategory = table.Column<string>(type: "varchar(50)", nullable: false),
                    RealCommissionCategory = table.Column<string>(type: "varchar(50)", nullable: false),
                    UplineCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSlabDetails", x => x.ChannelSlabDetailID);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetails_ChannelSlabMasters_ChannelSlabMasterID",
                        column: x => x.ChannelSlabMasterID,
                        principalTable: "ChannelSlabMasters",
                        principalColumn: "ChannelSlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetails_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetails_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseCommissions",
                columns: table => new
                {
                    CircleWiseCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    RetailerCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    DistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributorCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    DistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    SuperDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributorCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    SuperDistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    ClientComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClientCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    ClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleWiseCommissions", x => x.CircleWiseCommissionID);
                    table.ForeignKey(
                        name: "FK_CircleWiseCommissions_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseCommissions_SlabMasters_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMasters",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LapuMasters",
                columns: table => new
                {
                    LapuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    LapuNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    ProivderLapuID = table.Column<string>(type: "varchar(50)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuMasters", x => x.LapuMasterID);
                    table.ForeignKey(
                        name: "FK_LapuMasters_LapuPartyMasters_LapuPartyMasterID",
                        column: x => x.LapuPartyMasterID,
                        principalTable: "LapuPartyMasters",
                        principalColumn: "LapuPartyMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LapuMasters_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorSequences",
                columns: table => new
                {
                    OperatorSequenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSequences", x => x.OperatorSequenceID);
                    table.ForeignKey(
                        name: "FK_OperatorSequences_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequences_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequences_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorStateMappings",
                columns: table => new
                {
                    OperatorStateMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ZoneMasterID = table.Column<int>(type: "int", nullable: false),
                    HLR = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorStateMappings", x => x.OperatorStateMappingID);
                    table.ForeignKey(
                        name: "FK_OperatorStateMappings_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderConfigurationAdminCommissions",
                columns: table => new
                {
                    ProviderConfigurationAdminCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminCommType = table.Column<string>(type: "varchar(20)", nullable: false),
                    AdminCommissionCategory = table.Column<string>(type: "varchar(20)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfigurationAdminCommissions", x => x.ProviderConfigurationAdminCommissionID);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommissions_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommissions_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommissions_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderConfigurations",
                columns: table => new
                {
                    ConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OpCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", nullable: false),
                    Lapu = table.Column<string>(type: "varchar(200)", nullable: false),
                    Quantity = table.Column<string>(type: "varchar(3000)", nullable: false),
                    AdminComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminCommType = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CommissionCategory = table.Column<string>(type: "varchar(20)", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    PendingInMinute = table.Column<int>(type: "int", nullable: false),
                    FailCount = table.Column<int>(type: "int", nullable: false),
                    FailInMinute = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfigurations", x => x.ConfigurationID);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurations_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurations_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurations_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorCircleMasters",
                columns: table => new
                {
                    ProviderOperatorCircleMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    Opcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOperatorCircleMasters", x => x.ProviderOperatorCircleMasterID);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorCircleMasters_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorCircleMasters_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorDenominationMappings",
                columns: table => new
                {
                    ProviderOperatorDenominationMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    DenominationMasterID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOperatorDenominationMappings", x => x.ProviderOperatorDenominationMappingID);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMappings_DenominationMasters_DenominationMasterID",
                        column: x => x.DenominationMasterID,
                        principalTable: "DenominationMasters",
                        principalColumn: "DenominationMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMappings_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMappings_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RangeWiseSequences",
                columns: table => new
                {
                    RangeWiseSequenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangeWiseSequences", x => x.RangeWiseSequenceID);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequences_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequences_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequences_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RealProviderMappings",
                columns: table => new
                {
                    RealProviderMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealProviderMappings", x => x.RealProviderMappingID);
                    table.ForeignKey(
                        name: "FK_RealProviderMappings_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ROfferCommissions",
                columns: table => new
                {
                    ROfferCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommType = table.Column<string>(type: "varchar(500)", nullable: false),
                    RetailerCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: false),
                    DistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributorCommType = table.Column<string>(type: "varchar(500)", nullable: false),
                    DistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: false),
                    SuperDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributorCommType = table.Column<string>(type: "varchar(500)", nullable: false),
                    SuperDistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: false),
                    ClientComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClientCommType = table.Column<string>(type: "varchar(500)", nullable: false),
                    ClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROfferCommissions", x => x.ROfferCommissionID);
                    table.ForeignKey(
                        name: "FK_ROfferCommissions_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ROfferCommissions_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ROfferCommissions_SlabMasters_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMasters",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlabDetails",
                columns: table => new
                {
                    SlabDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RetailerCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    DistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributorCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    DistributorCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    SuperDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributorCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    SuperDistributorCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    ClientComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClientCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    ClientCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealRetailerComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealRetailerCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealRetailerCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealDistributorCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealDistributorCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealSuperDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealSuperDistributorCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealSuperDistributorCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealClientComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealClientCommType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RealClientCommissionCategory = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlabDetails", x => x.SlabDetailID);
                    table.ForeignKey(
                        name: "FK_SlabDetails_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlabDetails_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlabDetails_SlabMasters_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMasters",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APIUserCallbackRequestResponseLogs",
                columns: table => new
                {
                    APIUserCallbackRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Request = table.Column<string>(type: "varchar(max)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIUserCallbackRequestResponseLogs", x => x.APIUserCallbackRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_APIUserCallbackRequestResponseLogs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APIUserInitialRequestResponseLogs",
                columns: table => new
                {
                    APIUserInitialRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Request = table.Column<string>(type: "varchar(max)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIUserInitialRequestResponseLogs", x => x.APIUserInitialRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_APIUserInitialRequestResponseLogs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutoTransferUserwiseMappings",
                columns: table => new
                {
                    AutoTransferUserwiseMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OTP = table.Column<string>(type: "varchar(10)", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    MaxOutstanding = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RechargeLowBalanceReached = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RechargeAmountIncreasedBy = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMTLowBalanceReached = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMTAmountIncreasedBy = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoTransferUserwiseMappings", x => x.AutoTransferUserwiseMappingID);
                    table.ForeignKey(
                        name: "FK_AutoTransferUserwiseMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankRequestResponseLogs",
                columns: table => new
                {
                    BankRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    TransactionID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Through = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankRequestResponseLogs", x => x.BankRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_BankRequestResponseLogs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallbackMasters",
                columns: table => new
                {
                    CallbackMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CallbackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallbackMasters", x => x.CallbackMasterID);
                    table.ForeignKey(
                        name: "FK_CallbackMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallbackMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallMeRequests",
                columns: table => new
                {
                    CallMeRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CallMeCategoryMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    UplineRemark = table.Column<string>(type: "varchar(200)", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallMeRequests", x => x.CallMeRequestID);
                    table.ForeignKey(
                        name: "FK_CallMeRequests_CallMeCategoryMaster_CallMeCategoryMasterID",
                        column: x => x.CallMeCategoryMasterID,
                        principalTable: "CallMeCategoryMaster",
                        principalColumn: "CallMeCategoryMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallMeRequests_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashCollectionMasters",
                columns: table => new
                {
                    CashCollectionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Outstanding = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaxOutstanding = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCollectionMasters", x => x.CashCollectionMasterID);
                    table.ForeignKey(
                        name: "FK_CashCollectionMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseOperatorLimits",
                columns: table => new
                {
                    CircleWiseOperatorLimitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    AmountLimit = table.Column<decimal>(type: "decimal(18,5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleWiseOperatorLimits", x => x.CircleWiseOperatorLimitID);
                    table.ForeignKey(
                        name: "FK_CircleWiseOperatorLimits_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseOperatorLimits_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseUserWiseOperatorOffs",
                columns: table => new
                {
                    CircleWiseUserWiseOperatorOffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleWiseUserWiseOperatorOffs", x => x.CircleWiseUserWiseOperatorOffID);
                    table.ForeignKey(
                        name: "FK_CircleWiseUserWiseOperatorOffs_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseUserWiseOperatorOffs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintMasters",
                columns: table => new
                {
                    ComplaintMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CustomerRemark = table.Column<string>(type: "varchar(max)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(max)", nullable: false),
                    ComplaintStatus = table.Column<string>(type: "varchar(100)", nullable: false),
                    TransactionMasterDOCInt = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComplaintType = table.Column<string>(type: "varchar(100)", nullable: true),
                    NewConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "varchar(100)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "varchar(100)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintMasters", x => x.ComplaintMasterID);
                    table.ForeignKey(
                        name: "FK_ComplaintMasters_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyAuthenticationMasters",
                columns: table => new
                {
                    AuthenticationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Pipe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAuthenticationMasters", x => x.AuthenticationID);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMasters_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DMTSenders",
                columns: table => new
                {
                    SenderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsKYC = table.Column<bool>(type: "bit", nullable: false),
                    KYCDocumentURL = table.Column<string>(type: "varchar(2000)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: false),
                    Pincode = table.Column<string>(type: "varchar(10)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTSenders", x => x.SenderID);
                    table.ForeignKey(
                        name: "FK_DMTSenders_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EcollectionDetails",
                columns: table => new
                {
                    EcollectionDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    VirtualACCode = table.Column<string>(type: "varchar(30)", nullable: false),
                    UTR = table.Column<string>(type: "varchar(50)", nullable: false),
                    PayeeAccountNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcollectionDetails", x => x.EcollectionDetailsID);
                    table.ForeignKey(
                        name: "FK_EcollectionDetails_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eKYCLogs",
                columns: table => new
                {
                    eKYCLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCLogs", x => x.eKYCLogID);
                    table.ForeignKey(
                        name: "FK_eKYCLogs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "eKYCMasters",
                columns: table => new
                {
                    eKYCMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTPGenerateHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OTPGenerateRequest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTPGenerateResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerifyRequest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VerifyResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTPToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KYCToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    wadh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCMasters", x => x.eKYCMasterID);
                    table.ForeignKey(
                        name: "FK_eKYCMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundTransactions",
                columns: table => new
                {
                    FundTransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    ReceiverID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CashCollectionAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SenderOpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SenderClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ReceiverOpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ReceiverClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: false),
                    UTRNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(100)", nullable: false),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: false),
                    Through = table.Column<string>(type: "varchar(100)", nullable: false),
                    RequestToID = table.Column<int>(type: "int", nullable: false),
                    CashCollectionRemark = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", nullable: false),
                    BankName = table.Column<string>(type: "varchar(200)", nullable: false),
                    ReceiptPath = table.Column<string>(type: "varchar(200)", nullable: false),
                    TransactionDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDOCInt = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    RequestAcceptedUserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransactions", x => x.FundTransactionID);
                    table.ForeignKey(
                        name: "FK_FundTransactions_UserMasters_ReceiverID",
                        column: x => x.ReceiverID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransactions_UserMasters_RequestToID",
                        column: x => x.RequestToID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransactions_UserMasters_SenderID",
                        column: x => x.SenderID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransactions_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GSTHolds",
                columns: table => new
                {
                    GSTHoldID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    HoldAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GSTUploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptPath = table.Column<string>(type: "varchar(max)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    GSTUploadRemark = table.Column<string>(type: "varchar(max)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(max)", nullable: false),
                    GSTNo = table.Column<string>(type: "varchar(15)", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    TaxableValue = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSTHolds", x => x.GSTHoldID);
                    table.ForeignKey(
                        name: "FK_GSTHolds_StateMasters_StateID",
                        column: x => x.StateID,
                        principalTable: "StateMasters",
                        principalColumn: "StateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GSTHolds_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inboxes",
                columns: table => new
                {
                    InboxID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Status = table.Column<string>(type: "varchar(15)", nullable: false),
                    Through = table.Column<string>(type: "varchar(20)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    Param1 = table.Column<string>(type: "varchar(150)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inboxes", x => x.InboxID);
                    table.ForeignKey(
                        name: "FK_Inboxes_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inboxes_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inboxes_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberHistories",
                columns: table => new
                {
                    InvoiceNumberHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCategory = table.Column<string>(type: "varchar(50)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(16)", nullable: false),
                    InvoiceType = table.Column<string>(type: "varchar(16)", nullable: false),
                    IncrementValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberHistories", x => x.InvoiceNumberHistoryID);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberHistories_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IPMasters",
                columns: table => new
                {
                    IpMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IP = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPMasters", x => x.IpMasterID);
                    table.ForeignKey(
                        name: "FK_IPMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistories",
                columns: table => new
                {
                    LoginHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: false),
                    Through = table.Column<string>(type: "varchar(50)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoginMessage = table.Column<string>(type: "varchar(max)", nullable: false),
                    Location = table.Column<string>(type: "varchar(max)", nullable: false),
                    SimCardDetails = table.Column<string>(type: "varchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistories", x => x.LoginHistoryID);
                    table.ForeignKey(
                        name: "FK_LoginHistories_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LowBalanceConfigurations",
                columns: table => new
                {
                    LowBalanceConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    EmailID = table.Column<string>(type: "varchar(100)", nullable: true),
                    WebHookURL = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LowBalanceConfigurations", x => x.LowBalanceConfigurationID);
                    table.ForeignKey(
                        name: "FK_LowBalanceConfigurations_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LowBalanceConfigurations_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NewsMasters",
                columns: table => new
                {
                    NewsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    IsView = table.Column<bool>(type: "bit", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    HyperLink = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsMasters", x => x.NewsID);
                    table.ForeignKey(
                        name: "FK_NewsMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationalHistorys",
                columns: table => new
                {
                    OperationalHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    APIType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalHistorys", x => x.OperationalHistoryID);
                    table.ForeignKey(
                        name: "FK_OperationalHistorys_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorSequenceUserWises",
                columns: table => new
                {
                    OperatorSequenceUserWiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSequenceUserWises", x => x.OperatorSequenceUserWiseID);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWises_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWises_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWises_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWises_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorUserwises",
                columns: table => new
                {
                    OperatorUserwiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    BlockedAmount = table.Column<string>(type: "varchar(max)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorUserwises", x => x.OperatorUserwiseID);
                    table.ForeignKey(
                        name: "FK_OperatorUserwises_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorUserwises_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorUserwises_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OverdraftDetails",
                columns: table => new
                {
                    OverdraftDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    OverdraftOpening = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdraftAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverdraftClosing = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserRemark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminRemark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningWalletBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdraftDetails", x => x.OverdraftDetailID);
                    table.ForeignKey(
                        name: "FK_OverdraftDetails_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdraftDetails_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OverdraftMasters",
                columns: table => new
                {
                    OverdraftMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Overdraft = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxOverdraft = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdraftMasters", x => x.OverdraftMasterID);
                    table.ForeignKey(
                        name: "FK_OverdraftMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdraftMasters_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordChangeHistories",
                columns: table => new
                {
                    PasswordChangeHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordChangeHistories", x => x.PasswordChangeHistoryID);
                    table.ForeignKey(
                        name: "FK_PasswordChangeHistories_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGateways",
                columns: table => new
                {
                    PaymentGatewayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GatewayCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentProviderID = table.Column<int>(type: "int", nullable: false),
                    IsUpdate = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DistributerUserID = table.Column<int>(type: "int", nullable: false),
                    SuperDistributerUserID = table.Column<int>(type: "int", nullable: false),
                    DistributerCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SuperDistributerCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetailerTDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DistributerTDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SuperDistributerTDS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminGST = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateways", x => x.PaymentGatewayID);
                    table.ForeignKey(
                        name: "FK_PaymentGateways_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentGateways_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutBeneficiaryOTPs",
                columns: table => new
                {
                    PayoutBeneficiaryOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "varchar(10)", nullable: false),
                    PayoutBeneficaryID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBeneficiaryOTPs", x => x.PayoutBeneficiaryOTPID);
                    table.ForeignKey(
                        name: "FK_PayoutBeneficiaryOTPs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrepaidActivations",
                columns: table => new
                {
                    PrepaidActivationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ActivationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlternateNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidActivations", x => x.PrepaidActivationID);
                    table.ForeignKey(
                        name: "FK_PrepaidActivations_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderRequestResponseLogs",
                columns: table => new
                {
                    ProviderRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    TransactionID = table.Column<int>(type: "int", nullable: false),
                    Request = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Through = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRequestResponseLogs", x => x.ProviderRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_ProviderRequestResponseLogs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderUserMappings",
                columns: table => new
                {
                    ProviderUserMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUserMappings", x => x.ProviderUserMappingID);
                    table.ForeignKey(
                        name: "FK_ProviderUserMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Redeemcommissions",
                columns: table => new
                {
                    RedeemcommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(200)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    LastYearMonth = table.Column<string>(type: "varchar(50)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redeemcommissions", x => x.RedeemcommissionID);
                    table.ForeignKey(
                        name: "FK_Redeemcommissions_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralRegistrations",
                columns: table => new
                {
                    ReferralRegistrationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsBenefitUtilize = table.Column<bool>(type: "bit", nullable: false),
                    ReferralAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRegistrations", x => x.ReferralRegistrationID);
                    table.ForeignKey(
                        name: "FK_ReferralRegistrations_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: false),
                    JwtId = table.Column<string>(type: "varchar(500)", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: false),
                    UserAgent = table.Column<string>(type: "varchar(200)", nullable: false),
                    DeviceType = table.Column<string>(type: "varchar(50)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SDKBBPSUserMappings",
                columns: table => new
                {
                    SDKBBPSUserMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ConfirmationCallbackURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDKBBPSUserMappings", x => x.SDKBBPSUserMappingID);
                    table.ForeignKey(
                        name: "FK_SDKBBPSUserMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Senders",
                columns: table => new
                {
                    SenderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsKYC = table.Column<bool>(type: "bit", nullable: false),
                    KYCDocumentURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Senders", x => x.SenderID);
                    table.ForeignKey(
                        name: "FK_Senders_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportUserAccessDetails",
                columns: table => new
                {
                    SupportUserAccessDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuGUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportUserAccessDetails", x => x.SupportUserAccessDetailID);
                    table.ForeignKey(
                        name: "FK_SupportUserAccessDetails_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetSaleCommissions",
                columns: table => new
                {
                    TargetSaleCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    SaleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaleMonthDOC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetSaleCommissions", x => x.TargetSaleCommissionID);
                    table.ForeignKey(
                        name: "FK_TargetSaleCommissions_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TargetSaleCommissions_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketMasters",
                columns: table => new
                {
                    TicketMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketCategoryID = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadFilePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    Resolution = table.Column<string>(type: "varchar(max)", nullable: false),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ResolutionByUserID = table.Column<int>(type: "int", nullable: false),
                    UserTyepID = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMasters", x => x.TicketMasterID);
                    table.ForeignKey(
                        name: "FK_TicketMasters_TicketCategories_TicketCategoryID",
                        column: x => x.TicketCategoryID,
                        principalTable: "TicketCategories",
                        principalColumn: "TicketCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMasters_UserTypeMasters_UserTyepID",
                        column: x => x.UserTyepID,
                        principalTable: "UserTypeMasters",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionMasters",
                columns: table => new
                {
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ProviderOpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "varchar(500)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "varchar(1000)", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ProviderDOCNo = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ProviderRefNo = table.Column<string>(type: "varchar(1000)", nullable: false),
                    AdminGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Bill_CustomerMessageMobileNo = table.Column<string>(type: "varchar(10)", nullable: false),
                    RetailerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CashBack = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    HitBackCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(500)", nullable: false),
                    Bill_Optional1 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Bill_Optional2 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Bill_Optional3 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Bill_Optional4 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Bill_Optional5 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    Bill_Optional6 = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DMT_SenderID = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_SenderName = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_SenderNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_BeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_BeneficiaryName = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DMT_Bank = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_IFSC = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_BeneficaryNameByBank = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DMT_PaymentType = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_BillNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    DMT_OriginalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMT_StatusCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDispute = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "varchar(500)", nullable: false),
                    SimCardDetails = table.Column<string>(type: "varchar(500)", nullable: false),
                    Through = table.Column<string>(type: "varchar(20)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: false),
                    Circle = table.Column<string>(type: "varchar(100)", nullable: false),
                    DistributerUserID = table.Column<int>(type: "int", nullable: false),
                    SuperDistributerUserID = table.Column<int>(type: "int", nullable: false),
                    ComplaintStatus = table.Column<string>(type: "varchar(20)", nullable: false),
                    AdminCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CardType = table.Column<string>(type: "varchar(100)", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    IsROffer = table.Column<bool>(type: "bit", nullable: false),
                    IsOnlineDMT = table.Column<bool>(type: "bit", nullable: false),
                    IsProviderLowBalance = table.Column<bool>(type: "bit", nullable: false),
                    ChangeStatus = table.Column<string>(type: "varchar(100)", nullable: false),
                    ChangeStatusDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AEPSResponseJSON = table.Column<string>(type: "varchar(max)", nullable: false),
                    PendingProviderStatus = table.Column<string>(type: "varchar(50)", nullable: false),
                    AdminProfit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ResponseType = table.Column<string>(type: "varchar(50)", nullable: false),
                    ROfferCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SettlementAtProviderEnd = table.Column<bool>(type: "bit", nullable: false),
                    BBPS_CustomerName = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsCCF = table.Column<bool>(type: "bit", nullable: false),
                    NetCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionMasters", x => x.TransactionMasterID);
                    table.ForeignKey(
                        name: "FK_TransactionMasters_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMasters_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustableDevices",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Device = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustableDevices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrustableDevices_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UpiSenders",
                columns: table => new
                {
                    UpiSenderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpiSenders", x => x.UpiSenderID);
                    table.ForeignKey(
                        name: "FK_UpiSenders_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActionHistorys",
                columns: table => new
                {
                    UserHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionHistorys", x => x.UserHistoryID);
                    table.ForeignKey(
                        name: "FK_UserActionHistorys_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActionMasters",
                columns: table => new
                {
                    UserActionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FingurePrint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionMasters", x => x.UserActionMasterID);
                    table.ForeignKey(
                        name: "FK_UserActionMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBalanceCappings",
                columns: table => new
                {
                    UserBalanceCappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalanceCappings", x => x.UserBalanceCappingID);
                    table.ForeignKey(
                        name: "FK_UserBalanceCappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBalanceCappings_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBalanceMaster",
                columns: table => new
                {
                    UserBalanceMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalanceMaster", x => x.UserBalanceMasterID);
                    table.ForeignKey(
                        name: "FK_UserBalanceMaster_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBalanceMaster_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBankDetails",
                columns: table => new
                {
                    UserBankDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "varchar(500)", nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsOn = table.Column<bool>(type: "bit", nullable: false),
                    HolderName = table.Column<string>(type: "varchar(200)", nullable: false),
                    Branch = table.Column<string>(type: "varchar(100)", nullable: false),
                    QRCodeImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UPINumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBankDetails", x => x.UserBankDetailsID);
                    table.ForeignKey(
                        name: "FK_UserBankDetails_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserChannelSlabMappings",
                columns: table => new
                {
                    UserChannelSlabMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannelSlabMappings", x => x.UserChannelSlabMappingID);
                    table.ForeignKey(
                        name: "FK_UserChannelSlabMappings_ChannelSlabMasters_ChannelSlabMasterID",
                        column: x => x.ChannelSlabMasterID,
                        principalTable: "ChannelSlabMasters",
                        principalColumn: "ChannelSlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserChannelSlabMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationMasters",
                columns: table => new
                {
                    UserCreationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    OpeningIdCount = table.Column<int>(type: "int", nullable: false),
                    PurchaseCount = table.Column<int>(type: "int", nullable: false),
                    ClosingIdCount = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedUserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationMasters", x => x.UserCreationMasterID);
                    table.ForeignKey(
                        name: "FK_UserCreationMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCreationMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCreationMasters_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationSettings",
                columns: table => new
                {
                    UserCreationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinimumCount = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationSettings", x => x.UserCreationSettingID);
                    table.ForeignKey(
                        name: "FK_UserCreationSettings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationWallets",
                columns: table => new
                {
                    UserCreationWalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCount = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationWallets", x => x.UserCreationWalletID);
                    table.ForeignKey(
                        name: "FK_UserCreationWallets_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDocumentMasters",
                columns: table => new
                {
                    UserDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DocmentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocumentMasters", x => x.UserDocumentID);
                    table.ForeignKey(
                        name: "FK_UserDocumentMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMerchantMasters",
                columns: table => new
                {
                    UserMerchantMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PaySprintUPIMerchantCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaySprintStaticQR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaySprintUPIRefid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMerchantMasters", x => x.UserMerchantMasterID);
                    table.ForeignKey(
                        name: "FK_UserMerchantMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserOperatorLimits",
                columns: table => new
                {
                    UserOperatorLimitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    LimitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOperatorLimits", x => x.UserOperatorLimitID);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimits_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimits_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimits_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceMappings",
                columns: table => new
                {
                    UserServiceMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceMappings", x => x.UserServiceMappingID);
                    table.ForeignKey(
                        name: "FK_UserServiceMappings_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserServiceMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceSubscriptionDetailss",
                columns: table => new
                {
                    UserServiceSubscriptionDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceSubscriptionDetailss", x => x.UserServiceSubscriptionDetailsID);
                    table.ForeignKey(
                        name: "FK_UserServiceSubscriptionDetailss_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserServiceSubscriptionDetailss_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSlabMappings",
                columns: table => new
                {
                    UserSlabMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSlabMappings", x => x.UserSlabMappingID);
                    table.ForeignKey(
                        name: "FK_UserSlabMappings_SlabMasters_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMasters",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSlabMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWalletMappings",
                columns: table => new
                {
                    UserWalletMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWalletMappings", x => x.UserWalletMappingID);
                    table.ForeignKey(
                        name: "FK_UserWalletMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWalletMappings_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWalletMasters",
                columns: table => new
                {
                    UserWalletMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    BalanceHash = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWalletMasters", x => x.UserWalletMasterID);
                    table.ForeignKey(
                        name: "FK_UserWalletMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWalletMasters_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseDMTMappings",
                columns: table => new
                {
                    UserwiseDMTMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseDMTMappings", x => x.UserwiseDMTMappingID);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMappings_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMappings_ProviderMasters_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMasters",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMappings_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMappings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseLowBalances",
                columns: table => new
                {
                    UserwiseLowBalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseLowBalances", x => x.UserwiseLowBalanceID);
                    table.ForeignKey(
                        name: "FK_UserwiseLowBalances_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseLowBalances_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePayementGatewaySettings",
                columns: table => new
                {
                    UserWisePayementGatewaySettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PaymentGatewaySettingID = table.Column<int>(type: "int", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePayementGatewaySettings", x => x.UserWisePayementGatewaySettingID);
                    table.ForeignKey(
                        name: "FK_UserWisePayementGatewaySettings_PaymentGatewaySettings_PaymentGatewaySettingID",
                        column: x => x.PaymentGatewaySettingID,
                        principalTable: "PaymentGatewaySettings",
                        principalColumn: "PaymentGatewaySettingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWisePayementGatewaySettings_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePlanLimits",
                columns: table => new
                {
                    UserWisePlanLimitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    CircleHitCount = table.Column<int>(type: "int", nullable: false),
                    PlanHitCount = table.Column<int>(type: "int", nullable: false),
                    BillFetchHitCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePlanLimits", x => x.UserWisePlanLimitID);
                    table.ForeignKey(
                        name: "FK_UserWisePlanLimits_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePlanViews",
                columns: table => new
                {
                    UserWisePlanViewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePlanViews", x => x.UserWisePlanViewID);
                    table.ForeignKey(
                        name: "FK_UserWisePlanViews_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseSaleTargets",
                columns: table => new
                {
                    UserwiseSaleTargetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseSaleTargets", x => x.UserwiseSaleTargetID);
                    table.ForeignKey(
                        name: "FK_UserwiseSaleTargets_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseSaleTargets_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseServiceChargeMasters",
                columns: table => new
                {
                    UserwiseServiceChargeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OneTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseServiceChargeMasters", x => x.UserwiseServiceChargeMasterID);
                    table.ForeignKey(
                        name: "FK_UserwiseServiceChargeMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseServiceChargeMasters_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoucherDetails",
                columns: table => new
                {
                    VoucherDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(80)", nullable: true),
                    OutstandingOpeningBal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingClossingBal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(50)", nullable: true),
                    Credit = table.Column<string>(type: "varchar(50)", nullable: true),
                    Debit = table.Column<string>(type: "varchar(50)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherDetails", x => x.VoucherDetailsID);
                    table.ForeignKey(
                        name: "FK_VoucherDetails_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DMTBeneficiaries",
                columns: table => new
                {
                    BeneficiaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    AccountNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(20)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    SenderMobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTBeneficiaries", x => x.BeneficiaryID);
                    table.ForeignKey(
                        name: "FK_DMTBeneficiaries_DMTSenders_SenderID",
                        column: x => x.SenderID,
                        principalTable: "DMTSenders",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DMTBeneficiaries_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Beneficiarys",
                columns: table => new
                {
                    BeneficiaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiarys", x => x.BeneficiaryID);
                    table.ForeignKey(
                        name: "FK_Beneficiarys_Senders_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Senders",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Beneficiarys_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SenderOTPs",
                columns: table => new
                {
                    SenderOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderOTPs", x => x.SenderOTPID);
                    table.ForeignKey(
                        name: "FK_SenderOTPs_Senders_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Senders",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SenderOTPs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsTransactionMasters",
                columns: table => new
                {
                    SmsTransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsProcess = table.Column<bool>(type: "bit", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTransactionMasters", x => x.SmsTransactionMasterID);
                    table.ForeignKey(
                        name: "FK_SmsTransactionMasters_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmsTransactionMasters_TransactionMasters_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMasters",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThreewayReconCompletions",
                columns: table => new
                {
                    ThreewayReconCompletionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreewayReconCompletions", x => x.ThreewayReconCompletionID);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletions_OperatorMasters_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMasters",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletions_ServiceMasters_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMasters",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletions_TransactionMasters_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMasters",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionManualUpdates",
                columns: table => new
                {
                    TransactionManualUpdateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ManualUpdatedBy = table.Column<int>(type: "int", nullable: false),
                    ProviderrefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionManualUpdates", x => x.TransactionManualUpdateID);
                    table.ForeignKey(
                        name: "FK_TransactionManualUpdates_TransactionMasters_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMasters",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionManualUpdates_UserMasters_ManualUpdatedBy",
                        column: x => x.ManualUpdatedBy,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionProviderHistorys",
                columns: table => new
                {
                    RechargeProviderHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionProviderHistorys", x => x.RechargeProviderHistoryID);
                    table.ForeignKey(
                        name: "FK_TransactionProviderHistorys_TransactionMasters_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMasters",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatusChanges",
                columns: table => new
                {
                    TransactionStatusChangeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    TransactionDOCInt = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatusChanges", x => x.TransactionStatusChangeID);
                    table.ForeignKey(
                        name: "FK_TransactionStatusChanges_TransactionMasters_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMasters",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionStatusChanges_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UpiBeneficiarys",
                columns: table => new
                {
                    UpiBeneficiaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpiID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpiSenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpiBeneficiarys", x => x.UpiBeneficiaryID);
                    table.ForeignKey(
                        name: "FK_UpiBeneficiarys_UpiSenders_UpiSenderID",
                        column: x => x.UpiSenderID,
                        principalTable: "UpiSenders",
                        principalColumn: "UpiSenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UpiBeneficiarys_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryOTPs",
                columns: table => new
                {
                    BeneficiaryOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryOTPs", x => x.BeneficiaryOTPID);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTPs_Beneficiarys_BeneficiaryID",
                        column: x => x.BeneficiaryID,
                        principalTable: "Beneficiarys",
                        principalColumn: "BeneficiaryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTPs_Senders_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Senders",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTPs_UserMasters_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMasters",
                        principalColumn: "UserMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UPIVendorBeneficiaryMappings",
                columns: table => new
                {
                    UPIVendorBeneficiaryMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UPIVendorBeneficiaryID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpiBeneficiaryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UPIVendorBeneficiaryMappings", x => x.UPIVendorBeneficiaryMappingID);
                    table.ForeignKey(
                        name: "FK_UPIVendorBeneficiaryMappings_UpiBeneficiarys_UpiBeneficiaryID",
                        column: x => x.UpiBeneficiaryID,
                        principalTable: "UpiBeneficiarys",
                        principalColumn: "UpiBeneficiaryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APIUserCallbackRequestResponseLogs_UserID",
                table: "APIUserCallbackRequestResponseLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_APIUserInitialRequestResponseLogs_UserID",
                table: "APIUserInitialRequestResponseLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserID",
                table: "AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationMasters_UserTypeID",
                table: "AuthorizationMasters",
                column: "UserTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AutoTransferUserwiseMappings_UserID",
                table: "AutoTransferUserwiseMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BankRequestResponseLogs_UserID",
                table: "BankRequestResponseLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BBPSConfigurations_OperatorID",
                table: "BBPSConfigurations",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTPs_BeneficiaryID",
                table: "BeneficiaryOTPs",
                column: "BeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTPs_SenderID",
                table: "BeneficiaryOTPs",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTPs_UserID",
                table: "BeneficiaryOTPs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiarys_SenderID",
                table: "Beneficiarys",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiarys_UserID",
                table: "Beneficiarys",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CallbackMasters_ServiceID",
                table: "CallbackMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_CallbackMasters_UserID",
                table: "CallbackMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CallMeRequests_CallMeCategoryMasterID",
                table: "CallMeRequests",
                column: "CallMeCategoryMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_CallMeRequests_UserID",
                table: "CallMeRequests",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CashCollectionMasters_UserID",
                table: "CashCollectionMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryChannelMappings_ChannelCategoryID",
                table: "CategoryChannelMappings",
                column: "ChannelCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetails_ChannelSlabMasterID",
                table: "ChannelSlabDetails",
                column: "ChannelSlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetails_OperatorID",
                table: "ChannelSlabDetails",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetails_ServiceID",
                table: "ChannelSlabDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseCommissions_OperatorID",
                table: "CircleWiseCommissions",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseCommissions_SlabMasterID",
                table: "CircleWiseCommissions",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseOperatorLimits_OperatorID",
                table: "CircleWiseOperatorLimits",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseOperatorLimits_UserID",
                table: "CircleWiseOperatorLimits",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseUserWiseOperatorOffs_OperatorID",
                table: "CircleWiseUserWiseOperatorOffs",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseUserWiseOperatorOffs_UserID",
                table: "CircleWiseUserWiseOperatorOffs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMasters_OperatorID",
                table: "ComplaintMasters",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMasters_ServiceID",
                table: "ComplaintMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMasters_UserID",
                table: "ComplaintMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMasters_ProviderID",
                table: "DailyAuthenticationMasters",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMasters_ServiceID",
                table: "DailyAuthenticationMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMasters_UserID",
                table: "DailyAuthenticationMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DMTBeneficiaries_SenderID_AccountNo",
                table: "DMTBeneficiaries",
                columns: new[] { "SenderID", "AccountNo" });

            migrationBuilder.CreateIndex(
                name: "IX_DMTBeneficiaries_UserID",
                table: "DMTBeneficiaries",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DMTSenders_Mobile",
                table: "DMTSenders",
                column: "Mobile");

            migrationBuilder.CreateIndex(
                name: "IX_DMTSenders_UserID",
                table: "DMTSenders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_EcollectionDetails_UserID",
                table: "EcollectionDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_eKYCLogs_UserID",
                table: "eKYCLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_eKYCMasters_UserID",
                table: "eKYCMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_ReceiverID",
                table: "FundTransactions",
                column: "ReceiverID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_RequestToID",
                table: "FundTransactions",
                column: "RequestToID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_SenderID",
                table: "FundTransactions",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_WalletID",
                table: "FundTransactions",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_GSTHolds_StateID",
                table: "GSTHolds",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_GSTHolds_UserID",
                table: "GSTHolds",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_OperatorID",
                table: "Inboxes",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_ServiceID",
                table: "Inboxes",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Inboxes_UserID",
                table: "Inboxes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberHistories_UserID",
                table: "InvoiceNumberHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IPMasters_UserID",
                table: "IPMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LapuMasters_LapuPartyMasterID",
                table: "LapuMasters",
                column: "LapuPartyMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_LapuMasters_OperatorID",
                table: "LapuMasters",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserID_Date",
                table: "LoginHistories",
                columns: new[] { "UserID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_LowBalanceConfigurations_UserID",
                table: "LowBalanceConfigurations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LowBalanceConfigurations_WalletID",
                table: "LowBalanceConfigurations",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsMasters_UserID",
                table: "NewsMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingUserDocuments_OnboardingUserID",
                table: "OnboardingUserDocuments",
                column: "OnboardingUserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalHistorys_UserID",
                table: "OperationalHistorys",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorMasters_ServiceID",
                table: "OperatorMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequences_OperatorID",
                table: "OperatorSequences",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequences_ProviderID",
                table: "OperatorSequences",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequences_ServiceID",
                table: "OperatorSequences",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWises_OperatorID",
                table: "OperatorSequenceUserWises",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWises_ProviderID",
                table: "OperatorSequenceUserWises",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWises_ServiceID",
                table: "OperatorSequenceUserWises",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWises_UserID",
                table: "OperatorSequenceUserWises",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorStateMappings_OperatorID",
                table: "OperatorStateMappings",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwises_OperatorID",
                table: "OperatorUserwises",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwises_ServiceID",
                table: "OperatorUserwises",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwises_UserID",
                table: "OperatorUserwises",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftDetails_UserID",
                table: "OverdraftDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftDetails_WalletID",
                table: "OverdraftDetails",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftMasters_UserID",
                table: "OverdraftMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftMasters_WalletID",
                table: "OverdraftMasters",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordChangeHistories_UserID",
                table: "PasswordChangeHistories",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_ServiceID",
                table: "PaymentGateways",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateways_UserID",
                table: "PaymentGateways",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBeneficiaryOTPs_UserID",
                table: "PayoutBeneficiaryOTPs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutVendorBeneficiaryMappings_PayoutBeneficaryID",
                table: "PayoutVendorBeneficiaryMappings",
                column: "PayoutBeneficaryID");

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidActivations_UserID",
                table: "PrepaidActivations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommissions_OperatorID",
                table: "ProviderConfigurationAdminCommissions",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommissions_ProviderID",
                table: "ProviderConfigurationAdminCommissions",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommissions_ServiceID",
                table: "ProviderConfigurationAdminCommissions",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurations_OperatorID",
                table: "ProviderConfigurations",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurations_ProviderID",
                table: "ProviderConfigurations",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurations_ServiceID",
                table: "ProviderConfigurations",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderDetails_ProviderID",
                table: "ProviderDetails",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderDetails_ServiceID",
                table: "ProviderDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderMasters_ProviderName",
                table: "ProviderMasters",
                column: "ProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorCircleMasters_OperatorID",
                table: "ProviderOperatorCircleMasters",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorCircleMasters_ProviderID",
                table: "ProviderOperatorCircleMasters",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorDenominationMappings_DenominationMasterID",
                table: "ProviderOperatorDenominationMappings",
                column: "DenominationMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorDenominationMappings_OperatorID",
                table: "ProviderOperatorDenominationMappings",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorDenominationMappings_ProviderID",
                table: "ProviderOperatorDenominationMappings",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPurchases_ProviderID",
                table: "ProviderPurchases",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPurchases_WalletID",
                table: "ProviderPurchases",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestResponseLogs_UserID",
                table: "ProviderRequestResponseLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderResponseMasters_ProviderID",
                table: "ProviderResponseMasters",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderStateConfigurations_ProviderID",
                table: "ProviderStateConfigurations",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserMappings_UserID",
                table: "ProviderUserMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequences_OperatorID",
                table: "RangeWiseSequences",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequences_ProviderID",
                table: "RangeWiseSequences",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequences_ServiceID",
                table: "RangeWiseSequences",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_RealProviderMappings_OperatorID",
                table: "RealProviderMappings",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationSettings_ProviderID",
                table: "ReconciliationSettings",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_Redeemcommissions_UserID",
                table: "Redeemcommissions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRegistrations_UserID",
                table: "ReferralRegistrations",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_JwtId",
                table: "RefreshTokens",
                column: "JwtId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserID_IsRevoked_IsUsed",
                table: "RefreshTokens",
                columns: new[] { "UserID", "IsRevoked", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommissions_OperatorID",
                table: "ROfferCommissions",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommissions_ServiceID",
                table: "ROfferCommissions",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommissions_SlabMasterID",
                table: "ROfferCommissions",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SDKBBPSUserMappings_UserID",
                table: "SDKBBPSUserMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SenderOTPs_SenderID",
                table: "SenderOTPs",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_SenderOTPs_UserID",
                table: "SenderOTPs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Senders_UserID",
                table: "Senders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSubscriptionPlanDetailss_ServiceSubscriptionCreatePlanMasterID",
                table: "ServiceSubscriptionPlanDetailss",
                column: "ServiceSubscriptionCreatePlanMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWiseSliderSettings_ServiceID",
                table: "ServiceWiseSliderSettings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetails_OperatorID",
                table: "SlabDetails",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetails_ServiceID",
                table: "SlabDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetails_SlabMasterID",
                table: "SlabDetails",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTransactionMasters_ServiceID",
                table: "SmsTransactionMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTransactionMasters_TransactionMasterID",
                table: "SmsTransactionMasters",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategorys_CategoryID",
                table: "SubCategorys",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportUserAccessDetails_UserID",
                table: "SupportUserAccessDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TargetSaleCommissions_UserID",
                table: "TargetSaleCommissions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TargetSaleCommissions_WalletID",
                table: "TargetSaleCommissions",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletions_OperatorID",
                table: "ThreewayReconCompletions",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletions_ServiceID",
                table: "ThreewayReconCompletions",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletions_TransactionMasterID",
                table: "ThreewayReconCompletions",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMasters_TicketCategoryID",
                table: "TicketMasters",
                column: "TicketCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMasters_UserID",
                table: "TicketMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMasters_UserTyepID",
                table: "TicketMasters",
                column: "UserTyepID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionManualUpdates_ManualUpdatedBy",
                table: "TransactionManualUpdates",
                column: "ManualUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionManualUpdates_TransactionMasterID",
                table: "TransactionManualUpdates",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_OperatorID",
                table: "TransactionMasters",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_ProviderRequestID",
                table: "TransactionMasters",
                column: "ProviderRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_ServiceID",
                table: "TransactionMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_Status",
                table: "TransactionMasters",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_UserID",
                table: "TransactionMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMasters_WalletID",
                table: "TransactionMasters",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionProviderHistorys_TransactionMasterID",
                table: "TransactionProviderHistorys",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusChanges_TransactionMasterID",
                table: "TransactionStatusChanges",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusChanges_UserID",
                table: "TransactionStatusChanges",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TrustableDevices_UserID",
                table: "TrustableDevices",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiBeneficiarys_UpiSenderID",
                table: "UpiBeneficiarys",
                column: "UpiSenderID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiBeneficiarys_UserID",
                table: "UpiBeneficiarys",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiSenders_UserID",
                table: "UpiSenders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UPIVendorBeneficiaryMappings_UpiBeneficiaryID",
                table: "UPIVendorBeneficiaryMappings",
                column: "UpiBeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActionHistorys_UserID",
                table: "UserActionHistorys",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActionMasters_UserID",
                table: "UserActionMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceCappings_UserID",
                table: "UserBalanceCappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceCappings_WalletID",
                table: "UserBalanceCappings",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceMaster_UserID",
                table: "UserBalanceMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceMaster_WalletID",
                table: "UserBalanceMaster",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankDetails_UserID",
                table: "UserBankDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannelSlabMappings_ChannelSlabMasterID",
                table: "UserChannelSlabMappings",
                column: "ChannelSlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannelSlabMappings_UserID",
                table: "UserChannelSlabMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMasters_ServiceID",
                table: "UserCreationMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMasters_UserID",
                table: "UserCreationMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMasters_WalletID",
                table: "UserCreationMasters",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationSettings_UserID",
                table: "UserCreationSettings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationWallets_UserID",
                table: "UserCreationWallets",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentMasters_UserID",
                table: "UserDocumentMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_MobileNumber",
                table: "UserMasters",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_StateID",
                table: "UserMasters",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMasters_UserTypeID",
                table: "UserMasters",
                column: "UserTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMerchantMasters_UserID",
                table: "UserMerchantMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimits_OperatorID",
                table: "UserOperatorLimits",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimits_ServiceID",
                table: "UserOperatorLimits",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimits_UserID",
                table: "UserOperatorLimits",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceMappings_ServiceID",
                table: "UserServiceMappings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceMappings_UserID",
                table: "UserServiceMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceSubscriptionDetailss_ServiceID",
                table: "UserServiceSubscriptionDetailss",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceSubscriptionDetailss_UserID",
                table: "UserServiceSubscriptionDetailss",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSlabMappings_SlabMasterID",
                table: "UserSlabMappings",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSlabMappings_UserID",
                table: "UserSlabMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMappings_UserID",
                table: "UserWalletMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMappings_WalletID",
                table: "UserWalletMappings",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMasters_UserID",
                table: "UserWalletMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMasters_WalletID",
                table: "UserWalletMasters",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMappings_OperatorID",
                table: "UserwiseDMTMappings",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMappings_ProviderID",
                table: "UserwiseDMTMappings",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMappings_ServiceID",
                table: "UserwiseDMTMappings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMappings_UserID",
                table: "UserwiseDMTMappings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseLowBalances_UserID",
                table: "UserwiseLowBalances",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseLowBalances_WalletID",
                table: "UserwiseLowBalances",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePayementGatewaySettings_PaymentGatewaySettingID",
                table: "UserWisePayementGatewaySettings",
                column: "PaymentGatewaySettingID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePayementGatewaySettings_UserID",
                table: "UserWisePayementGatewaySettings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePlanLimits_UserID",
                table: "UserWisePlanLimits",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePlanViews_UserID",
                table: "UserWisePlanViews",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseSaleTargets_UserID",
                table: "UserwiseSaleTargets",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseSaleTargets_WalletID",
                table: "UserwiseSaleTargets",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseServiceChargeMasters_ServiceID",
                table: "UserwiseServiceChargeMasters",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseServiceChargeMasters_UserID",
                table: "UserwiseServiceChargeMasters",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDetails_UserID",
                table: "VoucherDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherMasters_VoucherTypeID",
                table: "VoucherMasters",
                column: "VoucherTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletServiceMappings_ServiceID",
                table: "WalletServiceMappings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletServiceMappings_WalletID",
                table: "WalletServiceMappings",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_WrongPasswordLogs_UserID_DateTime",
                table: "WrongPasswordLogs",
                columns: new[] { "UserID", "DateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountOpenSettings");

            migrationBuilder.DropTable(
                name: "AEPSSettings");

            migrationBuilder.DropTable(
                name: "AndroidMenuMasters");

            migrationBuilder.DropTable(
                name: "AndroidSectionMasters");

            migrationBuilder.DropTable(
                name: "APIUserCallbackRequestResponseLogs");

            migrationBuilder.DropTable(
                name: "APIUserInitialRequestResponseLogs");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AuthorizationMasters");

            migrationBuilder.DropTable(
                name: "AutoTransferUserwiseMappings");

            migrationBuilder.DropTable(
                name: "BankRequestResponseLogs");

            migrationBuilder.DropTable(
                name: "BBPSConfigurations");

            migrationBuilder.DropTable(
                name: "BBPSSettings");

            migrationBuilder.DropTable(
                name: "BeneficiaryOTPs");

            migrationBuilder.DropTable(
                name: "BlackListBankMasters");

            migrationBuilder.DropTable(
                name: "BrowserUserAgentMasters");

            migrationBuilder.DropTable(
                name: "BulkMessages");

            migrationBuilder.DropTable(
                name: "BulkPaySettings");

            migrationBuilder.DropTable(
                name: "CallbackMasters");

            migrationBuilder.DropTable(
                name: "CallMeRequests");

            migrationBuilder.DropTable(
                name: "CashCollectionDetails");

            migrationBuilder.DropTable(
                name: "CashCollectionMasters");

            migrationBuilder.DropTable(
                name: "CashDepositSettings");

            migrationBuilder.DropTable(
                name: "CategoryChannelMappings");

            migrationBuilder.DropTable(
                name: "ChannelSlabDetails");

            migrationBuilder.DropTable(
                name: "CircleAPIMasters");

            migrationBuilder.DropTable(
                name: "CircleWiseCommissions");

            migrationBuilder.DropTable(
                name: "CircleWiseOperatorLimits");

            migrationBuilder.DropTable(
                name: "CircleWiseUserWiseOperatorOffs");

            migrationBuilder.DropTable(
                name: "CMSSettings");

            migrationBuilder.DropTable(
                name: "CompanyMasters");

            migrationBuilder.DropTable(
                name: "ComplaintMasters");

            migrationBuilder.DropTable(
                name: "ConsumerTypeMasters");

            migrationBuilder.DropTable(
                name: "CreditCardSettings");

            migrationBuilder.DropTable(
                name: "CreditInstitutionMasters");

            migrationBuilder.DropTable(
                name: "DailyAuthenticationMasters");

            migrationBuilder.DropTable(
                name: "DateFormatMasters");

            migrationBuilder.DropTable(
                name: "DematSettings");

            migrationBuilder.DropTable(
                name: "DeviceWiseFailLoginHistorys");

            migrationBuilder.DropTable(
                name: "DMTBanks");

            migrationBuilder.DropTable(
                name: "DMTBeneficiaries");

            migrationBuilder.DropTable(
                name: "DMTSettings");

            migrationBuilder.DropTable(
                name: "EcollectionDetails");

            migrationBuilder.DropTable(
                name: "EcollectionSettings");

            migrationBuilder.DropTable(
                name: "eKYCLogs");

            migrationBuilder.DropTable(
                name: "eKYCMasters");

            migrationBuilder.DropTable(
                name: "eKYCSettings");

            migrationBuilder.DropTable(
                name: "EmailSettingMasters");

            migrationBuilder.DropTable(
                name: "EnquiryDetails");

            migrationBuilder.DropTable(
                name: "ExternalMedias");

            migrationBuilder.DropTable(
                name: "FailedSignUpDatas");

            migrationBuilder.DropTable(
                name: "FingerPrintDeviceTypes");

            migrationBuilder.DropTable(
                name: "FundTransactions");

            migrationBuilder.DropTable(
                name: "GSTHolds");

            migrationBuilder.DropTable(
                name: "Inboxes");

            migrationBuilder.DropTable(
                name: "InvoiceNumberHistories");

            migrationBuilder.DropTable(
                name: "InvoiceSettings");

            migrationBuilder.DropTable(
                name: "IPMasters");

            migrationBuilder.DropTable(
                name: "KYCApprovedDetails");

            migrationBuilder.DropTable(
                name: "KycDocumentMasters");

            migrationBuilder.DropTable(
                name: "LapuMasters");

            migrationBuilder.DropTable(
                name: "LapuTransactionDetails");

            migrationBuilder.DropTable(
                name: "LastChangeUserDetailss");

            migrationBuilder.DropTable(
                name: "LeadGenerationMasters");

            migrationBuilder.DropTable(
                name: "LeadGenerationSettings");

            migrationBuilder.DropTable(
                name: "LoanTypeMasters");

            migrationBuilder.DropTable(
                name: "LocationLookUps");

            migrationBuilder.DropTable(
                name: "LocationPinCodeMasters");

            migrationBuilder.DropTable(
                name: "LoginHistories");

            migrationBuilder.DropTable(
                name: "LowBalanceConfigurations");

            migrationBuilder.DropTable(
                name: "MediaMasters");

            migrationBuilder.DropTable(
                name: "MenuActionMappings");

            migrationBuilder.DropTable(
                name: "MenuItem");

            migrationBuilder.DropTable(
                name: "MenuMasters");

            migrationBuilder.DropTable(
                name: "MessageSettings");

            migrationBuilder.DropTable(
                name: "MicroATMSettings");

            migrationBuilder.DropTable(
                name: "MPinHistorys");

            migrationBuilder.DropTable(
                name: "NewsMasters");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NPCIBanks");

            migrationBuilder.DropTable(
                name: "OnboardingUserDocuments");

            migrationBuilder.DropTable(
                name: "OperationalHistorys");

            migrationBuilder.DropTable(
                name: "OperatorAmountWiseSettings");

            migrationBuilder.DropTable(
                name: "OperatorSequences");

            migrationBuilder.DropTable(
                name: "OperatorSequenceUserWises");

            migrationBuilder.DropTable(
                name: "OperatorStateMappings");

            migrationBuilder.DropTable(
                name: "OperatorUserwises");

            migrationBuilder.DropTable(
                name: "OperatorwiseCallbackSettings");

            migrationBuilder.DropTable(
                name: "OTPMasters");

            migrationBuilder.DropTable(
                name: "OTTSettings");

            migrationBuilder.DropTable(
                name: "OverdraftDetails");

            migrationBuilder.DropTable(
                name: "OverdraftMasters");

            migrationBuilder.DropTable(
                name: "PanCardSettings");

            migrationBuilder.DropTable(
                name: "PasswordChangeHistories");

            migrationBuilder.DropTable(
                name: "PaymentGatewayLogs");

            migrationBuilder.DropTable(
                name: "PaymentGateways");

            migrationBuilder.DropTable(
                name: "PaymentModeMasters");

            migrationBuilder.DropTable(
                name: "PayoutBeneficiaryOTPs");

            migrationBuilder.DropTable(
                name: "PayoutSettings");

            migrationBuilder.DropTable(
                name: "PayoutVendorBeneficiaryMappings");

            migrationBuilder.DropTable(
                name: "PaySprintBanks");

            migrationBuilder.DropTable(
                name: "PlanSettings");

            migrationBuilder.DropTable(
                name: "POSSettingMasters");

            migrationBuilder.DropTable(
                name: "PrepaidActivations");

            migrationBuilder.DropTable(
                name: "ProviderCallBackLogs");

            migrationBuilder.DropTable(
                name: "ProviderConfigurationAdminCommissions");

            migrationBuilder.DropTable(
                name: "ProviderConfigurations");

            migrationBuilder.DropTable(
                name: "ProviderDetails");

            migrationBuilder.DropTable(
                name: "ProviderOperatorCircleMasters");

            migrationBuilder.DropTable(
                name: "ProviderOperatorDenominationMappings");

            migrationBuilder.DropTable(
                name: "ProviderOperatorMasters");

            migrationBuilder.DropTable(
                name: "ProviderPurchases");

            migrationBuilder.DropTable(
                name: "ProviderRequestResponseLogs");

            migrationBuilder.DropTable(
                name: "ProviderResponseMasters");

            migrationBuilder.DropTable(
                name: "ProviderResponseTypeMasters");

            migrationBuilder.DropTable(
                name: "ProviderStateConfigurations");

            migrationBuilder.DropTable(
                name: "ProviderUserMappings");

            migrationBuilder.DropTable(
                name: "PureDMTTransferBeneDetails");

            migrationBuilder.DropTable(
                name: "RangeWiseSequences");

            migrationBuilder.DropTable(
                name: "RealProviderMappings");

            migrationBuilder.DropTable(
                name: "ReconciliationSettings");

            migrationBuilder.DropTable(
                name: "Redeemcommissions");

            migrationBuilder.DropTable(
                name: "RedirectUrlMasters");

            migrationBuilder.DropTable(
                name: "ReferralRegistrations");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RemainderOperatorAmountWises");

            migrationBuilder.DropTable(
                name: "RichTextMedias");

            migrationBuilder.DropTable(
                name: "ROfferCommissions");

            migrationBuilder.DropTable(
                name: "SDKBBPSUserMappings");

            migrationBuilder.DropTable(
                name: "SDKSettings");

            migrationBuilder.DropTable(
                name: "SenderOTPs");

            migrationBuilder.DropTable(
                name: "SentSMS");

            migrationBuilder.DropTable(
                name: "ServiceAndUserwiseSubscriptions");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionPlanDetailss");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionPlanMasters");

            migrationBuilder.DropTable(
                name: "ServiceWiseSliderSettings");

            migrationBuilder.DropTable(
                name: "SettingMasters");

            migrationBuilder.DropTable(
                name: "SignUps");

            migrationBuilder.DropTable(
                name: "SlabDetails");

            migrationBuilder.DropTable(
                name: "SliderMasters");

            migrationBuilder.DropTable(
                name: "SmsMasters");

            migrationBuilder.DropTable(
                name: "SmsTransactionMasters");

            migrationBuilder.DropTable(
                name: "SubCategorys");

            migrationBuilder.DropTable(
                name: "SubMenuItem");

            migrationBuilder.DropTable(
                name: "SubscriptionMasters");

            migrationBuilder.DropTable(
                name: "SupportUserAccessDetails");

            migrationBuilder.DropTable(
                name: "SupportUserAccessMasters");

            migrationBuilder.DropTable(
                name: "TargetSaleCommissions");

            migrationBuilder.DropTable(
                name: "ThirdPartyServiceProviders");

            migrationBuilder.DropTable(
                name: "ThreewayReconCompletions");

            migrationBuilder.DropTable(
                name: "TicketMasters");

            migrationBuilder.DropTable(
                name: "TransactionAPIUserRequestMasters");

            migrationBuilder.DropTable(
                name: "TransactionDetailss");

            migrationBuilder.DropTable(
                name: "TransactionManualUpdates");

            migrationBuilder.DropTable(
                name: "TransactionProviderHistorys");

            migrationBuilder.DropTable(
                name: "TransactionStatusChanges");

            migrationBuilder.DropTable(
                name: "TrustableDevices");

            migrationBuilder.DropTable(
                name: "UISettingMasters");

            migrationBuilder.DropTable(
                name: "UnitMasters");

            migrationBuilder.DropTable(
                name: "UPITransferSettings");

            migrationBuilder.DropTable(
                name: "UPIVendorBeneficiaryMappings");

            migrationBuilder.DropTable(
                name: "UrlTypeMasters");

            migrationBuilder.DropTable(
                name: "UserActionHistorys");

            migrationBuilder.DropTable(
                name: "UserActionMasters");

            migrationBuilder.DropTable(
                name: "UserBalanceCappings");

            migrationBuilder.DropTable(
                name: "UserBalanceHistories");

            migrationBuilder.DropTable(
                name: "UserBalanceMaster");

            migrationBuilder.DropTable(
                name: "UserBankDetails");

            migrationBuilder.DropTable(
                name: "UserChannelSlabMappings");

            migrationBuilder.DropTable(
                name: "UserCreationMasters");

            migrationBuilder.DropTable(
                name: "UserCreationSettings");

            migrationBuilder.DropTable(
                name: "UserCreationWallets");

            migrationBuilder.DropTable(
                name: "UserDocumentMasters");

            migrationBuilder.DropTable(
                name: "UserMerchantMasters");

            migrationBuilder.DropTable(
                name: "UserOperatorLimits");

            migrationBuilder.DropTable(
                name: "UserPaymentCollectMappings");

            migrationBuilder.DropTable(
                name: "UserPlanUtilizations");

            migrationBuilder.DropTable(
                name: "UserServiceCharges");

            migrationBuilder.DropTable(
                name: "UserServiceMappings");

            migrationBuilder.DropTable(
                name: "UserServiceSubscriptionCharges");

            migrationBuilder.DropTable(
                name: "UserServiceSubscriptionDetailss");

            migrationBuilder.DropTable(
                name: "UserSlabMappings");

            migrationBuilder.DropTable(
                name: "UserWalletMappings");

            migrationBuilder.DropTable(
                name: "UserWalletMasters");

            migrationBuilder.DropTable(
                name: "UserwiseDMTMappings");

            migrationBuilder.DropTable(
                name: "UserwiseLowBalances");

            migrationBuilder.DropTable(
                name: "UserWisePayementGatewaySettings");

            migrationBuilder.DropTable(
                name: "UserWisePlanLimits");

            migrationBuilder.DropTable(
                name: "UserWisePlanViews");

            migrationBuilder.DropTable(
                name: "UserwiseSaleTargets");

            migrationBuilder.DropTable(
                name: "UserwiseServiceChargeMasters");

            migrationBuilder.DropTable(
                name: "VerificationSettings");

            migrationBuilder.DropTable(
                name: "VirtualAccountMasters");

            migrationBuilder.DropTable(
                name: "VirtualBalances");

            migrationBuilder.DropTable(
                name: "VoucherDetails");

            migrationBuilder.DropTable(
                name: "VoucherMasters");

            migrationBuilder.DropTable(
                name: "WalletServiceMappings");

            migrationBuilder.DropTable(
                name: "WebHookLogs");

            migrationBuilder.DropTable(
                name: "WebHookSettings");

            migrationBuilder.DropTable(
                name: "WrongPasswordHistorys");

            migrationBuilder.DropTable(
                name: "WrongPasswordLogs");

            migrationBuilder.DropTable(
                name: "ZoneMasters");

            migrationBuilder.DropTable(
                name: "Beneficiarys");

            migrationBuilder.DropTable(
                name: "CallMeCategoryMaster");

            migrationBuilder.DropTable(
                name: "ChannelCategorys");

            migrationBuilder.DropTable(
                name: "DMTSenders");

            migrationBuilder.DropTable(
                name: "LapuPartyMasters");

            migrationBuilder.DropTable(
                name: "OnboardingUsers");

            migrationBuilder.DropTable(
                name: "PayoutBeneficiaries");

            migrationBuilder.DropTable(
                name: "DenominationMasters");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionCreatePlanMasters");

            migrationBuilder.DropTable(
                name: "Categorys");

            migrationBuilder.DropTable(
                name: "TicketCategories");

            migrationBuilder.DropTable(
                name: "TransactionMasters");

            migrationBuilder.DropTable(
                name: "UpiBeneficiarys");

            migrationBuilder.DropTable(
                name: "ChannelSlabMasters");

            migrationBuilder.DropTable(
                name: "SlabMasters");

            migrationBuilder.DropTable(
                name: "ProviderMasters");

            migrationBuilder.DropTable(
                name: "PaymentGatewaySettings");

            migrationBuilder.DropTable(
                name: "VoucherTypes");

            migrationBuilder.DropTable(
                name: "Senders");

            migrationBuilder.DropTable(
                name: "OperatorMasters");

            migrationBuilder.DropTable(
                name: "WalletMaster");

            migrationBuilder.DropTable(
                name: "UpiSenders");

            migrationBuilder.DropTable(
                name: "ServiceMasters");

            migrationBuilder.DropTable(
                name: "UserMasters");

            migrationBuilder.DropTable(
                name: "StateMasters");

            migrationBuilder.DropTable(
                name: "UserTypeMasters");
        }
    }
}
