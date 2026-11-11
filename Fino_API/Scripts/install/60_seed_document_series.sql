/*
    GST document numbering series, for the current financial year.

    WHY THIS SHIPS
    No series means no number, and no number means no invoice. This platform is installed by a
    non-technical person one client at a time, so a working set ships and the admin adjusts the
    prefixes.

    The GSTIN comes from dbo.InvoiceSettings, so each install seeds its own registration without
    anybody editing this file.

    NUMBER LENGTH
    prefix (<= 4) + FY (4) + "/" + 5 digits = 14 characters, inside the GST limit of 16. Widening
    the prefix past 4 characters will push a number over the limit, and
    TaxDocumentNumberService refuses rather than issuing something the portal will reject.

    WHAT IS NOT DONE HERE
    Only the CURRENT financial year is opened. A series for the next year has to exist before
    1 April or invoicing stops that morning, so opening it is a dated task for the admin - not
    something to generate blindly years ahead, because an unused open series still shows up in
    GSTR-1's "Documents Issued" table.

    Idempotent - each series inserts only when absent for that GSTIN and year.
*/

SET NOCOUNT ON;

DECLARE @Gstin varchar(15) = (SELECT TOP 1 LTRIM(RTRIM(GSTIN)) FROM dbo.InvoiceSettings ORDER BY InvoiceSettingID);

IF @Gstin IS NULL OR LEN(@Gstin) <> 15
BEGIN
    PRINT 'NOTE: Company GSTIN is not set in dbo.InvoiceSettings, so no document series was seeded.';
    PRINT '      Set the GSTIN on the company profile, then re-run this script.';
END
ELSE
BEGIN
    -- India's financial year runs April to March, so a date in, say, September 2026 is FY 2026-27.
    DECLARE @Today date = CAST(SYSDATETIME() AS date);
    DECLARE @StartYear int = CASE WHEN MONTH(@Today) >= 4 THEN YEAR(@Today) ELSE YEAR(@Today) - 1 END;
    DECLARE @FY varchar(8) = CONCAT(RIGHT('0' + CAST(@StartYear % 100 AS varchar(2)), 2),
                                    RIGHT('0' + CAST((@StartYear + 1) % 100 AS varchar(2)), 2));

    INSERT INTO dbo.TaxDocumentSeries
        (SeriesCode, DocumentType, CompanyGSTIN, Prefix, FinancialYear,
         CurrentNumber, NumberLength, ResetPolicy, IsActive, [DateTime], UpdatedateTime, IsDeleted)
    SELECT v.SeriesCode, v.DocumentType, @Gstin, v.Prefix, @FY,
           0, 5, 'FinancialYear', 1, SYSDATETIME(), SYSDATETIME(), 0
    FROM (VALUES
        -- Sales to registered parties and the consolidated B2C invoice share one tax-invoice
        -- series: GST wants the series consecutive, not split by customer type.
        ('SALE-TAX',  'TaxInvoice',   'SI/'),
        ('SALE-BOS',  'BillOfSupply', 'BS/'),
        ('SALE-CRN',  'CreditNote',   'CN/'),
        ('SALE-DBN',  'DebitNote',    'DN/'),
        -- Reverse charge: we raise an invoice to ourselves for a supply we must pay the tax on.
        ('SELF-INV',  'SelfInvoice',  'SV/')
    ) AS v(SeriesCode, DocumentType, Prefix)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TaxDocumentSeries s
        WHERE s.SeriesCode = v.SeriesCode AND s.CompanyGSTIN = @Gstin AND s.FinancialYear = @FY);

    DECLARE @n int = (SELECT COUNT(*) FROM dbo.TaxDocumentSeries WHERE CompanyGSTIN = @Gstin AND FinancialYear = @FY);
    PRINT CONCAT('Document series for GSTIN ', @Gstin, ' FY ', @FY, ': ', @n, ' row(s).');
END
