using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinovativeAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountOpenSetting",
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
                    table.PrimaryKey("PK_AccountOpenSetting", x => x.AccountOpenSettingID);
                });

            migrationBuilder.CreateTable(
                name: "AEPSSetting",
                columns: table => new
                {
                    AEPSSettingID = table.Column<int>(type: "int", nullable: false)
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
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsMerchantVerify = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AEPSSetting", x => x.AEPSSettingID);
                });

            migrationBuilder.CreateTable(
                name: "AndroidMenuMaster",
                columns: table => new
                {
                    AndroidMenuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuName = table.Column<string>(type: "varchar(200)", nullable: true),
                    Key = table.Column<string>(type: "varchar(1000)", nullable: true),
                    URL = table.Column<string>(type: "varchar(max)", nullable: true),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AndroidMenuMaster", x => x.AndroidMenuMasterID);
                });

            migrationBuilder.CreateTable(
                name: "AndroidSectionMaster",
                columns: table => new
                {
                    AndroidSectionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AndroidSectionMaster", x => x.AndroidSectionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "Archive_TransactionDetails",
                columns: table => new
                {
                    ArchiveTransactionDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionDetailID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ReferenceID = table.Column<int>(type: "int", nullable: false),
                    AssignedUserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    DOCType = table.Column<string>(type: "varchar(100)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AmountCreditDebit = table.Column<string>(type: "varchar(100)", nullable: false),
                    CommissionCreditDebit = table.Column<string>(type: "varchar(100)", nullable: false),
                    TransactionDescription = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archive_TransactionDetails", x => x.ArchiveTransactionDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "Archive_TransactionMaster",
                columns: table => new
                {
                    ArchiveTransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    InboxID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ProviderOpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "varchar(500)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    APIUserRequestID = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ProviderDOCNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ProviderRefNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    AdminGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Bill_CustomerMessageMobileNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    RetailerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CashBack = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    HitBackCount = table.Column<int>(type: "int", nullable: false),
                    LapuNo = table.Column<string>(type: "varchar(500)", nullable: true),
                    Bill_Optional1 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional2 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional3 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional4 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional5 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional6 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_SenderID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_SenderName = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_SenderNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficiaryName = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_Bank = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_IFSC = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficaryNameByBank = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_PaymentType = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BillNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_OriginalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMT_StatusCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsDispute = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "varchar(max)", nullable: true),
                    SimCardDetails = table.Column<string>(type: "varchar(max)", nullable: true),
                    Through = table.Column<string>(type: "varchar(20)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: true),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    Circle = table.Column<string>(type: "varchar(100)", nullable: true),
                    DistributerUserID = table.Column<int>(type: "int", nullable: false),
                    SuperDistributerUserID = table.Column<int>(type: "int", nullable: false),
                    ComplaintStatus = table.Column<string>(type: "varchar(20)", nullable: true),
                    AdminCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CardType = table.Column<string>(type: "varchar(100)", nullable: true),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    IsROffer = table.Column<bool>(type: "bit", nullable: false),
                    IsOnlineDMT = table.Column<bool>(type: "bit", nullable: false),
                    IsProviderLowBalance = table.Column<bool>(type: "bit", nullable: false),
                    ChangeStatus = table.Column<string>(type: "varchar(100)", nullable: true),
                    ChangeStatusDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AEPSResponseJSON = table.Column<string>(type: "varchar(max)", nullable: true),
                    PendingProviderStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    AdminProfit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ResponseType = table.Column<string>(type: "varchar(50)", nullable: true),
                    ROfferCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SettlementAtProviderEnd = table.Column<bool>(type: "bit", nullable: false),
                    BBPS_CustomerName = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archive_TransactionMaster", x => x.ArchiveTransactionMasterID);
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
                name: "BlackListBankMaster",
                columns: table => new
                {
                    BlackListBankMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "varchar(200)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(50)", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListBankMaster", x => x.BlackListBankMasterID);
                });

            migrationBuilder.CreateTable(
                name: "BrowserUserAgentMaster",
                columns: table => new
                {
                    BrowserUserAgentMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserAgent = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowserUserAgentMaster", x => x.BrowserUserAgentMasterID);
                });

            migrationBuilder.CreateTable(
                name: "BulkMessage",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkMessage", x => x.BulkMessageID);
                });

            migrationBuilder.CreateTable(
                name: "BulkPaySetting",
                columns: table => new
                {
                    BulkPaySettingID = table.Column<int>(type: "int", nullable: false)
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
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsPendingForFailed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkPaySetting", x => x.BulkPaySettingID);
                });

            migrationBuilder.CreateTable(
                name: "BusSettingMaster",
                columns: table => new
                {
                    BusSettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    ActionURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusSettingMaster", x => x.BusSettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CallMeCategoryMaster",
                columns: table => new
                {
                    CallMeCategoryMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallMeCategoryMaster", x => x.CallMeCategoryMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CashCollectionDetail",
                columns: table => new
                {
                    CashCollectionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    OutstandingOpening = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OutstandingClosing = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ParentRemark = table.Column<string>(type: "varchar(100)", nullable: true),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: true),
                    Credit = table.Column<string>(type: "varchar(50)", nullable: true),
                    Debit = table.Column<string>(type: "varchar(50)", nullable: true),
                    UTRNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    Type = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCollectionDetail", x => x.CashCollectionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "CashDepositSetting",
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
                    table.PrimaryKey("PK_CashDepositSetting", x => x.CashDepositSettingID);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ChannelCategory",
                columns: table => new
                {
                    ChannelCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelCategory", x => x.ChannelCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSlabMaster",
                columns: table => new
                {
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabName = table.Column<string>(type: "varchar(80)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSlabMaster", x => x.ChannelSlabMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CircleAPIMaster",
                columns: table => new
                {
                    CircleAPIMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CircleAPIURL = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleAPIMaster", x => x.CircleAPIMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CMSSetting",
                columns: table => new
                {
                    CMSSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CMSSetting", x => x.CMSSettingID);
                });

            migrationBuilder.CreateTable(
                name: "CompanyMaster",
                columns: table => new
                {
                    CompanyMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyMaster", x => x.CompanyMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerTypeMaster",
                columns: table => new
                {
                    ConsumerTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumerType = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerTypeMaster", x => x.ConsumerTypeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardSetting",
                columns: table => new
                {
                    CreditCardSettingID = table.Column<int>(type: "int", nullable: false)
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
                    table.PrimaryKey("PK_CreditCardSetting", x => x.CreditCardSettingID);
                });

            migrationBuilder.CreateTable(
                name: "CreditInstitutionMaster",
                columns: table => new
                {
                    CreditInstitutionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: true),
                    BranchID = table.Column<string>(type: "varchar(100)", nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    EncryptionKey = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditInstitutionMaster", x => x.CreditInstitutionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "CyberPlateSetting",
                columns: table => new
                {
                    CyberPlateSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AP = table.Column<string>(type: "varchar(500)", nullable: true),
                    OP = table.Column<string>(type: "varchar(500)", nullable: true),
                    SD = table.Column<string>(type: "varchar(500)", nullable: true),
                    Password = table.Column<string>(type: "varchar(500)", nullable: true),
                    TermID = table.Column<string>(type: "varchar(500)", nullable: true),
                    AID = table.Column<string>(type: "varchar(500)", nullable: true),
                    SecretKeyPath = table.Column<string>(type: "varchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyberPlateSetting", x => x.CyberPlateSettingID);
                });

            migrationBuilder.CreateTable(
                name: "DateFormatMaster",
                columns: table => new
                {
                    DateFormatMasterkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateFormat = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateFormatMaster", x => x.DateFormatMasterkID);
                });

            migrationBuilder.CreateTable(
                name: "DematSetting",
                columns: table => new
                {
                    DematSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    UTMSource = table.Column<string>(type: "varchar(200)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(200)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DematSetting", x => x.DematSettingID);
                });

            migrationBuilder.CreateTable(
                name: "DenominationMaster",
                columns: table => new
                {
                    DenominationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    DenominationRange = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsStop = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenominationMaster", x => x.DenominationMasterID);
                });

            migrationBuilder.CreateTable(
                name: "DeviceWiseFailLoginHistory",
                columns: table => new
                {
                    DeviceWiseFailLoginHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "varchar(200)", nullable: false),
                    DeviceID = table.Column<string>(type: "varchar(500)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceWiseFailLoginHistory", x => x.DeviceWiseFailLoginHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "DMTBank",
                columns: table => new
                {
                    DMTBankID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    DefaultIFSC = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsIMPS = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTBank", x => x.DMTBankID);
                });

            migrationBuilder.CreateTable(
                name: "DMTSetting",
                columns: table => new
                {
                    DMTSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: true),
                    WalletID = table.Column<string>(type: "varchar(500)", nullable: true),
                    CallBackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(500)", nullable: true),
                    SplitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SenderLimit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsCCF = table.Column<bool>(type: "bit", nullable: false),
                    IsNeft = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsPayoutDMT = table.Column<bool>(type: "bit", nullable: false),
                    ProviderCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsExpressDMT = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DMTSetting", x => x.DMTSettingID);
                });

            migrationBuilder.CreateTable(
                name: "EcollectionSetting",
                columns: table => new
                {
                    EcollectionSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrivateKeyPath = table.Column<string>(type: "varchar(500)", nullable: true),
                    PrivateKeyPassword = table.Column<string>(type: "varchar(500)", nullable: true),
                    PublicKeyPath = table.Column<string>(type: "varchar(500)", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "varchar(100)", nullable: true),
                    IFSCCode = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcollectionSetting", x => x.EcollectionSettingID);
                });

            migrationBuilder.CreateTable(
                name: "eKYCLog",
                columns: table => new
                {
                    eKYCLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNo = table.Column<string>(type: "varchar(100)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    DocumentType = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCLog", x => x.eKYCLogID);
                });

            migrationBuilder.CreateTable(
                name: "eKYCMaster",
                columns: table => new
                {
                    eKYCMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<string>(type: "varchar(100)", nullable: true),
                    OTPGenerateHash = table.Column<string>(type: "varchar(500)", nullable: true),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OTPGenerateRequest = table.Column<string>(type: "varchar(max)", nullable: true),
                    OTPGenerateResponse = table.Column<string>(type: "varchar(max)", nullable: true),
                    VerifyRequest = table.Column<string>(type: "varchar(max)", nullable: true),
                    VerifyResponse = table.Column<string>(type: "varchar(max)", nullable: true),
                    OTPToken = table.Column<string>(type: "varchar(200)", nullable: true),
                    KYCToken = table.Column<string>(type: "varchar(200)", nullable: true),
                    wadh = table.Column<string>(type: "varchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCMaster", x => x.eKYCMasterID);
                });

            migrationBuilder.CreateTable(
                name: "eKYCSetting",
                columns: table => new
                {
                    eKYCSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    AuthKey = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    EncryptionKey = table.Column<string>(type: "varchar(100)", nullable: true),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: true),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eKYCSetting", x => x.eKYCSettingID);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettingMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettingMaster", x => x.EmailSettingMasterID);
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
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    EnquiryType = table.Column<string>(type: "varchar(50)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(200)", nullable: true),
                    AdminStatus = table.Column<string>(type: "varchar(200)", nullable: true),
                    AdminRemark = table.Column<string>(type: "varchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryDetails", x => x.EnquiryDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "ExternalMedia",
                columns: table => new
                {
                    ExternalMediaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(100)", nullable: true),
                    Value = table.Column<string>(type: "varchar(200)", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMedia", x => x.ExternalMediaID);
                });

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
                    Description = table.Column<string>(type: "varchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedSignUpData", x => x.FailedSignUpDataID);
                });

            migrationBuilder.CreateTable(
                name: "FingerPrintDeviceType",
                columns: table => new
                {
                    FingerPrintDeviceTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "varchar(200)", nullable: false),
                    Value = table.Column<string>(type: "varchar(200)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FingerPrintDeviceType", x => x.FingerPrintDeviceTypeID);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSetting",
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
                    table.PrimaryKey("PK_InvoiceSetting", x => x.InvoiceSettingID);
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KYCApprovedDetails", x => x.KYCApprovedDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "KycDocumentMaster",
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
                    table.PrimaryKey("PK_KycDocumentMaster", x => x.KycDocumentMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LapuPartyMaster",
                columns: table => new
                {
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailID = table.Column<string>(type: "varchar(70)", nullable: true),
                    MobileNumber = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuPartyMaster", x => x.LapuPartyMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LapuTransactionDetail",
                columns: table => new
                {
                    LapuTransactionDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProivderID = table.Column<int>(type: "int", nullable: false),
                    LapuMasterID = table.Column<int>(type: "int", nullable: false),
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false),
                    LapuNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOCType = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    TxnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuTransactionDetail", x => x.LapuTransactionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "LastChangeUserDetails",
                columns: table => new
                {
                    LastChangeUserDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false),
                    Change = table.Column<string>(type: "varchar(500)", nullable: true),
                    OldChanges = table.Column<string>(type: "varchar(500)", nullable: true),
                    NewChanges = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastChangeUserDetails", x => x.LastChangeUserDetailsID);
                });

            migrationBuilder.CreateTable(
                name: "LeadGenerationMaster",
                columns: table => new
                {
                    LeadGenerationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    FormID = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantName = table.Column<string>(type: "varchar(100)", nullable: false),
                    RetailerName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ConsumerName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(100)", nullable: false),
                    AadharNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    PanNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    ConsumerType = table.Column<string>(type: "varchar(100)", nullable: false),
                    RequiredAmount = table.Column<string>(type: "varchar(100)", nullable: false),
                    LoanType = table.Column<string>(type: "varchar(100)", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", nullable: true),
                    ApprovedAmount = table.Column<string>(type: "varchar(100)", nullable: true),
                    Gender = table.Column<string>(type: "varchar(50)", nullable: true),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadGenerationMaster", x => x.LeadGenerationMasterID);
                });

            migrationBuilder.CreateTable(
                name: "LeadGenerationSetting",
                columns: table => new
                {
                    LeadGenerationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadGenerationSetting", x => x.LeadGenerationSettingID);
                });

            migrationBuilder.CreateTable(
                name: "LoanTypeMaster",
                columns: table => new
                {
                    LoanTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanType = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanTypeMaster", x => x.LoanTypeMasterID);
                });

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

            migrationBuilder.CreateTable(
                name: "LOBMaster",
                columns: table => new
                {
                    LOBMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineOfBusiness = table.Column<string>(type: "varchar(150)", nullable: false),
                    LOBCode = table.Column<string>(type: "varchar(150)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOBMaster", x => x.LOBMasterID);
                });

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

            migrationBuilder.CreateTable(
                name: "LocationLookUp",
                columns: table => new
                {
                    LocationLookUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableID = table.Column<int>(type: "int", nullable: false),
                    SimCardDetails = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(150)", nullable: false),
                    Location = table.Column<string>(type: "varchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(max)", nullable: false),
                    Latitude = table.Column<string>(type: "varchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationLookUp", x => x.LocationLookUpID);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Thread = table.Column<string>(type: "varchar(max)", nullable: true),
                    Level = table.Column<string>(type: "varchar(max)", nullable: true),
                    Logger = table.Column<string>(type: "varchar(max)", nullable: true),
                    Message = table.Column<string>(type: "varchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "varchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MediaMaster",
                columns: table => new
                {
                    MediaMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MediaType = table.Column<string>(type: "varchar(100)", nullable: false),
                    MediaPath = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaMaster", x => x.MediaMasterID);
                });

            migrationBuilder.CreateTable(
                name: "MenuActionMapping",
                columns: table => new
                {
                    MenuActionMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Controller = table.Column<string>(type: "varchar(100)", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "varchar(100)", nullable: true),
                    MenuMasterGUID = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuActionMapping", x => x.MenuActionMappingID);
                });

            migrationBuilder.CreateTable(
                name: "MenuMaster",
                columns: table => new
                {
                    MenuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuSequence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    Url = table.Column<string>(type: "varchar(100)", nullable: true),
                    Icon = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsSuperDistributer = table.Column<bool>(type: "bit", nullable: false),
                    IsDistributer = table.Column<bool>(type: "bit", nullable: false),
                    IsRetailer = table.Column<bool>(type: "bit", nullable: false),
                    IsAPIUser = table.Column<bool>(type: "bit", nullable: false),
                    IsSupport = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    GUID = table.Column<string>(type: "varchar(100)", nullable: true),
                    ParentMenuGUID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuMaster", x => x.MenuMasterID);
                });

            migrationBuilder.CreateTable(
                name: "MessageSetting",
                columns: table => new
                {
                    MessageSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsSMS = table.Column<bool>(type: "bit", nullable: false),
                    IsMail = table.Column<bool>(type: "bit", nullable: false),
                    IsWhatsUp = table.Column<bool>(type: "bit", nullable: false),
                    WhatsUpMessage = table.Column<string>(type: "varchar(max)", nullable: true),
                    SmsMessage = table.Column<string>(type: "varchar(500)", nullable: true),
                    EmailMessage = table.Column<string>(type: "varchar(max)", nullable: true),
                    TemplateID = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsOTP = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSetting", x => x.MessageSettingID);
                });

            migrationBuilder.CreateTable(
                name: "MicroATMSetting",
                columns: table => new
                {
                    MicroATMSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Password = table.Column<string>(type: "varchar(100)", nullable: true),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicroATMSetting", x => x.MicroATMSettingID);
                });

            migrationBuilder.CreateTable(
                name: "MosambeeAEPSBank",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    IIN = table.Column<string>(type: "varchar(50)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MosambeeAEPSBank", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MPinHistory",
                columns: table => new
                {
                    MPinHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mpin = table.Column<string>(type: "varchar(6)", nullable: false),
                    DeviceID = table.Column<string>(type: "varchar(200)", nullable: false),
                    Device = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MPinHistory", x => x.MPinHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "NewsMaster",
                columns: table => new
                {
                    NewsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    IsView = table.Column<bool>(type: "bit", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    HyperLink = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsMaster", x => x.NewsID);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationText = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    NotificationImagePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationID);
                });

            migrationBuilder.CreateTable(
                name: "NPCIBank",
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
                    table.PrimaryKey("PK_NPCIBank", x => x.NPCIBankID);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingUser",
                columns: table => new
                {
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileID = table.Column<string>(type: "varchar(500)", nullable: true),
                    ReferenceID = table.Column<string>(type: "varchar(500)", nullable: true),
                    DocumentID = table.Column<string>(type: "varchar(500)", nullable: true),
                    Name = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOB = table.Column<string>(type: "varchar(50)", nullable: true),
                    PanNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    AadhaarNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    Residential_Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    Residential_Locality = table.Column<string>(type: "varchar(500)", nullable: true),
                    State = table.Column<string>(type: "varchar(500)", nullable: true),
                    PinCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    Country = table.Column<string>(type: "varchar(50)", nullable: true),
                    EmailID = table.Column<string>(type: "varchar(50)", nullable: true),
                    MobileNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    SupportStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    AdminStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    SupportRemark = table.Column<string>(type: "varchar(50)", nullable: true),
                    AdminRemark = table.Column<string>(type: "varchar(50)", nullable: true),
                    Gender = table.Column<string>(type: "varchar(50)", nullable: true),
                    Business_Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    Business_Locality = table.Column<string>(type: "varchar(500)", nullable: true),
                    FirmName = table.Column<string>(type: "varchar(500)", nullable: true),
                    OrderID = table.Column<string>(type: "varchar(50)", nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    GSTNo = table.Column<string>(type: "varchar(100)", nullable: true),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Profile_URL = table.Column<string>(type: "varchar(500)", nullable: true),
                    ShopType = table.Column<string>(type: "varchar(500)", nullable: true),
                    LineOfBusiness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LOB_MasterID = table.Column<string>(type: "varchar(500)", nullable: true),
                    SlabID = table.Column<int>(type: "int", nullable: false),
                    supportUserID = table.Column<int>(type: "int", nullable: false),
                    supportUserVerify = table.Column<bool>(type: "bit", nullable: false),
                    AdminVerify = table.Column<bool>(type: "bit", nullable: false),
                    AgentID = table.Column<string>(type: "varchar(20)", nullable: true),
                    Residential_District = table.Column<string>(type: "varchar(200)", nullable: true),
                    Referral_Code = table.Column<string>(type: "varchar(200)", nullable: true),
                    Type_of_Shop = table.Column<string>(type: "varchar(200)", nullable: true),
                    Business_District = table.Column<string>(type: "varchar(200)", nullable: true),
                    Business_City = table.Column<string>(type: "varchar(200)", nullable: true),
                    Business_State = table.Column<string>(type: "varchar(200)", nullable: true),
                    Business_Pincode = table.Column<string>(type: "varchar(200)", nullable: true),
                    Latitude = table.Column<string>(type: "varchar(200)", nullable: true),
                    Longitude = table.Column<string>(type: "varchar(200)", nullable: true),
                    Shop_Address_Proof_Document = table.Column<string>(type: "varchar(200)", nullable: true),
                    Address_Proof_Document_Number = table.Column<string>(type: "varchar(200)", nullable: true),
                    Operating_Time = table.Column<string>(type: "varchar(200)", nullable: true),
                    Working_Days = table.Column<string>(type: "varchar(200)", nullable: true),
                    Category = table.Column<string>(type: "varchar(200)", nullable: true),
                    Handicapped = table.Column<string>(type: "varchar(200)", nullable: true),
                    Occupation = table.Column<string>(type: "varchar(200)", nullable: true),
                    Alternate_Occupation = table.Column<string>(type: "varchar(200)", nullable: true),
                    Highest_Qualification = table.Column<string>(type: "varchar(200)", nullable: true),
                    Small_Merchant = table.Column<string>(type: "varchar(200)", nullable: true),
                    CreateRole = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<string>(type: "varchar(200)", nullable: true),
                    BeneficiaryName = table.Column<string>(type: "varchar(200)", nullable: true),
                    trade_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    pan_number = table.Column<string>(type: "varchar(200)", nullable: true),
                    account_exists = table.Column<string>(type: "varchar(200)", nullable: true),
                    Address_Proof = table.Column<string>(type: "varchar(200)", nullable: true),
                    IIBF_Certification = table.Column<string>(type: "varchar(200)", nullable: true),
                    IIBF_Certification_Number = table.Column<string>(type: "varchar(200)", nullable: true),
                    GST_Address = table.Column<string>(type: "varchar(200)", nullable: true),
                    VerifiedPage = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUser", x => x.OnboardingUserID);
                });

            migrationBuilder.CreateTable(
                name: "OperatorAmountWiseSetting",
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
                    table.PrimaryKey("PK_OperatorAmountWiseSetting", x => x.OperatorAmountWiseSettingID);
                });

            migrationBuilder.CreateTable(
                name: "OperatorwiseCallbackSetting",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorwiseCallbackSetting", x => x.OperatorwiseCallbackSettingID);
                });

            migrationBuilder.CreateTable(
                name: "OTPMaster",
                columns: table => new
                {
                    OTPMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPMaster", x => x.OTPMasterID);
                });

            migrationBuilder.CreateTable(
                name: "OTTSetting",
                columns: table => new
                {
                    OTTSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    ActionURL = table.Column<string>(type: "varchar(500)", nullable: false),
                    StatusCheckURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTTSetting", x => x.OTTSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PanCardSetting",
                columns: table => new
                {
                    PanCardSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanCardSetting", x => x.PanCardSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGatewayLog",
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
                    OrderID = table.Column<string>(type: "varchar(150)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGatewayLog", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGatewaySetting",
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
                    table.PrimaryKey("PK_PaymentGatewaySetting", x => x.PaymentGatewaySettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentModeMaster",
                columns: table => new
                {
                    PaymentModeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentModeMaster", x => x.PaymentModeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "PayoutBeneficiary",
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
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBeneficiary", x => x.PayoutBeneficaryID);
                });

            migrationBuilder.CreateTable(
                name: "PayoutSetting",
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
                    table.PrimaryKey("PK_PayoutSetting", x => x.PayoutSettingID);
                });

            migrationBuilder.CreateTable(
                name: "PaySprintBank",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaySprintBank", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PlanSetting",
                columns: table => new
                {
                    PlanSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    UserID = table.Column<string>(type: "varchar(500)", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Circle = table.Column<string>(type: "varchar(200)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanSetting", x => x.PlanSettingID);
                });

            migrationBuilder.CreateTable(
                name: "POSSettingMaster",
                columns: table => new
                {
                    POSSettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Password = table.Column<string>(type: "varchar(100)", nullable: true),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    Slat = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AmountWithCommission = table.Column<string>(type: "varchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSSettingMaster", x => x.POSSettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ProductImageMapping",
                columns: table => new
                {
                    ProductImageMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductImageName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    ProductImagePath = table.Column<string>(type: "varchar(500)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImageMapping", x => x.ProductImageMappingID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderCallBackLog",
                columns: table => new
                {
                    ProviderCallBackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderReply = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderCallBackLog", x => x.ProviderCallBackID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderConfigurationSample",
                columns: table => new
                {
                    ConfigurationSampleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "varchar(100)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OpCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfigurationSample", x => x.ConfigurationSampleID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderDetailsSample",
                columns: table => new
                {
                    ProviderDetailsSampleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "varchar(500)", nullable: false),
                    ProviderURL = table.Column<string>(type: "varchar(max)", nullable: false),
                    UrlType = table.Column<string>(type: "varchar(200)", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(500)", nullable: true),
                    DateFormat = table.Column<string>(type: "varchar(200)", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderDetailsSample", x => x.ProviderDetailsSampleID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderMaster",
                columns: table => new
                {
                    ProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "varchar(150)", nullable: false),
                    ProviderUrl = table.Column<string>(type: "varchar(max)", nullable: false),
                    ProviderUniqueID = table.Column<string>(type: "varchar(150)", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsHLR = table.Column<bool>(type: "bit", nullable: false),
                    RechargeBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMTBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ResponseGroupID = table.Column<int>(type: "int", nullable: false),
                    IsRealAPI = table.Column<bool>(type: "bit", nullable: false),
                    LoginID = table.Column<string>(type: "varchar(200)", nullable: true),
                    Token = table.Column<string>(type: "varchar(200)", nullable: true),
                    SupportNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    LowBalanceAmount = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderMaster", x => x.ProviderID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorCircleMaster",
                columns: table => new
                {
                    ProviderOperatorCircleMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    Opcode = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOperatorCircleMaster", x => x.ProviderOperatorCircleMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderOperatorMaster", x => x.ProviderOperatorID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderResponseMasterSample",
                columns: table => new
                {
                    ProviderResponseMasterSampleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "varchar(50)", nullable: false),
                    TextMustexits = table.Column<string>(type: "varchar(250)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    Response = table.Column<string>(type: "varchar(1000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderResponseMasterSample", x => x.ProviderResponseMasterSampleID);
                });

            migrationBuilder.CreateTable(
                name: "ProviderResponseTypeMaster",
                columns: table => new
                {
                    ProviderResponseTypeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "varchar(200)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderResponseTypeMaster", x => x.ProviderResponseTypeMasterID);
                });

            migrationBuilder.CreateTable(
                name: "PureDMTTransferBeneDetail",
                columns: table => new
                {
                    PureDMTTransferBeneDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    BeneID = table.Column<string>(type: "varchar(50)", nullable: true),
                    AccountNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    VerifyBeneName = table.Column<string>(type: "varchar(150)", nullable: true),
                    IsVerify = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PureDMTTransferBeneDetail", x => x.PureDMTTransferBeneDetailID);
                });

            migrationBuilder.CreateTable(
                name: "RedirectUrlMaster",
                columns: table => new
                {
                    RedirectUrlMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    RedirectUrl = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedirectUrlMaster", x => x.RedirectUrlMasterID);
                });

            migrationBuilder.CreateTable(
                name: "RemainderOperatorAmountWise",
                columns: table => new
                {
                    RemainderOperatorAmountWiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemainderOperatorAmountWise", x => x.RemainderOperatorAmountWiseID);
                });

            migrationBuilder.CreateTable(
                name: "ResponseKeyValue",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    Value = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseKeyValue", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RichTextMedia",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: true),
                    Content = table.Column<string>(type: "varchar(max)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RichTextMedia", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SDKSetting",
                columns: table => new
                {
                    SDKSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    MerchantID = table.Column<string>(type: "varchar(100)", nullable: false),
                    Token = table.Column<string>(type: "varchar(200)", nullable: false),
                    URL = table.Column<string>(type: "varchar(200)", nullable: false),
                    PublicKey = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDKSetting", x => x.SDKSettingID);
                });

            migrationBuilder.CreateTable(
                name: "SentSms",
                columns: table => new
                {
                    SentSMSID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", nullable: false),
                    Message = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    SMSMasterID = table.Column<int>(type: "int", nullable: false),
                    TemplateID = table.Column<string>(type: "varchar(100)", nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    Request = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentSms", x => x.SentSMSID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAndUserwiseSubscription",
                columns: table => new
                {
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    SubscriptionType = table.Column<string>(type: "varchar(10)", nullable: true),
                    OriginalCharge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remark = table.Column<string>(type: "varchar(max)", nullable: false),
                    LoginID = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsAutoDebit = table.Column<bool>(type: "bit", nullable: false),
                    ServiceSubscriptionPlanMasterID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(max)", nullable: true),
                    DaysCount = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAndUserwiseSubscription", x => x.ServiceAndUserwiseSubscriptionID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMaster",
                columns: table => new
                {
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDisplay = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    ServiceCategory = table.Column<string>(type: "varchar(100)", nullable: true),
                    HSNCode = table.Column<string>(type: "varchar(200)", nullable: true),
                    HSNCodeForSelfInvoice = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsOutside = table.Column<bool>(type: "bit", nullable: false),
                    IsBBPS = table.Column<bool>(type: "bit", nullable: true),
                    IsSubscriptionAllow = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMaster", x => x.ServiceID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionCreatePlanMaster",
                columns: table => new
                {
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ImageURL = table.Column<string>(type: "varchar(800)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionCreatePlanMaster", x => x.ServiceSubscriptionCreatePlanMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceSubscriptionPlanMaster",
                columns: table => new
                {
                    ServiceSubscriptionPlanMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSubscriptionPlanMaster", x => x.ServiceSubscriptionPlanMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SettingMaster",
                columns: table => new
                {
                    SettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false),
                    IsOn = table.Column<bool>(type: "bit", nullable: false),
                    IsSecure = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsPassword = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingMaster", x => x.SettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SignUp",
                columns: table => new
                {
                    SignUpID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "varchar(10)", nullable: false),
                    PANNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    ReferralCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    EmployeeID = table.Column<string>(type: "varchar(50)", nullable: true),
                    OrderID = table.Column<string>(type: "varchar(50)", nullable: true),
                    ProfileID = table.Column<string>(type: "varchar(200)", nullable: true),
                    RedirectURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignUp", x => x.SignUpID);
                });

            migrationBuilder.CreateTable(
                name: "SlabMaster",
                columns: table => new
                {
                    SlabMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabName = table.Column<string>(type: "varchar(80)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlabMaster", x => x.SlabMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SliderMaster",
                columns: table => new
                {
                    SliderMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SliderPath = table.Column<string>(type: "varchar(max)", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SliderMaster", x => x.SliderMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SmsMaster",
                columns: table => new
                {
                    SMSMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SMSString = table.Column<string>(type: "varchar(max)", nullable: false),
                    BalanceURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    WhatsAppURL = table.Column<string>(type: "varchar(1000)", nullable: true),
                    APIName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    MethodTypeWhatsApp = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJsonWhatsApp = table.Column<string>(type: "varchar(max)", nullable: false),
                    MobileWithOrWithout91 = table.Column<string>(type: "varchar(30)", nullable: true),
                    SMSOTPURL = table.Column<string>(type: "varchar(1000)", nullable: true),
                    IsOTP = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMaster", x => x.SMSMasterID);
                });

            migrationBuilder.CreateTable(
                name: "StateMaster",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(300)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMaster", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionMaster",
                columns: table => new
                {
                    SubscriptionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<string>(type: "varchar(500)", nullable: true),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionMaster", x => x.SubscriptionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SupportUserAccessMaster",
                columns: table => new
                {
                    SupportUserAccessMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MenuGUID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportUserAccessMaster", x => x.SupportUserAccessMasterID);
                });

            migrationBuilder.CreateTable(
                name: "TempTransactionMaster",
                columns: table => new
                {
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempTransactionMaster", x => x.TransactionMasterID);
                });

            migrationBuilder.CreateTable(
                name: "ThirdPartyServiceProvider",
                columns: table => new
                {
                    ThirdPartyServiceProviderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Token = table.Column<string>(type: "varchar(1000)", nullable: false),
                    DomainName = table.Column<string>(type: "varchar(1000)", nullable: false),
                    AppID = table.Column<string>(type: "varchar(1000)", nullable: true),
                    AllowedServices = table.Column<string>(type: "varchar(1000)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdPartyServiceProvider", x => x.ThirdPartyServiceProviderID);
                });

            migrationBuilder.CreateTable(
                name: "TicketCategory",
                columns: table => new
                {
                    TicketCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketCategory", x => x.TicketCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionAPIUserRequestMaster",
                columns: table => new
                {
                    TransactionAPIUserRequestMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ApiUserRequestID = table.Column<string>(type: "varchar(100)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAPIUserRequestMaster", x => x.TransactionAPIUserRequestMasterID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionDetails",
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
                    DOCType = table.Column<string>(type: "varchar(100)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AmountCreditDebit = table.Column<string>(type: "varchar(100)", nullable: false),
                    CommissionCreditDebit = table.Column<string>(type: "varchar(100)", nullable: false),
                    TransactionDescription = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionDetails", x => x.TransactionDetailID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionProviderHistory",
                columns: table => new
                {
                    RechargeProviderHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionProviderHistory", x => x.RechargeProviderHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "UISettingMaster",
                columns: table => new
                {
                    UISettingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(200)", nullable: false),
                    Value = table.Column<string>(type: "varchar(500)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UISettingMaster", x => x.UISettingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "UnitMaster",
                columns: table => new
                {
                    UnitMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMaster", x => x.UnitMasterID);
                });

            migrationBuilder.CreateTable(
                name: "UPITransferSetting",
                columns: table => new
                {
                    UPITransferSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    DisplayName = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: true),
                    CallBackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(500)", nullable: true),
                    SenderLimit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AuthCode = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsCommonUPI = table.Column<bool>(type: "bit", nullable: false),
                    IsUPITransferWithSender = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UPITransferSetting", x => x.UPITransferSettingID);
                });

            migrationBuilder.CreateTable(
                name: "UrlTypeMaster",
                columns: table => new
                {
                    UrlTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrlType = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlTypeMaster", x => x.UrlTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserBalanceHistory",
                columns: table => new
                {
                    UserBalanceHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalanceHistory", x => x.UserBalanceHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "UserPaymentCollectMapping",
                columns: table => new
                {
                    UserPaymentCollectMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "varchar(50)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "varchar(50)", nullable: true),
                    Value = table.Column<string>(type: "varchar(max)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentCollectMapping", x => x.UserPaymentCollectMappingID);
                });

            migrationBuilder.CreateTable(
                name: "UserPlanUtilization",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlanUtilization", x => x.UserPlanUtilizationID);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceCharge",
                columns: table => new
                {
                    UserServiceChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceCharge", x => x.UserServiceChargeID);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceSubscriptionCharge",
                columns: table => new
                {
                    UserServiceSubscriptionChargeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(500)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsAdminApproved = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceSubscriptionCharge", x => x.UserServiceSubscriptionChargeID);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptionPlanMapping",
                columns: table => new
                {
                    UserSubscriptionPlanMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceSubscriptionCreatePlanMasterID = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(500)", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptionPlanMapping", x => x.UserSubscriptionPlanMappingID);
                });

            migrationBuilder.CreateTable(
                name: "UserTypeMaster",
                columns: table => new
                {
                    UserTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserType = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsOutside = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AliasName = table.Column<string>(type: "varchar(10)", nullable: true),
                    FundTransferMinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypeMaster", x => x.UserTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePlanLimit",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePlanLimit", x => x.UserWisePlanLimitID);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePlanView",
                columns: table => new
                {
                    UserWisePlanViewID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePlanView", x => x.UserWisePlanViewID);
                });

            migrationBuilder.CreateTable(
                name: "VerificationSetting",
                columns: table => new
                {
                    VerificationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActiveForWebsite = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", nullable: true),
                    CallBackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserName = table.Column<string>(type: "varchar(200)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    AuthCode = table.Column<string>(type: "varchar(200)", nullable: true),
                    Mode = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsAccountVerify = table.Column<bool>(type: "bit", nullable: false),
                    IsUPIVerify = table.Column<bool>(type: "bit", nullable: false),
                    IsPayoutVerify = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationSetting", x => x.VerificationSettingID);
                });

            migrationBuilder.CreateTable(
                name: "VirtualAccountMaster",
                columns: table => new
                {
                    VirtualAccountMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(100)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualAccountMaster", x => x.VirtualAccountMasterID);
                });

            migrationBuilder.CreateTable(
                name: "VirtualBalance",
                columns: table => new
                {
                    VirtualBalanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    DOCType = table.Column<string>(type: "varchar(100)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualBalance", x => x.VirtualBalanceID);
                });

            migrationBuilder.CreateTable(
                name: "VoucherType",
                columns: table => new
                {
                    VoucherTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeName = table.Column<string>(type: "varchar(30)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherType", x => x.VoucherTypeID);
                });

            migrationBuilder.CreateTable(
                name: "WalletMaster",
                columns: table => new
                {
                    WalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveForPaymentGateway = table.Column<bool>(type: "bit", nullable: false),
                    IsValidForTransfer = table.Column<bool>(type: "bit", nullable: false),
                    ShortName = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsWalletTransferEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UISequence = table.Column<int>(type: "int", nullable: false),
                    IsActiveForStaticQR = table.Column<bool>(type: "bit", nullable: false),
                    IsActiveForeCollection = table.Column<bool>(type: "bit", nullable: false),
                    IsValidUserCreation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletMaster", x => x.WalletID);
                });

            migrationBuilder.CreateTable(
                name: "WebHookLog",
                columns: table => new
                {
                    WebHookLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookLog", x => x.WebHookLogID);
                });

            migrationBuilder.CreateTable(
                name: "WebHookSetting",
                columns: table => new
                {
                    WebHookSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    URL = table.Column<string>(type: "varchar(max)", nullable: false),
                    MethodType = table.Column<string>(type: "varchar(30)", nullable: false),
                    HeaderJson = table.Column<string>(type: "varchar(max)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookSetting", x => x.WebHookSettingID);
                });

            migrationBuilder.CreateTable(
                name: "WrongPasswordHistory",
                columns: table => new
                {
                    WrongPasswordHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WrongPasswordHistory", x => x.WrongPasswordHistoryID);
                });

            migrationBuilder.CreateTable(
                name: "ZoneMaster",
                columns: table => new
                {
                    ZoneMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(300)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneMaster", x => x.ZoneMasterID);
                });

            migrationBuilder.CreateTable(
                name: "SubCategory",
                columns: table => new
                {
                    SubCategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    SubCategoryName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    ShortDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    LongDescription = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategory", x => x.SubCategoryID);
                    table.ForeignKey(
                        name: "FK_SubCategory_Category_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Category",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryChannelMapping",
                columns: table => new
                {
                    CategoryChannelMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelCategoryID = table.Column<int>(type: "int", nullable: false),
                    ChannelName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ChannelImagePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryChannelMapping", x => x.CategoryChannelMappingID);
                    table.ForeignKey(
                        name: "FK_CategoryChannelMapping_ChannelCategory_ChannelCategoryID",
                        column: x => x.ChannelCategoryID,
                        principalTable: "ChannelCategory",
                        principalColumn: "ChannelCategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingUserDocument",
                columns: table => new
                {
                    OnboardingUserDocumentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ref_ID = table.Column<string>(type: "varchar(500)", nullable: true),
                    Type = table.Column<string>(type: "varchar(500)", nullable: false),
                    OldImage = table.Column<string>(type: "varchar(500)", nullable: true),
                    NewImage = table.Column<string>(type: "varchar(500)", nullable: true),
                    OnboardingUserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingUserDocument", x => x.OnboardingUserDocumentID);
                    table.ForeignKey(
                        name: "FK_OnboardingUserDocument_OnboardingUser_OnboardingUserID",
                        column: x => x.OnboardingUserID,
                        principalTable: "OnboardingUser",
                        principalColumn: "OnboardingUserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutVendorBeneficiaryMapping",
                columns: table => new
                {
                    PayoutVendorBeneficiaryMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    PayoutVendorBeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: true),
                    PayoutBeneficaryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutVendorBeneficiaryMapping", x => x.PayoutVendorBeneficiaryMappingID);
                    table.ForeignKey(
                        name: "FK_PayoutVendorBeneficiaryMapping_PayoutBeneficiary_PayoutBeneficaryID",
                        column: x => x.PayoutBeneficaryID,
                        principalTable: "PayoutBeneficiary",
                        principalColumn: "PayoutBeneficaryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderResponseMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderResponseMaster", x => x.ProviderResponseMasterID);
                    table.ForeignKey(
                        name: "FK_ProviderResponseMaster_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderStateConfiguration",
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
                    table.PrimaryKey("PK_ProviderStateConfiguration", x => x.ProviderStateConfigurationID);
                    table.ForeignKey(
                        name: "FK_ProviderStateConfiguration_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationSetting",
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
                    table.PrimaryKey("PK_ReconciliationSetting", x => x.ReconciliationSettingID);
                    table.ForeignKey(
                        name: "FK_ReconciliationSetting_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DematGrowthLossMaster",
                columns: table => new
                {
                    DematGrowthLossMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceSubscriptionPlanMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    GrowthLoss = table.Column<string>(type: "varchar(100)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    APIProviderId = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    ManualDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManualDOCInt = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DematGrowthLossMaster", x => x.DematGrowthLossMasterID);
                    table.ForeignKey(
                        name: "FK_DematGrowthLossMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorMaster",
                columns: table => new
                {
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MinimumRechargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumRechargeAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RepeatRechargeInMinute = table.Column<int>(type: "int", nullable: false),
                    IsTimelyBlocked = table.Column<bool>(type: "bit", nullable: true),
                    BlockedTill = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRandom = table.Column<bool>(type: "bit", nullable: true),
                    OperatorImagePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    Opcode = table.Column<string>(type: "varchar(50)", nullable: true),
                    BusinessModel = table.Column<string>(type: "varchar(50)", nullable: true),
                    NumberLength = table.Column<int>(type: "int", nullable: false),
                    HSNCode = table.Column<string>(type: "varchar(max)", nullable: true),
                    Prefix = table.Column<string>(type: "varchar(max)", nullable: true),
                    AccountStartWith = table.Column<string>(type: "varchar(max)", nullable: true),
                    SequenceLogic = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsBBPS = table.Column<bool>(type: "bit", nullable: true),
                    MinLength = table.Column<int>(type: "int", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: false),
                    AccountName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: true),
                    TollFreeNumber = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsBillFetch = table.Column<bool>(type: "bit", nullable: false),
                    IsPartialBill = table.Column<bool>(type: "bit", nullable: false),
                    UISequence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlockedAmount = table.Column<string>(type: "varchar(max)", nullable: true),
                    AllowedFor = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsViewRequired = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorMaster", x => x.OperatorID);
                    table.ForeignKey(
                        name: "FK_OperatorMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentGateway",
                columns: table => new
                {
                    PaymentGatewayID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<string>(type: "varchar(200)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GatewayCharge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(200)", nullable: false),
                    TransactionID = table.Column<string>(type: "varchar(200)", nullable: true),
                    PayMode = table.Column<string>(type: "varchar(200)", nullable: true),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    PaymentProviderID = table.Column<int>(type: "int", nullable: false),
                    IsUpdate = table.Column<int>(type: "int", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: true),
                    DistributerUserID = table.Column<int>(type: "int", nullable: false),
                    SuperDistributerUserID = table.Column<int>(type: "int", nullable: false),
                    DistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ReferenceID = table.Column<string>(type: "varchar(100)", nullable: true),
                    Sender = table.Column<string>(type: "varchar(500)", nullable: true),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(200)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentGateway", x => x.PaymentGatewayID);
                    table.ForeignKey(
                        name: "FK_PaymentGateway_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
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
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TransactionIDPrefix = table.Column<string>(type: "varchar(20)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderDetails", x => x.ProviderDetailID);
                    table.ForeignKey(
                        name: "FK_ProviderDetails_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderDetails_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceWiseSliderSetting",
                columns: table => new
                {
                    ServiceWiseSliderSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    ReferenceLink = table.Column<string>(type: "varchar(500)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "varchar(10)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceWiseSliderSetting", x => x.ServiceWiseSliderSettingID);
                    table.ForeignKey(
                        name: "FK_ServiceWiseSliderSetting_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationMaster",
                columns: table => new
                {
                    AuthorizationMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Controller = table.Column<string>(type: "varchar(150)", nullable: false),
                    Action = table.Column<string>(type: "varchar(150)", nullable: false),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationMaster", x => x.AuthorizationMasterID);
                    table.ForeignKey(
                        name: "FK_AuthorizationMaster_UserTypeMaster_UserTypeID",
                        column: x => x.UserTypeID,
                        principalTable: "UserTypeMaster",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMaster",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirmName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Password = table.Column<string>(type: "varchar(50)", nullable: false),
                    EmailID = table.Column<string>(type: "varchar(70)", nullable: false),
                    GSTNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    PANNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    CIN = table.Column<string>(type: "varchar(20)", nullable: true),
                    AadharNo = table.Column<string>(type: "varchar(16)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsInitialPassword = table.Column<bool>(type: "bit", nullable: false),
                    UniqueReferenceNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    Mpin = table.Column<string>(type: "varchar(6)", nullable: false),
                    IsKYCApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsLoginOTP = table.Column<bool>(type: "bit", nullable: false),
                    AppFCMToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    WebFCMToken = table.Column<string>(type: "varchar(max)", nullable: true),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    APIToken = table.Column<string>(type: "varchar(100)", nullable: true),
                    UDD = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsWeb = table.Column<bool>(type: "bit", nullable: false),
                    IsApp = table.Column<bool>(type: "bit", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProfileImagePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsIPSecure = table.Column<bool>(type: "bit", nullable: false),
                    AllowRealAPI = table.Column<bool>(type: "bit", nullable: false),
                    IsChannel = table.Column<bool>(type: "bit", nullable: false),
                    IsReverseRechargeApplicable = table.Column<bool>(type: "bit", nullable: false),
                    VirtualAccountNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsInvoiceApplicable = table.Column<bool>(type: "bit", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(200)", nullable: true),
                    JoiningByUserID = table.Column<int>(type: "int", nullable: false),
                    RegistrationType = table.Column<string>(type: "varchar(100)", nullable: true),
                    InstantPayOutletID = table.Column<string>(type: "varchar(100)", nullable: true),
                    Comm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    CommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsTwoFA = table.Column<bool>(type: "bit", nullable: false),
                    TwoFASecretKey = table.Column<string>(type: "varchar(100)", nullable: true),
                    CityName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Pincode = table.Column<string>(type: "varchar(100)", nullable: true),
                    WalletIDforEcollection = table.Column<int>(type: "int", nullable: false),
                    StaticQRWalletID = table.Column<int>(type: "int", nullable: false),
                    AliasName = table.Column<string>(type: "varchar(100)", nullable: true),
                    MahagramAEPSAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    MahagramBBPSAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    PaysprintAEPSAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    EkoAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    FingpayAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    ApiClubAgentID = table.Column<string>(type: "varchar(15)", nullable: true),
                    ICICI_Static_GUID = table.Column<string>(type: "varchar(15)", nullable: true),
                    ResponseType = table.Column<string>(type: "varchar(50)", nullable: true),
                    PayoutAccountCount = table.Column<int>(type: "int", nullable: false),
                    IsPin = table.Column<bool>(type: "bit", nullable: false),
                    WhatsAppNumber = table.Column<string>(type: "varchar(21)", maxLength: 21, nullable: true),
                    NumberFirst = table.Column<bool>(type: "bit", nullable: false),
                    GSTHoldCutting = table.Column<string>(type: "varchar(50)", nullable: true),
                    PayOneMoneyAgentCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    GSTPrinciple = table.Column<string>(type: "varchar(100)", nullable: true),
                    EzyPayRetailerID = table.Column<string>(type: "varchar(30)", nullable: true),
                    ReferralCode = table.Column<string>(type: "varchar(5)", nullable: true),
                    ReferralAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ReferralBenefitMinAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IseKYC = table.Column<bool>(type: "bit", nullable: false),
                    IsCommissionHold = table.Column<bool>(type: "bit", nullable: false),
                    BankITAgentCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    CredoPay_LoginID = table.Column<string>(type: "varchar(100)", nullable: true),
                    CredoPay_Password = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsReferralPopup = table.Column<bool>(type: "bit", nullable: false),
                    MorePayMerchantCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsMorePayEKYC = table.Column<bool>(type: "bit", nullable: false),
                    RoyalPayAgentCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    RMGPayAgentCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    ShopAddress = table.Column<string>(type: "varchar(max)", nullable: true),
                    NPCIAgentID = table.Column<string>(type: "varchar(100)", nullable: true),
                    PaySwiffMerchantId = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaster", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_UserMaster_StateMaster_StateID",
                        column: x => x.StateID,
                        principalTable: "StateMaster",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMaster_UserTypeMaster_UserTypeID",
                        column: x => x.UserTypeID,
                        principalTable: "UserTypeMaster",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoucherMaster",
                columns: table => new
                {
                    VoucherMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeID = table.Column<int>(type: "int", nullable: false),
                    SelectDate = table.Column<string>(type: "varchar(70)", nullable: false),
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherMaster", x => x.VoucherMasterID);
                    table.ForeignKey(
                        name: "FK_VoucherMaster_VoucherType_VoucherTypeID",
                        column: x => x.VoucherTypeID,
                        principalTable: "VoucherType",
                        principalColumn: "VoucherTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderPurchase",
                columns: table => new
                {
                    ProviderPurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(100)", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseDOCInt = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderPurchase", x => x.ProviderPurchaseID);
                    table.ForeignKey(
                        name: "FK_ProviderPurchase_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderPurchase_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletServiceMapping",
                columns: table => new
                {
                    WalletServiceMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletServiceMapping", x => x.WalletServiceMappingID);
                    table.ForeignKey(
                        name: "FK_WalletServiceMapping_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletServiceMapping_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    ProductDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryID = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    BuyingPrice = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalStock = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.ProductDetailsID);
                    table.ForeignKey(
                        name: "FK_ProductDetails_SubCategory_SubCategoryID",
                        column: x => x.SubCategoryID,
                        principalTable: "SubCategory",
                        principalColumn: "SubCategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BBPSConfiguration",
                columns: table => new
                {
                    BBPSConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceHolder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMeeage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    ConfigList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDisplay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BBPSConfiguration", x => x.BBPSConfigurationID);
                    table.ForeignKey(
                        name: "FK_BBPSConfiguration_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChannelSlabDetail",
                columns: table => new
                {
                    ChannelSlabDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ChannelSlabMasterID1 = table.Column<int>(type: "int", nullable: true),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CommissionType = table.Column<string>(type: "varchar(100)", nullable: true),
                    FromAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ToAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    RealCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealCommissionType = table.Column<string>(type: "varchar(100)", nullable: true),
                    GeneralCommissionCategory = table.Column<string>(type: "varchar(50)", nullable: true),
                    RealCommissionCategory = table.Column<string>(type: "varchar(50)", nullable: true),
                    UplineCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSlabDetail", x => x.ChannelSlabDetailID);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetail_ChannelSlabMaster_ChannelSlabMasterID",
                        column: x => x.ChannelSlabMasterID,
                        principalTable: "ChannelSlabMaster",
                        principalColumn: "ChannelSlabMasterID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetail_ChannelSlabMaster_ChannelSlabMasterID1",
                        column: x => x.ChannelSlabMasterID1,
                        principalTable: "ChannelSlabMaster",
                        principalColumn: "ChannelSlabMasterID");
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetail_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChannelSlabDetail_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseCommission",
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
                    table.PrimaryKey("PK_CircleWiseCommission", x => x.CircleWiseCommissionID);
                    table.ForeignKey(
                        name: "FK_CircleWiseCommission_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseCommission_SlabMaster_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMaster",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LapuMaster",
                columns: table => new
                {
                    LapuMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LapuPartyMasterID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    LapuNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    ProivderLapuID = table.Column<string>(type: "varchar(50)", nullable: true),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LapuMaster", x => x.LapuMasterID);
                    table.ForeignKey(
                        name: "FK_LapuMaster_LapuPartyMaster_LapuPartyMasterID",
                        column: x => x.LapuPartyMasterID,
                        principalTable: "LapuPartyMaster",
                        principalColumn: "LapuPartyMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LapuMaster_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineProductMaster",
                columns: table => new
                {
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    PackageMRP = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    BookingAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Validity = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    OpType = table.Column<string>(type: "varchar(100)", nullable: true),
                    Remark = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineProductMaster", x => x.OfflineProductMasterID);
                    table.ForeignKey(
                        name: "FK_OfflineProductMaster_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineProductMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorSequence",
                columns: table => new
                {
                    OperatorSequenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSequence", x => x.OperatorSequenceID);
                    table.ForeignKey(
                        name: "FK_OperatorSequence_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequence_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequence_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorStateMapping",
                columns: table => new
                {
                    OperatorStateMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ZoneMasterID = table.Column<int>(type: "int", nullable: false),
                    HLR = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorStateMapping", x => x.OperatorStateMappingID);
                    table.ForeignKey(
                        name: "FK_OperatorStateMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderConfiguration",
                columns: table => new
                {
                    ConfigurationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OpCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", nullable: true),
                    Lapu = table.Column<string>(type: "varchar(200)", nullable: true),
                    Quantity = table.Column<string>(type: "varchar(3000)", nullable: true),
                    AdminComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminCommType = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CommissionCategory = table.Column<string>(type: "varchar(20)", nullable: true),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    PendingInMinute = table.Column<int>(type: "int", nullable: false),
                    FailCount = table.Column<int>(type: "int", nullable: false),
                    FailInMinute = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfiguration", x => x.ConfigurationID);
                    table.ForeignKey(
                        name: "FK_ProviderConfiguration_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfiguration_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfiguration_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderConfigurationAdminCommission",
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
                    AdminCommissionCategory = table.Column<string>(type: "varchar(20)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderConfigurationAdminCommission", x => x.ProviderConfigurationAdminCommissionID);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommission_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommission_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderConfigurationAdminCommission_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderOperatorDenominationMapping",
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
                    table.PrimaryKey("PK_ProviderOperatorDenominationMapping", x => x.ProviderOperatorDenominationMappingID);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMapping_DenominationMaster_DenominationMasterID",
                        column: x => x.DenominationMasterID,
                        principalTable: "DenominationMaster",
                        principalColumn: "DenominationMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProviderOperatorDenominationMapping_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RangeWiseSequence",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangeWiseSequence", x => x.RangeWiseSequenceID);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequence_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequence_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RangeWiseSequence_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RealProviderMapping",
                columns: table => new
                {
                    RealProviderMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealProviderMapping", x => x.RealProviderMappingID);
                    table.ForeignKey(
                        name: "FK_RealProviderMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ROfferCommission",
                columns: table => new
                {
                    ROfferCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
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
                    ClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROfferCommission", x => x.ROfferCommissionID);
                    table.ForeignKey(
                        name: "FK_ROfferCommission_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ROfferCommission_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ROfferCommission_SlabMaster_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMaster",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlabDetail",
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
                    ClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealRetailerComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealRetailerCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealRetailerCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealDistributorCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealDistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealSuperDistributorComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealSuperDistributorCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealSuperDistributorCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealClientComm = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RealClientCommType = table.Column<string>(type: "varchar(500)", nullable: true),
                    RealClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlabDetail", x => x.SlabDetailID);
                    table.ForeignKey(
                        name: "FK_SlabDetail_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlabDetail_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlabDetail_SlabMaster_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMaster",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ViewProviderMapping",
                columns: table => new
                {
                    ViewProviderMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewProviderMapping", x => x.ViewProviderMappingID);
                    table.ForeignKey(
                        name: "FK_ViewProviderMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APIUserCallbackRequestResponseLog",
                columns: table => new
                {
                    APIUserCallbackRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Request = table.Column<string>(type: "varchar(max)", nullable: true),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: true),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIUserCallbackRequestResponseLog", x => x.APIUserCallbackRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_APIUserCallbackRequestResponseLog_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "APIUserInitialRequestResponseLog",
                columns: table => new
                {
                    APIUserInitialRequestResponseLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Request = table.Column<string>(type: "varchar(max)", nullable: true),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    APIUserRequestID = table.Column<string>(type: "varchar(100)", nullable: true),
                    ConsumerNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIUserInitialRequestResponseLog", x => x.APIUserInitialRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_APIUserInitialRequestResponseLog_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutoTransferUserwiseMapping",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoTransferUserwiseMapping", x => x.AutoTransferUserwiseMappingID);
                    table.ForeignKey(
                        name: "FK_AutoTransferUserwiseMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankRequestResponseLog",
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
                    ServiceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankRequestResponseLog", x => x.BankRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_BankRequestResponseLog_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallbackMaster",
                columns: table => new
                {
                    CallbackMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CallbackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallbackMaster", x => x.CallbackMasterID);
                    table.ForeignKey(
                        name: "FK_CallbackMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallbackMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CallMeRequest",
                columns: table => new
                {
                    CallMeRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CallMeCategoryMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    UplineRemark = table.Column<string>(type: "varchar(200)", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallMeRequest", x => x.CallMeRequestID);
                    table.ForeignKey(
                        name: "FK_CallMeRequest_CallMeCategoryMaster_CallMeCategoryMasterID",
                        column: x => x.CallMeCategoryMasterID,
                        principalTable: "CallMeCategoryMaster",
                        principalColumn: "CallMeCategoryMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallMeRequest_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashCollectionMaster",
                columns: table => new
                {
                    CashCollectionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Outstanding = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaxOutstanding = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCollectionMaster", x => x.CashCollectionMasterID);
                    table.ForeignKey(
                        name: "FK_CashCollectionMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseOperatorLimit",
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
                    table.PrimaryKey("PK_CircleWiseOperatorLimit", x => x.CircleWiseOperatorLimitID);
                    table.ForeignKey(
                        name: "FK_CircleWiseOperatorLimit_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseOperatorLimit_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CircleWiseUserWiseOperatorOff",
                columns: table => new
                {
                    CircleWiseUserWiseOperatorOffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CircleWiseUserWiseOperatorOff", x => x.CircleWiseUserWiseOperatorOffID);
                    table.ForeignKey(
                        name: "FK_CircleWiseUserWiseOperatorOff_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CircleWiseUserWiseOperatorOff_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintMaster", x => x.ComplaintMasterID);
                    table.ForeignKey(
                        name: "FK_ComplaintMaster_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailyAuthenticationMaster",
                columns: table => new
                {
                    AuthenticationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    Pipe = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAuthenticationMaster", x => x.AuthenticationID);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMaster_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyAuthenticationMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
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
                    VirtualACCode = table.Column<string>(type: "varchar(30)", nullable: true),
                    UTR = table.Column<string>(type: "varchar(50)", nullable: true),
                    PayeeAccountNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", nullable: true),
                    Reason = table.Column<string>(type: "varchar(100)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcollectionDetails", x => x.EcollectionDetailsID);
                    table.ForeignKey(
                        name: "FK_EcollectionDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundTransaction",
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
                    UTRNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    Status = table.Column<string>(type: "varchar(100)", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(100)", nullable: true),
                    PayMode = table.Column<string>(type: "varchar(100)", nullable: true),
                    Through = table.Column<string>(type: "varchar(100)", nullable: true),
                    RequestToID = table.Column<int>(type: "int", nullable: false),
                    CashCollectionRemark = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true),
                    BankName = table.Column<string>(type: "varchar(200)", nullable: true),
                    ReceiptPath = table.Column<string>(type: "varchar(200)", nullable: true),
                    TransactionDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDOCInt = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    RequestAcceptedUserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransaction", x => x.FundTransactionID);
                    table.ForeignKey(
                        name: "FK_FundTransaction_UserMaster_ReceiverID",
                        column: x => x.ReceiverID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransaction_UserMaster_RequestToID",
                        column: x => x.RequestToID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransaction_UserMaster_SenderID",
                        column: x => x.SenderID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundTransaction_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GSTHold",
                columns: table => new
                {
                    GSTHoldID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    HoldAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GSTUploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceiptPath = table.Column<string>(type: "varchar(max)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    GSTUploadRemark = table.Column<string>(type: "varchar(max)", nullable: true),
                    AdminRemark = table.Column<string>(type: "varchar(max)", nullable: true),
                    GSTNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    TaxableValue = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSTHold", x => x.GSTHoldID);
                    table.ForeignKey(
                        name: "FK_GSTHold_StateMaster_StateID",
                        column: x => x.StateID,
                        principalTable: "StateMaster",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GSTHold_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inbox",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbox", x => x.InboxID);
                    table.ForeignKey(
                        name: "FK_Inbox_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inbox_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inbox_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberHistory",
                columns: table => new
                {
                    InvoiceNumberHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCategory = table.Column<string>(type: "varchar(50)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(16)", nullable: true),
                    InvoiceType = table.Column<string>(type: "varchar(16)", nullable: true),
                    IncrementValue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberHistory", x => x.InvoiceNumberHistoryID);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberHistory_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IPMaster",
                columns: table => new
                {
                    IpMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IP = table.Column<string>(type: "varchar(100)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPMaster", x => x.IpMasterID);
                    table.ForeignKey(
                        name: "FK_IPMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistrory",
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
                    SimCardDetails = table.Column<string>(type: "varchar(max)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistrory", x => x.LoginHistoryID);
                    table.ForeignKey(
                        name: "FK_LoginHistrory_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LowBalanceConfiguration",
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
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LowBalanceConfiguration", x => x.LowBalanceConfigurationID);
                    table.ForeignKey(
                        name: "FK_LowBalanceConfiguration_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LowBalanceConfiguration_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationalHistory",
                columns: table => new
                {
                    OperationalHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    APIType = table.Column<string>(type: "varchar(150)", nullable: false),
                    DocNo = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalHistory", x => x.OperationalHistoryID);
                    table.ForeignKey(
                        name: "FK_OperationalHistory_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorSequenceUserWise",
                columns: table => new
                {
                    OperatorSequenceUserWiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorSequenceUserWise", x => x.OperatorSequenceUserWiseID);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWise_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWise_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWise_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorSequenceUserWise_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperatorUserwise",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorUserwise", x => x.OperatorUserwiseID);
                    table.ForeignKey(
                        name: "FK_OperatorUserwise_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorUserwise_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OperatorUserwise_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderHeader",
                columns: table => new
                {
                    OrderHeaderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "varchar(500)", nullable: false),
                    OrderPhoneNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    OrderEmail = table.Column<string>(type: "varchar(100)", nullable: false),
                    OrderStatus = table.Column<string>(type: "varchar(100)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "varchar(100)", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "varchar(100)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHeader", x => x.OrderHeaderID);
                    table.ForeignKey(
                        name: "FK_OrderHeader_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OverdraftDetail",
                columns: table => new
                {
                    OverdraftDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    OverdraftOpening = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OverdraftAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OverdraftClosing = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    UserRemark = table.Column<string>(type: "varchar(100)", nullable: true),
                    AdminRemark = table.Column<string>(type: "varchar(100)", nullable: true),
                    Status = table.Column<string>(type: "varchar(100)", nullable: false),
                    OpeningWalletBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingWalletBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdraftDetail", x => x.OverdraftDetailID);
                    table.ForeignKey(
                        name: "FK_OverdraftDetail_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdraftDetail_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OverdraftMaster",
                columns: table => new
                {
                    OverdraftMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Overdraft = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaxOverdraft = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdraftMaster", x => x.OverdraftMasterID);
                    table.ForeignKey(
                        name: "FK_OverdraftMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdraftMaster_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PanMitraOnboardedUser",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    VleID = table.Column<string>(type: "varchar(150)", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanMitraOnboardedUser", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PanMitraOnboardedUser_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordChangeHistory",
                columns: table => new
                {
                    PasswordChangeHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(150)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordChangeHistory", x => x.PasswordChangeHistoryID);
                    table.ForeignKey(
                        name: "FK_PasswordChangeHistory_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayoutBeneficiaryOTP",
                columns: table => new
                {
                    PayoutBeneficiaryOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTP = table.Column<string>(type: "varchar(10)", nullable: false),
                    PayoutBeneficaryID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutBeneficiaryOTP", x => x.PayoutBeneficiaryOTPID);
                    table.ForeignKey(
                        name: "FK_PayoutBeneficiaryOTP_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaysprintOnboardedUser",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AgentCode = table.Column<string>(type: "varchar(150)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaysprintOnboardedUser", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PaysprintOnboardedUser_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrepaidActivation",
                columns: table => new
                {
                    PrepaidActivationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ActivationType = table.Column<string>(type: "varchar(100)", nullable: true),
                    AlternateNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    Name = table.Column<string>(type: "varchar(100)", nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    Pincode = table.Column<string>(type: "varchar(50)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrepaidActivation", x => x.PrepaidActivationID);
                    table.ForeignKey(
                        name: "FK_PrepaidActivation_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderRequestResponseLog",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderRequestResponseLog", x => x.ProviderRequestResponseLogID);
                    table.ForeignKey(
                        name: "FK_ProviderRequestResponseLog_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderUserMapping",
                columns: table => new
                {
                    ProviderUserMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "varchar(max)", nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderUserMapping", x => x.ProviderUserMappingID);
                    table.ForeignKey(
                        name: "FK_ProviderUserMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Redeemcommission",
                columns: table => new
                {
                    RedeemcommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    AdminRemark = table.Column<string>(type: "varchar(200)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    LastYearMonth = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redeemcommission", x => x.RedeemcommissionID);
                    table.ForeignKey(
                        name: "FK_Redeemcommission_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReferralRegistration",
                columns: table => new
                {
                    ReferralRegistrationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    MobileNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ReferralCode = table.Column<string>(type: "varchar(5)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsBenefitUtilize = table.Column<bool>(type: "bit", nullable: false),
                    ReferralAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRegistration", x => x.ReferralRegistrationID);
                    table.ForeignKey(
                        name: "FK_ReferralRegistration_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokenMaster",
                columns: table => new
                {
                    RefreshTokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefreshToken = table.Column<string>(type: "varchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokenMaster", x => x.RefreshTokenID);
                    table.ForeignKey(
                        name: "FK_RefreshTokenMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SDKBBPSUserMapping",
                columns: table => new
                {
                    SDKBBPSUserMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ConfirmationCallbackURL = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDKBBPSUserMapping", x => x.SDKBBPSUserMappingID);
                    table.ForeignKey(
                        name: "FK_SDKBBPSUserMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sender",
                columns: table => new
                {
                    SenderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsKYC = table.Column<bool>(type: "bit", nullable: false),
                    KYCDocumentURL = table.Column<string>(type: "varchar(2000)", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    Pincode = table.Column<string>(type: "varchar(10)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sender", x => x.SenderID);
                    table.ForeignKey(
                        name: "FK_Sender_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportUserAccessDetail",
                columns: table => new
                {
                    SupportUserAccessDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Controller = table.Column<string>(type: "varchar(100)", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", nullable: true),
                    MenuGUID = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportUserAccessDetail", x => x.SupportUserAccessDetailID);
                    table.ForeignKey(
                        name: "FK_SupportUserAccessDetail_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetSaleCommission",
                columns: table => new
                {
                    TargetSaleCommissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    SaleAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    SaleMonthDOC = table.Column<string>(type: "varchar(200)", nullable: true),
                    Status = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetSaleCommission", x => x.TargetSaleCommissionID);
                    table.ForeignKey(
                        name: "FK_TargetSaleCommission_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TargetSaleCommission_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketMaster",
                columns: table => new
                {
                    TicketMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketCategoryID = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadFilePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", nullable: true),
                    Resolution = table.Column<string>(type: "varchar(max)", nullable: true),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ResolutionByUserID = table.Column<int>(type: "int", nullable: false),
                    UserTyepID = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMaster", x => x.TicketMasterID);
                    table.ForeignKey(
                        name: "FK_TicketMaster_TicketCategory_TicketCategoryID",
                        column: x => x.TicketCategoryID,
                        principalTable: "TicketCategory",
                        principalColumn: "TicketCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketMaster_UserTypeMaster_UserTyepID",
                        column: x => x.UserTyepID,
                        principalTable: "UserTypeMaster",
                        principalColumn: "UserTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustableDevice",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Browser = table.Column<string>(type: "varchar(50)", nullable: false),
                    Device = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustableDevice", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrustableDevice_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UpiSender",
                columns: table => new
                {
                    UpiSenderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    Pincode = table.Column<string>(type: "varchar(10)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpiSender", x => x.UpiSenderID);
                    table.ForeignKey(
                        name: "FK_UpiSender_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActionHistory",
                columns: table => new
                {
                    UserHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(500)", nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: true),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionHistory", x => x.UserHistoryID);
                    table.ForeignKey(
                        name: "FK_UserActionHistory_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserActionMaster",
                columns: table => new
                {
                    UserActionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(500)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    Platform = table.Column<string>(type: "varchar(50)", nullable: false),
                    FingurePrint = table.Column<string>(type: "varchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionMaster", x => x.UserActionMasterID);
                    table.ForeignKey(
                        name: "FK_UserActionMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBalanceCapping",
                columns: table => new
                {
                    UserBalanceCappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalanceCapping", x => x.UserBalanceCappingID);
                    table.ForeignKey(
                        name: "FK_UserBalanceCapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBalanceCapping_WalletMaster_WalletID",
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
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBankDetails", x => x.UserBankDetailsID);
                    table.ForeignKey(
                        name: "FK_UserBankDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserChannelSlabMapping",
                columns: table => new
                {
                    UserChannelSlabMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ChannelSlabMasterID = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    UplineUserID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannelSlabMapping", x => x.UserChannelSlabMappingID);
                    table.ForeignKey(
                        name: "FK_UserChannelSlabMapping_ChannelSlabMaster_ChannelSlabMasterID",
                        column: x => x.ChannelSlabMasterID,
                        principalTable: "ChannelSlabMaster",
                        principalColumn: "ChannelSlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserChannelSlabMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationMaster", x => x.UserCreationMasterID);
                    table.ForeignKey(
                        name: "FK_UserCreationMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCreationMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCreationMaster_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationSetting",
                columns: table => new
                {
                    UserCreationSettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MinimumCount = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationSetting", x => x.UserCreationSettingID);
                    table.ForeignKey(
                        name: "FK_UserCreationSetting_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCreationWallet",
                columns: table => new
                {
                    UserCreationWalletID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCount = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCreationWallet", x => x.UserCreationWalletID);
                    table.ForeignKey(
                        name: "FK_UserCreationWallet_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDocumentMaster",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocumentMaster", x => x.UserDocumentID);
                    table.ForeignKey(
                        name: "FK_UserDocumentMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMerchantMaster",
                columns: table => new
                {
                    UserMerchantMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PaySprintUPIMerchantCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    PaySprintStaticQR = table.Column<string>(type: "varchar(200)", nullable: true),
                    PaySprintUPIRefid = table.Column<string>(type: "varchar(100)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMerchantMaster", x => x.UserMerchantMasterID);
                    table.ForeignKey(
                        name: "FK_UserMerchantMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserOperatorLimit",
                columns: table => new
                {
                    UserOperatorLimitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    LimitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOperatorLimit", x => x.UserOperatorLimitID);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimit_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimit_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOperatorLimit_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceMapping",
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
                    table.PrimaryKey("PK_UserServiceMapping", x => x.UserServiceMappingID);
                    table.ForeignKey(
                        name: "FK_UserServiceMapping_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserServiceMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceSubscriptionDetails",
                columns: table => new
                {
                    UserServiceSubscriptionDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    ServiceAndUserwiseSubscriptionID = table.Column<int>(type: "int", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceSubscriptionDetails", x => x.UserServiceSubscriptionDetailsID);
                    table.ForeignKey(
                        name: "FK_UserServiceSubscriptionDetails_ServiceAndUserwiseSubscription_ServiceAndUserwiseSubscriptionID",
                        column: x => x.ServiceAndUserwiseSubscriptionID,
                        principalTable: "ServiceAndUserwiseSubscription",
                        principalColumn: "ServiceAndUserwiseSubscriptionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserServiceSubscriptionDetails_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserServiceSubscriptionDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSlabMapping",
                columns: table => new
                {
                    UserSlabMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSlabMapping", x => x.UserSlabMappingID);
                    table.ForeignKey(
                        name: "FK_UserSlabMapping_SlabMaster_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMaster",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSlabMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWalletMaster",
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
                    table.PrimaryKey("PK_UserWalletMaster", x => x.UserWalletMasterID);
                    table.ForeignKey(
                        name: "FK_UserWalletMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWalletMaster_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseDMTMapping",
                columns: table => new
                {
                    UserwiseDMTMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseDMTMapping", x => x.UserwiseDMTMappingID);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMapping_ProviderMaster_ProviderID",
                        column: x => x.ProviderID,
                        principalTable: "ProviderMaster",
                        principalColumn: "ProviderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMapping_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseDMTMapping_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseLowBalance",
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
                    table.PrimaryKey("PK_UserwiseLowBalance", x => x.UserwiseLowBalanceID);
                    table.ForeignKey(
                        name: "FK_UserwiseLowBalance_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseLowBalance_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWisePayementGatewaySetting",
                columns: table => new
                {
                    UserWisePayementGatewaySettingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PaymentGatewaySettingID = table.Column<int>(type: "int", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWisePayementGatewaySetting", x => x.UserWisePayementGatewaySettingID);
                    table.ForeignKey(
                        name: "FK_UserWisePayementGatewaySetting_PaymentGatewaySetting_PaymentGatewaySettingID",
                        column: x => x.PaymentGatewaySettingID,
                        principalTable: "PaymentGatewaySetting",
                        principalColumn: "PaymentGatewaySettingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWisePayementGatewaySetting_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseSaleTarget",
                columns: table => new
                {
                    UserwiseSaleTargetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CommissionType = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseSaleTarget", x => x.UserwiseSaleTargetID);
                    table.ForeignKey(
                        name: "FK_UserwiseSaleTarget_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseSaleTarget_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserwiseServiceChargeMaster",
                columns: table => new
                {
                    UserwiseServiceChargeMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    OneTime = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserwiseServiceChargeMaster", x => x.UserwiseServiceChargeMasterID);
                    table.ForeignKey(
                        name: "FK_UserwiseServiceChargeMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserwiseServiceChargeMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
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
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherDetails", x => x.VoucherDetailsID);
                    table.ForeignKey(
                        name: "FK_VoucherDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DthPackageChannelMapping",
                columns: table => new
                {
                    DthPackageChannelMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false),
                    CategoryChannelMappingID = table.Column<int>(type: "int", nullable: false),
                    ChannelCategoryID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DthPackageChannelMapping", x => x.DthPackageChannelMappingID);
                    table.ForeignKey(
                        name: "FK_DthPackageChannelMapping_CategoryChannelMapping_CategoryChannelMappingID",
                        column: x => x.CategoryChannelMappingID,
                        principalTable: "CategoryChannelMapping",
                        principalColumn: "CategoryChannelMappingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DthPackageChannelMapping_ChannelCategory_ChannelCategoryID",
                        column: x => x.ChannelCategoryID,
                        principalTable: "ChannelCategory",
                        principalColumn: "ChannelCategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DthPackageChannelMapping_OfflineProductMaster_OfflineProductMasterID",
                        column: x => x.OfflineProductMasterID,
                        principalTable: "OfflineProductMaster",
                        principalColumn: "OfflineProductMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineBookingDetails",
                columns: table => new
                {
                    OfflineBookingDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(15)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", nullable: true),
                    Pincode = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineBookingDetails", x => x.OfflineBookingDetailsID);
                    table.ForeignKey(
                        name: "FK_OfflineBookingDetails_OfflineProductMaster_OfflineProductMasterID",
                        column: x => x.OfflineProductMasterID,
                        principalTable: "OfflineProductMaster",
                        principalColumn: "OfflineProductMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineBookingDetails_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineProductMasterImages",
                columns: table => new
                {
                    OfflineProductMasterImagesID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false),
                    ImageName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Path = table.Column<string>(type: "varchar(max)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineProductMasterImages", x => x.OfflineProductMasterImagesID);
                    table.ForeignKey(
                        name: "FK_OfflineProductMasterImages_OfflineProductMaster_OfflineProductMasterID",
                        column: x => x.OfflineProductMasterID,
                        principalTable: "OfflineProductMaster",
                        principalColumn: "OfflineProductMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineProductUIControlMapping",
                columns: table => new
                {
                    OfflineProductUIControlMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfflineServiceMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "varchar(500)", nullable: false),
                    ControlType = table.Column<string>(type: "varchar(100)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MinLength = table.Column<int>(type: "int", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineProductUIControlMapping", x => x.OfflineProductUIControlMappingID);
                    table.ForeignKey(
                        name: "FK_OfflineProductUIControlMapping_OfflineProductMaster_OfflineProductMasterID",
                        column: x => x.OfflineProductMasterID,
                        principalTable: "OfflineProductMaster",
                        principalColumn: "OfflineProductMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineProductUIControlMapping_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineProductUIControlMapping_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfflineSlabDetail",
                columns: table => new
                {
                    OfflineSlabDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlabMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    OfflineProductMasterID = table.Column<int>(type: "int", nullable: false),
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
                    ClientCommissionCategory = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineSlabDetail", x => x.OfflineSlabDetailID);
                    table.ForeignKey(
                        name: "FK_OfflineSlabDetail_OfflineProductMaster_OfflineProductMasterID",
                        column: x => x.OfflineProductMasterID,
                        principalTable: "OfflineProductMaster",
                        principalColumn: "OfflineProductMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineSlabDetail_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineSlabDetail_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfflineSlabDetail_SlabMaster_SlabMasterID",
                        column: x => x.SlabMasterID,
                        principalTable: "SlabMaster",
                        principalColumn: "SlabMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionMaster",
                columns: table => new
                {
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    InboxID = table.Column<int>(type: "int", nullable: false),
                    WalletID = table.Column<int>(type: "int", nullable: false),
                    ConsumerNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: true),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    ProviderOpeningBalance = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ProviderClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProviderRequestID = table.Column<string>(type: "varchar(500)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    APIUserRequestID = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ProviderDOCNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ProviderRefNo = table.Column<string>(type: "varchar(1000)", nullable: true),
                    AdminGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerTDS = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Bill_CustomerMessageMobileNo = table.Column<string>(type: "varchar(10)", nullable: true),
                    RetailerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CashBack = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    HitBackCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "varchar(500)", nullable: true),
                    Bill_Optional1 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional2 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional3 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional4 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional5 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Bill_Optional6 = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_SenderID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_SenderName = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_SenderNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficiaryName = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_Bank = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_IFSC = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BeneficaryNameByBank = table.Column<string>(type: "varchar(1000)", nullable: true),
                    DMT_PaymentType = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_BillNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    DMT_OriginalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DMT_StatusCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsDispute = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "varchar(500)", nullable: true),
                    SimCardDetails = table.Column<string>(type: "varchar(500)", nullable: true),
                    Through = table.Column<string>(type: "varchar(20)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(100)", nullable: true),
                    Response = table.Column<string>(type: "varchar(max)", nullable: true),
                    Circle = table.Column<string>(type: "varchar(100)", nullable: true),
                    DistributerUserID = table.Column<int>(type: "int", nullable: false),
                    SuperDistributerUserID = table.Column<int>(type: "int", nullable: false),
                    ComplaintStatus = table.Column<string>(type: "varchar(20)", nullable: true),
                    AdminCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CardType = table.Column<string>(type: "varchar(100)", nullable: true),
                    StateID = table.Column<int>(type: "int", nullable: false),
                    IsROffer = table.Column<bool>(type: "bit", nullable: false),
                    IsOnlineDMT = table.Column<bool>(type: "bit", nullable: false),
                    IsProviderLowBalance = table.Column<bool>(type: "bit", nullable: false),
                    ChangeStatus = table.Column<string>(type: "varchar(100)", nullable: true),
                    ChangeStatusDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AEPSResponseJSON = table.Column<string>(type: "varchar(max)", nullable: true),
                    PendingProviderStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    AdminProfit = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    ResponseType = table.Column<string>(type: "varchar(50)", nullable: true),
                    ROfferCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    RetailerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SuperDistributerCommissionHold = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SettlementAtProviderEnd = table.Column<bool>(type: "bit", nullable: false),
                    BBPS_CustomerName = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsCCF = table.Column<bool>(type: "bit", nullable: false),
                    NetCommission = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Charge = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionMaster", x => x.TransactionMasterID);
                    table.ForeignKey(
                        name: "FK_TransactionMaster_Inbox_InboxID",
                        column: x => x.InboxID,
                        principalTable: "Inbox",
                        principalColumn: "InboxID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMaster_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMaster_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionMaster_WalletMaster_WalletID",
                        column: x => x.WalletID,
                        principalTable: "WalletMaster",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderHeaderID = table.Column<int>(type: "int", nullable: false),
                    ProductDetailsID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailsID);
                    table.ForeignKey(
                        name: "FK_OrderDetails_OrderHeader_OrderHeaderID",
                        column: x => x.OrderHeaderID,
                        principalTable: "OrderHeader",
                        principalColumn: "OrderHeaderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetails_ProductDetails_ProductDetailsID",
                        column: x => x.ProductDetailsID,
                        principalTable: "ProductDetails",
                        principalColumn: "ProductDetailsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Beneficiary",
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
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficiary", x => x.BeneficiaryID);
                    table.ForeignKey(
                        name: "FK_Beneficiary_Sender_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Sender",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Beneficiary_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashFreeBeneficiary",
                columns: table => new
                {
                    CashFreeBeneficiaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    BankName = table.Column<string>(type: "varchar(50)", nullable: false),
                    AccountNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    IFSC = table.Column<string>(type: "varchar(20)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    SenderMobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFreeBeneficiary", x => x.CashFreeBeneficiaryID);
                    table.ForeignKey(
                        name: "FK_CashFreeBeneficiary_Sender_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Sender",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashFreeBeneficiary_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SenderOTP",
                columns: table => new
                {
                    SenderOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    OTP = table.Column<string>(type: "varchar(10)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderOTP", x => x.SenderOTPID);
                    table.ForeignKey(
                        name: "FK_SenderOTP_Sender_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Sender",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SenderOTP_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UpiBeneficiary",
                columns: table => new
                {
                    UpiBeneficiaryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    UpiID = table.Column<string>(type: "varchar(30)", nullable: false),
                    GUID = table.Column<string>(type: "varchar(50)", nullable: false),
                    SenderMobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    IPAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    UpiSenderID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsNameVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpiBeneficiary", x => x.UpiBeneficiaryID);
                    table.ForeignKey(
                        name: "FK_UpiBeneficiary_UpiSender_UpiSenderID",
                        column: x => x.UpiSenderID,
                        principalTable: "UpiSender",
                        principalColumn: "UpiSenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UpiBeneficiary_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsTransactionMaster",
                columns: table => new
                {
                    SmsTransactionMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    BillNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsProcess = table.Column<bool>(type: "bit", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTransactionMaster", x => x.SmsTransactionMasterID);
                    table.ForeignKey(
                        name: "FK_SmsTransactionMaster_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmsTransactionMaster_TransactionMaster_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMaster",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThreewayReconCompletion",
                columns: table => new
                {
                    ThreewayReconCompletionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ServiceID = table.Column<int>(type: "int", nullable: false),
                    OperatorID = table.Column<int>(type: "int", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreewayReconCompletion", x => x.ThreewayReconCompletionID);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletion_OperatorMaster_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "OperatorMaster",
                        principalColumn: "OperatorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletion_ServiceMaster_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "ServiceMaster",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreewayReconCompletion_TransactionMaster_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMaster",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionManualUpdate",
                columns: table => new
                {
                    TransactionManualUpdateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    ManualUpdatedBy = table.Column<int>(type: "int", nullable: false),
                    ProviderrefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    NewStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionManualUpdate", x => x.TransactionManualUpdateID);
                    table.ForeignKey(
                        name: "FK_TransactionManualUpdate_TransactionMaster_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMaster",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionManualUpdate_UserMaster_ManualUpdatedBy",
                        column: x => x.ManualUpdatedBy,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatusChange",
                columns: table => new
                {
                    TransactionStatusChangeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TransactionMasterID = table.Column<int>(type: "int", nullable: false),
                    TransactionDOCInt = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    NewStatus = table.Column<string>(type: "varchar(50)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Remark = table.Column<string>(type: "varchar(500)", nullable: true),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatusChange", x => x.TransactionStatusChangeID);
                    table.ForeignKey(
                        name: "FK_TransactionStatusChange_TransactionMaster_TransactionMasterID",
                        column: x => x.TransactionMasterID,
                        principalTable: "TransactionMaster",
                        principalColumn: "TransactionMasterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BeneficiaryOTP",
                columns: table => new
                {
                    BeneficiaryOTPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderMobile = table.Column<string>(type: "varchar(15)", nullable: false),
                    OTP = table.Column<string>(type: "varchar(10)", nullable: false),
                    SenderID = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsOTPVerified = table.Column<bool>(type: "bit", nullable: false),
                    DOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDOC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOCInt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficiaryOTP", x => x.BeneficiaryOTPID);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTP_Beneficiary_BeneficiaryID",
                        column: x => x.BeneficiaryID,
                        principalTable: "Beneficiary",
                        principalColumn: "BeneficiaryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTP_Sender_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Sender",
                        principalColumn: "SenderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BeneficiaryOTP_UserMaster_UserID",
                        column: x => x.UserID,
                        principalTable: "UserMaster",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorBeneficiaryMapping",
                columns: table => new
                {
                    VendorBeneficiaryMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    VendorBeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: true),
                    BeneficiaryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorBeneficiaryMapping", x => x.VendorBeneficiaryMappingID);
                    table.ForeignKey(
                        name: "FK_VendorBeneficiaryMapping_Beneficiary_BeneficiaryID",
                        column: x => x.BeneficiaryID,
                        principalTable: "Beneficiary",
                        principalColumn: "BeneficiaryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UPIVendorBeneficiaryMapping",
                columns: table => new
                {
                    UPIVendorBeneficiaryMappingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderID = table.Column<int>(type: "int", nullable: false),
                    UPIVendorBeneficiaryID = table.Column<string>(type: "varchar(100)", nullable: true),
                    UpiBeneficiaryID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UPIVendorBeneficiaryMapping", x => x.UPIVendorBeneficiaryMappingID);
                    table.ForeignKey(
                        name: "FK_UPIVendorBeneficiaryMapping_UpiBeneficiary_UpiBeneficiaryID",
                        column: x => x.UpiBeneficiaryID,
                        principalTable: "UpiBeneficiary",
                        principalColumn: "UpiBeneficiaryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APIUserCallbackRequestResponseLog_UserID",
                table: "APIUserCallbackRequestResponseLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_APIUserInitialRequestResponseLog_UserID",
                table: "APIUserInitialRequestResponseLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationMaster_UserTypeID",
                table: "AuthorizationMaster",
                column: "UserTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AutoTransferUserwiseMapping_UserID",
                table: "AutoTransferUserwiseMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BankRequestResponseLog_UserID",
                table: "BankRequestResponseLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BBPSConfiguration_OperatorID",
                table: "BBPSConfiguration",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_SenderID",
                table: "Beneficiary",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiary_UserID",
                table: "Beneficiary",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTP_BeneficiaryID",
                table: "BeneficiaryOTP",
                column: "BeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTP_SenderID",
                table: "BeneficiaryOTP",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficiaryOTP_UserID",
                table: "BeneficiaryOTP",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CallbackMaster_ServiceID",
                table: "CallbackMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_CallbackMaster_UserID",
                table: "CallbackMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CallMeRequest_CallMeCategoryMasterID",
                table: "CallMeRequest",
                column: "CallMeCategoryMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_CallMeRequest_UserID",
                table: "CallMeRequest",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CashCollectionMaster_UserID",
                table: "CashCollectionMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CashFreeBeneficiary_SenderID",
                table: "CashFreeBeneficiary",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_CashFreeBeneficiary_UserID",
                table: "CashFreeBeneficiary",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryChannelMapping_ChannelCategoryID",
                table: "CategoryChannelMapping",
                column: "ChannelCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetail_ChannelSlabMasterID",
                table: "ChannelSlabDetail",
                column: "ChannelSlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetail_ChannelSlabMasterID1",
                table: "ChannelSlabDetail",
                column: "ChannelSlabMasterID1");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetail_OperatorID",
                table: "ChannelSlabDetail",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSlabDetail_ServiceID",
                table: "ChannelSlabDetail",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseCommission_OperatorID",
                table: "CircleWiseCommission",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseCommission_SlabMasterID",
                table: "CircleWiseCommission",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseOperatorLimit_OperatorID",
                table: "CircleWiseOperatorLimit",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseOperatorLimit_UserID",
                table: "CircleWiseOperatorLimit",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseUserWiseOperatorOff_OperatorID",
                table: "CircleWiseUserWiseOperatorOff",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_CircleWiseUserWiseOperatorOff_UserID",
                table: "CircleWiseUserWiseOperatorOff",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMaster_OperatorID",
                table: "ComplaintMaster",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMaster_ServiceID",
                table: "ComplaintMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintMaster_UserID",
                table: "ComplaintMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMaster_ProviderID",
                table: "DailyAuthenticationMaster",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMaster_ServiceID",
                table: "DailyAuthenticationMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAuthenticationMaster_UserID",
                table: "DailyAuthenticationMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DematGrowthLossMaster_ServiceID",
                table: "DematGrowthLossMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_DthPackageChannelMapping_CategoryChannelMappingID",
                table: "DthPackageChannelMapping",
                column: "CategoryChannelMappingID");

            migrationBuilder.CreateIndex(
                name: "IX_DthPackageChannelMapping_ChannelCategoryID",
                table: "DthPackageChannelMapping",
                column: "ChannelCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_DthPackageChannelMapping_OfflineProductMasterID",
                table: "DthPackageChannelMapping",
                column: "OfflineProductMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_EcollectionDetails_UserID",
                table: "EcollectionDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransaction_ReceiverID",
                table: "FundTransaction",
                column: "ReceiverID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransaction_RequestToID",
                table: "FundTransaction",
                column: "RequestToID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransaction_SenderID",
                table: "FundTransaction",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransaction_WalletID",
                table: "FundTransaction",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_GSTHold_StateID",
                table: "GSTHold",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_GSTHold_UserID",
                table: "GSTHold",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_OperatorID",
                table: "Inbox",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_ServiceID",
                table: "Inbox",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_UserID",
                table: "Inbox",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberHistory_UserID",
                table: "InvoiceNumberHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IPMaster_UserID",
                table: "IPMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LapuMaster_LapuPartyMasterID",
                table: "LapuMaster",
                column: "LapuPartyMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_LapuMaster_OperatorID",
                table: "LapuMaster",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistrory_UserID",
                table: "LoginHistrory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LowBalanceConfiguration_UserID",
                table: "LowBalanceConfiguration",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LowBalanceConfiguration_WalletID",
                table: "LowBalanceConfiguration",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineBookingDetails_OfflineProductMasterID",
                table: "OfflineBookingDetails",
                column: "OfflineProductMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineBookingDetails_UserID",
                table: "OfflineBookingDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductMaster_OperatorID",
                table: "OfflineProductMaster",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductMaster_ServiceID",
                table: "OfflineProductMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductMasterImages_OfflineProductMasterID",
                table: "OfflineProductMasterImages",
                column: "OfflineProductMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductUIControlMapping_OfflineProductMasterID",
                table: "OfflineProductUIControlMapping",
                column: "OfflineProductMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductUIControlMapping_OperatorID",
                table: "OfflineProductUIControlMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineProductUIControlMapping_ServiceID",
                table: "OfflineProductUIControlMapping",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSlabDetail_OfflineProductMasterID",
                table: "OfflineSlabDetail",
                column: "OfflineProductMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSlabDetail_OperatorID",
                table: "OfflineSlabDetail",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSlabDetail_ServiceID",
                table: "OfflineSlabDetail",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineSlabDetail_SlabMasterID",
                table: "OfflineSlabDetail",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingUserDocument_OnboardingUserID",
                table: "OnboardingUserDocument",
                column: "OnboardingUserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalHistory_UserID",
                table: "OperationalHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorMaster_ServiceID",
                table: "OperatorMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequence_OperatorID",
                table: "OperatorSequence",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequence_ProviderID",
                table: "OperatorSequence",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequence_ServiceID",
                table: "OperatorSequence",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWise_OperatorID",
                table: "OperatorSequenceUserWise",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWise_ProviderID",
                table: "OperatorSequenceUserWise",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWise_ServiceID",
                table: "OperatorSequenceUserWise",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorSequenceUserWise_UserID",
                table: "OperatorSequenceUserWise",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorStateMapping_OperatorID",
                table: "OperatorStateMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwise_OperatorID",
                table: "OperatorUserwise",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwise_ServiceID",
                table: "OperatorUserwise",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorUserwise_UserID",
                table: "OperatorUserwise",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderHeaderID",
                table: "OrderDetails",
                column: "OrderHeaderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductDetailsID",
                table: "OrderDetails",
                column: "ProductDetailsID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeader_UserID",
                table: "OrderHeader",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftDetail_UserID",
                table: "OverdraftDetail",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftDetail_WalletID",
                table: "OverdraftDetail",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftMaster_UserID",
                table: "OverdraftMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_OverdraftMaster_WalletID",
                table: "OverdraftMaster",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_PanMitraOnboardedUser_UserID",
                table: "PanMitraOnboardedUser",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordChangeHistory_UserID",
                table: "PasswordChangeHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentGateway_ServiceID",
                table: "PaymentGateway",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutBeneficiaryOTP_UserID",
                table: "PayoutBeneficiaryOTP",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutVendorBeneficiaryMapping_PayoutBeneficaryID",
                table: "PayoutVendorBeneficiaryMapping",
                column: "PayoutBeneficaryID");

            migrationBuilder.CreateIndex(
                name: "IX_PaysprintOnboardedUser_UserID",
                table: "PaysprintOnboardedUser",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PrepaidActivation_UserID",
                table: "PrepaidActivation",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_SubCategoryID",
                table: "ProductDetails",
                column: "SubCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfiguration_OperatorID",
                table: "ProviderConfiguration",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfiguration_ProviderID",
                table: "ProviderConfiguration",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfiguration_ServiceID",
                table: "ProviderConfiguration",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommission_OperatorID",
                table: "ProviderConfigurationAdminCommission",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommission_ProviderID",
                table: "ProviderConfigurationAdminCommission",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderConfigurationAdminCommission_ServiceID",
                table: "ProviderConfigurationAdminCommission",
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
                name: "IX_ProviderOperatorDenominationMapping_DenominationMasterID",
                table: "ProviderOperatorDenominationMapping",
                column: "DenominationMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorDenominationMapping_OperatorID",
                table: "ProviderOperatorDenominationMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderOperatorDenominationMapping_ProviderID",
                table: "ProviderOperatorDenominationMapping",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPurchase_ProviderID",
                table: "ProviderPurchase",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPurchase_WalletID",
                table: "ProviderPurchase",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderRequestResponseLog_UserID",
                table: "ProviderRequestResponseLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderResponseMaster_ProviderID",
                table: "ProviderResponseMaster",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderStateConfiguration_ProviderID",
                table: "ProviderStateConfiguration",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderUserMapping_UserID",
                table: "ProviderUserMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequence_OperatorID",
                table: "RangeWiseSequence",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequence_ProviderID",
                table: "RangeWiseSequence",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_RangeWiseSequence_ServiceID",
                table: "RangeWiseSequence",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_RealProviderMapping_OperatorID",
                table: "RealProviderMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationSetting_ProviderID",
                table: "ReconciliationSetting",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_Redeemcommission_UserID",
                table: "Redeemcommission",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRegistration_UserID",
                table: "ReferralRegistration",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenMaster_UserID",
                table: "RefreshTokenMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommission_OperatorID",
                table: "ROfferCommission",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommission_ServiceID",
                table: "ROfferCommission",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ROfferCommission_SlabMasterID",
                table: "ROfferCommission",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SDKBBPSUserMapping_UserID",
                table: "SDKBBPSUserMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Sender_UserID",
                table: "Sender",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SenderOTP_SenderID",
                table: "SenderOTP",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_SenderOTP_UserID",
                table: "SenderOTP",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSubscriptionPlanDetails_ServiceSubscriptionCreatePlanMasterID",
                table: "ServiceSubscriptionPlanDetails",
                column: "ServiceSubscriptionCreatePlanMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceWiseSliderSetting_ServiceID",
                table: "ServiceWiseSliderSetting",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetail_OperatorID",
                table: "SlabDetail",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetail_ServiceID",
                table: "SlabDetail",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SlabDetail_SlabMasterID",
                table: "SlabDetail",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTransactionMaster_ServiceID",
                table: "SmsTransactionMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTransactionMaster_TransactionMasterID",
                table: "SmsTransactionMaster",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategory_CategoryID",
                table: "SubCategory",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_SupportUserAccessDetail_UserID",
                table: "SupportUserAccessDetail",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TargetSaleCommission_UserID",
                table: "TargetSaleCommission",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TargetSaleCommission_WalletID",
                table: "TargetSaleCommission",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletion_OperatorID",
                table: "ThreewayReconCompletion",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletion_ServiceID",
                table: "ThreewayReconCompletion",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ThreewayReconCompletion_TransactionMasterID",
                table: "ThreewayReconCompletion",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMaster_TicketCategoryID",
                table: "TicketMaster",
                column: "TicketCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMaster_UserID",
                table: "TicketMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMaster_UserTyepID",
                table: "TicketMaster",
                column: "UserTyepID");

            migrationBuilder.CreateIndex(
                name: "IX_UserID_ApiRequestID",
                table: "TransactionAPIUserRequestMaster",
                columns: new[] { "UserID", "ApiUserRequestID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionManualUpdate_ManualUpdatedBy",
                table: "TransactionManualUpdate",
                column: "ManualUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionManualUpdate_TransactionMasterID",
                table: "TransactionManualUpdate",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMaster_InboxID",
                table: "TransactionMaster",
                column: "InboxID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMaster_OperatorID",
                table: "TransactionMaster",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMaster_ServiceID",
                table: "TransactionMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMaster_UserID",
                table: "TransactionMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionMaster_WalletID",
                table: "TransactionMaster",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionStatusChange_TransactionMasterID",
                table: "TransactionStatusChange",
                column: "TransactionMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_TrustableDevice_UserID",
                table: "TrustableDevice",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiBeneficiary_UpiSenderID",
                table: "UpiBeneficiary",
                column: "UpiSenderID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiBeneficiary_UserID",
                table: "UpiBeneficiary",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UpiSender_UserID",
                table: "UpiSender",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UPIVendorBeneficiaryMapping_UpiBeneficiaryID",
                table: "UPIVendorBeneficiaryMapping",
                column: "UpiBeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActionHistory_UserID",
                table: "UserActionHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActionMaster_UserID",
                table: "UserActionMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceCapping_UserID",
                table: "UserBalanceCapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalanceCapping_WalletID",
                table: "UserBalanceCapping",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankDetails_UserID",
                table: "UserBankDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannelSlabMapping_ChannelSlabMasterID",
                table: "UserChannelSlabMapping",
                column: "ChannelSlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannelSlabMapping_UserID",
                table: "UserChannelSlabMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMaster_ServiceID",
                table: "UserCreationMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMaster_UserID",
                table: "UserCreationMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationMaster_WalletID",
                table: "UserCreationMaster",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationSetting_UserID",
                table: "UserCreationSetting",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCreationWallet_UserID",
                table: "UserCreationWallet",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentMaster_UserID",
                table: "UserDocumentMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_EmailID",
                table: "UserMaster",
                column: "EmailID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_MobileNumber",
                table: "UserMaster",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_StateID",
                table: "UserMaster",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMaster_UserTypeID",
                table: "UserMaster",
                column: "UserTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserMerchantMaster_UserID",
                table: "UserMerchantMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimit_OperatorID",
                table: "UserOperatorLimit",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimit_ServiceID",
                table: "UserOperatorLimit",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserOperatorLimit_UserID",
                table: "UserOperatorLimit",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceMapping_ServiceID",
                table: "UserServiceMapping",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceMapping_UserID",
                table: "UserServiceMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceSubscriptionDetails_ServiceAndUserwiseSubscriptionID",
                table: "UserServiceSubscriptionDetails",
                column: "ServiceAndUserwiseSubscriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceSubscriptionDetails_ServiceID",
                table: "UserServiceSubscriptionDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceSubscriptionDetails_UserID",
                table: "UserServiceSubscriptionDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSlabMapping_SlabMasterID",
                table: "UserSlabMapping",
                column: "SlabMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSlabMapping_UserID",
                table: "UserSlabMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMaster_UserID",
                table: "UserWalletMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletMaster_WalletID",
                table: "UserWalletMaster",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMapping_OperatorID",
                table: "UserwiseDMTMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMapping_ProviderID",
                table: "UserwiseDMTMapping",
                column: "ProviderID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMapping_ServiceID",
                table: "UserwiseDMTMapping",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseDMTMapping_UserID",
                table: "UserwiseDMTMapping",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseLowBalance_UserID",
                table: "UserwiseLowBalance",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseLowBalance_WalletID",
                table: "UserwiseLowBalance",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePayementGatewaySetting_PaymentGatewaySettingID",
                table: "UserWisePayementGatewaySetting",
                column: "PaymentGatewaySettingID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWisePayementGatewaySetting_UserID",
                table: "UserWisePayementGatewaySetting",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseSaleTarget_UserID",
                table: "UserwiseSaleTarget",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseSaleTarget_WalletID",
                table: "UserwiseSaleTarget",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseServiceChargeMaster_ServiceID",
                table: "UserwiseServiceChargeMaster",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserwiseServiceChargeMaster_UserID",
                table: "UserwiseServiceChargeMaster",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VendorBeneficiaryMapping_BeneficiaryID",
                table: "VendorBeneficiaryMapping",
                column: "BeneficiaryID");

            migrationBuilder.CreateIndex(
                name: "IX_ViewProviderMapping_OperatorID",
                table: "ViewProviderMapping",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherDetails_UserID",
                table: "VoucherDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherMaster_VoucherTypeID",
                table: "VoucherMaster",
                column: "VoucherTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletServiceMapping_ServiceID",
                table: "WalletServiceMapping",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_WalletServiceMapping_WalletID",
                table: "WalletServiceMapping",
                column: "WalletID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountOpenSetting");

            migrationBuilder.DropTable(
                name: "AEPSSetting");

            migrationBuilder.DropTable(
                name: "AndroidMenuMaster");

            migrationBuilder.DropTable(
                name: "AndroidSectionMaster");

            migrationBuilder.DropTable(
                name: "APIUserCallbackRequestResponseLog");

            migrationBuilder.DropTable(
                name: "APIUserInitialRequestResponseLog");

            migrationBuilder.DropTable(
                name: "Archive_TransactionDetails");

            migrationBuilder.DropTable(
                name: "Archive_TransactionMaster");

            migrationBuilder.DropTable(
                name: "AuthorizationMaster");

            migrationBuilder.DropTable(
                name: "AutoTransferUserwiseMapping");

            migrationBuilder.DropTable(
                name: "BankRequestResponseLog");

            migrationBuilder.DropTable(
                name: "BBPSConfiguration");

            migrationBuilder.DropTable(
                name: "BBPSSettings");

            migrationBuilder.DropTable(
                name: "BeneficiaryOTP");

            migrationBuilder.DropTable(
                name: "BlackListBankMaster");

            migrationBuilder.DropTable(
                name: "BrowserUserAgentMaster");

            migrationBuilder.DropTable(
                name: "BulkMessage");

            migrationBuilder.DropTable(
                name: "BulkPaySetting");

            migrationBuilder.DropTable(
                name: "BusSettingMaster");

            migrationBuilder.DropTable(
                name: "CallbackMaster");

            migrationBuilder.DropTable(
                name: "CallMeRequest");

            migrationBuilder.DropTable(
                name: "CashCollectionDetail");

            migrationBuilder.DropTable(
                name: "CashCollectionMaster");

            migrationBuilder.DropTable(
                name: "CashDepositSetting");

            migrationBuilder.DropTable(
                name: "CashFreeBeneficiary");

            migrationBuilder.DropTable(
                name: "ChannelSlabDetail");

            migrationBuilder.DropTable(
                name: "CircleAPIMaster");

            migrationBuilder.DropTable(
                name: "CircleWiseCommission");

            migrationBuilder.DropTable(
                name: "CircleWiseOperatorLimit");

            migrationBuilder.DropTable(
                name: "CircleWiseUserWiseOperatorOff");

            migrationBuilder.DropTable(
                name: "CMSSetting");

            migrationBuilder.DropTable(
                name: "CompanyMaster");

            migrationBuilder.DropTable(
                name: "ComplaintMaster");

            migrationBuilder.DropTable(
                name: "ConsumerTypeMaster");

            migrationBuilder.DropTable(
                name: "CreditCardSetting");

            migrationBuilder.DropTable(
                name: "CreditInstitutionMaster");

            migrationBuilder.DropTable(
                name: "CyberPlateSetting");

            migrationBuilder.DropTable(
                name: "DailyAuthenticationMaster");

            migrationBuilder.DropTable(
                name: "DateFormatMaster");

            migrationBuilder.DropTable(
                name: "DematGrowthLossMaster");

            migrationBuilder.DropTable(
                name: "DematSetting");

            migrationBuilder.DropTable(
                name: "DeviceWiseFailLoginHistory");

            migrationBuilder.DropTable(
                name: "DMTBank");

            migrationBuilder.DropTable(
                name: "DMTSetting");

            migrationBuilder.DropTable(
                name: "DthPackageChannelMapping");

            migrationBuilder.DropTable(
                name: "EcollectionDetails");

            migrationBuilder.DropTable(
                name: "EcollectionSetting");

            migrationBuilder.DropTable(
                name: "eKYCLog");

            migrationBuilder.DropTable(
                name: "eKYCMaster");

            migrationBuilder.DropTable(
                name: "eKYCSetting");

            migrationBuilder.DropTable(
                name: "EmailSettingMaster");

            migrationBuilder.DropTable(
                name: "EnquiryDetails");

            migrationBuilder.DropTable(
                name: "ExternalMedia");

            migrationBuilder.DropTable(
                name: "FailedSignUpData");

            migrationBuilder.DropTable(
                name: "FingerPrintDeviceType");

            migrationBuilder.DropTable(
                name: "FundTransaction");

            migrationBuilder.DropTable(
                name: "GSTHold");

            migrationBuilder.DropTable(
                name: "InvoiceNumberHistory");

            migrationBuilder.DropTable(
                name: "InvoiceSetting");

            migrationBuilder.DropTable(
                name: "IPMaster");

            migrationBuilder.DropTable(
                name: "KYCApprovedDetails");

            migrationBuilder.DropTable(
                name: "KycDocumentMaster");

            migrationBuilder.DropTable(
                name: "LapuMaster");

            migrationBuilder.DropTable(
                name: "LapuTransactionDetail");

            migrationBuilder.DropTable(
                name: "LastChangeUserDetails");

            migrationBuilder.DropTable(
                name: "LeadGenerationMaster");

            migrationBuilder.DropTable(
                name: "LeadGenerationSetting");

            migrationBuilder.DropTable(
                name: "LoanTypeMaster");

            migrationBuilder.DropTable(
                name: "LOB_Details");

            migrationBuilder.DropTable(
                name: "LOBMaster");

            migrationBuilder.DropTable(
                name: "Location_PinCodeMaster");

            migrationBuilder.DropTable(
                name: "LocationLookUp");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "LoginHistrory");

            migrationBuilder.DropTable(
                name: "LowBalanceConfiguration");

            migrationBuilder.DropTable(
                name: "MediaMaster");

            migrationBuilder.DropTable(
                name: "MenuActionMapping");

            migrationBuilder.DropTable(
                name: "MenuMaster");

            migrationBuilder.DropTable(
                name: "MessageSetting");

            migrationBuilder.DropTable(
                name: "MicroATMSetting");

            migrationBuilder.DropTable(
                name: "MosambeeAEPSBank");

            migrationBuilder.DropTable(
                name: "MPinHistory");

            migrationBuilder.DropTable(
                name: "NewsMaster");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "NPCIBank");

            migrationBuilder.DropTable(
                name: "OfflineBookingDetails");

            migrationBuilder.DropTable(
                name: "OfflineProductMasterImages");

            migrationBuilder.DropTable(
                name: "OfflineProductUIControlMapping");

            migrationBuilder.DropTable(
                name: "OfflineSlabDetail");

            migrationBuilder.DropTable(
                name: "OnboardingUserDocument");

            migrationBuilder.DropTable(
                name: "OperationalHistory");

            migrationBuilder.DropTable(
                name: "OperatorAmountWiseSetting");

            migrationBuilder.DropTable(
                name: "OperatorSequence");

            migrationBuilder.DropTable(
                name: "OperatorSequenceUserWise");

            migrationBuilder.DropTable(
                name: "OperatorStateMapping");

            migrationBuilder.DropTable(
                name: "OperatorUserwise");

            migrationBuilder.DropTable(
                name: "OperatorwiseCallbackSetting");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OTPMaster");

            migrationBuilder.DropTable(
                name: "OTTSetting");

            migrationBuilder.DropTable(
                name: "OverdraftDetail");

            migrationBuilder.DropTable(
                name: "OverdraftMaster");

            migrationBuilder.DropTable(
                name: "PanCardSetting");

            migrationBuilder.DropTable(
                name: "PanMitraOnboardedUser");

            migrationBuilder.DropTable(
                name: "PasswordChangeHistory");

            migrationBuilder.DropTable(
                name: "PaymentGateway");

            migrationBuilder.DropTable(
                name: "PaymentGatewayLog");

            migrationBuilder.DropTable(
                name: "PaymentModeMaster");

            migrationBuilder.DropTable(
                name: "PayoutBeneficiaryOTP");

            migrationBuilder.DropTable(
                name: "PayoutSetting");

            migrationBuilder.DropTable(
                name: "PayoutVendorBeneficiaryMapping");

            migrationBuilder.DropTable(
                name: "PaySprintBank");

            migrationBuilder.DropTable(
                name: "PaysprintOnboardedUser");

            migrationBuilder.DropTable(
                name: "PlanSetting");

            migrationBuilder.DropTable(
                name: "POSSettingMaster");

            migrationBuilder.DropTable(
                name: "PrepaidActivation");

            migrationBuilder.DropTable(
                name: "ProductImageMapping");

            migrationBuilder.DropTable(
                name: "ProviderCallBackLog");

            migrationBuilder.DropTable(
                name: "ProviderConfiguration");

            migrationBuilder.DropTable(
                name: "ProviderConfigurationAdminCommission");

            migrationBuilder.DropTable(
                name: "ProviderConfigurationSample");

            migrationBuilder.DropTable(
                name: "ProviderDetails");

            migrationBuilder.DropTable(
                name: "ProviderDetailsSample");

            migrationBuilder.DropTable(
                name: "ProviderOperatorCircleMaster");

            migrationBuilder.DropTable(
                name: "ProviderOperatorDenominationMapping");

            migrationBuilder.DropTable(
                name: "ProviderOperatorMaster");

            migrationBuilder.DropTable(
                name: "ProviderPurchase");

            migrationBuilder.DropTable(
                name: "ProviderRequestResponseLog");

            migrationBuilder.DropTable(
                name: "ProviderResponseMaster");

            migrationBuilder.DropTable(
                name: "ProviderResponseMasterSample");

            migrationBuilder.DropTable(
                name: "ProviderResponseTypeMaster");

            migrationBuilder.DropTable(
                name: "ProviderStateConfiguration");

            migrationBuilder.DropTable(
                name: "ProviderUserMapping");

            migrationBuilder.DropTable(
                name: "PureDMTTransferBeneDetail");

            migrationBuilder.DropTable(
                name: "RangeWiseSequence");

            migrationBuilder.DropTable(
                name: "RealProviderMapping");

            migrationBuilder.DropTable(
                name: "ReconciliationSetting");

            migrationBuilder.DropTable(
                name: "Redeemcommission");

            migrationBuilder.DropTable(
                name: "RedirectUrlMaster");

            migrationBuilder.DropTable(
                name: "ReferralRegistration");

            migrationBuilder.DropTable(
                name: "RefreshTokenMaster");

            migrationBuilder.DropTable(
                name: "RemainderOperatorAmountWise");

            migrationBuilder.DropTable(
                name: "ResponseKeyValue");

            migrationBuilder.DropTable(
                name: "RichTextMedia");

            migrationBuilder.DropTable(
                name: "ROfferCommission");

            migrationBuilder.DropTable(
                name: "SDKBBPSUserMapping");

            migrationBuilder.DropTable(
                name: "SDKSetting");

            migrationBuilder.DropTable(
                name: "SenderOTP");

            migrationBuilder.DropTable(
                name: "SentSms");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionPlanDetails");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionPlanMaster");

            migrationBuilder.DropTable(
                name: "ServiceWiseSliderSetting");

            migrationBuilder.DropTable(
                name: "SettingMaster");

            migrationBuilder.DropTable(
                name: "SignUp");

            migrationBuilder.DropTable(
                name: "SlabDetail");

            migrationBuilder.DropTable(
                name: "SliderMaster");

            migrationBuilder.DropTable(
                name: "SmsMaster");

            migrationBuilder.DropTable(
                name: "SmsTransactionMaster");

            migrationBuilder.DropTable(
                name: "SubscriptionMaster");

            migrationBuilder.DropTable(
                name: "SupportUserAccessDetail");

            migrationBuilder.DropTable(
                name: "SupportUserAccessMaster");

            migrationBuilder.DropTable(
                name: "TargetSaleCommission");

            migrationBuilder.DropTable(
                name: "TempTransactionMaster");

            migrationBuilder.DropTable(
                name: "ThirdPartyServiceProvider");

            migrationBuilder.DropTable(
                name: "ThreewayReconCompletion");

            migrationBuilder.DropTable(
                name: "TicketMaster");

            migrationBuilder.DropTable(
                name: "TransactionAPIUserRequestMaster");

            migrationBuilder.DropTable(
                name: "TransactionDetails");

            migrationBuilder.DropTable(
                name: "TransactionManualUpdate");

            migrationBuilder.DropTable(
                name: "TransactionProviderHistory");

            migrationBuilder.DropTable(
                name: "TransactionStatusChange");

            migrationBuilder.DropTable(
                name: "TrustableDevice");

            migrationBuilder.DropTable(
                name: "UISettingMaster");

            migrationBuilder.DropTable(
                name: "UnitMaster");

            migrationBuilder.DropTable(
                name: "UPITransferSetting");

            migrationBuilder.DropTable(
                name: "UPIVendorBeneficiaryMapping");

            migrationBuilder.DropTable(
                name: "UrlTypeMaster");

            migrationBuilder.DropTable(
                name: "UserActionHistory");

            migrationBuilder.DropTable(
                name: "UserActionMaster");

            migrationBuilder.DropTable(
                name: "UserBalanceCapping");

            migrationBuilder.DropTable(
                name: "UserBalanceHistory");

            migrationBuilder.DropTable(
                name: "UserBankDetails");

            migrationBuilder.DropTable(
                name: "UserChannelSlabMapping");

            migrationBuilder.DropTable(
                name: "UserCreationMaster");

            migrationBuilder.DropTable(
                name: "UserCreationSetting");

            migrationBuilder.DropTable(
                name: "UserCreationWallet");

            migrationBuilder.DropTable(
                name: "UserDocumentMaster");

            migrationBuilder.DropTable(
                name: "UserMerchantMaster");

            migrationBuilder.DropTable(
                name: "UserOperatorLimit");

            migrationBuilder.DropTable(
                name: "UserPaymentCollectMapping");

            migrationBuilder.DropTable(
                name: "UserPlanUtilization");

            migrationBuilder.DropTable(
                name: "UserServiceCharge");

            migrationBuilder.DropTable(
                name: "UserServiceMapping");

            migrationBuilder.DropTable(
                name: "UserServiceSubscriptionCharge");

            migrationBuilder.DropTable(
                name: "UserServiceSubscriptionDetails");

            migrationBuilder.DropTable(
                name: "UserSlabMapping");

            migrationBuilder.DropTable(
                name: "UserSubscriptionPlanMapping");

            migrationBuilder.DropTable(
                name: "UserWalletMaster");

            migrationBuilder.DropTable(
                name: "UserwiseDMTMapping");

            migrationBuilder.DropTable(
                name: "UserwiseLowBalance");

            migrationBuilder.DropTable(
                name: "UserWisePayementGatewaySetting");

            migrationBuilder.DropTable(
                name: "UserWisePlanLimit");

            migrationBuilder.DropTable(
                name: "UserWisePlanView");

            migrationBuilder.DropTable(
                name: "UserwiseSaleTarget");

            migrationBuilder.DropTable(
                name: "UserwiseServiceChargeMaster");

            migrationBuilder.DropTable(
                name: "VendorBeneficiaryMapping");

            migrationBuilder.DropTable(
                name: "VerificationSetting");

            migrationBuilder.DropTable(
                name: "ViewProviderMapping");

            migrationBuilder.DropTable(
                name: "VirtualAccountMaster");

            migrationBuilder.DropTable(
                name: "VirtualBalance");

            migrationBuilder.DropTable(
                name: "VoucherDetails");

            migrationBuilder.DropTable(
                name: "VoucherMaster");

            migrationBuilder.DropTable(
                name: "WalletServiceMapping");

            migrationBuilder.DropTable(
                name: "WebHookLog");

            migrationBuilder.DropTable(
                name: "WebHookSetting");

            migrationBuilder.DropTable(
                name: "WrongPasswordHistory");

            migrationBuilder.DropTable(
                name: "ZoneMaster");

            migrationBuilder.DropTable(
                name: "CallMeCategoryMaster");

            migrationBuilder.DropTable(
                name: "CategoryChannelMapping");

            migrationBuilder.DropTable(
                name: "LapuPartyMaster");

            migrationBuilder.DropTable(
                name: "OfflineProductMaster");

            migrationBuilder.DropTable(
                name: "OnboardingUser");

            migrationBuilder.DropTable(
                name: "OrderHeader");

            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.DropTable(
                name: "PayoutBeneficiary");

            migrationBuilder.DropTable(
                name: "DenominationMaster");

            migrationBuilder.DropTable(
                name: "ServiceSubscriptionCreatePlanMaster");

            migrationBuilder.DropTable(
                name: "TicketCategory");

            migrationBuilder.DropTable(
                name: "TransactionMaster");

            migrationBuilder.DropTable(
                name: "UpiBeneficiary");

            migrationBuilder.DropTable(
                name: "ChannelSlabMaster");

            migrationBuilder.DropTable(
                name: "ServiceAndUserwiseSubscription");

            migrationBuilder.DropTable(
                name: "SlabMaster");

            migrationBuilder.DropTable(
                name: "ProviderMaster");

            migrationBuilder.DropTable(
                name: "PaymentGatewaySetting");

            migrationBuilder.DropTable(
                name: "Beneficiary");

            migrationBuilder.DropTable(
                name: "VoucherType");

            migrationBuilder.DropTable(
                name: "ChannelCategory");

            migrationBuilder.DropTable(
                name: "SubCategory");

            migrationBuilder.DropTable(
                name: "Inbox");

            migrationBuilder.DropTable(
                name: "WalletMaster");

            migrationBuilder.DropTable(
                name: "UpiSender");

            migrationBuilder.DropTable(
                name: "Sender");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "OperatorMaster");

            migrationBuilder.DropTable(
                name: "UserMaster");

            migrationBuilder.DropTable(
                name: "ServiceMaster");

            migrationBuilder.DropTable(
                name: "StateMaster");

            migrationBuilder.DropTable(
                name: "UserTypeMaster");
        }
    }
}
