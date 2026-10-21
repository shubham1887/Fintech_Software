<#
.SYNOPSIS
    Seeds the fixture the invoice-generation verifier runs against.
.DESCRIPTION
    Creates:
      - wallet 9, mapped to ONE gross-basis service. Every real wallet in the catalogue mixes
        gross and charge services and is therefore correctly refused for top-up invoicing, so a
        correctly separated wallet has to be created to exercise the happy path at all.
      - a tax profile for two parties: one REGISTERED (one invoice per top-up) and one
        UNREGISTERED (one consolidated invoice per day).
      - four successful wallet credits: one for the registered party and three on the same day
        for the unregistered one, so consolidation has something to consolidate.
      - the SALE-TAX numbering series, if absent.

    Also clears any documents from a previous run and resets the counter, so the verifier starts
    from a known state.

    NEVER run against finocore_db.
.EXAMPLE
    .\Seed-TestData.ps1 -Database FintechApp
#>
[CmdletBinding()]
param(
    [string] $Server = "localhost\SQLEXPRESS",
    [Parameter(Mandatory = $true)] [string] $Database,
    [string] $CompanyGstin = "27ABCCS3656G1Z8"
)
$ErrorActionPreference = "Stop"
if ($Database -ieq "finocore_db") { throw "Refusing to seed test data into the live database." }

$sqlcmd = (Get-Command sqlcmd -ErrorAction SilentlyContinue).Source
if ($null -eq $sqlcmd) {
    $sqlcmd = (Get-ChildItem "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC" -Filter sqlcmd.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
}
if ($null -eq $sqlcmd) { throw "sqlcmd not found." }

# SET QUOTED_IDENTIFIER ON is required for any DML on the invoice tables: they carry filtered
# indexes and SQL Server refuses otherwise. sqlcmd does not default it ON.
$sql = @"
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

/* A wallet with exactly one GROSS service - what a correctly separated wallet looks like. */
IF NOT EXISTS (SELECT 1 FROM dbo.WalletMaster WHERE WalletID = 9)
BEGIN
    SET IDENTITY_INSERT dbo.WalletMaster ON;
    INSERT INTO dbo.WalletMaster (WalletID, ShortName, Name, UISequence, IsActive, [DateTime], UpdatedateTime, IsDeleted)
    VALUES (9, 'TGRS', 'Test Gross Wallet', 9, 1, SYSDATETIME(), SYSDATETIME(), 0);
    SET IDENTITY_INSERT dbo.WalletMaster OFF;
END

DECLARE @GrossService int = (
    SELECT TOP 1 ServiceID FROM dbo.ServiceMasters
    WHERE IsDeleted = 0 AND IsActive = 1 AND ServiceCategory IN ('Recharge','Mobile Services','DTH Services')
    ORDER BY ServiceID);

IF @GrossService IS NULL
    PRINT 'NOTE: no gross-basis service in the catalogue; the verifier will have nothing to invoice.';
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.WalletServiceMappings WHERE WalletID = 9)
    INSERT INTO dbo.WalletServiceMappings (WalletID, ServiceID, [DateTime], UpdatedateTime, IsDeleted)
    VALUES (9, @GrossService, SYSDATETIME(), SYSDATETIME(), 0);

/* Two parties: the lowest non-admin id is registered, the next is not. */
DECLARE @Regd int = (SELECT TOP 1 UserMasterID FROM dbo.UserMasters WHERE UserTypeID <> 1 ORDER BY UserMasterID);
DECLARE @Unregd int = (SELECT TOP 1 UserMasterID FROM dbo.UserMasters WHERE UserTypeID <> 1 AND UserMasterID <> @Regd ORDER BY UserMasterID);

IF @Regd IS NULL OR @Unregd IS NULL
    PRINT 'NOTE: fewer than two non-admin users exist; the verifier needs two parties.';
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.PartyTaxProfiles WHERE UserID = @Regd)
        INSERT INTO dbo.PartyTaxProfiles (UserID, GSTIN, RegistrationStatus, PAN, PlaceOfSupplyStateCode, [DateTime], UpdatedateTime, IsDeleted)
        VALUES (@Regd, '27AAAAA0000A1Z5', 'REGISTERED', 'ABCDF0102G', '27', SYSDATETIME(), SYSDATETIME(), 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.PartyTaxProfiles WHERE UserID = @Unregd)
        INSERT INTO dbo.PartyTaxProfiles (UserID, GSTIN, RegistrationStatus, PAN, PlaceOfSupplyStateCode, [DateTime], UpdatedateTime, IsDeleted)
        VALUES (@Unregd, '', 'UNREGISTERED', '', '27', SYSDATETIME(), SYSDATETIME(), 0);

    DECLARE @Admin int = (SELECT TOP 1 UserMasterID FROM dbo.UserMasters WHERE UserTypeID = 1 ORDER BY UserMasterID);

    DELETE FROM dbo.FundTransactions WHERE Remark = 'ZZPROBE';
    INSERT INTO dbo.FundTransactions (SenderID, ReceiverID, WalletID, Amount, OriginalAmount, CashCollectionAmount,
      SenderOpeningBalance, SenderClosingBalance, ReceiverOpeningBalance, ReceiverClosingBalance, Remark, UTRNumber,
      Status, Type, AdminRemark, PayMode, Through, RequestToID, CashCollectionRemark, Description, BankName,
      ReceiptPath, TransactionDOC, TransactionDOCInt, IsDelete, RequestAcceptedUserID, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
      (@Admin, @Regd,   9, 10000.00, 10000.00, 0, 0,0,0,0, 'ZZPROBE','', 'SUCCESS','TRANSFER','','','',@Admin,'','','','', SYSDATETIME(), 0, 0, 0, '2026-10-05T10:00:00', SYSDATETIME(), 0),
      (@Admin, @Unregd, 9,  2000.00,  2000.00, 0, 0,0,0,0, 'ZZPROBE','', 'SUCCESS','TRANSFER','','','',@Admin,'','','','', SYSDATETIME(), 0, 0, 0, '2026-10-06T09:00:00', SYSDATETIME(), 0),
      (@Admin, @Unregd, 9,  3000.00,  3000.00, 0, 0,0,0,0, 'ZZPROBE','', 'SUCCESS','TRANSFER','','','',@Admin,'','','','', SYSDATETIME(), 0, 0, 0, '2026-10-06T14:00:00', SYSDATETIME(), 0),
      (@Admin, @Unregd, 9,  5000.00,  5000.00, 0, 0,0,0,0, 'ZZPROBE','', 'SUCCESS','TRANSFER','','','',@Admin,'','','','', SYSDATETIME(), 0, 0, 0, '2026-10-06T18:00:00', SYSDATETIME(), 0);

    PRINT CONCAT('Parties: registered=', @Regd, ', unregistered=', @Unregd);
END

/* The numbering series the verifier allocates from. */
IF NOT EXISTS (SELECT 1 FROM dbo.TaxDocumentSeries WHERE SeriesCode='SALE-TAX' AND CompanyGSTIN='$CompanyGstin' AND FinancialYear='2627')
    INSERT INTO dbo.TaxDocumentSeries (SeriesCode,DocumentType,CompanyGSTIN,Prefix,FinancialYear,CurrentNumber,NumberLength,ResetPolicy,IsActive,[DateTime],UpdatedateTime,IsDeleted)
    VALUES ('SALE-TAX','TaxInvoice','$CompanyGstin','SI/','2627',0,5,'FinancialYear',1,SYSDATETIME(),SYSDATETIME(),0);

/* Start from a known state. */
DELETE FROM dbo.TaxInvoiceAnnexureLines;
DELETE FROM dbo.TaxInvoiceLines;
DELETE FROM dbo.TaxInvoiceHeaders;
UPDATE dbo.TaxDocumentSeries SET CurrentNumber = 0;

DECLARE @n int = (SELECT COUNT(*) FROM dbo.FundTransactions WHERE Remark='ZZPROBE');
PRINT CONCAT('Fixture ready: ', @n, ' top-up(s), documents cleared, counter reset.');
"@

& $sqlcmd -S $Server -E -C -d $Database -Q $sql
