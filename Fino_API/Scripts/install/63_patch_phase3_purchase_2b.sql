/*
    63 - Phase 3: GSTR-2B snapshots, the ITC register, and the recon result on the 2B line.

    WHY THIS EXISTS
    A purchase document is already a TaxInvoiceHeader with Direction = PURCHASE - sale and purchase
    share that table on purpose, so both sides carry the same columns and can be compared. Phase 3
    adds the three things a purchase cannot be reconciled without:

      1. What the government says we bought (GSTR-2B, imported as a snapshot + its lines), so our
         books can be matched against the suppliers' own filings.
      2. The input-tax-credit ledger, which turns a matched purchase into a credit with two
         deadlines on it (Rule 37's 180 days and Section 16(4)'s 30-November).

    The 4-bucket reconciliation writes its result back onto the 2B line (MatchStatus / MatchedHeaderID)
    so the match survives a reload; it needs no table of its own.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does, and an INSERT that omits them passes every dev test and
    fails in production with "Cannot insert the value NULL".

    The unique indexes are controls, not tidiness:
      - one 2B document per supplier per number within an import, so a re-parse cannot double the 2B;
      - one ITC entry per purchase document, so a credit cannot be counted twice.
    Both are created in their OWN guarded blocks, never inside the CREATE TABLE's IF - a failure
    after the table commits would otherwise leave a permanently un-indexed table the re-run skips.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
-- Required for the FILTERED unique indexes below, and for any later INSERT/UPDATE on these tables.
SET QUOTED_IDENTIFIER ON;

/* ── 1. GSTR-2B import (one snapshot per registration per period) ───────────────────────── */
IF OBJECT_ID(N'dbo.Gstr2bImports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gstr2bImports (
        [Gstr2bImportID]  INT          NOT NULL IDENTITY(1,1),
        [CompanyGSTIN]    VARCHAR(15)  NOT NULL,
        [PeriodKey]       VARCHAR(8)   NOT NULL,
        [GeneratedDate]   DATETIME2    NULL,
        [Source]          VARCHAR(20)  NOT NULL CONSTRAINT DF_Gstr2bImports_Source DEFAULT ('JSON'),
        [LineCount]       INT          NOT NULL CONSTRAINT DF_Gstr2bImports_LineCount DEFAULT (0),
        [TotalTaxable]    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bImports_TotalTaxable DEFAULT (0),
        [TotalTax]        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bImports_TotalTax DEFAULT (0),
        [Status]          VARCHAR(20)  NOT NULL CONSTRAINT DF_Gstr2bImports_Status DEFAULT ('Active'),
        [ImportedByUserID] INT         NOT NULL CONSTRAINT DF_Gstr2bImports_ImportedBy DEFAULT (0),
        [DateTime]        DATETIME2    NOT NULL CONSTRAINT DF_Gstr2bImports_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]  DATETIME2    NOT NULL CONSTRAINT DF_Gstr2bImports_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]       BIT          NOT NULL CONSTRAINT DF_Gstr2bImports_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_Gstr2bImports] PRIMARY KEY ([Gstr2bImportID])
    );
    PRINT 'Created table dbo.Gstr2bImports';
END
GO

-- At most one ACTIVE snapshot per registration per period. Filtered on Status so a superseded
-- snapshot can stay for history without blocking the new one.
IF OBJECT_ID(N'dbo.Gstr2bImports', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Gstr2bImports_ActivePeriod' AND object_id = OBJECT_ID(N'dbo.Gstr2bImports'))
BEGIN
    CREATE UNIQUE INDEX UX_Gstr2bImports_ActivePeriod
        ON dbo.Gstr2bImports (CompanyGSTIN, PeriodKey)
        WHERE Status = 'Active' AND IsDeleted = 0;
    PRINT 'Created index UX_Gstr2bImports_ActivePeriod';
END
GO

/* ── 2. GSTR-2B lines (one supplier document per row) ───────────────────────────────────── */
IF OBJECT_ID(N'dbo.Gstr2bLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gstr2bLines (
        [Gstr2bLineID]            INT          NOT NULL IDENTITY(1,1),
        [Gstr2bImportID]          INT          NOT NULL,
        [SupplierGSTIN]           VARCHAR(15)  NOT NULL,
        [SupplierName]            VARCHAR(200) NOT NULL CONSTRAINT DF_Gstr2bLines_SupplierName DEFAULT (''),
        [DocumentType]            VARCHAR(10)  NOT NULL CONSTRAINT DF_Gstr2bLines_DocType DEFAULT ('INV'),
        [DocumentNo]              VARCHAR(30)  NOT NULL,
        [DocumentDate]            DATETIME2    NOT NULL,
        [PlaceOfSupplyStateCode]  VARCHAR(2)   NOT NULL CONSTRAINT DF_Gstr2bLines_Pos DEFAULT (''),
        [ReverseCharge]           BIT          NOT NULL CONSTRAINT DF_Gstr2bLines_Rcm DEFAULT (0),
        [TaxableValue]            DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Taxable DEFAULT (0),
        [IGST]                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Igst DEFAULT (0),
        [CGST]                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Cgst DEFAULT (0),
        [SGST]                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Sgst DEFAULT (0),
        [Cess]                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Cess DEFAULT (0),
        [ItcAvailable]            BIT          NOT NULL CONSTRAINT DF_Gstr2bLines_Itc DEFAULT (1),
        [ItcUnavailableReason]    VARCHAR(100) NOT NULL CONSTRAINT DF_Gstr2bLines_ItcReason DEFAULT (''),
        [MatchStatus]             VARCHAR(20)  NOT NULL CONSTRAINT DF_Gstr2bLines_Match DEFAULT ('Unmatched'),
        [MatchedHeaderID]         INT          NULL,
        [TaxVariance]             DECIMAL(18,2) NOT NULL CONSTRAINT DF_Gstr2bLines_Var DEFAULT (0),
        [DateTime]                DATETIME2    NOT NULL CONSTRAINT DF_Gstr2bLines_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]          DATETIME2    NOT NULL CONSTRAINT DF_Gstr2bLines_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]               BIT          NOT NULL CONSTRAINT DF_Gstr2bLines_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_Gstr2bLines] PRIMARY KEY ([Gstr2bLineID]),
        CONSTRAINT [FK_Gstr2bLines_Import] FOREIGN KEY ([Gstr2bImportID]) REFERENCES dbo.Gstr2bImports ([Gstr2bImportID])
    );
    PRINT 'Created table dbo.Gstr2bLines';
END
GO

-- One supplier document per number per type within an import. Not filtered by direction the way
-- the sale/purchase index is - a 2B import is already one direction (inward).
IF OBJECT_ID(N'dbo.Gstr2bLines', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Gstr2bLines_Doc' AND object_id = OBJECT_ID(N'dbo.Gstr2bLines'))
BEGIN
    CREATE UNIQUE INDEX UX_Gstr2bLines_Doc
        ON dbo.Gstr2bLines (Gstr2bImportID, SupplierGSTIN, DocumentNo, DocumentType)
        WHERE IsDeleted = 0;
    PRINT 'Created index UX_Gstr2bLines_Doc';
END
GO

-- The recon reads 2B lines by import and by supplier+number; both are hot on every run.
IF OBJECT_ID(N'dbo.Gstr2bLines', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Gstr2bLines_Supplier' AND object_id = OBJECT_ID(N'dbo.Gstr2bLines'))
BEGIN
    CREATE INDEX IX_Gstr2bLines_Supplier ON dbo.Gstr2bLines (SupplierGSTIN, DocumentNo);
    PRINT 'Created index IX_Gstr2bLines_Supplier';
END
GO

/* ── 3. ITC register (one row per purchase document that carries credit) ─────────────────── */
IF OBJECT_ID(N'dbo.ItcRegisterEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItcRegisterEntries (
        [ItcRegisterEntryID]  INT          NOT NULL IDENTITY(1,1),
        [TaxInvoiceHeaderID]  INT          NOT NULL,
        [PeriodKey]           VARCHAR(8)   NOT NULL,
        [SupplierGSTIN]       VARCHAR(15)  NOT NULL CONSTRAINT DF_Itc_SupplierGstin DEFAULT (''),
        [SupplierName]        VARCHAR(200) NOT NULL CONSTRAINT DF_Itc_SupplierName DEFAULT (''),
        [TaxableValue]        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Taxable DEFAULT (0),
        [IGST]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Igst DEFAULT (0),
        [CGST]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Cgst DEFAULT (0),
        [SGST]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Sgst DEFAULT (0),
        [Cess]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Cess DEFAULT (0),
        [EligibleITC]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_Itc_Eligible DEFAULT (0),
        [Status]              VARCHAR(20)  NOT NULL CONSTRAINT DF_Itc_Status DEFAULT ('Eligible'),
        [ReconBucket]         VARCHAR(20)  NOT NULL CONSTRAINT DF_Itc_Bucket DEFAULT (''),
        [Matched2BLineID]     INT          NULL,
        [ClaimedInPeriod]     VARCHAR(8)   NOT NULL CONSTRAINT DF_Itc_ClaimedPeriod DEFAULT (''),
        [ClaimedOn]           DATETIME2    NULL,
        [ClaimedByUserID]     INT          NOT NULL CONSTRAINT DF_Itc_ClaimedBy DEFAULT (0),
        [ReversedOn]          DATETIME2    NULL,
        [ReversedByUserID]    INT          NOT NULL CONSTRAINT DF_Itc_ReversedBy DEFAULT (0),
        [ReversalReason]      VARCHAR(20)  NOT NULL CONSTRAINT DF_Itc_ReversalReason DEFAULT (''),
        [Rule37DueDate]       DATETIME2    NOT NULL CONSTRAINT DF_Itc_Rule37 DEFAULT ('9999-12-31'),
        [Section16_4Deadline] DATETIME2    NOT NULL CONSTRAINT DF_Itc_164 DEFAULT ('9999-12-31'),
        [SupplierPaid]        BIT          NOT NULL CONSTRAINT DF_Itc_Paid DEFAULT (0),
        [SupplierPaidOn]      DATETIME2    NULL,
        [DateTime]            DATETIME2    NOT NULL CONSTRAINT DF_Itc_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2    NOT NULL CONSTRAINT DF_Itc_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT          NOT NULL CONSTRAINT DF_Itc_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_ItcRegisterEntries] PRIMARY KEY ([ItcRegisterEntryID]),
        CONSTRAINT [FK_ItcRegisterEntries_Header] FOREIGN KEY ([TaxInvoiceHeaderID]) REFERENCES dbo.TaxInvoiceHeaders ([TaxInvoiceHeaderID])
    );
    PRINT 'Created table dbo.ItcRegisterEntries';
END
GO

-- One ITC entry per purchase document, so re-running the register builder cannot double a credit.
IF OBJECT_ID(N'dbo.ItcRegisterEntries', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ItcRegisterEntries_Header' AND object_id = OBJECT_ID(N'dbo.ItcRegisterEntries'))
BEGIN
    CREATE UNIQUE INDEX UX_ItcRegisterEntries_Header
        ON dbo.ItcRegisterEntries (TaxInvoiceHeaderID)
        WHERE IsDeleted = 0;
    PRINT 'Created index UX_ItcRegisterEntries_Header';
END
GO

-- The alert queries scan by the two deadlines and by status; a narrow index keeps them off the table.
IF OBJECT_ID(N'dbo.ItcRegisterEntries', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ItcRegisterEntries_Status' AND object_id = OBJECT_ID(N'dbo.ItcRegisterEntries'))
BEGIN
    CREATE INDEX IX_ItcRegisterEntries_Status ON dbo.ItcRegisterEntries (Status, PeriodKey);
    PRINT 'Created index IX_ItcRegisterEntries_Status';
END
GO

/* ── 4. Three-way link on the shared header (purchase side only, both nullable) ─────────── */
-- Added to TaxInvoiceHeaders rather than a side table because a provider wallet top-up IS a
-- purchase, and the link is a property of that document. Nullable, so every existing sale and
-- the Phase 2 generators that never set them keep inserting exactly as before.
IF COL_LENGTH('dbo.TaxInvoiceHeaders', 'ProviderID') IS NULL
BEGIN
    ALTER TABLE dbo.TaxInvoiceHeaders ADD [ProviderID] INT NULL;
    PRINT 'Added dbo.TaxInvoiceHeaders.ProviderID';
END
IF COL_LENGTH('dbo.TaxInvoiceHeaders', 'WalletID') IS NULL
BEGIN
    ALTER TABLE dbo.TaxInvoiceHeaders ADD [WalletID] INT NULL;
    PRINT 'Added dbo.TaxInvoiceHeaders.WalletID';
END
GO

PRINT 'Phase 3 purchase / 2B / ITC schema is in place.';

