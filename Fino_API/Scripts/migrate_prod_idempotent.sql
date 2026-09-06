IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AccountOpenSettings] (
        [AccountOpenSettingID] int NOT NULL IDENTITY,
        [Name] varchar(500) NULL,
        [IsActiveForWebsite] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [UserName] varchar(500) NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(500) NULL,
        [CallBackURL] varchar(500) NULL,
        [DisplayName] varchar(100) NULL,
        [Sequence] int NOT NULL,
        [Mode] varchar(50) NULL,
        CONSTRAINT [PK_AccountOpenSettings] PRIMARY KEY ([AccountOpenSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AEPSSettings] (
        [AEPSSettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [PublicKey] varchar(500) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,5) NOT NULL,
        [MaximumAmount] decimal(18,5) NOT NULL,
        [DisplayName] varchar(100) NOT NULL,
        [Sequence] int NOT NULL,
        [Mode] varchar(10) NOT NULL,
        [IsMerchantVerify] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AEPSSettings] PRIMARY KEY ([AEPSSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AndroidMenuMasters] (
        [AndroidMenuMasterID] int NOT NULL IDENTITY,
        [MenuName] nvarchar(max) NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [URL] nvarchar(max) NOT NULL,
        [UserTypeID] int NOT NULL,
        [Sequence] int NOT NULL,
        [SectionName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_AndroidMenuMasters] PRIMARY KEY ([AndroidMenuMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AndroidSectionMasters] (
        [AndroidSectionMasterID] int NOT NULL IDENTITY,
        [SectionName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AndroidSectionMasters] PRIMARY KEY ([AndroidSectionMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [AuditLogID] bigint NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Action] varchar(100) NOT NULL,
        [Module] varchar(100) NOT NULL,
        [OldValue] varchar(max) NOT NULL,
        [NewValue] varchar(max) NOT NULL,
        [IPAddress] varchar(50) NOT NULL,
        [AdditionalInfo] varchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditLogID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BBPSSettings] (
        [BBPSSettingID] int NOT NULL IDENTITY,
        [Name] varchar(500) NULL,
        [IsActiveForWebsite] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Token] varchar(500) NULL,
        [AgentID] varchar(500) NULL,
        [PartnerID] varchar(500) NULL,
        [MerchantID] varchar(500) NULL,
        CONSTRAINT [PK_BBPSSettings] PRIMARY KEY ([BBPSSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BlackListBankMasters] (
        [BlackListBankMasterID] int NOT NULL IDENTITY,
        [BankName] nvarchar(max) NOT NULL,
        [IFSC] nvarchar(max) NOT NULL,
        [AccountNumber] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_BlackListBankMasters] PRIMARY KEY ([BlackListBankMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BrowserUserAgentMasters] (
        [BrowserUserAgentMasterID] int NOT NULL IDENTITY,
        [UserAgent] varchar(500) NOT NULL,
        CONSTRAINT [PK_BrowserUserAgentMasters] PRIMARY KEY ([BrowserUserAgentMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BulkMessages] (
        [BulkMessageID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Message] varchar(max) NOT NULL,
        [IsSMS] bit NOT NULL,
        [IsEmail] bit NOT NULL,
        [IsWhatsUp] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BulkMessages] PRIMARY KEY ([BulkMessageID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BulkPaySettings] (
        [BulkPaySettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [ActionURL] nvarchar(max) NOT NULL,
        [StatusCheckURL] nvarchar(max) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_BulkPaySettings] PRIMARY KEY ([BulkPaySettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CallMeCategoryMaster] (
        [CallMeCategoryMasterID] int NOT NULL IDENTITY,
        [Name] varchar(200) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CallMeCategoryMaster] PRIMARY KEY ([CallMeCategoryMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CashCollectionDetails] (
        [CashCollectionDetailID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ParentID] int NOT NULL,
        [OutstandingOpening] decimal(18,5) NOT NULL,
        [OutstandingAmount] decimal(18,5) NOT NULL,
        [OutstandingClosing] decimal(18,5) NOT NULL,
        [ParentRemark] varchar(100) NOT NULL,
        [PayMode] varchar(100) NOT NULL,
        [Credit] varchar(50) NOT NULL,
        [Debit] varchar(50) NOT NULL,
        [UTRNumber] varchar(100) NOT NULL,
        [Type] varchar(50) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CashCollectionDetails] PRIMARY KEY ([CashCollectionDetailID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CashDepositSettings] (
        [CashDepositSettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [PublicKey] varchar(500) NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,5) NOT NULL,
        [MaximumAmount] decimal(18,5) NOT NULL,
        [Mode] varchar(10) NULL,
        CONSTRAINT [PK_CashDepositSettings] PRIMARY KEY ([CashDepositSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Categorys] (
        [CategoryID] int NOT NULL IDENTITY,
        [CategoryName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Categorys] PRIMARY KEY ([CategoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ChannelCategorys] (
        [ChannelCategoryID] int NOT NULL IDENTITY,
        [Category] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ChannelCategorys] PRIMARY KEY ([ChannelCategoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ChannelSlabMasters] (
        [ChannelSlabMasterID] int NOT NULL IDENTITY,
        [SlabName] varchar(80) NOT NULL,
        [IsActive] bit NOT NULL,
        [UplineUserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ChannelSlabMasters] PRIMARY KEY ([ChannelSlabMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CircleAPIMasters] (
        [CircleAPIMasterID] int NOT NULL IDENTITY,
        [CircleAPIURL] varchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CircleAPIMasters] PRIMARY KEY ([CircleAPIMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CMSSettings] (
        [CMSSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [PublicKey] nvarchar(max) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CMSSettings] PRIMARY KEY ([CMSSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CompanyMasters] (
        [CompanyMasterID] int NOT NULL IDENTITY,
        [Name] varchar(max) NOT NULL,
        [Value] varchar(max) NOT NULL,
        CONSTRAINT [PK_CompanyMasters] PRIMARY KEY ([CompanyMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ConsumerTypeMasters] (
        [ConsumerTypeMasterID] int NOT NULL IDENTITY,
        [ConsumerType] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ConsumerTypeMasters] PRIMARY KEY ([ConsumerTypeMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CreditCardSettings] (
        [CreditCardSettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [PublicKey] varchar(500) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,5) NOT NULL,
        [MaximumAmount] decimal(18,5) NOT NULL,
        [Mode] varchar(10) NOT NULL,
        CONSTRAINT [PK_CreditCardSettings] PRIMARY KEY ([CreditCardSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CreditInstitutionMasters] (
        [CreditInstitutionMasterID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [BranchID] nvarchar(max) NOT NULL,
        [AccountNumber] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [EncryptionKey] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        CONSTRAINT [PK_CreditInstitutionMasters] PRIMARY KEY ([CreditInstitutionMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DateFormatMasters] (
        [DateFormatMasterkID] int NOT NULL IDENTITY,
        [DateFormat] varchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_DateFormatMasters] PRIMARY KEY ([DateFormatMasterkID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DematSettings] (
        [DematSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [UTMSource] nvarchar(max) NOT NULL,
        [EmailID] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Sequence] int NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_DematSettings] PRIMARY KEY ([DematSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DenominationMasters] (
        [DenominationMasterID] int NOT NULL IDENTITY,
        [Description] varchar(max) NOT NULL,
        [DenominationRange] varchar(max) NOT NULL,
        [IsStop] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DenominationMasters] PRIMARY KEY ([DenominationMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DeviceWiseFailLoginHistorys] (
        [DeviceWiseFailLoginHistoryID] int NOT NULL IDENTITY,
        [UserName] nvarchar(max) NOT NULL,
        [DeviceID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_DeviceWiseFailLoginHistorys] PRIMARY KEY ([DeviceWiseFailLoginHistoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DMTBanks] (
        [DMTBankID] int NOT NULL IDENTITY,
        [Name] varchar(200) NOT NULL,
        [DefaultIFSC] varchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsIMPS] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DMTBanks] PRIMARY KEY ([DMTBankID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DMTSettings] (
        [DMTSettingID] int NOT NULL IDENTITY,
        [Name] varchar(500) NOT NULL,
        [DisplayName] varchar(100) NOT NULL,
        [IsActiveForWebsite] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Token] varchar(500) NOT NULL,
        [WalletID] varchar(500) NOT NULL,
        [CallBackURL] varchar(500) NOT NULL,
        [UserName] varchar(500) NOT NULL,
        [SplitAmount] decimal(18,5) NOT NULL,
        [MinAmount] decimal(18,5) NOT NULL,
        [SenderLimit] decimal(18,5) NOT NULL,
        [IsCCF] bit NOT NULL,
        [IsNeft] bit NOT NULL,
        [Sequence] int NOT NULL,
        [Mode] varchar(10) NOT NULL,
        [IsPayoutDMT] bit NOT NULL,
        [ProviderCode] varchar(100) NOT NULL,
        [IsExpressDMT] bit NOT NULL,
        CONSTRAINT [PK_DMTSettings] PRIMARY KEY ([DMTSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [EcollectionSettings] (
        [EcollectionSettingID] int NOT NULL IDENTITY,
        [PrivateKeyPath] varchar(500) NOT NULL,
        [PrivateKeyPassword] varchar(500) NOT NULL,
        [PublicKeyPath] varchar(500) NOT NULL,
        [Name] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [WalletID] int NOT NULL,
        [Prefix] varchar(100) NOT NULL,
        [IFSCCode] varchar(20) NOT NULL,
        CONSTRAINT [PK_EcollectionSettings] PRIMARY KEY ([EcollectionSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [eKYCSettings] (
        [eKYCSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [AuthKey] nvarchar(max) NOT NULL,
        [ChargeAmount] decimal(18,2) NOT NULL,
        [EncryptionKey] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        CONSTRAINT [PK_eKYCSettings] PRIMARY KEY ([eKYCSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [EmailSettingMasters] (
        [EmailSettingMasterID] int NOT NULL IDENTITY,
        [Email] varchar(100) NOT NULL,
        [Password] varchar(100) NOT NULL,
        [HostName] varchar(100) NOT NULL,
        [Port] int NOT NULL,
        [IsSSL] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EmailSettingMasters] PRIMARY KEY ([EmailSettingMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [EnquiryDetails] (
        [EnquiryDetailsID] int NOT NULL IDENTITY,
        [PersonName] varchar(50) NOT NULL,
        [MobileNumber] varchar(20) NOT NULL,
        [EmailID] varchar(50) NOT NULL,
        [Address] varchar(500) NOT NULL,
        [EnquiryType] varchar(50) NOT NULL,
        [Remark] varchar(200) NOT NULL,
        [AdminStatus] varchar(200) NOT NULL,
        [AdminRemark] varchar(200) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EnquiryDetails] PRIMARY KEY ([EnquiryDetailsID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ExternalMedias] (
        [ExternalMediaID] int NOT NULL IDENTITY,
        [Key] varchar(100) NOT NULL,
        [Value] varchar(200) NOT NULL,
        [Description] varchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ExternalMedias] PRIMARY KEY ([ExternalMediaID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [FailedSignUpDatas] (
        [FailedSignUpDataID] int NOT NULL IDENTITY,
        [MobileNumber] nvarchar(max) NOT NULL,
        [PANNo] nvarchar(max) NOT NULL,
        [ReferralCode] nvarchar(max) NOT NULL,
        [EmployeeID] nvarchar(max) NOT NULL,
        [OrderID] nvarchar(max) NOT NULL,
        [RedirectURL] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [PinCode] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FailedSignUpDatas] PRIMARY KEY ([FailedSignUpDataID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [FingerPrintDeviceTypes] (
        [FingerPrintDeviceTypeID] int NOT NULL IDENTITY,
        [DeviceName] varchar(200) NOT NULL,
        [Value] varchar(200) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FingerPrintDeviceTypes] PRIMARY KEY ([FingerPrintDeviceTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [InvoiceSettings] (
        [InvoiceSettingID] int NOT NULL IDENTITY,
        [CompanyName] varchar(500) NOT NULL,
        [LogoURL] varchar(500) NOT NULL,
        [CompanyPrefix] varchar(10) NOT NULL,
        [Address] varchar(max) NOT NULL,
        [ShortAddress] varchar(max) NOT NULL,
        [PinCode] varchar(max) NOT NULL,
        [District] varchar(max) NOT NULL,
        [Country] varchar(max) NOT NULL,
        [GSTIN] varchar(max) NOT NULL,
        [PAN] varchar(10) NOT NULL,
        [CIN] varchar(25) NOT NULL,
        [State] varchar(50) NOT NULL,
        [AccountNo] varchar(100) NOT NULL,
        [AccountType] varchar(30) NOT NULL,
        [IFSC] varchar(30) NOT NULL,
        [AccountHolderName] varchar(100) NOT NULL,
        [BankName] varchar(max) NOT NULL,
        [BranchName] varchar(max) NOT NULL,
        [Email] varchar(100) NOT NULL,
        [WebsiteName] varchar(100) NOT NULL,
        [Note] varchar(max) NOT NULL,
        [FullAddress] varchar(max) NOT NULL,
        [SignatureURL] varchar(500) NOT NULL,
        CONSTRAINT [PK_InvoiceSettings] PRIMARY KEY ([InvoiceSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [KYCApprovedDetails] (
        [KYCApprovedDetailsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ApporvedUserID] int NOT NULL,
        [Remark] varchar(200) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_KYCApprovedDetails] PRIMARY KEY ([KYCApprovedDetailsID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [KycDocumentMasters] (
        [KycDocumentMasterID] int NOT NULL IDENTITY,
        [Label] varchar(100) NULL,
        [DocumentType] varchar(100) NULL,
        [Remark] varchar(200) NULL,
        [IsRequired] bit NOT NULL,
        [RegularRxpression] varchar(500) NULL,
        [SampleImageURL] nvarchar(max) NULL,
        CONSTRAINT [PK_KycDocumentMasters] PRIMARY KEY ([KycDocumentMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LapuPartyMasters] (
        [LapuPartyMasterID] int NOT NULL IDENTITY,
        [PartyName] nvarchar(500) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [State] nvarchar(100) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [EmailID] varchar(70) NOT NULL,
        [MobileNumber] varchar(10) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LapuPartyMasters] PRIMARY KEY ([LapuPartyMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LapuTransactionDetails] (
        [LapuTransactionDetailID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ProivderID] int NOT NULL,
        [LapuMasterID] int NOT NULL,
        [LapuPartyMasterID] int NOT NULL,
        [LapuNo] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ConsumerNumber] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ClosingBalance] decimal(18,2) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LapuTransactionDetails] PRIMARY KEY ([LapuTransactionDetailID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LastChangeUserDetailss] (
        [LastChangeUserDetailsID] int NOT NULL IDENTITY,
        [OnboardingUserID] int NOT NULL,
        [Change] nvarchar(max) NOT NULL,
        [OldChanges] nvarchar(max) NOT NULL,
        [NewChanges] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LastChangeUserDetailss] PRIMARY KEY ([LastChangeUserDetailsID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LeadGenerationMasters] (
        [LeadGenerationMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [FormID] nvarchar(max) NOT NULL,
        [MerchantName] nvarchar(max) NOT NULL,
        [ConsumerName] nvarchar(max) NOT NULL,
        [MobileNumber] nvarchar(max) NOT NULL,
        [EmailID] nvarchar(max) NOT NULL,
        [PanNumber] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [LoanType] nvarchar(max) NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [APIUserRequestID] nvarchar(max) NOT NULL,
        [DOB] datetime2 NOT NULL,
        CONSTRAINT [PK_LeadGenerationMasters] PRIMARY KEY ([LeadGenerationMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LeadGenerationSettings] (
        [LeadGenerationSettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [PublicKey] varchar(500) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Mode] varchar(10) NOT NULL,
        [DisplayName] varchar(100) NOT NULL,
        [Sequence] int NOT NULL,
        [IsActiveForWebsite] bit NOT NULL,
        CONSTRAINT [PK_LeadGenerationSettings] PRIMARY KEY ([LeadGenerationSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LoanTypeMasters] (
        [LoanTypeMasterID] int NOT NULL IDENTITY,
        [LoanType] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LoanTypeMasters] PRIMARY KEY ([LoanTypeMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LocationLookUps] (
        [LocationLookUpID] int NOT NULL IDENTITY,
        [TableID] int NOT NULL,
        [SimCardDetails] varchar(max) NOT NULL,
        [Type] varchar(150) NOT NULL,
        [Location] varchar(max) NOT NULL,
        [IPAddress] varchar(max) NOT NULL,
        [Latitude] varchar(max) NOT NULL,
        [Longitude] varchar(max) NOT NULL,
        CONSTRAINT [PK_LocationLookUps] PRIMARY KEY ([LocationLookUpID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LocationPinCodeMasters] (
        [SR_NO] int NOT NULL IDENTITY,
        [PINCODE] varchar(100) NOT NULL,
        [HPY_DISTRICT] varchar(100) NOT NULL,
        [HYP_STATE] varchar(100) NOT NULL,
        [HPY_TALUKA_CITY] varchar(100) NOT NULL,
        [REGIONNAME] varchar(100) NOT NULL,
        [HPY_REGION] varchar(100) NOT NULL,
        [HPY_ZONE] varchar(100) NOT NULL,
        [HPY_AREA] varchar(100) NOT NULL,
        [HPY_NSM] varchar(100) NOT NULL,
        [HPY_RH] varchar(100) NOT NULL,
        [HPY_ZSM] varchar(100) NOT NULL,
        [HPY_SH] varchar(100) NOT NULL,
        [HPY_CM] varchar(100) NOT NULL,
        [HPY_ASM] varchar(100) NOT NULL,
        [HPY_FLS] varchar(100) NOT NULL,
        [ACTIVATED] varchar(100) NOT NULL,
        [ACTIVATION_DATE] varchar(100) NOT NULL,
        [LOCATION] varchar(100) NOT NULL,
        [WITHIN_CITY_LIMITS] varchar(100) NOT NULL,
        [DISTANCE_FROM_CBD] varchar(100) NOT NULL,
        [INDIAPOST_OFFICE_NAME] varchar(100) NOT NULL,
        [DIVISIONNAME] varchar(100) NOT NULL,
        [LONGITUDE] varchar(100) NOT NULL,
        [LATITUDE] varchar(100) NOT NULL,
        [EMPLOYEE_CODE] varchar(100) NOT NULL,
        [EMPLOYEE_NAME] varchar(100) NOT NULL,
        [EMPLOYEE_MOBILE] varchar(100) NOT NULL,
        [EMPLOYEE_EMAIL] varchar(100) NOT NULL,
        [RESERVEFIELD_5] varchar(100) NOT NULL,
        [RESERVEFIELD_6] varchar(100) NOT NULL,
        [RESERVEFIELD_7] varchar(100) NOT NULL,
        [RESERVEFIELD_8] varchar(100) NOT NULL,
        [RESERVEFIELD_9] varchar(100) NOT NULL,
        [RESERVEFIELD_10] varchar(100) NOT NULL,
        [RESERVEFIELD_11] varchar(100) NOT NULL,
        [RESERVEFIELD_12] varchar(100) NOT NULL,
        [RESERVEFIELD_13] varchar(100) NOT NULL,
        [RESERVEFIELD_14] varchar(100) NOT NULL,
        [RESERVEFIELD_15] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        CONSTRAINT [PK_LocationPinCodeMasters] PRIMARY KEY ([SR_NO])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MediaMasters] (
        [MediaMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [MediaType] varchar(100) NOT NULL,
        [MediaPath] varchar(1000) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MediaMasters] PRIMARY KEY ([MediaMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MenuActionMappings] (
        [MenuActionMappingID] int NOT NULL IDENTITY,
        [Controller] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [MenuMasterGUID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MenuActionMappings] PRIMARY KEY ([MenuActionMappingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MenuItem] (
        [MenuItemID] int NOT NULL IDENTITY,
        [MenuItemName] nvarchar(max) NOT NULL,
        [UserTypeID] int NOT NULL,
        [Sequence] int NOT NULL,
        [MappingCode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MenuItem] PRIMARY KEY ([MenuItemID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MenuMasters] (
        [MenuMasterID] int NOT NULL IDENTITY,
        [MenuSequence] decimal(18,2) NOT NULL,
        [Name] varchar(100) NOT NULL,
        [Url] varchar(100) NOT NULL,
        [Icon] varchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsAdmin] bit NOT NULL,
        [IsSuperDistributer] bit NOT NULL,
        [IsDistributer] bit NOT NULL,
        [IsRetailer] bit NOT NULL,
        [IsAPIUser] bit NOT NULL,
        [IsSupport] bit NOT NULL,
        [Level] int NOT NULL,
        [GUID] varchar(100) NOT NULL,
        [ParentMenuGUID] varchar(100) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MenuMasters] PRIMARY KEY ([MenuMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MessageSettings] (
        [MessageSettingID] int NOT NULL IDENTITY,
        [Type] varchar(100) NOT NULL,
        [IsSMS] bit NOT NULL,
        [IsMail] bit NOT NULL,
        [IsWhatsUp] bit NOT NULL,
        [WhatsUpMessage] varchar(max) NOT NULL,
        [SmsMessage] varchar(500) NOT NULL,
        [EmailMessage] varchar(max) NOT NULL,
        [TemplateID] varchar(100) NOT NULL,
        [IsOTP] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MessageSettings] PRIMARY KEY ([MessageSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MicroATMSettings] (
        [MicroATMSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [PublicKey] nvarchar(max) NOT NULL,
        [MinimumAmount] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Sequence] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        CONSTRAINT [PK_MicroATMSettings] PRIMARY KEY ([MicroATMSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [MPinHistorys] (
        [MPinHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Mpin] int NOT NULL,
        [DeviceID] nvarchar(max) NOT NULL,
        [Device] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_MPinHistorys] PRIMARY KEY ([MPinHistoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationID] int NOT NULL IDENTITY,
        [NotificationText] nvarchar(500) NOT NULL,
        [NotificationImagePath] varchar(500) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [NPCIBanks] (
        [NPCIBankID] int NOT NULL IDENTITY,
        [BankCode] varchar(50) NULL,
        [BankName] varchar(200) NOT NULL,
        [IFSC] varchar(50) NULL,
        [IIN] varchar(50) NULL,
        [ParentBank] varchar(200) NULL,
        [BankType] varchar(50) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_NPCIBanks] PRIMARY KEY ([NPCIBankID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OnboardingUsers] (
        [OnboardingUserID] int NOT NULL IDENTITY,
        [ExternalID] nvarchar(max) NOT NULL,
        [ProfileID] nvarchar(max) NOT NULL,
        [ReferenceID] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [DOB] nvarchar(max) NOT NULL,
        [PanNo] nvarchar(max) NOT NULL,
        [AadhaarNo] nvarchar(max) NOT NULL,
        [MobileNumber] nvarchar(max) NOT NULL,
        [EmailID] nvarchar(max) NOT NULL,
        [State] nvarchar(max) NOT NULL,
        [PinCode] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [AdminStatus] nvarchar(max) NOT NULL,
        [SupportStatus] nvarchar(max) NOT NULL,
        [AdminRemark] nvarchar(max) NOT NULL,
        [SupportRemark] nvarchar(max) NOT NULL,
        [OrderID] nvarchar(max) NOT NULL,
        [AccountNumber] nvarchar(max) NOT NULL,
        [IFSC] nvarchar(max) NOT NULL,
        [BankName] nvarchar(max) NOT NULL,
        [SlabID] int NOT NULL,
        [AgentID] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OnboardingUsers] PRIMARY KEY ([OnboardingUserID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorAmountWiseSettings] (
        [OperatorAmountWiseSettingID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [SerialCount] int NOT NULL,
        CONSTRAINT [PK_OperatorAmountWiseSettings] PRIMARY KEY ([OperatorAmountWiseSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorwiseCallbackSettings] (
        [OperatorwiseCallbackSettingID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [Url] varchar(100) NULL,
        [MethodType] varchar(30) NOT NULL,
        [HeaderJson] varchar(max) NOT NULL,
        [DateFormat] varchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorwiseCallbackSettings] PRIMARY KEY ([OperatorwiseCallbackSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OTPMasters] (
        [OTPMasterID] int NOT NULL IDENTITY,
        [OTP] varchar(50) NULL,
        [IsVerified] bit NOT NULL,
        [Type] varchar(50) NULL,
        [UserID] int NOT NULL,
        [MobileNumber] varchar(20) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OTPMasters] PRIMARY KEY ([OTPMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OTTSettings] (
        [OTTSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [ActionURL] nvarchar(max) NOT NULL,
        [StatusCheckURL] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_OTTSettings] PRIMARY KEY ([OTTSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PanCardSettings] (
        [PanCardSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [PublicKey] nvarchar(max) NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PanCardSettings] PRIMARY KEY ([PanCardSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PaymentGatewayLogs] (
        [ID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [Type] varchar(50) NOT NULL,
        [GatewayName] varchar(50) NOT NULL,
        [Request] varchar(150) NOT NULL,
        [Response] varchar(max) NOT NULL,
        [OrderID] varchar(150) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PaymentGatewayLogs] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PaymentGatewaySettings] (
        [PaymentGatewaySettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [SuccessURL] varchar(500) NOT NULL,
        [ActionURL] varchar(500) NOT NULL,
        [StatusCheckURL] varchar(500) NULL,
        [FailURL] varchar(500) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,5) NOT NULL,
        [MaximumAmount] decimal(18,5) NOT NULL,
        [IsUPI] bit NOT NULL,
        [IsManualStatusChange] bit NOT NULL,
        [SettlementHoldInMinutes] int NOT NULL,
        CONSTRAINT [PK_PaymentGatewaySettings] PRIMARY KEY ([PaymentGatewaySettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PaymentModeMasters] (
        [PaymentModeMasterID] int NOT NULL IDENTITY,
        [Type] varchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_PaymentModeMasters] PRIMARY KEY ([PaymentModeMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PayoutBeneficiaries] (
        [PayoutBeneficaryID] int NOT NULL IDENTITY,
        [AccountHolderName] varchar(200) NOT NULL,
        [BankName] varchar(50) NOT NULL,
        [AccountNo] varchar(30) NOT NULL,
        [IFSC] varchar(20) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [AdminRemark] varchar(max) NOT NULL,
        [Document] varchar(500) NULL,
        [UserID] int NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [IsNameVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PayoutBeneficiaries] PRIMARY KEY ([PayoutBeneficaryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PayoutSettings] (
        [PayoutSettingID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [MerchantID] varchar(100) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [Slat] varchar(200) NOT NULL,
        [ActionURL] varchar(500) NOT NULL,
        [StatusCheckURL] varchar(500) NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,5) NOT NULL,
        [MaximumAmount] decimal(18,5) NOT NULL,
        [Mode] varchar(10) NULL,
        CONSTRAINT [PK_PayoutSettings] PRIMARY KEY ([PayoutSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PaySprintBanks] (
        [ID] int NOT NULL IDENTITY,
        [BankName] nvarchar(max) NOT NULL,
        [Sequence] int NOT NULL,
        CONSTRAINT [PK_PaySprintBanks] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PlanSettings] (
        [PlanSettingID] int NOT NULL IDENTITY,
        [Name] varchar(500) NOT NULL,
        [UserID] varchar(500) NOT NULL,
        [Token] varchar(500) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [Circle] varchar(200) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_PlanSettings] PRIMARY KEY ([PlanSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [POSSettingMasters] (
        [POSSettingMasterID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [Slat] nvarchar(max) NOT NULL,
        [PublicKey] nvarchar(max) NOT NULL,
        [IsActiveWeb] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [MinimumAmount] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_POSSettingMasters] PRIMARY KEY ([POSSettingMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderCallBackLogs] (
        [ProviderCallBackID] int NOT NULL IDENTITY,
        [ProviderReply] nvarchar(max) NOT NULL,
        [ProviderID] int NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderCallBackLogs] PRIMARY KEY ([ProviderCallBackID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderMasters] (
        [ProviderID] int NOT NULL IDENTITY,
        [ProviderName] varchar(150) NOT NULL,
        [ProviderUrl] varchar(max) NOT NULL,
        [ProviderUniqueID] varchar(150) NOT NULL,
        [IPAddress] varchar(100) NOT NULL,
        [IsDelete] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [IsHLR] bit NOT NULL,
        [RechargeBalance] decimal(18,5) NOT NULL,
        [DMTBalance] decimal(18,5) NOT NULL,
        [ResponseGroupID] int NOT NULL,
        [IsRealAPI] bit NOT NULL,
        [LoginID] varchar(200) NOT NULL,
        [Token] varchar(200) NOT NULL,
        [SupportNumber] varchar(10) NOT NULL,
        [LowBalanceAmount] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderMasters] PRIMARY KEY ([ProviderID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderOperatorMasters] (
        [ProviderOperatorID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [Operatorcode] varchar(150) NULL,
        [MarginType] varchar(50) NULL,
        [CommissionAdmin] decimal(18,5) NOT NULL,
        [PendingCount] int NOT NULL,
        [FailureCount] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderOperatorMasters] PRIMARY KEY ([ProviderOperatorID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderResponseTypeMasters] (
        [ProviderResponseTypeMasterID] int NOT NULL IDENTITY,
        [Type] varchar(200) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderResponseTypeMasters] PRIMARY KEY ([ProviderResponseTypeMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PureDMTTransferBeneDetails] (
        [PureDMTTransferBeneDetailID] int NOT NULL IDENTITY,
        [SenderNumber] nvarchar(max) NOT NULL,
        [ProviderID] int NOT NULL,
        [BeneID] nvarchar(max) NOT NULL,
        [AccountNumber] nvarchar(max) NOT NULL,
        [VerifyBeneName] nvarchar(max) NOT NULL,
        [IsVerify] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PureDMTTransferBeneDetails] PRIMARY KEY ([PureDMTTransferBeneDetailID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RedirectUrlMasters] (
        [RedirectUrlMasterID] int NOT NULL IDENTITY,
        [Name] varchar(500) NOT NULL,
        [RedirectUrl] varchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_RedirectUrlMasters] PRIMARY KEY ([RedirectUrlMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RemainderOperatorAmountWises] (
        [RemainderOperatorAmountWiseID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Days] int NOT NULL,
        CONSTRAINT [PK_RemainderOperatorAmountWises] PRIMARY KEY ([RemainderOperatorAmountWiseID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RichTextMedias] (
        [ID] int NOT NULL IDENTITY,
        [Key] varchar(max) NOT NULL,
        [Content] varchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RichTextMedias] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SDKSettings] (
        [SDKSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [MerchantID] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [URL] nvarchar(max) NOT NULL,
        [PublicKey] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SDKSettings] PRIMARY KEY ([SDKSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SentSMS] (
        [SentSMSID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Mobile] varchar(20) NOT NULL,
        [Message] varchar(max) NOT NULL,
        [Type] varchar(50) NOT NULL,
        [Response] varchar(max) NOT NULL,
        [SMSMasterID] int NOT NULL,
        [TemplateID] varchar(100) NOT NULL,
        [WhatsAppNumber] varchar(10) NOT NULL,
        [Request] varchar(500) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SentSMS] PRIMARY KEY ([SentSMSID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceAndUserwiseSubscriptions] (
        [ServiceAndUserwiseSubscriptionID] int NOT NULL IDENTITY,
        [UserTypeID] int NOT NULL,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [SubscriptionType] varchar(10) NOT NULL,
        [OriginalCharge] decimal(18,5) NOT NULL,
        [Charge] decimal(18,5) NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Remark] varchar(max) NOT NULL,
        [LoginID] varchar(max) NOT NULL,
        [IsAutoDebit] bit NOT NULL,
        [ServiceSubscriptionPlanMasterID] int NOT NULL,
        [Status] varchar(max) NOT NULL,
        [DaysCount] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceAndUserwiseSubscriptions] PRIMARY KEY ([ServiceAndUserwiseSubscriptionID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceMasters] (
        [ServiceID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        [ServiceCategory] varchar(100) NOT NULL,
        [HSNCode] varchar(200) NOT NULL,
        [HSNCodeForSelfInvoice] varchar(100) NOT NULL,
        [Charge] decimal(18,5) NOT NULL,
        [MinBalance] decimal(18,5) NOT NULL,
        [IsDelete] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDisplay] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [IsOutside] bit NOT NULL,
        [IsBBPS] bit NULL,
        [IsSubscriptionAllow] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceMasters] PRIMARY KEY ([ServiceID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceSubscriptionCreatePlanMasters] (
        [ServiceSubscriptionCreatePlanMasterID] int NOT NULL IDENTITY,
        [PlanName] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ImageURL] nvarchar(max) NOT NULL,
        [IsDefault] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceSubscriptionCreatePlanMasters] PRIMARY KEY ([ServiceSubscriptionCreatePlanMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceSubscriptionPlanMasters] (
        [ServiceSubscriptionPlanMasterID] int NOT NULL IDENTITY,
        [PlanName] varchar(500) NOT NULL,
        CONSTRAINT [PK_ServiceSubscriptionPlanMasters] PRIMARY KEY ([ServiceSubscriptionPlanMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SettingMasters] (
        [SettingMasterID] int NOT NULL IDENTITY,
        [Key] varchar(max) NOT NULL,
        [Value] varchar(max) NOT NULL,
        [IsOn] bit NOT NULL,
        [IsSecure] bit NOT NULL,
        [Description] varchar(max) NOT NULL,
        [IsPassword] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SettingMasters] PRIMARY KEY ([SettingMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SignUps] (
        [SignUpID] int NOT NULL IDENTITY,
        [MobileNumber] nvarchar(max) NOT NULL,
        [PANNo] nvarchar(max) NOT NULL,
        [ReferralCode] nvarchar(max) NOT NULL,
        [EmployeeID] nvarchar(max) NOT NULL,
        [OrderID] nvarchar(max) NOT NULL,
        [ProfileID] nvarchar(max) NOT NULL,
        [RedirectURL] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [PinCode] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SignUps] PRIMARY KEY ([SignUpID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SlabMasters] (
        [SlabMasterID] int NOT NULL IDENTITY,
        [SlabName] varchar(80) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SlabMasters] PRIMARY KEY ([SlabMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SliderMasters] (
        [SliderMasterID] int NOT NULL IDENTITY,
        [SliderPath] varchar(max) NOT NULL,
        [Description] varchar(max) NOT NULL,
        [Type] varchar(10) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SliderMasters] PRIMARY KEY ([SliderMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SmsMasters] (
        [SMSMasterID] int NOT NULL IDENTITY,
        [SMSString] varchar(max) NOT NULL,
        [BalanceURL] varchar(500) NOT NULL,
        [WhatsAppURL] varchar(1000) NOT NULL,
        [APIName] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [MethodType] varchar(30) NOT NULL,
        [HeaderJson] varchar(max) NOT NULL,
        [MethodTypeWhatsApp] varchar(30) NOT NULL,
        [HeaderJsonWhatsApp] varchar(max) NOT NULL,
        [MobileWithOrWithout91] varchar(30) NOT NULL,
        [SMSOTPURL] varchar(1000) NOT NULL,
        [IsOTP] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SmsMasters] PRIMARY KEY ([SMSMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [StateMasters] (
        [StateID] int NOT NULL IDENTITY,
        [StateName] varchar(100) NOT NULL,
        [StateCode] varchar(10) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StateMasters] PRIMARY KEY ([StateID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SubMenuItem] (
        [MenuItemID] int NOT NULL IDENTITY,
        [MenuItemName] nvarchar(max) NOT NULL,
        [UserTypeID] int NOT NULL,
        [Sequence] int NOT NULL,
        [MappingCode] nvarchar(max) NOT NULL,
        [SubMappingCode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SubMenuItem] PRIMARY KEY ([MenuItemID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SubscriptionMasters] (
        [SubscriptionMasterID] int NOT NULL IDENTITY,
        [StartDate] datetime2 NOT NULL,
        [EndDate] nvarchar(max) NOT NULL,
        [UpdatedDOC] datetime2 NOT NULL,
        [UpdatedDOCInt] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_SubscriptionMasters] PRIMARY KEY ([SubscriptionMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SupportUserAccessMasters] (
        [SupportUserAccessMasterID] int NOT NULL IDENTITY,
        [MenuMasterID] int NOT NULL,
        [UserID] int NOT NULL,
        [MenuGUID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_SupportUserAccessMasters] PRIMARY KEY ([SupportUserAccessMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ThirdPartyServiceProviders] (
        [ThirdPartyServiceProviderID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [DomainName] nvarchar(max) NOT NULL,
        [AppID] nvarchar(max) NOT NULL,
        [AllowedServices] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ThirdPartyServiceProviders] PRIMARY KEY ([ThirdPartyServiceProviderID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TicketCategories] (
        [TicketCategoryID] int NOT NULL IDENTITY,
        [Name] varchar(100) NOT NULL,
        CONSTRAINT [PK_TicketCategories] PRIMARY KEY ([TicketCategoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionAPIUserRequestMasters] (
        [TransactionAPIUserRequestMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ApiUserRequestID] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TransactionAPIUserRequestMasters] PRIMARY KEY ([TransactionAPIUserRequestMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionDetailss] (
        [TransactionDetailID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ReferenceID] int NOT NULL,
        [AssignedUserID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [WalletID] int NOT NULL,
        [DOCType] nvarchar(max) NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ClosingBalance] decimal(18,2) NOT NULL,
        [TransactionAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [GST] decimal(18,2) NOT NULL,
        [TDS] decimal(18,2) NOT NULL,
        [AmountCreditDebit] nvarchar(max) NOT NULL,
        [CommissionCreditDebit] nvarchar(max) NOT NULL,
        [TransactionDescription] nvarchar(max) NOT NULL,
        [ConsumerNumber] nvarchar(max) NOT NULL,
        [PayMode] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TransactionDetailss] PRIMARY KEY ([TransactionDetailID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UISettingMasters] (
        [UISettingMasterID] int NOT NULL IDENTITY,
        [Key] varchar(200) NOT NULL,
        [Value] varchar(500) NOT NULL,
        [Description] varchar(500) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UISettingMasters] PRIMARY KEY ([UISettingMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UnitMasters] (
        [UnitMasterID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_UnitMasters] PRIMARY KEY ([UnitMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UPITransferSettings] (
        [UPITransferSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [CallBackURL] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [SenderLimit] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        [MinAmount] decimal(18,2) NOT NULL,
        [Sequence] int NOT NULL,
        [AuthCode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_UPITransferSettings] PRIMARY KEY ([UPITransferSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UrlTypeMasters] (
        [UrlTypeID] int NOT NULL IDENTITY,
        [UrlType] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_UrlTypeMasters] PRIMARY KEY ([UrlTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserBalanceHistories] (
        [UserBalanceHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Balance] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserBalanceHistories] PRIMARY KEY ([UserBalanceHistoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserPaymentCollectMappings] (
        [UserPaymentCollectMappingID] int NOT NULL IDENTITY,
        [ProviderName] nvarchar(max) NOT NULL,
        [UserID] int NOT NULL,
        [Key] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserPaymentCollectMappings] PRIMARY KEY ([UserPaymentCollectMappingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserPlanUtilizations] (
        [UserPlanUtilizationID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [CircleFetchCount] int NOT NULL,
        [PlanFetchCount] int NOT NULL,
        [BillFetchCount] int NOT NULL,
        [TotalPlanCount] int NOT NULL,
        [TotalCircleCount] int NOT NULL,
        [TotalBillFetchCount] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserPlanUtilizations] PRIMARY KEY ([UserPlanUtilizationID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserServiceCharges] (
        [UserServiceChargeID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [Charge] decimal(18,5) NOT NULL,
        [Remark] varchar(100) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserServiceCharges] PRIMARY KEY ([UserServiceChargeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserServiceSubscriptionCharges] (
        [UserServiceSubscriptionChargeID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [ServiceAndUserwiseSubscriptionID] int NOT NULL,
        [Charge] decimal(18,2) NOT NULL,
        [Sequence] int NOT NULL,
        [IsAdminApproved] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserServiceSubscriptionCharges] PRIMARY KEY ([UserServiceSubscriptionChargeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserTypeMasters] (
        [UserTypeID] int NOT NULL IDENTITY,
        [UserType] varchar(100) NOT NULL,
        [AliasName] varchar(10) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserTypeMasters] PRIMARY KEY ([UserTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VerificationSettings] (
        [VerificationSettingID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [CallBackURL] nvarchar(max) NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Sequence] int NOT NULL,
        [AuthCode] nvarchar(max) NOT NULL,
        [Mode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_VerificationSettings] PRIMARY KEY ([VerificationSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VirtualAccountMasters] (
        [VirtualAccountMasterID] int NOT NULL IDENTITY,
        [Prefix] nvarchar(max) NOT NULL,
        [BankName] nvarchar(max) NOT NULL,
        [IFSC] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_VirtualAccountMasters] PRIMARY KEY ([VirtualAccountMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VirtualBalances] (
        [VirtualBalanceID] int NOT NULL IDENTITY,
        [Amount] decimal(18,2) NOT NULL,
        [WalletID] int NOT NULL,
        [DOCType] nvarchar(max) NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ClosingBalance] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_VirtualBalances] PRIMARY KEY ([VirtualBalanceID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VoucherTypes] (
        [VoucherTypeID] int NOT NULL IDENTITY,
        [VoucherTypeName] varchar(30) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_VoucherTypes] PRIMARY KEY ([VoucherTypeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WalletMaster] (
        [WalletID] int NOT NULL IDENTITY,
        [ShortName] varchar(50) NOT NULL,
        [Name] varchar(50) NOT NULL,
        [UISequence] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WalletMaster] PRIMARY KEY ([WalletID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WebHookLogs] (
        [WebHookLogID] int NOT NULL IDENTITY,
        [Type] nvarchar(max) NOT NULL,
        [RequestURL] nvarchar(max) NOT NULL,
        [Response] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WebHookLogs] PRIMARY KEY ([WebHookLogID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WebHookSettings] (
        [WebHookSettingID] int NOT NULL IDENTITY,
        [IsActive] bit NOT NULL,
        [Type] varchar(100) NOT NULL,
        [URL] varchar(max) NOT NULL,
        [MethodType] varchar(30) NOT NULL,
        [HeaderJson] varchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WebHookSettings] PRIMARY KEY ([WebHookSettingID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WrongPasswordHistorys] (
        [WrongPasswordHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [MobileNumber] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WrongPasswordHistorys] PRIMARY KEY ([WrongPasswordHistoryID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WrongPasswordLogs] (
        [WrongPasswordLogID] bigint NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WrongPassword] varchar(500) NOT NULL,
        [IPAddress] varchar(50) NOT NULL,
        [LogAction] varchar(100) NOT NULL,
        [MobileNumber] varchar(20) NOT NULL,
        [UserAgent] varchar(200) NOT NULL,
        [AttemptCount] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WrongPasswordLogs] PRIMARY KEY ([WrongPasswordLogID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ZoneMasters] (
        [ZoneMasterID] int NOT NULL IDENTITY,
        [Name] varchar(300) NOT NULL,
        CONSTRAINT [PK_ZoneMasters] PRIMARY KEY ([ZoneMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SubCategorys] (
        [SubCategoryID] int NOT NULL IDENTITY,
        [CategoryID] int NOT NULL,
        [SubCategoryName] nvarchar(max) NOT NULL,
        [ShortDescription] nvarchar(max) NOT NULL,
        [LongDescription] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SubCategorys] PRIMARY KEY ([SubCategoryID]),
        CONSTRAINT [FK_SubCategorys_Categorys_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [Categorys] ([CategoryID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CategoryChannelMappings] (
        [CategoryChannelMappingID] int NOT NULL IDENTITY,
        [ChannelCategoryID] int NOT NULL,
        [ChannelName] nvarchar(max) NOT NULL,
        [ChannelImagePath] nvarchar(max) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CategoryChannelMappings] PRIMARY KEY ([CategoryChannelMappingID]),
        CONSTRAINT [FK_CategoryChannelMappings_ChannelCategorys_ChannelCategoryID] FOREIGN KEY ([ChannelCategoryID]) REFERENCES [ChannelCategorys] ([ChannelCategoryID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OnboardingUserDocuments] (
        [OnboardingUserDocumentID] int NOT NULL IDENTITY,
        [Ref_ID] nvarchar(max) NOT NULL,
        [Type] int NOT NULL,
        [OldImage] nvarchar(max) NOT NULL,
        [NewImage] nvarchar(max) NOT NULL,
        [OnboardingUserID] int NOT NULL,
        CONSTRAINT [PK_OnboardingUserDocuments] PRIMARY KEY ([OnboardingUserDocumentID]),
        CONSTRAINT [FK_OnboardingUserDocuments_OnboardingUsers_OnboardingUserID] FOREIGN KEY ([OnboardingUserID]) REFERENCES [OnboardingUsers] ([OnboardingUserID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PayoutVendorBeneficiaryMappings] (
        [PayoutVendorBeneficiaryMappingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [PayoutVendorBeneficiaryID] varchar(100) NOT NULL,
        [PayoutBeneficaryID] int NOT NULL,
        CONSTRAINT [PK_PayoutVendorBeneficiaryMappings] PRIMARY KEY ([PayoutVendorBeneficiaryMappingID]),
        CONSTRAINT [FK_PayoutVendorBeneficiaryMappings_PayoutBeneficiaries_PayoutBeneficaryID] FOREIGN KEY ([PayoutBeneficaryID]) REFERENCES [PayoutBeneficiaries] ([PayoutBeneficaryID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderResponseMasters] (
        [ProviderResponseMasterID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [TextMustexits] varchar(250) NOT NULL,
        [Type] varchar(50) NOT NULL,
        [Status] varchar(50) NOT NULL,
        [Response] varchar(1000) NOT NULL,
        [IsReference] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderResponseMasters] PRIMARY KEY ([ProviderResponseMasterID]),
        CONSTRAINT [FK_ProviderResponseMasters_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderStateConfigurations] (
        [ProviderStateConfigurationID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [StateID] int NOT NULL,
        [StateCode] varchar(30) NOT NULL,
        CONSTRAINT [PK_ProviderStateConfigurations] PRIMARY KEY ([ProviderStateConfigurationID]),
        CONSTRAINT [FK_ProviderStateConfigurations_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ReconciliationSettings] (
        [ReconciliationSettingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [ConsumerNumber] varchar(100) NOT NULL,
        [Amount] varchar(100) NOT NULL,
        [Status] varchar(100) NOT NULL,
        [WorksheetName] varchar(100) NOT NULL,
        [APIUserRequestID] varchar(100) NOT NULL,
        [SuccessStatusAs] varchar(100) NOT NULL,
        [FailureStatusAs] varchar(100) NOT NULL,
        CONSTRAINT [PK_ReconciliationSettings] PRIMARY KEY ([ReconciliationSettingID]),
        CONSTRAINT [FK_ReconciliationSettings_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorMasters] (
        [OperatorID] int NOT NULL IDENTITY,
        [ServiceID] int NOT NULL,
        [OperatorName] varchar(100) NOT NULL,
        [MinimumRechargeAmount] decimal(18,5) NOT NULL,
        [MaximumRechargeAmount] decimal(18,5) NOT NULL,
        [RepeatRechargeInMinute] int NOT NULL,
        [BlockedTill] datetime2 NOT NULL,
        [OperatorImagePath] varchar(500) NOT NULL,
        [Opcode] varchar(50) NOT NULL,
        [BusinessModel] varchar(50) NOT NULL,
        [NumberLength] int NOT NULL,
        [HSNCode] varchar(max) NOT NULL,
        [Prefix] varchar(max) NOT NULL,
        [AccountStartWith] varchar(max) NOT NULL,
        [SequenceLogic] varchar(50) NOT NULL,
        [MinLength] int NOT NULL,
        [MaxLength] int NOT NULL,
        [AccountName] varchar(100) NOT NULL,
        [Remark] varchar(100) NOT NULL,
        [TollFreeNumber] varchar(max) NOT NULL,
        [UISequence] decimal(18,2) NOT NULL,
        [BlockedAmount] varchar(max) NOT NULL,
        [AllowedFor] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NULL,
        [IsTimelyBlocked] bit NULL,
        [IsRandom] bit NULL,
        [IsBBPS] bit NULL,
        [IsBillFetch] bit NOT NULL,
        [IsPartialBill] bit NOT NULL,
        [IsViewRequired] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorMasters] PRIMARY KEY ([OperatorID]),
        CONSTRAINT [FK_OperatorMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderDetails] (
        [ProviderDetailID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [UrlType] varchar(100) NOT NULL,
        [ProviderURL] varchar(max) NOT NULL,
        [MethodType] varchar(30) NOT NULL,
        [Balance] decimal(18,5) NOT NULL,
        [HeaderJson] varchar(max) NOT NULL,
        [DateFormat] varchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [TransactionIDPrefix] varchar(20) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderDetails] PRIMARY KEY ([ProviderDetailID]),
        CONSTRAINT [FK_ProviderDetails_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderDetails_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceWiseSliderSettings] (
        [ServiceWiseSliderSettingID] int NOT NULL IDENTITY,
        [ServiceID] int NOT NULL,
        [ImagePath] varchar(500) NOT NULL,
        [Description] varchar(max) NOT NULL,
        [ReferenceLink] varchar(500) NOT NULL,
        [Priority] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [Type] varchar(10) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceWiseSliderSettings] PRIMARY KEY ([ServiceWiseSliderSettingID]),
        CONSTRAINT [FK_ServiceWiseSliderSettings_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ServiceSubscriptionPlanDetailss] (
        [ServiceSubscriptionPlanDetailsID] int NOT NULL IDENTITY,
        [ServiceSubscriptionCreatePlanMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ServiceSubscriptionPlanDetailss] PRIMARY KEY ([ServiceSubscriptionPlanDetailsID]),
        CONSTRAINT [FK_ServiceSubscriptionPlanDetailss_ServiceSubscriptionCreatePlanMasters_ServiceSubscriptionCreatePlanMasterID] FOREIGN KEY ([ServiceSubscriptionCreatePlanMasterID]) REFERENCES [ServiceSubscriptionCreatePlanMasters] ([ServiceSubscriptionCreatePlanMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AuthorizationMasters] (
        [AuthorizationMasterID] int NOT NULL IDENTITY,
        [Controller] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [UserTypeID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AuthorizationMasters] PRIMARY KEY ([AuthorizationMasterID]),
        CONSTRAINT [FK_AuthorizationMasters_UserTypeMasters_UserTypeID] FOREIGN KEY ([UserTypeID]) REFERENCES [UserTypeMasters] ([UserTypeID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserMasters] (
        [UserMasterID] int NOT NULL IDENTITY,
        [ParentID] int NULL,
        [UserTypeID] int NOT NULL,
        [PersonName] varchar(200) NOT NULL,
        [FirmName] varchar(100) NOT NULL,
        [MobileNumber] varchar(20) NOT NULL,
        [PasswordHash] varchar(200) NOT NULL,
        [EmailID] varchar(70) NOT NULL,
        [GSTNo] varchar(15) NOT NULL,
        [PANNo] varchar(10) NOT NULL,
        [CIN] varchar(20) NOT NULL,
        [AadharNo] varchar(16) NOT NULL,
        [Address] varchar(max) NOT NULL,
        [Mpin] int NOT NULL,
        [UniqueReferenceNo] varchar(50) NOT NULL,
        [IsKYCApproved] bit NOT NULL,
        [IsLoginOTP] bit NOT NULL,
        [StateID] int NULL,
        [APIToken] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [ProfileImagePath] varchar(500) NOT NULL,
        [VirtualAccountNumber] varchar(50) NOT NULL,
        [IsInvoiceApplicable] bit NOT NULL,
        [JoiningByUserID] int NULL,
        [RegistrationType] varchar(100) NOT NULL,
        [IsTwoFA] bit NOT NULL,
        [CityName] varchar(100) NOT NULL,
        [Pincode] varchar(100) NOT NULL,
        [WalletIDforEcollection] int NULL,
        [StaticQRWalletID] int NOT NULL,
        [AliasName] varchar(100) NOT NULL,
        [IsPin] bit NOT NULL,
        [WhatsAppNumber] varchar(21) NOT NULL,
        [ReferralCode] varchar(5) NOT NULL,
        [ReferralAmount] decimal(18,5) NOT NULL,
        [ReferralBenefitMinAmount] decimal(18,5) NOT NULL,
        [IseKYC] bit NOT NULL,
        [IsCommissionHold] bit NOT NULL,
        [IsInitialPassword] bit NOT NULL,
        [ShopAddress] varchar(max) NOT NULL,
        [BusinessGSTStateCode] varchar(150) NOT NULL,
        [PartnerConfigJson] varchar(200) NOT NULL,
        [AppFCMToken] varchar(500) NOT NULL,
        [WebFCMToken] varchar(500) NOT NULL,
        [NPCIAgentID] varchar(100) NOT NULL,
        [PaySwiffMerchantId] varchar(100) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserMasters] PRIMARY KEY ([UserMasterID]),
        CONSTRAINT [FK_UserMasters_StateMasters_StateID] FOREIGN KEY ([StateID]) REFERENCES [StateMasters] ([StateID]),
        CONSTRAINT [FK_UserMasters_UserTypeMasters_UserTypeID] FOREIGN KEY ([UserTypeID]) REFERENCES [UserTypeMasters] ([UserTypeID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VoucherMasters] (
        [VoucherMasterID] int NOT NULL IDENTITY,
        [VoucherTypeID] int NOT NULL,
        [SelectDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [CrGroup] varchar(70) NULL,
        [CrName] varchar(70) NULL,
        [DrGroup] varchar(70) NULL,
        [DrName] varchar(70) NULL,
        [Remark] varchar(70) NULL,
        [Status] varchar(70) NULL,
        [SubmitedBy] int NOT NULL,
        [ApproveBy] int NOT NULL,
        [UserID] int NOT NULL,
        [Is_Delete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_VoucherMasters] PRIMARY KEY ([VoucherMasterID]),
        CONSTRAINT [FK_VoucherMasters_VoucherTypes_VoucherTypeID] FOREIGN KEY ([VoucherTypeID]) REFERENCES [VoucherTypes] ([VoucherTypeID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderPurchases] (
        [ProviderPurchaseID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [Remark] varchar(100) NOT NULL,
        [PurchaseDate] datetime2 NOT NULL,
        [PurchaseDOCInt] int NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderPurchases] PRIMARY KEY ([ProviderPurchaseID]),
        CONSTRAINT [FK_ProviderPurchases_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderPurchases_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [WalletServiceMappings] (
        [WalletServiceMappingID] int NOT NULL IDENTITY,
        [WalletID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WalletServiceMappings] PRIMARY KEY ([WalletServiceMappingID]),
        CONSTRAINT [FK_WalletServiceMappings_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WalletServiceMappings_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BBPSConfigurations] (
        [BBPSConfigurationID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [Label] nvarchar(max) NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [PlaceHolder] nvarchar(max) NOT NULL,
        [IsRequired] bit NOT NULL,
        [ErrorMeeage] nvarchar(max) NOT NULL,
        [Sequence] int NOT NULL,
        [Length] int NOT NULL,
        [ConfigList] nvarchar(max) NOT NULL,
        [IsDisplay] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BBPSConfigurations] PRIMARY KEY ([BBPSConfigurationID]),
        CONSTRAINT [FK_BBPSConfigurations_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ChannelSlabDetails] (
        [ChannelSlabDetailID] int NOT NULL IDENTITY,
        [ChannelSlabMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [Commission] decimal(18,5) NOT NULL,
        [CommissionType] varchar(100) NOT NULL,
        [FromAmount] decimal(18,5) NOT NULL,
        [ToAmount] decimal(18,5) NOT NULL,
        [UplineUserID] int NOT NULL,
        [RealCommission] decimal(18,5) NOT NULL,
        [RealCommissionType] varchar(100) NOT NULL,
        [GeneralCommissionCategory] varchar(50) NOT NULL,
        [RealCommissionCategory] varchar(50) NOT NULL,
        [UplineCommission] decimal(18,5) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ChannelSlabDetails] PRIMARY KEY ([ChannelSlabDetailID]),
        CONSTRAINT [FK_ChannelSlabDetails_ChannelSlabMasters_ChannelSlabMasterID] FOREIGN KEY ([ChannelSlabMasterID]) REFERENCES [ChannelSlabMasters] ([ChannelSlabMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChannelSlabDetails_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChannelSlabDetails_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CircleWiseCommissions] (
        [CircleWiseCommissionID] int NOT NULL IDENTITY,
        [SlabMasterID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [StateID] int NOT NULL,
        [FromAmount] decimal(18,5) NOT NULL,
        [ToAmount] decimal(18,5) NOT NULL,
        [RetailerComm] decimal(18,5) NOT NULL,
        [RetailerCommType] varchar(500) NULL,
        [RetailerCommissionCategory] varchar(500) NULL,
        [DistributorComm] decimal(18,5) NOT NULL,
        [DistributorCommType] varchar(500) NULL,
        [DistributorCommissionCategory] varchar(500) NULL,
        [SuperDistributorComm] decimal(18,5) NOT NULL,
        [SuperDistributorCommType] varchar(500) NULL,
        [SuperDistributorCommissionCategory] varchar(500) NULL,
        [ClientComm] decimal(18,5) NOT NULL,
        [ClientCommType] varchar(500) NULL,
        [ClientCommissionCategory] varchar(500) NULL,
        CONSTRAINT [PK_CircleWiseCommissions] PRIMARY KEY ([CircleWiseCommissionID]),
        CONSTRAINT [FK_CircleWiseCommissions_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CircleWiseCommissions_SlabMasters_SlabMasterID] FOREIGN KEY ([SlabMasterID]) REFERENCES [SlabMasters] ([SlabMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LapuMasters] (
        [LapuMasterID] int NOT NULL IDENTITY,
        [LapuPartyMasterID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [LapuNo] varchar(50) NOT NULL,
        [ProivderLapuID] varchar(50) NOT NULL,
        [ClosingBalance] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LapuMasters] PRIMARY KEY ([LapuMasterID]),
        CONSTRAINT [FK_LapuMasters_LapuPartyMasters_LapuPartyMasterID] FOREIGN KEY ([LapuPartyMasterID]) REFERENCES [LapuPartyMasters] ([LapuPartyMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LapuMasters_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorSequences] (
        [OperatorSequenceID] int NOT NULL IDENTITY,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [Count] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorSequences] PRIMARY KEY ([OperatorSequenceID]),
        CONSTRAINT [FK_OperatorSequences_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorSequences_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorSequences_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorStateMappings] (
        [OperatorStateMappingID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [ZoneMasterID] int NOT NULL,
        [HLR] varchar(max) NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorStateMappings] PRIMARY KEY ([OperatorStateMappingID]),
        CONSTRAINT [FK_OperatorStateMappings_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderConfigurationAdminCommissions] (
        [ProviderConfigurationAdminCommissionID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [FromAmount] decimal(18,5) NOT NULL,
        [ToAmount] decimal(18,5) NOT NULL,
        [AdminComm] decimal(18,5) NOT NULL,
        [AdminCommType] varchar(20) NOT NULL,
        [AdminCommissionCategory] varchar(20) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderConfigurationAdminCommissions] PRIMARY KEY ([ProviderConfigurationAdminCommissionID]),
        CONSTRAINT [FK_ProviderConfigurationAdminCommissions_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderConfigurationAdminCommissions_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderConfigurationAdminCommissions_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderConfigurations] (
        [ConfigurationID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OpCode] varchar(20) NOT NULL,
        [Type] varchar(20) NOT NULL,
        [Lapu] varchar(200) NOT NULL,
        [Quantity] varchar(3000) NOT NULL,
        [AdminComm] decimal(18,5) NOT NULL,
        [AdminCommType] varchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CommissionCategory] varchar(20) NOT NULL,
        [PendingCount] int NOT NULL,
        [PendingInMinute] int NOT NULL,
        [FailCount] int NOT NULL,
        [FailInMinute] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderConfigurations] PRIMARY KEY ([ConfigurationID]),
        CONSTRAINT [FK_ProviderConfigurations_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderConfigurations_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderConfigurations_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderOperatorCircleMasters] (
        [ProviderOperatorCircleMasterID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [StateID] int NOT NULL,
        [Opcode] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ProviderOperatorCircleMasters] PRIMARY KEY ([ProviderOperatorCircleMasterID]),
        CONSTRAINT [FK_ProviderOperatorCircleMasters_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderOperatorCircleMasters_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderOperatorDenominationMappings] (
        [ProviderOperatorDenominationMappingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [DenominationMasterID] int NOT NULL,
        CONSTRAINT [PK_ProviderOperatorDenominationMappings] PRIMARY KEY ([ProviderOperatorDenominationMappingID]),
        CONSTRAINT [FK_ProviderOperatorDenominationMappings_DenominationMasters_DenominationMasterID] FOREIGN KEY ([DenominationMasterID]) REFERENCES [DenominationMasters] ([DenominationMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderOperatorDenominationMappings_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProviderOperatorDenominationMappings_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RangeWiseSequences] (
        [RangeWiseSequenceID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [FromAmount] decimal(18,5) NOT NULL,
        [ToAmount] decimal(18,5) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RangeWiseSequences] PRIMARY KEY ([RangeWiseSequenceID]),
        CONSTRAINT [FK_RangeWiseSequences_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RangeWiseSequences_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RangeWiseSequences_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RealProviderMappings] (
        [RealProviderMappingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        CONSTRAINT [PK_RealProviderMappings] PRIMARY KEY ([RealProviderMappingID]),
        CONSTRAINT [FK_RealProviderMappings_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ROfferCommissions] (
        [ROfferCommissionID] int NOT NULL IDENTITY,
        [SlabMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [RetailerComm] decimal(18,5) NOT NULL,
        [RetailerCommType] varchar(500) NOT NULL,
        [RetailerCommissionCategory] varchar(500) NOT NULL,
        [DistributorComm] decimal(18,5) NOT NULL,
        [DistributorCommType] varchar(500) NOT NULL,
        [DistributorCommissionCategory] varchar(500) NOT NULL,
        [SuperDistributorComm] decimal(18,5) NOT NULL,
        [SuperDistributorCommType] varchar(500) NOT NULL,
        [SuperDistributorCommissionCategory] varchar(500) NOT NULL,
        [ClientComm] decimal(18,5) NOT NULL,
        [ClientCommType] varchar(500) NOT NULL,
        [ClientCommissionCategory] varchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ROfferCommissions] PRIMARY KEY ([ROfferCommissionID]),
        CONSTRAINT [FK_ROfferCommissions_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ROfferCommissions_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ROfferCommissions_SlabMasters_SlabMasterID] FOREIGN KEY ([SlabMasterID]) REFERENCES [SlabMasters] ([SlabMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SlabDetails] (
        [SlabDetailID] int NOT NULL IDENTITY,
        [SlabMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [FromAmount] decimal(18,5) NOT NULL,
        [ToAmount] decimal(18,5) NOT NULL,
        [RetailerComm] decimal(18,5) NOT NULL,
        [RetailerCommType] varchar(100) NOT NULL,
        [RetailerCommissionCategory] varchar(100) NOT NULL,
        [DistributorComm] decimal(18,5) NOT NULL,
        [DistributorCommType] varchar(100) NOT NULL,
        [DistributorCommissionCategory] varchar(100) NOT NULL,
        [SuperDistributorComm] decimal(18,5) NOT NULL,
        [SuperDistributorCommType] varchar(100) NOT NULL,
        [SuperDistributorCommissionCategory] varchar(100) NOT NULL,
        [ClientComm] decimal(18,5) NOT NULL,
        [ClientCommType] varchar(100) NOT NULL,
        [ClientCommissionCategory] varchar(100) NOT NULL,
        [RealRetailerComm] decimal(18,5) NOT NULL,
        [RealRetailerCommType] varchar(100) NOT NULL,
        [RealRetailerCommissionCategory] varchar(100) NOT NULL,
        [RealDistributorComm] decimal(18,5) NOT NULL,
        [RealDistributorCommType] varchar(100) NOT NULL,
        [RealDistributorCommissionCategory] varchar(100) NOT NULL,
        [RealSuperDistributorComm] decimal(18,5) NOT NULL,
        [RealSuperDistributorCommType] varchar(100) NOT NULL,
        [RealSuperDistributorCommissionCategory] varchar(100) NOT NULL,
        [RealClientComm] decimal(18,5) NOT NULL,
        [RealClientCommType] varchar(100) NOT NULL,
        [RealClientCommissionCategory] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SlabDetails] PRIMARY KEY ([SlabDetailID]),
        CONSTRAINT [FK_SlabDetails_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SlabDetails_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SlabDetails_SlabMasters_SlabMasterID] FOREIGN KEY ([SlabMasterID]) REFERENCES [SlabMasters] ([SlabMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [APIUserCallbackRequestResponseLogs] (
        [APIUserCallbackRequestResponseLogID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Request] varchar(max) NOT NULL,
        [Response] varchar(max) NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [APIUserRequestID] varchar(100) NOT NULL,
        [ConsumerNumber] varchar(100) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_APIUserCallbackRequestResponseLogs] PRIMARY KEY ([APIUserCallbackRequestResponseLogID]),
        CONSTRAINT [FK_APIUserCallbackRequestResponseLogs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [APIUserInitialRequestResponseLogs] (
        [APIUserInitialRequestResponseLogID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Request] varchar(max) NOT NULL,
        [Response] varchar(max) NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [APIUserRequestID] varchar(100) NOT NULL,
        [ConsumerNumber] varchar(100) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_APIUserInitialRequestResponseLogs] PRIMARY KEY ([APIUserInitialRequestResponseLogID]),
        CONSTRAINT [FK_APIUserInitialRequestResponseLogs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [AutoTransferUserwiseMappings] (
        [AutoTransferUserwiseMappingID] int NOT NULL IDENTITY,
        [ParentID] int NOT NULL,
        [UserID] int NOT NULL,
        [OTP] varchar(10) NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [MaxOutstanding] decimal(18,5) NOT NULL,
        [RechargeLowBalanceReached] decimal(18,5) NOT NULL,
        [RechargeAmountIncreasedBy] decimal(18,5) NOT NULL,
        [DMTLowBalanceReached] decimal(18,5) NOT NULL,
        [DMTAmountIncreasedBy] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AutoTransferUserwiseMappings] PRIMARY KEY ([AutoTransferUserwiseMappingID]),
        CONSTRAINT [FK_AutoTransferUserwiseMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BankRequestResponseLogs] (
        [BankRequestResponseLogID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [TransactionID] int NOT NULL,
        [ConsumerNumber] nvarchar(max) NOT NULL,
        [Request] nvarchar(max) NOT NULL,
        [Response] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [Through] nvarchar(max) NOT NULL,
        [ServiceID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BankRequestResponseLogs] PRIMARY KEY ([BankRequestResponseLogID]),
        CONSTRAINT [FK_BankRequestResponseLogs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CallbackMasters] (
        [CallbackMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [CallbackURL] varchar(500) NULL,
        [IsDelete] bit NOT NULL,
        [ServiceID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CallbackMasters] PRIMARY KEY ([CallbackMasterID]),
        CONSTRAINT [FK_CallbackMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CallbackMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CallMeRequests] (
        [CallMeRequestID] int NOT NULL IDENTITY,
        [CallMeCategoryMasterID] int NOT NULL,
        [UserID] int NOT NULL,
        [UplineUserID] int NOT NULL,
        [UplineRemark] varchar(200) NOT NULL,
        [Description] varchar(200) NOT NULL,
        [Status] varchar(50) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CallMeRequests] PRIMARY KEY ([CallMeRequestID]),
        CONSTRAINT [FK_CallMeRequests_CallMeCategoryMaster_CallMeCategoryMasterID] FOREIGN KEY ([CallMeCategoryMasterID]) REFERENCES [CallMeCategoryMaster] ([CallMeCategoryMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CallMeRequests_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CashCollectionMasters] (
        [CashCollectionMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Outstanding] decimal(18,5) NOT NULL,
        [MaxOutstanding] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CashCollectionMasters] PRIMARY KEY ([CashCollectionMasterID]),
        CONSTRAINT [FK_CashCollectionMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CircleWiseOperatorLimits] (
        [CircleWiseOperatorLimitID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [StateID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [AmountLimit] decimal(18,5) NOT NULL,
        CONSTRAINT [PK_CircleWiseOperatorLimits] PRIMARY KEY ([CircleWiseOperatorLimitID]),
        CONSTRAINT [FK_CircleWiseOperatorLimits_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CircleWiseOperatorLimits_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [CircleWiseUserWiseOperatorOffs] (
        [CircleWiseUserWiseOperatorOffID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [StateID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CircleWiseUserWiseOperatorOffs] PRIMARY KEY ([CircleWiseUserWiseOperatorOffID]),
        CONSTRAINT [FK_CircleWiseUserWiseOperatorOffs_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CircleWiseUserWiseOperatorOffs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ComplaintMasters] (
        [ComplaintMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ConsumerNumber] varchar(50) NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [CustomerRemark] varchar(max) NOT NULL,
        [AdminRemark] varchar(max) NOT NULL,
        [ComplaintStatus] varchar(100) NOT NULL,
        [TransactionMasterDOCInt] int NOT NULL,
        [TransactionDate] datetime2 NOT NULL,
        [ComplaintType] varchar(100) NULL,
        [NewConsumerNumber] varchar(50) NULL,
        [ProviderID] int NOT NULL,
        [ProviderRequestID] varchar(100) NULL,
        [ReferenceNo] varchar(100) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ComplaintMasters] PRIMARY KEY ([ComplaintMasterID]),
        CONSTRAINT [FK_ComplaintMasters_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComplaintMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ComplaintMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DailyAuthenticationMasters] (
        [AuthenticationID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [Pipe] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DailyAuthenticationMasters] PRIMARY KEY ([AuthenticationID]),
        CONSTRAINT [FK_DailyAuthenticationMasters_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DailyAuthenticationMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DailyAuthenticationMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DMTSenders] (
        [SenderID] int NOT NULL IDENTITY,
        [Name] varchar(200) NOT NULL,
        [Mobile] varchar(15) NOT NULL,
        [GUID] varchar(50) NOT NULL,
        [Address] varchar(500) NOT NULL,
        [UserID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [IsKYC] bit NOT NULL,
        [KYCDocumentURL] varchar(2000) NOT NULL,
        [IPAddress] varchar(500) NOT NULL,
        [Pincode] varchar(10) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DMTSenders] PRIMARY KEY ([SenderID]),
        CONSTRAINT [FK_DMTSenders_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [EcollectionDetails] (
        [EcollectionDetailsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [VirtualACCode] varchar(30) NOT NULL,
        [UTR] varchar(50) NOT NULL,
        [PayeeAccountNumber] varchar(30) NOT NULL,
        [Status] varchar(30) NOT NULL,
        [Reason] varchar(100) NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_EcollectionDetails] PRIMARY KEY ([EcollectionDetailsID]),
        CONSTRAINT [FK_EcollectionDetails_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [eKYCLogs] (
        [eKYCLogID] int NOT NULL IDENTITY,
        [DocumentNo] nvarchar(max) NOT NULL,
        [UserID] int NOT NULL,
        [Response] nvarchar(max) NOT NULL,
        [DocumentType] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_eKYCLogs] PRIMARY KEY ([eKYCLogID]),
        CONSTRAINT [FK_eKYCLogs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [eKYCMasters] (
        [eKYCMasterID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [UserID] int NOT NULL,
        [OrderID] nvarchar(max) NOT NULL,
        [OTPGenerateHash] nvarchar(max) NOT NULL,
        [ChargeAmount] decimal(18,2) NOT NULL,
        [OTPGenerateRequest] nvarchar(max) NOT NULL,
        [OTPGenerateResponse] nvarchar(max) NOT NULL,
        [VerifyRequest] nvarchar(max) NOT NULL,
        [VerifyResponse] nvarchar(max) NOT NULL,
        [OTPToken] nvarchar(max) NOT NULL,
        [KYCToken] nvarchar(max) NOT NULL,
        [wadh] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_eKYCMasters] PRIMARY KEY ([eKYCMasterID]),
        CONSTRAINT [FK_eKYCMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [FundTransactions] (
        [FundTransactionID] int NOT NULL IDENTITY,
        [SenderID] int NOT NULL,
        [ReceiverID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [OriginalAmount] decimal(18,5) NOT NULL,
        [CashCollectionAmount] decimal(18,5) NOT NULL,
        [SenderOpeningBalance] decimal(18,5) NOT NULL,
        [SenderClosingBalance] decimal(18,5) NOT NULL,
        [ReceiverOpeningBalance] decimal(18,5) NOT NULL,
        [ReceiverClosingBalance] decimal(18,5) NOT NULL,
        [Remark] varchar(100) NOT NULL,
        [UTRNumber] varchar(100) NOT NULL,
        [Status] varchar(100) NOT NULL,
        [Type] varchar(100) NOT NULL,
        [AdminRemark] varchar(100) NOT NULL,
        [PayMode] varchar(100) NOT NULL,
        [Through] varchar(100) NOT NULL,
        [RequestToID] int NOT NULL,
        [CashCollectionRemark] varchar(100) NOT NULL,
        [Description] varchar(200) NOT NULL,
        [BankName] varchar(200) NOT NULL,
        [ReceiptPath] varchar(200) NOT NULL,
        [TransactionDOC] datetime2 NOT NULL,
        [TransactionDOCInt] int NOT NULL,
        [IsDelete] bit NOT NULL,
        [RequestAcceptedUserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FundTransactions] PRIMARY KEY ([FundTransactionID]),
        CONSTRAINT [FK_FundTransactions_UserMasters_ReceiverID] FOREIGN KEY ([ReceiverID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundTransactions_UserMasters_RequestToID] FOREIGN KEY ([RequestToID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundTransactions_UserMasters_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FundTransactions_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [GSTHolds] (
        [GSTHoldID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Month] int NOT NULL,
        [Year] int NOT NULL,
        [HoldAmount] decimal(18,5) NOT NULL,
        [GSTUploadedDate] datetime2 NOT NULL,
        [ReceiptPath] varchar(max) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [GSTUploadRemark] varchar(max) NOT NULL,
        [AdminRemark] varchar(max) NOT NULL,
        [GSTNo] varchar(15) NOT NULL,
        [StateID] int NOT NULL,
        [TaxableValue] decimal(18,5) NOT NULL,
        [IGST] decimal(18,5) NOT NULL,
        [CGST] decimal(18,5) NOT NULL,
        [SGST] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_GSTHolds] PRIMARY KEY ([GSTHoldID]),
        CONSTRAINT [FK_GSTHolds_StateMasters_StateID] FOREIGN KEY ([StateID]) REFERENCES [StateMasters] ([StateID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GSTHolds_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Inboxes] (
        [InboxID] int NOT NULL IDENTITY,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [UserID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ConsumerNumber] varchar(50) NOT NULL,
        [Status] varchar(15) NOT NULL,
        [Through] varchar(20) NOT NULL,
        [IPAddress] varchar(150) NOT NULL,
        [Param1] varchar(150) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Inboxes] PRIMARY KEY ([InboxID]),
        CONSTRAINT [FK_Inboxes_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Inboxes_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Inboxes_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [InvoiceNumberHistories] (
        [InvoiceNumberHistoryID] int NOT NULL IDENTITY,
        [ServiceCategory] varchar(50) NOT NULL,
        [UserID] int NOT NULL,
        [Month] int NOT NULL,
        [Year] int NOT NULL,
        [InvoiceNo] varchar(16) NOT NULL,
        [InvoiceType] varchar(16) NOT NULL,
        [IncrementValue] int NOT NULL,
        CONSTRAINT [PK_InvoiceNumberHistories] PRIMARY KEY ([InvoiceNumberHistoryID]),
        CONSTRAINT [FK_InvoiceNumberHistories_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [IPMasters] (
        [IpMasterID] int NOT NULL IDENTITY,
        [IP] varchar(100) NOT NULL,
        [UserID] int NOT NULL,
        [Status] varchar(50) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_IPMasters] PRIMARY KEY ([IpMasterID]),
        CONSTRAINT [FK_IPMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LoginHistories] (
        [LoginHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [IPAddress] varchar(150) NOT NULL,
        [Token] varchar(500) NOT NULL,
        [Through] varchar(50) NOT NULL,
        [Date] datetime2 NOT NULL,
        [LoginMessage] varchar(max) NOT NULL,
        [Location] varchar(max) NOT NULL,
        [SimCardDetails] varchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LoginHistories] PRIMARY KEY ([LoginHistoryID]),
        CONSTRAINT [FK_LoginHistories_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [LowBalanceConfigurations] (
        [LowBalanceConfigurationID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [MobileNumber] varchar(10) NULL,
        [EmailID] varchar(100) NULL,
        [WebHookURL] varchar(max) NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_LowBalanceConfigurations] PRIMARY KEY ([LowBalanceConfigurationID]),
        CONSTRAINT [FK_LowBalanceConfigurations_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LowBalanceConfigurations_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [NewsMasters] (
        [NewsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Title] nvarchar(1000) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsView] bit NOT NULL,
        [ImagePath] nvarchar(500) NOT NULL,
        [HyperLink] nvarchar(200) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_NewsMasters] PRIMARY KEY ([NewsID]),
        CONSTRAINT [FK_NewsMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperationalHistorys] (
        [OperationalHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [APIType] nvarchar(max) NOT NULL,
        [DocNo] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperationalHistorys] PRIMARY KEY ([OperationalHistoryID]),
        CONSTRAINT [FK_OperationalHistorys_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorSequenceUserWises] (
        [OperatorSequenceUserWiseID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorSequenceUserWises] PRIMARY KEY ([OperatorSequenceUserWiseID]),
        CONSTRAINT [FK_OperatorSequenceUserWises_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorSequenceUserWises_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorSequenceUserWises_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorSequenceUserWises_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OperatorUserwises] (
        [OperatorUserwiseID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [UsageLimit] int NOT NULL,
        [ServiceID] int NOT NULL,
        [BlockedAmount] varchar(max) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OperatorUserwises] PRIMARY KEY ([OperatorUserwiseID]),
        CONSTRAINT [FK_OperatorUserwises_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorUserwises_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperatorUserwises_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OverdraftDetails] (
        [OverdraftDetailID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [OverdraftOpening] decimal(18,2) NOT NULL,
        [OverdraftAmount] decimal(18,2) NOT NULL,
        [OverdraftClosing] decimal(18,2) NOT NULL,
        [UserRemark] nvarchar(max) NOT NULL,
        [AdminRemark] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [OpeningWalletBalance] decimal(18,2) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OverdraftDetails] PRIMARY KEY ([OverdraftDetailID]),
        CONSTRAINT [FK_OverdraftDetails_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OverdraftDetails_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [OverdraftMasters] (
        [OverdraftMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Overdraft] decimal(18,2) NOT NULL,
        [MaxOverdraft] decimal(18,2) NOT NULL,
        [WalletID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_OverdraftMasters] PRIMARY KEY ([OverdraftMasterID]),
        CONSTRAINT [FK_OverdraftMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OverdraftMasters_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PasswordChangeHistories] (
        [PasswordChangeHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Type] varchar(50) NULL,
        [IPAddress] varchar(150) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PasswordChangeHistories] PRIMARY KEY ([PasswordChangeHistoryID]),
        CONSTRAINT [FK_PasswordChangeHistories_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PaymentGateways] (
        [PaymentGatewayID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [OrderID] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [GatewayCharge] decimal(18,2) NOT NULL,
        [Commission] decimal(18,2) NOT NULL,
        [WalletID] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [TransactionID] nvarchar(max) NOT NULL,
        [PayMode] nvarchar(max) NOT NULL,
        [Response] nvarchar(max) NOT NULL,
        [PaymentProviderID] int NOT NULL,
        [IsUpdate] int NOT NULL,
        [OpeningBalance] decimal(18,2) NOT NULL,
        [ClosingBalance] decimal(18,2) NOT NULL,
        [Remark] nvarchar(max) NOT NULL,
        [DistributerUserID] int NOT NULL,
        [SuperDistributerUserID] int NOT NULL,
        [DistributerCommission] decimal(18,2) NOT NULL,
        [SuperDistributerCommission] decimal(18,2) NOT NULL,
        [RetailerTDS] decimal(18,2) NOT NULL,
        [DistributerTDS] decimal(18,2) NOT NULL,
        [SuperDistributerTDS] decimal(18,2) NOT NULL,
        [AdminGST] decimal(18,2) NOT NULL,
        [ReferenceID] nvarchar(max) NOT NULL,
        [ServiceID] int NOT NULL,
        [PaymentStatus] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PaymentGateways] PRIMARY KEY ([PaymentGatewayID]),
        CONSTRAINT [FK_PaymentGateways_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PaymentGateways_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PayoutBeneficiaryOTPs] (
        [PayoutBeneficiaryOTPID] int NOT NULL IDENTITY,
        [OTP] varchar(10) NOT NULL,
        [PayoutBeneficaryID] int NOT NULL,
        [UserID] int NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PayoutBeneficiaryOTPs] PRIMARY KEY ([PayoutBeneficiaryOTPID]),
        CONSTRAINT [FK_PayoutBeneficiaryOTPs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [PrepaidActivations] (
        [PrepaidActivationID] int NOT NULL IDENTITY,
        [OperatorID] int NOT NULL,
        [UserID] int NOT NULL,
        [ActivationType] nvarchar(max) NOT NULL,
        [AlternateNumber] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Pincode] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PrepaidActivations] PRIMARY KEY ([PrepaidActivationID]),
        CONSTRAINT [FK_PrepaidActivations_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderRequestResponseLogs] (
        [ProviderRequestResponseLogID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [TransactionID] int NOT NULL,
        [Request] nvarchar(max) NOT NULL,
        [Response] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [Through] nvarchar(max) NOT NULL,
        [ServiceID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderRequestResponseLogs] PRIMARY KEY ([ProviderRequestResponseLogID]),
        CONSTRAINT [FK_ProviderRequestResponseLogs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ProviderUserMappings] (
        [ProviderUserMappingID] int NOT NULL IDENTITY,
        [Key] varchar(max) NOT NULL,
        [Value] varchar(max) NOT NULL,
        [ProviderID] int NOT NULL,
        [UserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ProviderUserMappings] PRIMARY KEY ([ProviderUserMappingID]),
        CONSTRAINT [FK_ProviderUserMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Redeemcommissions] (
        [RedeemcommissionID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Month] int NOT NULL,
        [Year] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [AdminRemark] varchar(200) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [LastYearMonth] varchar(50) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Redeemcommissions] PRIMARY KEY ([RedeemcommissionID]),
        CONSTRAINT [FK_Redeemcommissions_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ReferralRegistrations] (
        [ReferralRegistrationID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [MobileNumber] nvarchar(max) NOT NULL,
        [ReferralCode] nvarchar(max) NOT NULL,
        [CreatedByUserID] int NOT NULL,
        [IsBenefitUtilize] bit NOT NULL,
        [ReferralAmount] decimal(18,2) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ReferralRegistrations] PRIMARY KEY ([ReferralRegistrationID]),
        CONSTRAINT [FK_ReferralRegistrations_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [RefreshTokenID] bigint NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Token] varchar(500) NOT NULL,
        [JwtId] varchar(500) NOT NULL,
        [ExpiryDateTime] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL,
        [IsRevoked] bit NOT NULL,
        [IPAddress] varchar(50) NOT NULL,
        [UserAgent] varchar(200) NOT NULL,
        [DeviceType] varchar(50) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([RefreshTokenID]),
        CONSTRAINT [FK_RefreshTokens_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SDKBBPSUserMappings] (
        [SDKBBPSUserMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ConfirmationCallbackURL] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SDKBBPSUserMappings] PRIMARY KEY ([SDKBBPSUserMappingID]),
        CONSTRAINT [FK_SDKBBPSUserMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Senders] (
        [SenderID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Mobile] nvarchar(max) NOT NULL,
        [GUID] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [UserID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [IsKYC] bit NOT NULL,
        [KYCDocumentURL] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [Pincode] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Senders] PRIMARY KEY ([SenderID]),
        CONSTRAINT [FK_Senders_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SupportUserAccessDetails] (
        [SupportUserAccessDetailID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Controller] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [MenuGUID] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SupportUserAccessDetails] PRIMARY KEY ([SupportUserAccessDetailID]),
        CONSTRAINT [FK_SupportUserAccessDetails_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TargetSaleCommissions] (
        [TargetSaleCommissionID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [ParentID] int NOT NULL,
        [SaleAmount] decimal(18,2) NOT NULL,
        [CommissionAmount] decimal(18,2) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [SaleMonthDOC] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TargetSaleCommissions] PRIMARY KEY ([TargetSaleCommissionID]),
        CONSTRAINT [FK_TargetSaleCommissions_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TargetSaleCommissions_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TicketMasters] (
        [TicketMasterID] int NOT NULL IDENTITY,
        [TicketCategoryID] int NOT NULL,
        [Subject] nvarchar(500) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [UploadFilePath] varchar(500) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [Resolution] varchar(max) NOT NULL,
        [ResolutionDate] datetime2 NOT NULL,
        [UserID] int NOT NULL,
        [ResolutionByUserID] int NOT NULL,
        [UserTyepID] int NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TicketMasters] PRIMARY KEY ([TicketMasterID]),
        CONSTRAINT [FK_TicketMasters_TicketCategories_TicketCategoryID] FOREIGN KEY ([TicketCategoryID]) REFERENCES [TicketCategories] ([TicketCategoryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TicketMasters_UserTypeMasters_UserTyepID] FOREIGN KEY ([UserTyepID]) REFERENCES [UserTypeMasters] ([UserTypeID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionMasters] (
        [TransactionMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [WalletID] int NOT NULL,
        [ConsumerNumber] varchar(50) NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [DebitAmount] decimal(18,5) NOT NULL,
        [OpeningBalance] decimal(18,5) NOT NULL,
        [ClosingBalance] decimal(18,5) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [Description] varchar(250) NOT NULL,
        [ProviderID] int NOT NULL,
        [ProviderOpeningBalance] decimal(18,5) NOT NULL,
        [ProviderCommission] decimal(18,5) NOT NULL,
        [ProviderClosingBalance] decimal(18,2) NOT NULL,
        [ProviderRequestID] varchar(500) NOT NULL,
        [ReferenceNo] varchar(1000) NOT NULL,
        [APIUserRequestID] varchar(1000) NOT NULL,
        [ProviderDOCNo] varchar(1000) NOT NULL,
        [ProviderRefNo] varchar(1000) NOT NULL,
        [AdminGST] decimal(18,5) NOT NULL,
        [RetailerTDS] decimal(18,5) NOT NULL,
        [DistributerTDS] decimal(18,5) NOT NULL,
        [SuperDistributerTDS] decimal(18,5) NOT NULL,
        [RetailerGST] decimal(18,5) NOT NULL,
        [DistributerGST] decimal(18,5) NOT NULL,
        [SuperDistributerGST] decimal(18,5) NOT NULL,
        [Bill_CustomerMessageMobileNo] varchar(10) NOT NULL,
        [RetailerCommission] decimal(18,5) NOT NULL,
        [DistributerCommission] decimal(18,5) NOT NULL,
        [SuperDistributerCommission] decimal(18,5) NOT NULL,
        [CashBack] decimal(18,5) NOT NULL,
        [HitBackCount] int NOT NULL,
        [ErrorCode] varchar(500) NOT NULL,
        [Bill_Optional1] varchar(1000) NOT NULL,
        [Bill_Optional2] varchar(1000) NOT NULL,
        [Bill_Optional3] varchar(1000) NOT NULL,
        [Bill_Optional4] varchar(1000) NOT NULL,
        [Bill_Optional5] varchar(1000) NOT NULL,
        [Bill_Optional6] varchar(1000) NOT NULL,
        [DMT_SenderID] varchar(100) NOT NULL,
        [DMT_SenderName] varchar(100) NOT NULL,
        [DMT_SenderNumber] varchar(100) NOT NULL,
        [DMT_BeneficiaryID] varchar(100) NOT NULL,
        [DMT_BeneficiaryName] varchar(1000) NOT NULL,
        [DMT_Bank] varchar(100) NOT NULL,
        [DMT_IFSC] varchar(100) NOT NULL,
        [DMT_BeneficaryNameByBank] varchar(1000) NOT NULL,
        [DMT_PaymentType] varchar(100) NOT NULL,
        [DMT_BillNumber] varchar(100) NOT NULL,
        [DMT_OriginalAmount] decimal(18,5) NOT NULL,
        [DMT_StatusCode] varchar(100) NOT NULL,
        [IsDispute] bit NOT NULL,
        [Location] varchar(500) NOT NULL,
        [SimCardDetails] varchar(500) NOT NULL,
        [Through] varchar(20) NOT NULL,
        [IPAddress] varchar(100) NOT NULL,
        [Response] varchar(max) NOT NULL,
        [Circle] varchar(100) NOT NULL,
        [DistributerUserID] int NOT NULL,
        [SuperDistributerUserID] int NOT NULL,
        [ComplaintStatus] varchar(20) NOT NULL,
        [AdminCommission] decimal(18,5) NOT NULL,
        [CardType] varchar(100) NOT NULL,
        [StateID] int NOT NULL,
        [IsROffer] bit NOT NULL,
        [IsOnlineDMT] bit NOT NULL,
        [IsProviderLowBalance] bit NOT NULL,
        [ChangeStatus] varchar(100) NOT NULL,
        [ChangeStatusDOC] datetime2 NOT NULL,
        [AEPSResponseJSON] varchar(max) NOT NULL,
        [PendingProviderStatus] varchar(50) NOT NULL,
        [AdminProfit] decimal(18,5) NOT NULL,
        [ResponseType] varchar(50) NOT NULL,
        [ROfferCommission] decimal(18,5) NOT NULL,
        [RetailerCommissionHold] decimal(18,5) NOT NULL,
        [DistributerCommissionHold] decimal(18,5) NOT NULL,
        [SuperDistributerCommissionHold] decimal(18,5) NOT NULL,
        [SettlementAtProviderEnd] bit NOT NULL,
        [BBPS_CustomerName] varchar(500) NOT NULL,
        [IsCCF] bit NOT NULL,
        [NetCommission] decimal(18,5) NOT NULL,
        [Charge] decimal(18,5) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TransactionMasters] PRIMARY KEY ([TransactionMasterID]),
        CONSTRAINT [FK_TransactionMasters_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionMasters_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TrustableDevices] (
        [ID] int NOT NULL IDENTITY,
        [IPAddress] nvarchar(max) NOT NULL,
        [UserID] int NOT NULL,
        [IsVerified] bit NOT NULL,
        [Browser] nvarchar(max) NOT NULL,
        [Device] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TrustableDevices] PRIMARY KEY ([ID]),
        CONSTRAINT [FK_TrustableDevices_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UpiSenders] (
        [UpiSenderID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Mobile] nvarchar(max) NOT NULL,
        [GUID] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [UserID] int NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [Pincode] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UpiSenders] PRIMARY KEY ([UpiSenderID]),
        CONSTRAINT [FK_UpiSenders_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserActionHistorys] (
        [UserHistoryID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserActionHistorys] PRIMARY KEY ([UserHistoryID]),
        CONSTRAINT [FK_UserActionHistorys_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserActionMasters] (
        [UserActionMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Platform] nvarchar(max) NOT NULL,
        [FingurePrint] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserActionMasters] PRIMARY KEY ([UserActionMasterID]),
        CONSTRAINT [FK_UserActionMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserBalanceCappings] (
        [UserBalanceCappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserBalanceCappings] PRIMARY KEY ([UserBalanceCappingID]),
        CONSTRAINT [FK_UserBalanceCappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserBalanceCappings_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserBalanceMaster] (
        [UserBalanceMasterID] int NOT NULL IDENTITY,
        [WalletID] int NOT NULL,
        [Balance] decimal(18,5) NOT NULL,
        [UserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserBalanceMaster] PRIMARY KEY ([UserBalanceMasterID]),
        CONSTRAINT [FK_UserBalanceMaster_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserBalanceMaster_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserBankDetails] (
        [UserBankDetailsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [BankName] varchar(500) NOT NULL,
        [AccountNumber] varchar(50) NOT NULL,
        [IFSC] varchar(50) NOT NULL,
        [IsOn] bit NOT NULL,
        [HolderName] varchar(200) NOT NULL,
        [Branch] varchar(100) NOT NULL,
        [QRCodeImageURL] nvarchar(max) NULL,
        [UPINumber] varchar(100) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserBankDetails] PRIMARY KEY ([UserBankDetailsID]),
        CONSTRAINT [FK_UserBankDetails_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserChannelSlabMappings] (
        [UserChannelSlabMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ChannelSlabMasterID] int NOT NULL,
        [UplineUserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserChannelSlabMappings] PRIMARY KEY ([UserChannelSlabMappingID]),
        CONSTRAINT [FK_UserChannelSlabMappings_ChannelSlabMasters_ChannelSlabMasterID] FOREIGN KEY ([ChannelSlabMasterID]) REFERENCES [ChannelSlabMasters] ([ChannelSlabMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserChannelSlabMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserCreationMasters] (
        [UserCreationMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [WalletID] int NOT NULL,
        [OpeningIdCount] int NOT NULL,
        [PurchaseCount] int NOT NULL,
        [ClosingIdCount] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        [OpeningBalance] decimal(18,5) NOT NULL,
        [ClosingBalance] decimal(18,5) NOT NULL,
        [Type] varchar(100) NULL,
        [CreatedUserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserCreationMasters] PRIMARY KEY ([UserCreationMasterID]),
        CONSTRAINT [FK_UserCreationMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserCreationMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserCreationMasters_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserCreationSettings] (
        [UserCreationSettingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Cost] decimal(18,5) NOT NULL,
        [MinimumCount] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserCreationSettings] PRIMARY KEY ([UserCreationSettingID]),
        CONSTRAINT [FK_UserCreationSettings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserCreationWallets] (
        [UserCreationWalletID] int NOT NULL IDENTITY,
        [IdCount] int NOT NULL,
        [UserID] int NOT NULL,
        CONSTRAINT [PK_UserCreationWallets] PRIMARY KEY ([UserCreationWalletID]),
        CONSTRAINT [FK_UserCreationWallets_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserDocumentMasters] (
        [UserDocumentID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [DocmentType] nvarchar(max) NOT NULL,
        [DocumentNo] nvarchar(max) NOT NULL,
        [ImageURL] nvarchar(max) NOT NULL,
        [Remark] varchar(100) NULL,
        [Status] varchar(20) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserDocumentMasters] PRIMARY KEY ([UserDocumentID]),
        CONSTRAINT [FK_UserDocumentMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserMerchantMasters] (
        [UserMerchantMasterID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [PaySprintUPIMerchantCode] nvarchar(max) NOT NULL,
        [PaySprintStaticQR] nvarchar(max) NOT NULL,
        [PaySprintUPIRefid] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserMerchantMasters] PRIMARY KEY ([UserMerchantMasterID]),
        CONSTRAINT [FK_UserMerchantMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserOperatorLimits] (
        [UserOperatorLimitID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [LimitAmount] decimal(18,5) NOT NULL,
        [IsActive] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserOperatorLimits] PRIMARY KEY ([UserOperatorLimitID]),
        CONSTRAINT [FK_UserOperatorLimits_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserOperatorLimits_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserOperatorLimits_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserServiceMappings] (
        [UserServiceMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_UserServiceMappings] PRIMARY KEY ([UserServiceMappingID]),
        CONSTRAINT [FK_UserServiceMappings_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserServiceMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserServiceSubscriptionDetailss] (
        [UserServiceSubscriptionDetailsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [ServiceAndUserwiseSubscriptionID] int NOT NULL,
        [Charge] decimal(18,2) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserServiceSubscriptionDetailss] PRIMARY KEY ([UserServiceSubscriptionDetailsID]),
        CONSTRAINT [FK_UserServiceSubscriptionDetailss_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserServiceSubscriptionDetailss_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserSlabMappings] (
        [UserSlabMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [SlabMasterID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserSlabMappings] PRIMARY KEY ([UserSlabMappingID]),
        CONSTRAINT [FK_UserSlabMappings_SlabMasters_SlabMasterID] FOREIGN KEY ([SlabMasterID]) REFERENCES [SlabMasters] ([SlabMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserSlabMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserWalletMappings] (
        [UserWalletMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        CONSTRAINT [PK_UserWalletMappings] PRIMARY KEY ([UserWalletMappingID]),
        CONSTRAINT [FK_UserWalletMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserWalletMappings_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserWalletMasters] (
        [UserWalletMasterID] int NOT NULL IDENTITY,
        [WalletID] int NOT NULL,
        [Balance] decimal(18,5) NOT NULL,
        [UserID] int NOT NULL,
        [BalanceHash] varchar(500) NULL,
        CONSTRAINT [PK_UserWalletMasters] PRIMARY KEY ([UserWalletMasterID]),
        CONSTRAINT [FK_UserWalletMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserWalletMasters_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserwiseDMTMappings] (
        [UserwiseDMTMappingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ProviderID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserwiseDMTMappings] PRIMARY KEY ([UserwiseDMTMappingID]),
        CONSTRAINT [FK_UserwiseDMTMappings_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseDMTMappings_ProviderMasters_ProviderID] FOREIGN KEY ([ProviderID]) REFERENCES [ProviderMasters] ([ProviderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseDMTMappings_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseDMTMappings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserwiseLowBalances] (
        [UserwiseLowBalanceID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [Amount] decimal(18,5) NOT NULL,
        CONSTRAINT [PK_UserwiseLowBalances] PRIMARY KEY ([UserwiseLowBalanceID]),
        CONSTRAINT [FK_UserwiseLowBalances_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseLowBalances_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserWisePayementGatewaySettings] (
        [UserWisePayementGatewaySettingID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [PaymentGatewaySettingID] int NOT NULL,
        [MinimumAmount] decimal(18,2) NOT NULL,
        [MaximumAmount] decimal(18,2) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserWisePayementGatewaySettings] PRIMARY KEY ([UserWisePayementGatewaySettingID]),
        CONSTRAINT [FK_UserWisePayementGatewaySettings_PaymentGatewaySettings_PaymentGatewaySettingID] FOREIGN KEY ([PaymentGatewaySettingID]) REFERENCES [PaymentGatewaySettings] ([PaymentGatewaySettingID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserWisePayementGatewaySettings_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserWisePlanLimits] (
        [UserWisePlanLimitID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [UserTypeID] int NOT NULL,
        [CircleHitCount] int NOT NULL,
        [PlanHitCount] int NOT NULL,
        [BillFetchHitCount] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserWisePlanLimits] PRIMARY KEY ([UserWisePlanLimitID]),
        CONSTRAINT [FK_UserWisePlanLimits_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserWisePlanViews] (
        [UserWisePlanViewID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [ConsumerNumber] nvarchar(max) NOT NULL,
        [ProviderID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserWisePlanViews] PRIMARY KEY ([UserWisePlanViewID]),
        CONSTRAINT [FK_UserWisePlanViews_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserwiseSaleTargets] (
        [UserwiseSaleTargetID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [WalletID] int NOT NULL,
        [ParentID] int NOT NULL,
        [TargetAmount] decimal(18,2) NOT NULL,
        [Commission] decimal(18,2) NOT NULL,
        [CommissionType] nvarchar(max) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserwiseSaleTargets] PRIMARY KEY ([UserwiseSaleTargetID]),
        CONSTRAINT [FK_UserwiseSaleTargets_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseSaleTargets_WalletMaster_WalletID] FOREIGN KEY ([WalletID]) REFERENCES [WalletMaster] ([WalletID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UserwiseServiceChargeMasters] (
        [UserwiseServiceChargeMasterID] int NOT NULL IDENTITY,
        [ServiceID] int NOT NULL,
        [UserID] int NOT NULL,
        [OneTime] decimal(18,2) NOT NULL,
        [IsDelete] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserwiseServiceChargeMasters] PRIMARY KEY ([UserwiseServiceChargeMasterID]),
        CONSTRAINT [FK_UserwiseServiceChargeMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserwiseServiceChargeMasters_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [VoucherDetails] (
        [VoucherDetailsID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [Mode] varchar(80) NULL,
        [OutstandingOpeningBal] decimal(18,2) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [OutstandingClossingBal] decimal(18,2) NOT NULL,
        [Remark] varchar(50) NULL,
        [Credit] varchar(50) NULL,
        [Debit] varchar(50) NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_VoucherDetails] PRIMARY KEY ([VoucherDetailsID]),
        CONSTRAINT [FK_VoucherDetails_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [DMTBeneficiaries] (
        [BeneficiaryID] int NOT NULL IDENTITY,
        [Name] varchar(200) NOT NULL,
        [BankName] varchar(50) NOT NULL,
        [AccountNo] varchar(30) NOT NULL,
        [IFSC] varchar(20) NOT NULL,
        [GUID] varchar(50) NOT NULL,
        [SenderMobile] varchar(15) NOT NULL,
        [IPAddress] varchar(500) NOT NULL,
        [SenderID] int NOT NULL,
        [UserID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [IsNameVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DMTBeneficiaries] PRIMARY KEY ([BeneficiaryID]),
        CONSTRAINT [FK_DMTBeneficiaries_DMTSenders_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [DMTSenders] ([SenderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DMTBeneficiaries_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [Beneficiarys] (
        [BeneficiaryID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [BankName] nvarchar(max) NOT NULL,
        [AccountNo] nvarchar(max) NOT NULL,
        [IFSC] nvarchar(max) NOT NULL,
        [GUID] nvarchar(max) NOT NULL,
        [SenderMobile] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [SenderID] int NOT NULL,
        [UserID] int NOT NULL,
        [IsActive] bit NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [IsNameVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Beneficiarys] PRIMARY KEY ([BeneficiaryID]),
        CONSTRAINT [FK_Beneficiarys_Senders_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [Senders] ([SenderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Beneficiarys_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SenderOTPs] (
        [SenderOTPID] int NOT NULL IDENTITY,
        [Mobile] nvarchar(max) NOT NULL,
        [OTP] nvarchar(max) NOT NULL,
        [SenderID] int NOT NULL,
        [UserID] int NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SenderOTPs] PRIMARY KEY ([SenderOTPID]),
        CONSTRAINT [FK_SenderOTPs_Senders_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [Senders] ([SenderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SenderOTPs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [SmsTransactionMasters] (
        [SmsTransactionMasterID] int NOT NULL IDENTITY,
        [TransactionMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [BillNumber] nvarchar(max) NOT NULL,
        [IsProcess] bit NOT NULL,
        [IsSuccess] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SmsTransactionMasters] PRIMARY KEY ([SmsTransactionMasterID]),
        CONSTRAINT [FK_SmsTransactionMasters_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SmsTransactionMasters_TransactionMasters_TransactionMasterID] FOREIGN KEY ([TransactionMasterID]) REFERENCES [TransactionMasters] ([TransactionMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [ThreewayReconCompletions] (
        [ThreewayReconCompletionID] int NOT NULL IDENTITY,
        [TransactionMasterID] int NOT NULL,
        [ServiceID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ThreewayReconCompletions] PRIMARY KEY ([ThreewayReconCompletionID]),
        CONSTRAINT [FK_ThreewayReconCompletions_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThreewayReconCompletions_ServiceMasters_ServiceID] FOREIGN KEY ([ServiceID]) REFERENCES [ServiceMasters] ([ServiceID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ThreewayReconCompletions_TransactionMasters_TransactionMasterID] FOREIGN KEY ([TransactionMasterID]) REFERENCES [TransactionMasters] ([TransactionMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionManualUpdates] (
        [TransactionManualUpdateID] int NOT NULL IDENTITY,
        [TransactionMasterID] int NOT NULL,
        [ManualUpdatedBy] int NOT NULL,
        [ProviderrefNo] nvarchar(max) NOT NULL,
        [OldStatus] nvarchar(max) NOT NULL,
        [NewStatus] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TransactionManualUpdates] PRIMARY KEY ([TransactionManualUpdateID]),
        CONSTRAINT [FK_TransactionManualUpdates_TransactionMasters_TransactionMasterID] FOREIGN KEY ([TransactionMasterID]) REFERENCES [TransactionMasters] ([TransactionMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionManualUpdates_UserMasters_ManualUpdatedBy] FOREIGN KEY ([ManualUpdatedBy]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionProviderHistorys] (
        [RechargeProviderHistoryID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [ProviderRequestID] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TransactionProviderHistorys] PRIMARY KEY ([RechargeProviderHistoryID]),
        CONSTRAINT [FK_TransactionProviderHistorys_TransactionMasters_TransactionMasterID] FOREIGN KEY ([TransactionMasterID]) REFERENCES [TransactionMasters] ([TransactionMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [TransactionStatusChanges] (
        [TransactionStatusChangeID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [TransactionMasterID] int NOT NULL,
        [TransactionDOCInt] int NOT NULL,
        [OldStatus] nvarchar(max) NOT NULL,
        [NewStatus] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Remark] nvarchar(max) NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_TransactionStatusChanges] PRIMARY KEY ([TransactionStatusChangeID]),
        CONSTRAINT [FK_TransactionStatusChanges_TransactionMasters_TransactionMasterID] FOREIGN KEY ([TransactionMasterID]) REFERENCES [TransactionMasters] ([TransactionMasterID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionStatusChanges_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UpiBeneficiarys] (
        [UpiBeneficiaryID] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [UpiID] nvarchar(max) NOT NULL,
        [GUID] nvarchar(max) NOT NULL,
        [SenderMobile] nvarchar(max) NOT NULL,
        [IPAddress] nvarchar(max) NOT NULL,
        [UpiSenderID] int NOT NULL,
        [UserID] int NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UpiBeneficiarys] PRIMARY KEY ([UpiBeneficiaryID]),
        CONSTRAINT [FK_UpiBeneficiarys_UpiSenders_UpiSenderID] FOREIGN KEY ([UpiSenderID]) REFERENCES [UpiSenders] ([UpiSenderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UpiBeneficiarys_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [BeneficiaryOTPs] (
        [BeneficiaryOTPID] int NOT NULL IDENTITY,
        [SenderMobile] nvarchar(max) NOT NULL,
        [OTP] nvarchar(max) NOT NULL,
        [SenderID] int NOT NULL,
        [BeneficiaryID] int NOT NULL,
        [UserID] int NOT NULL,
        [IsOTPVerified] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_BeneficiaryOTPs] PRIMARY KEY ([BeneficiaryOTPID]),
        CONSTRAINT [FK_BeneficiaryOTPs_Beneficiarys_BeneficiaryID] FOREIGN KEY ([BeneficiaryID]) REFERENCES [Beneficiarys] ([BeneficiaryID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BeneficiaryOTPs_Senders_SenderID] FOREIGN KEY ([SenderID]) REFERENCES [Senders] ([SenderID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BeneficiaryOTPs_UserMasters_UserID] FOREIGN KEY ([UserID]) REFERENCES [UserMasters] ([UserMasterID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE TABLE [UPIVendorBeneficiaryMappings] (
        [UPIVendorBeneficiaryMappingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [UPIVendorBeneficiaryID] nvarchar(max) NOT NULL,
        [UpiBeneficiaryID] int NOT NULL,
        CONSTRAINT [PK_UPIVendorBeneficiaryMappings] PRIMARY KEY ([UPIVendorBeneficiaryMappingID]),
        CONSTRAINT [FK_UPIVendorBeneficiaryMappings_UpiBeneficiarys_UpiBeneficiaryID] FOREIGN KEY ([UpiBeneficiaryID]) REFERENCES [UpiBeneficiarys] ([UpiBeneficiaryID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_APIUserCallbackRequestResponseLogs_UserID] ON [APIUserCallbackRequestResponseLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_APIUserInitialRequestResponseLogs_UserID] ON [APIUserInitialRequestResponseLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserID] ON [AuditLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_AuthorizationMasters_UserTypeID] ON [AuthorizationMasters] ([UserTypeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_AutoTransferUserwiseMappings_UserID] ON [AutoTransferUserwiseMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_BankRequestResponseLogs_UserID] ON [BankRequestResponseLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_BBPSConfigurations_OperatorID] ON [BBPSConfigurations] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_BeneficiaryOTPs_BeneficiaryID] ON [BeneficiaryOTPs] ([BeneficiaryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_BeneficiaryOTPs_SenderID] ON [BeneficiaryOTPs] ([SenderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_BeneficiaryOTPs_UserID] ON [BeneficiaryOTPs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Beneficiarys_SenderID] ON [Beneficiarys] ([SenderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Beneficiarys_UserID] ON [Beneficiarys] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CallbackMasters_ServiceID] ON [CallbackMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CallbackMasters_UserID] ON [CallbackMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CallMeRequests_CallMeCategoryMasterID] ON [CallMeRequests] ([CallMeCategoryMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CallMeRequests_UserID] ON [CallMeRequests] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CashCollectionMasters_UserID] ON [CashCollectionMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CategoryChannelMappings_ChannelCategoryID] ON [CategoryChannelMappings] ([ChannelCategoryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ChannelSlabDetails_ChannelSlabMasterID] ON [ChannelSlabDetails] ([ChannelSlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ChannelSlabDetails_OperatorID] ON [ChannelSlabDetails] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ChannelSlabDetails_ServiceID] ON [ChannelSlabDetails] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseCommissions_OperatorID] ON [CircleWiseCommissions] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseCommissions_SlabMasterID] ON [CircleWiseCommissions] ([SlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseOperatorLimits_OperatorID] ON [CircleWiseOperatorLimits] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseOperatorLimits_UserID] ON [CircleWiseOperatorLimits] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseUserWiseOperatorOffs_OperatorID] ON [CircleWiseUserWiseOperatorOffs] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_CircleWiseUserWiseOperatorOffs_UserID] ON [CircleWiseUserWiseOperatorOffs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ComplaintMasters_OperatorID] ON [ComplaintMasters] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ComplaintMasters_ServiceID] ON [ComplaintMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ComplaintMasters_UserID] ON [ComplaintMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DailyAuthenticationMasters_ProviderID] ON [DailyAuthenticationMasters] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DailyAuthenticationMasters_ServiceID] ON [DailyAuthenticationMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DailyAuthenticationMasters_UserID] ON [DailyAuthenticationMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DMTBeneficiaries_SenderID_AccountNo] ON [DMTBeneficiaries] ([SenderID], [AccountNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DMTBeneficiaries_UserID] ON [DMTBeneficiaries] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DMTSenders_Mobile] ON [DMTSenders] ([Mobile]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_DMTSenders_UserID] ON [DMTSenders] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_EcollectionDetails_UserID] ON [EcollectionDetails] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_eKYCLogs_UserID] ON [eKYCLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_eKYCMasters_UserID] ON [eKYCMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_ReceiverID] ON [FundTransactions] ([ReceiverID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_RequestToID] ON [FundTransactions] ([RequestToID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_SenderID] ON [FundTransactions] ([SenderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_WalletID] ON [FundTransactions] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_GSTHolds_StateID] ON [GSTHolds] ([StateID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_GSTHolds_UserID] ON [GSTHolds] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Inboxes_OperatorID] ON [Inboxes] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Inboxes_ServiceID] ON [Inboxes] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Inboxes_UserID] ON [Inboxes] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_InvoiceNumberHistories_UserID] ON [InvoiceNumberHistories] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_IPMasters_UserID] ON [IPMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_LapuMasters_LapuPartyMasterID] ON [LapuMasters] ([LapuPartyMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_LapuMasters_OperatorID] ON [LapuMasters] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_LoginHistories_UserID_Date] ON [LoginHistories] ([UserID], [Date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_LowBalanceConfigurations_UserID] ON [LowBalanceConfigurations] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_LowBalanceConfigurations_WalletID] ON [LowBalanceConfigurations] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_NewsMasters_UserID] ON [NewsMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OnboardingUserDocuments_OnboardingUserID] ON [OnboardingUserDocuments] ([OnboardingUserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperationalHistorys_UserID] ON [OperationalHistorys] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorMasters_ServiceID] ON [OperatorMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequences_OperatorID] ON [OperatorSequences] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequences_ProviderID] ON [OperatorSequences] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequences_ServiceID] ON [OperatorSequences] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequenceUserWises_OperatorID] ON [OperatorSequenceUserWises] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequenceUserWises_ProviderID] ON [OperatorSequenceUserWises] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequenceUserWises_ServiceID] ON [OperatorSequenceUserWises] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorSequenceUserWises_UserID] ON [OperatorSequenceUserWises] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorStateMappings_OperatorID] ON [OperatorStateMappings] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorUserwises_OperatorID] ON [OperatorUserwises] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorUserwises_ServiceID] ON [OperatorUserwises] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OperatorUserwises_UserID] ON [OperatorUserwises] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OverdraftDetails_UserID] ON [OverdraftDetails] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OverdraftDetails_WalletID] ON [OverdraftDetails] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OverdraftMasters_UserID] ON [OverdraftMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_OverdraftMasters_WalletID] ON [OverdraftMasters] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PasswordChangeHistories_UserID] ON [PasswordChangeHistories] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PaymentGateways_ServiceID] ON [PaymentGateways] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PaymentGateways_UserID] ON [PaymentGateways] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PayoutBeneficiaryOTPs_UserID] ON [PayoutBeneficiaryOTPs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PayoutVendorBeneficiaryMappings_PayoutBeneficaryID] ON [PayoutVendorBeneficiaryMappings] ([PayoutBeneficaryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_PrepaidActivations_UserID] ON [PrepaidActivations] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurationAdminCommissions_OperatorID] ON [ProviderConfigurationAdminCommissions] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurationAdminCommissions_ProviderID] ON [ProviderConfigurationAdminCommissions] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurationAdminCommissions_ServiceID] ON [ProviderConfigurationAdminCommissions] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurations_OperatorID] ON [ProviderConfigurations] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurations_ProviderID] ON [ProviderConfigurations] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderConfigurations_ServiceID] ON [ProviderConfigurations] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderDetails_ProviderID] ON [ProviderDetails] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderDetails_ServiceID] ON [ProviderDetails] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderMasters_ProviderName] ON [ProviderMasters] ([ProviderName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderOperatorCircleMasters_OperatorID] ON [ProviderOperatorCircleMasters] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderOperatorCircleMasters_ProviderID] ON [ProviderOperatorCircleMasters] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderOperatorDenominationMappings_DenominationMasterID] ON [ProviderOperatorDenominationMappings] ([DenominationMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderOperatorDenominationMappings_OperatorID] ON [ProviderOperatorDenominationMappings] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderOperatorDenominationMappings_ProviderID] ON [ProviderOperatorDenominationMappings] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderPurchases_ProviderID] ON [ProviderPurchases] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderPurchases_WalletID] ON [ProviderPurchases] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderRequestResponseLogs_UserID] ON [ProviderRequestResponseLogs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderResponseMasters_ProviderID] ON [ProviderResponseMasters] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderStateConfigurations_ProviderID] ON [ProviderStateConfigurations] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ProviderUserMappings_UserID] ON [ProviderUserMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RangeWiseSequences_OperatorID] ON [RangeWiseSequences] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RangeWiseSequences_ProviderID] ON [RangeWiseSequences] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RangeWiseSequences_ServiceID] ON [RangeWiseSequences] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RealProviderMappings_OperatorID] ON [RealProviderMappings] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ReconciliationSettings_ProviderID] ON [ReconciliationSettings] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Redeemcommissions_UserID] ON [Redeemcommissions] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ReferralRegistrations_UserID] ON [ReferralRegistrations] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_JwtId] ON [RefreshTokens] ([JwtId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserID_IsRevoked_IsUsed] ON [RefreshTokens] ([UserID], [IsRevoked], [IsUsed]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ROfferCommissions_OperatorID] ON [ROfferCommissions] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ROfferCommissions_ServiceID] ON [ROfferCommissions] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ROfferCommissions_SlabMasterID] ON [ROfferCommissions] ([SlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SDKBBPSUserMappings_UserID] ON [SDKBBPSUserMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SenderOTPs_SenderID] ON [SenderOTPs] ([SenderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SenderOTPs_UserID] ON [SenderOTPs] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_Senders_UserID] ON [Senders] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ServiceSubscriptionPlanDetailss_ServiceSubscriptionCreatePlanMasterID] ON [ServiceSubscriptionPlanDetailss] ([ServiceSubscriptionCreatePlanMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ServiceWiseSliderSettings_ServiceID] ON [ServiceWiseSliderSettings] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SlabDetails_OperatorID] ON [SlabDetails] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SlabDetails_ServiceID] ON [SlabDetails] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SlabDetails_SlabMasterID] ON [SlabDetails] ([SlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SmsTransactionMasters_ServiceID] ON [SmsTransactionMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SmsTransactionMasters_TransactionMasterID] ON [SmsTransactionMasters] ([TransactionMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SubCategorys_CategoryID] ON [SubCategorys] ([CategoryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_SupportUserAccessDetails_UserID] ON [SupportUserAccessDetails] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TargetSaleCommissions_UserID] ON [TargetSaleCommissions] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TargetSaleCommissions_WalletID] ON [TargetSaleCommissions] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ThreewayReconCompletions_OperatorID] ON [ThreewayReconCompletions] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ThreewayReconCompletions_ServiceID] ON [ThreewayReconCompletions] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_ThreewayReconCompletions_TransactionMasterID] ON [ThreewayReconCompletions] ([TransactionMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TicketMasters_TicketCategoryID] ON [TicketMasters] ([TicketCategoryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TicketMasters_UserID] ON [TicketMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TicketMasters_UserTyepID] ON [TicketMasters] ([UserTyepID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionManualUpdates_ManualUpdatedBy] ON [TransactionManualUpdates] ([ManualUpdatedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionManualUpdates_TransactionMasterID] ON [TransactionManualUpdates] ([TransactionMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_OperatorID] ON [TransactionMasters] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_ProviderRequestID] ON [TransactionMasters] ([ProviderRequestID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_ServiceID] ON [TransactionMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_Status] ON [TransactionMasters] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_UserID] ON [TransactionMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionMasters_WalletID] ON [TransactionMasters] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionProviderHistorys_TransactionMasterID] ON [TransactionProviderHistorys] ([TransactionMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionStatusChanges_TransactionMasterID] ON [TransactionStatusChanges] ([TransactionMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TransactionStatusChanges_UserID] ON [TransactionStatusChanges] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_TrustableDevices_UserID] ON [TrustableDevices] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UpiBeneficiarys_UpiSenderID] ON [UpiBeneficiarys] ([UpiSenderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UpiBeneficiarys_UserID] ON [UpiBeneficiarys] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UpiSenders_UserID] ON [UpiSenders] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UPIVendorBeneficiaryMappings_UpiBeneficiaryID] ON [UPIVendorBeneficiaryMappings] ([UpiBeneficiaryID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserActionHistorys_UserID] ON [UserActionHistorys] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserActionMasters_UserID] ON [UserActionMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserBalanceCappings_UserID] ON [UserBalanceCappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserBalanceCappings_WalletID] ON [UserBalanceCappings] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserBalanceMaster_UserID] ON [UserBalanceMaster] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserBalanceMaster_WalletID] ON [UserBalanceMaster] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserBankDetails_UserID] ON [UserBankDetails] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserChannelSlabMappings_ChannelSlabMasterID] ON [UserChannelSlabMappings] ([ChannelSlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserChannelSlabMappings_UserID] ON [UserChannelSlabMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserCreationMasters_ServiceID] ON [UserCreationMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserCreationMasters_UserID] ON [UserCreationMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserCreationMasters_WalletID] ON [UserCreationMasters] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserCreationSettings_UserID] ON [UserCreationSettings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserCreationWallets_UserID] ON [UserCreationWallets] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserDocumentMasters_UserID] ON [UserDocumentMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserMasters_MobileNumber] ON [UserMasters] ([MobileNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserMasters_StateID] ON [UserMasters] ([StateID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserMasters_UserTypeID] ON [UserMasters] ([UserTypeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserMerchantMasters_UserID] ON [UserMerchantMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserOperatorLimits_OperatorID] ON [UserOperatorLimits] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserOperatorLimits_ServiceID] ON [UserOperatorLimits] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserOperatorLimits_UserID] ON [UserOperatorLimits] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserServiceMappings_ServiceID] ON [UserServiceMappings] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserServiceMappings_UserID] ON [UserServiceMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserServiceSubscriptionDetailss_ServiceID] ON [UserServiceSubscriptionDetailss] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserServiceSubscriptionDetailss_UserID] ON [UserServiceSubscriptionDetailss] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserSlabMappings_SlabMasterID] ON [UserSlabMappings] ([SlabMasterID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserSlabMappings_UserID] ON [UserSlabMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWalletMappings_UserID] ON [UserWalletMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWalletMappings_WalletID] ON [UserWalletMappings] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWalletMasters_UserID] ON [UserWalletMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWalletMasters_WalletID] ON [UserWalletMasters] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseDMTMappings_OperatorID] ON [UserwiseDMTMappings] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseDMTMappings_ProviderID] ON [UserwiseDMTMappings] ([ProviderID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseDMTMappings_ServiceID] ON [UserwiseDMTMappings] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseDMTMappings_UserID] ON [UserwiseDMTMappings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseLowBalances_UserID] ON [UserwiseLowBalances] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseLowBalances_WalletID] ON [UserwiseLowBalances] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWisePayementGatewaySettings_PaymentGatewaySettingID] ON [UserWisePayementGatewaySettings] ([PaymentGatewaySettingID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWisePayementGatewaySettings_UserID] ON [UserWisePayementGatewaySettings] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWisePlanLimits_UserID] ON [UserWisePlanLimits] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserWisePlanViews_UserID] ON [UserWisePlanViews] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseSaleTargets_UserID] ON [UserwiseSaleTargets] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseSaleTargets_WalletID] ON [UserwiseSaleTargets] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseServiceChargeMasters_ServiceID] ON [UserwiseServiceChargeMasters] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_UserwiseServiceChargeMasters_UserID] ON [UserwiseServiceChargeMasters] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_VoucherDetails_UserID] ON [VoucherDetails] ([UserID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_VoucherMasters_VoucherTypeID] ON [VoucherMasters] ([VoucherTypeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_WalletServiceMappings_ServiceID] ON [WalletServiceMappings] ([ServiceID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_WalletServiceMappings_WalletID] ON [WalletServiceMappings] ([WalletID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    CREATE INDEX [IX_WrongPasswordLogs_UserID_DateTime] ON [WrongPasswordLogs] ([UserID], [DateTime]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260623055656_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260623055656_Inicial', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704050401_SecondMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704050401_SecondMigration', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704103639_AddWebsitePageMaster'
)
BEGIN
    CREATE TABLE [WebsitePageMasters] (
        [WebsitePageMasterID] int NOT NULL IDENTITY,
        [Slug] varchar(100) NOT NULL,
        [Title] varchar(200) NOT NULL,
        [MetaDescription] varchar(500) NOT NULL,
        [SectionsJson] varchar(max) NOT NULL,
        [MenuOrder] int NOT NULL,
        [ShowInMenu] bit NOT NULL,
        [IsPublished] bit NOT NULL,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_WebsitePageMasters] PRIMARY KEY ([WebsitePageMasterID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704103639_AddWebsitePageMaster'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704103639_AddWebsitePageMaster', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704105216_WebsitePageUnicode'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WebsitePageMasters]') AND [c].[name] = N'Title');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [WebsitePageMasters] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [WebsitePageMasters] ALTER COLUMN [Title] nvarchar(200) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704105216_WebsitePageUnicode'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WebsitePageMasters]') AND [c].[name] = N'SectionsJson');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [WebsitePageMasters] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [WebsitePageMasters] ALTER COLUMN [SectionsJson] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704105216_WebsitePageUnicode'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WebsitePageMasters]') AND [c].[name] = N'MetaDescription');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [WebsitePageMasters] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [WebsitePageMasters] ALTER COLUMN [MetaDescription] nvarchar(500) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704105216_WebsitePageUnicode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704105216_WebsitePageUnicode', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704165044_AddWalletDisableServiceGSTVirtualBalanceUser'
)
BEGIN
    ALTER TABLE [VirtualBalances] ADD [UserID] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704165044_AddWalletDisableServiceGSTVirtualBalanceUser'
)
BEGIN
    ALTER TABLE [ServiceMasters] ADD [IsGSTInclusive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260704165044_AddWalletDisableServiceGSTVirtualBalanceUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260704165044_AddWalletDisableServiceGSTVirtualBalanceUser', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705152556_AddViewProviderMapping'
)
BEGIN
    CREATE TABLE [ViewProviderMappings] (
        [ViewProviderMappingID] int NOT NULL IDENTITY,
        [ProviderID] int NOT NULL,
        [OperatorID] int NOT NULL,
        CONSTRAINT [PK_ViewProviderMappings] PRIMARY KEY ([ViewProviderMappingID]),
        CONSTRAINT [FK_ViewProviderMappings_OperatorMasters_OperatorID] FOREIGN KEY ([OperatorID]) REFERENCES [OperatorMasters] ([OperatorID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705152556_AddViewProviderMapping'
)
BEGIN
    CREATE INDEX [IX_ViewProviderMappings_OperatorID] ON [ViewProviderMappings] ([OperatorID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705152556_AddViewProviderMapping'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705152556_AddViewProviderMapping', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705181039_ProviderLogUserOptional'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProviderRequestResponseLogs]') AND [c].[name] = N'UserID');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ProviderRequestResponseLogs] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [ProviderRequestResponseLogs] ALTER COLUMN [UserID] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705181039_ProviderLogUserOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705181039_ProviderLogUserOptional', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706193643_ServiceIconAndSequence'
)
BEGIN
    ALTER TABLE [ServiceMasters] ADD [ServiceImagePath] varchar(500) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706193643_ServiceIconAndSequence'
)
BEGIN
    ALTER TABLE [ServiceMasters] ADD [UISequence] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260706193643_ServiceIconAndSequence'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706193643_ServiceIconAndSequence', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707123028_ServiceReportKind'
)
BEGIN
    ALTER TABLE [ServiceMasters] ADD [ReportKind] varchar(50) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707123028_ServiceReportKind'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707123028_ServiceReportKind', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    ALTER TABLE [FundTransactions] ADD [ModeID] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    ALTER TABLE [FundTransactions] ADD [SubModeID] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    CREATE TABLE [Modes] (
        [ModeID] int NOT NULL IDENTITY,
        [ModeName] varchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [UpdatedBy] int NOT NULL,
        CONSTRAINT [PK_Modes] PRIMARY KEY ([ModeID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    CREATE TABLE [SubModes] (
        [SubModeID] int NOT NULL IDENTITY,
        [ModeID] int NOT NULL,
        [SubModeName] varchar(150) NOT NULL,
        [Balance] decimal(18,5) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDelete] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedDate] datetime2 NULL,
        [UpdatedBy] int NOT NULL,
        CONSTRAINT [PK_SubModes] PRIMARY KEY ([SubModeID]),
        CONSTRAINT [FK_SubModes_Modes_ModeID] FOREIGN KEY ([ModeID]) REFERENCES [Modes] ([ModeID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_ModeID] ON [FundTransactions] ([ModeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    CREATE INDEX [IX_FundTransactions_SubModeID] ON [FundTransactions] ([SubModeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    CREATE INDEX [IX_SubModes_ModeID] ON [SubModes] ([ModeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    ALTER TABLE [FundTransactions] ADD CONSTRAINT [FK_FundTransactions_Modes_ModeID] FOREIGN KEY ([ModeID]) REFERENCES [Modes] ([ModeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    ALTER TABLE [FundTransactions] ADD CONSTRAINT [FK_FundTransactions_SubModes_SubModeID] FOREIGN KEY ([SubModeID]) REFERENCES [SubModes] ([SubModeID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707184603_AddFundModeSubMode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707184603_AddFundModeSubMode', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [AccountNumber] varchar(50) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [BankName] varchar(150) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [BeneficiaryName] varchar(150) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [IFSC] varchar(20) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [QRCode] varchar(200) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    ALTER TABLE [SubModes] ADD [QRImage] varchar(500) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260707193707_AddSubModeDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260707193707_AddSubModeDetails', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710052521_OperatorDenominationFlag'
)
BEGIN
    ALTER TABLE [OperatorMasters] ADD [IsDenominationApplicable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260710052521_OperatorDenominationFlag'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260710052521_OperatorDenominationFlag', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716164426_AddVirtualBalanceDateTime'
)
BEGIN
    ALTER TABLE [VirtualBalances] ADD [DateTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716164426_AddVirtualBalanceDateTime'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716164426_AddVirtualBalanceDateTime', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717195718_AddWalletHoldBalance'
)
BEGIN
    ALTER TABLE [UserWalletMasters] ADD [HoldBalance] decimal(18,5) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717195718_AddWalletHoldBalance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717195718_AddWalletHoldBalance', N'9.0.10');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730045218_AddApiUserIpSecurity'
)
BEGIN
    ALTER TABLE [UserMasters] ADD [IsIPSecure] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730045218_AddApiUserIpSecurity'
)
BEGIN
    ALTER TABLE [UserMasters] ADD [WhitelistedIPs] varchar(1000) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260730045218_AddApiUserIpSecurity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260730045218_AddApiUserIpSecurity', N'9.0.10');
END;

COMMIT;
GO

