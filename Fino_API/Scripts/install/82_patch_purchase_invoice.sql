/*
    Purchase Invoice: the items a supplier bills, and quantity x rate on a line.

    Capturing a supplier bill used to mean typing the description, the SAC and every tax head by
    hand, with the provider and the wallet entered as bare numbers. Three things change that:

      1. PurchaseItemMasters - the handful of things a telecom / fintech supplier actually bills
         (E-Topup, Domestic Money Transfer, Bill Payment ...), each with its own SAC, GST rate and
         usual rate. Picking one fills the line; the list is edited from the Purchase Invoice
         screen, so a supplier who bills something new needs no release.

      2. TaxInvoiceLines.UnitQuantity / UnitRate / RateType - the "how much, at what rate" a bill
         is written in. Quantity (int) stays what it was: the number of source transactions on a
         SALE line, which GSTR-1 table 12 sums. A purchase quantity is a rupee value with paise,
         so it gets its own column rather than a type change under the returns.

      3. Two Key Settings for the GST portal sign-in name, kept beside the other keys. Nothing
         reads the password: the portal cannot be signed in to by a program (captcha + OTP), and a
         GSP connection uses the user name and an OTP, never the password.

    Idempotent: safe to run twice.
*/

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.PurchaseItemMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseItemMasters
    (
        PurchaseItemMasterID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseItemMasters PRIMARY KEY,
        Name            varchar(150)  NOT NULL,
        SACCode         varchar(10)   NOT NULL CONSTRAINT DF_PurchaseItemMasters_SAC DEFAULT (''),
        GSTRatePercent  decimal(9,4)  NOT NULL CONSTRAINT DF_PurchaseItemMasters_GST DEFAULT (18),
        DefaultRate     decimal(18,6) NOT NULL CONSTRAINT DF_PurchaseItemMasters_Rate DEFAULT (0),
        RateType        varchar(10)   NOT NULL CONSTRAINT DF_PurchaseItemMasters_RateType DEFAULT ('PERCENT'),
        ITCEligible     bit           NOT NULL CONSTRAINT DF_PurchaseItemMasters_ITC DEFAULT (1),
        UISequence      int           NOT NULL CONSTRAINT DF_PurchaseItemMasters_Seq DEFAULT (0),
        IsActive        bit           NOT NULL CONSTRAINT DF_PurchaseItemMasters_Active DEFAULT (1),
        [DateTime]      datetime2     NOT NULL,
        UpdatedateTime  datetime2     NOT NULL,
        IsDeleted       bit           NOT NULL
    );
    PRINT 'Created table PurchaseItemMasters.';
END
ELSE
    PRINT 'Table PurchaseItemMasters already present - left alone.';
GO

IF COL_LENGTH(N'dbo.TaxInvoiceLines', N'UnitQuantity') IS NULL
BEGIN
    ALTER TABLE dbo.TaxInvoiceLines
        ADD UnitQuantity decimal(18,3) NOT NULL CONSTRAINT DF_TaxInvoiceLines_UnitQuantity DEFAULT (0);
    PRINT 'Added TaxInvoiceLines.UnitQuantity.';
END
GO

IF COL_LENGTH(N'dbo.TaxInvoiceLines', N'UnitRate') IS NULL
BEGIN
    ALTER TABLE dbo.TaxInvoiceLines
        ADD UnitRate decimal(18,6) NOT NULL CONSTRAINT DF_TaxInvoiceLines_UnitRate DEFAULT (0);
    PRINT 'Added TaxInvoiceLines.UnitRate.';
END
GO

IF COL_LENGTH(N'dbo.TaxInvoiceLines', N'RateType') IS NULL
BEGIN
    ALTER TABLE dbo.TaxInvoiceLines
        ADD RateType varchar(10) NOT NULL CONSTRAINT DF_TaxInvoiceLines_RateType DEFAULT ('');
    PRINT 'Added TaxInvoiceLines.RateType.';
END
GO

/*  The starting list. Seeded only into an EMPTY table, so an install that has edited or removed
    an item never has it put back. The SACs are the ones this product already stamps on its own
    documents (998413 recharge, 997112 money transfer, 997159 other financial services); the
    rate is left at 0 because it is the supplier's and differs for every one of them.            */
IF OBJECT_ID(N'dbo.PurchaseItemMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.PurchaseItemMasters)
BEGIN
    INSERT INTO dbo.PurchaseItemMasters
        (Name, SACCode, GSTRatePercent, DefaultRate, RateType, ITCEligible, UISequence, IsActive,
         [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('E-Topup',                    '998413', 18, 0, 'PERCENT', 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Domestic Money Transfer',    '997112', 18, 0, 'PERCENT', 1, 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Bill Payment',               '998599', 18, 0, 'PERCENT', 1, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Fund Transfer / Payout',     '997112', 18, 0, 'PERCENT', 1, 4, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('AEPS / Micro ATM',           '997159', 18, 0, 'PERCENT', 1, 5, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Verification',               '997159', 18, 0, 'UNIT',    1, 6, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Other Financial Services',   '997159', 18, 0, 'PERCENT', 1, 7, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0),
        ('Software / API Charges',     '998314', 18, 0, 'UNIT',    1, 8, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    IF @@ROWCOUNT > 0 PRINT 'Seeded the starting purchase items.';
END
GO

IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'GstPortalUsername')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('GstPortalUsername', '', 1, 1,
         'The user name this business signs in to the GST portal (services.gst.gov.in) with. Shown on the GST Returns screen as a reminder, and used as the taxpayer user name when a GSP connection is set up for automatic filing.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    IF @@ROWCOUNT = 1 PRINT 'Inserted SettingMaster: GstPortalUsername (empty).';
END
GO

IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'GstPortalPassword')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('GstPortalPassword', '', 1, 1,
         'The GST portal password, kept here only so the person filing has it to hand. No part of the software reads it: the portal asks for a captcha and an OTP, so it cannot be signed in to automatically, and a GSP connection never needs the password.',
         1, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    IF @@ROWCOUNT = 1 PRINT 'Inserted SettingMaster: GstPortalPassword (empty).';
END
GO
