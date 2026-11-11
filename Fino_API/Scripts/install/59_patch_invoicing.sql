/*
    GST documents - series, invoices, notes, IRN log and exchange legs. Phase 2.

    WHY THIS EXISTS
    Sale invoices were being produced by hand. That is why "18 crore" was a figure somebody
    totalled rather than a figure the system could stand behind, and why nothing could say which
    supplies were never billed at all. These tables make a document a record with a number, a
    basis, a party and a state - something a return can be built from and a reconciliation can
    compare.

    TWO THINGS WORTH READING BEFORE CHANGING ANYTHING HERE

    1. Sale and purchase share TaxInvoiceHeaders. Both sides therefore carry a gross value AND a
       taxable value AND the basis between them. Separate tables drift, and the 18cr-against-19cr
       mismatch is what drift looks like from the outside.

    2. The unique indexes are controls, not tidiness:
         - a sale number cannot repeat under one registration (the most expensive GST error);
         - the same supplier bill cannot be entered twice (a purchase register that inflates is
           half of what the 1 crore question was about);
         - one exchange leg per wallet movement per direction, so replaying the swap detector
           cannot double-count a barter.
       The first two are FILTERED by direction, because a purchase carries the supplier's own
       number and two suppliers may both legitimately have an "INV-001".

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database
    has none on those columns while dev does, and an INSERT that omits them passes every dev test
    and fails in production.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
-- Required for the FILTERED indexes below. sqlcmd does not always default it ON, and without it
-- CREATE INDEX fails with "SET options have incorrect settings: 'QUOTED_IDENTIFIER'".
SET QUOTED_IDENTIFIER ON;

/* ── 1. Numbering series ───────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxDocumentSeries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxDocumentSeries (
        [TaxDocumentSeriesID] INT          NOT NULL IDENTITY(1,1),
        [SeriesCode]          VARCHAR(40)  NOT NULL,
        [DocumentType]        VARCHAR(20)  NOT NULL,
        [CompanyGSTIN]        VARCHAR(15)  NOT NULL,
        [Prefix]              VARCHAR(10)  NOT NULL CONSTRAINT DF_TaxDocumentSeries_Prefix DEFAULT (''),
        [FinancialYear]       VARCHAR(8)   NOT NULL,
        -- The last number ISSUED. Allocated inside the database, never from a counter in
        -- application memory: two workers would otherwise hand out the same number twice.
        [CurrentNumber]       INT          NOT NULL CONSTRAINT DF_TaxDocumentSeries_CurrentNumber DEFAULT (0),
        [NumberLength]        INT          NOT NULL CONSTRAINT DF_TaxDocumentSeries_NumberLength DEFAULT (5),
        [ResetPolicy]         VARCHAR(20)  NOT NULL CONSTRAINT DF_TaxDocumentSeries_ResetPolicy DEFAULT ('FinancialYear'),
        [IsActive]            BIT          NOT NULL CONSTRAINT DF_TaxDocumentSeries_IsActive DEFAULT (1),
        [DateTime]            DATETIME2    NOT NULL CONSTRAINT DF_TaxDocumentSeries_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2    NOT NULL CONSTRAINT DF_TaxDocumentSeries_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT          NOT NULL CONSTRAINT DF_TaxDocumentSeries_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxDocumentSeries] PRIMARY KEY ([TaxDocumentSeriesID])
    );
    PRINT 'Created table dbo.TaxDocumentSeries';
END
ELSE PRINT 'dbo.TaxDocumentSeries already exists - skipped.';

/* ── 2. Document header ────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxInvoiceHeaders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxInvoiceHeaders (
        [TaxInvoiceHeaderID]        INT           NOT NULL IDENTITY(1,1),
        [Direction]                 VARCHAR(10)   NOT NULL,
        [DocumentType]              VARCHAR(20)   NOT NULL,
        [TaxDocumentSeriesID]       INT           NULL,
        -- Our number on a sale; the supplier's on a purchase.
        [DocumentNo]                VARCHAR(20)   NOT NULL,
        [DocumentDate]              DATETIME2     NOT NULL,
        -- Ezytm and Erech both dated 30-Sep but took their IRN on 5-Oct, so document date and
        -- the date the wallet was actually credited differ. Which one governs the month is a CA
        -- question; both are stored so either answer reports without re-entry.
        [WalletCreditDate]          DATETIME2     NULL,
        [PartyUserID]               INT           NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PartyUserID DEFAULT (0),
        [PartyName]                 VARCHAR(200)  NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PartyName DEFAULT (''),
        [PartyGSTIN]                VARCHAR(15)   NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PartyGSTIN DEFAULT (''),
        [PartyRegistrationStatus]   VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PartyRegistrationStatus DEFAULT (''),
        [CompanyGSTIN]              VARCHAR(15)   NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_CompanyGSTIN DEFAULT (''),
        [SupplierStateCode]         VARCHAR(2)    NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_SupplierStateCode DEFAULT (''),
        [PlaceOfSupplyStateCode]    VARCHAR(2)    NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PlaceOfSupplyStateCode DEFAULT (''),
        [SupplyNature]              VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_SupplyNature DEFAULT (''),
        -- Both bases, always.
        [GrossTotal]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_GrossTotal DEFAULT (0),
        [TaxableTotal]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_TaxableTotal DEFAULT (0),
        [CGSTTotal]                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_CGSTTotal DEFAULT (0),
        [SGSTTotal]                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_SGSTTotal DEFAULT (0),
        [IGSTTotal]                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_IGSTTotal DEFAULT (0),
        [CessTotal]                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_CessTotal DEFAULT (0),
        [RoundOff]                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_RoundOff DEFAULT (0),
        [GrandTotal]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_GrandTotal DEFAULT (0),
        [OriginalInvoiceHeaderID]   INT           NULL,
        [NoteReason]                VARCHAR(300)  NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_NoteReason DEFAULT (''),
        -- 30 November of the year after the original supply's FY, or the annual return, whichever
        -- is earlier. Past it a reversal no longer recovers the tax.
        [NoteTimeLimitDate]         DATETIME2     NULL,
        [Status]                    VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_Status DEFAULT ('Draft'),
        [PeriodKey]                 VARCHAR(8)    NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_PeriodKey DEFAULT (''),
        [IsConsolidated]            BIT           NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_IsConsolidated DEFAULT (0),
        [ExchangePartnerLegID]      INT           NULL,
        [Remark]                    VARCHAR(500)  NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_Remark DEFAULT (''),
        [CreatedByUserID]           INT           NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_CreatedByUserID DEFAULT (0),
        [ApprovedByUserID]          INT           NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_ApprovedByUserID DEFAULT (0),
        [ApprovedOn]                DATETIME2     NULL,
        [DateTime]                  DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]            DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]                 BIT           NOT NULL CONSTRAINT DF_TaxInvoiceHeaders_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxInvoiceHeaders] PRIMARY KEY ([TaxInvoiceHeaderID]),
        CONSTRAINT [FK_TaxInvoiceHeaders_TaxDocumentSeries] FOREIGN KEY ([TaxDocumentSeriesID])
            REFERENCES dbo.TaxDocumentSeries ([TaxDocumentSeriesID])
    );
    PRINT 'Created table dbo.TaxInvoiceHeaders';
END
ELSE PRINT 'dbo.TaxInvoiceHeaders already exists - skipped.';

/* ── 3. Document lines ─────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxInvoiceLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxInvoiceLines (
        [TaxInvoiceLineID]    INT           NOT NULL IDENTITY(1,1),
        [TaxInvoiceHeaderID]  INT           NOT NULL,
        [LineNo]              INT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_LineNo DEFAULT (1),
        [ServiceID]           INT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_ServiceID DEFAULT (0),
        [Description]         VARCHAR(300)  NOT NULL CONSTRAINT DF_TaxInvoiceLines_Description DEFAULT (''),
        [SACCode]             VARCHAR(10)   NOT NULL CONSTRAINT DF_TaxInvoiceLines_SACCode DEFAULT (''),
        -- Count of underlying transactions on a rolled-up line; JaiJui prints this as "Count".
        [Quantity]            INT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_Quantity DEFAULT (1),
        [BillingBasis]        VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxInvoiceLines_BillingBasis DEFAULT (''),
        -- Throughput AND taxable value on every line. JaiJui's own invoice shows 1,506.00 against
        -- 15.00 on one line; a system storing only the second cannot say what moved.
        [GrossValue]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_GrossValue DEFAULT (0),
        [TaxableValue]        DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_TaxableValue DEFAULT (0),
        [CGSTRate]            DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TaxInvoiceLines_CGSTRate DEFAULT (0),
        [CGSTAmount]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_CGSTAmount DEFAULT (0),
        [SGSTRate]            DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TaxInvoiceLines_SGSTRate DEFAULT (0),
        [SGSTAmount]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_SGSTAmount DEFAULT (0),
        [IGSTRate]            DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TaxInvoiceLines_IGSTRate DEFAULT (0),
        [IGSTAmount]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_IGSTAmount DEFAULT (0),
        [CessAmount]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_CessAmount DEFAULT (0),
        [LineTotal]           DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceLines_LineTotal DEFAULT (0),
        [TaxRuleMasterID]     INT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_TaxRuleMasterID DEFAULT (0),
        [ITCEligible]         BIT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_ITCEligible DEFAULT (0),
        [ReverseCharge]       BIT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_ReverseCharge DEFAULT (0),
        [DateTime]            DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceLines_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceLines_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT           NOT NULL CONSTRAINT DF_TaxInvoiceLines_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxInvoiceLines] PRIMARY KEY ([TaxInvoiceLineID]),
        CONSTRAINT [FK_TaxInvoiceLines_TaxInvoiceHeaders] FOREIGN KEY ([TaxInvoiceHeaderID])
            REFERENCES dbo.TaxInvoiceHeaders ([TaxInvoiceHeaderID])
    );
    PRINT 'Created table dbo.TaxInvoiceLines';
END
ELSE PRINT 'dbo.TaxInvoiceLines already exists - skipped.';

/* ── 4. Annexure: the transactions behind a rolled-up line ─────────────────── */
IF OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxInvoiceAnnexureLines (
        [TaxInvoiceAnnexureLineID] INT           NOT NULL IDENTITY(1,1),
        [TaxInvoiceHeaderID]       INT           NOT NULL,
        [TaxInvoiceLineID]         INT           NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_TaxInvoiceLineID DEFAULT (0),
        [TransactionMasterID]      INT           NOT NULL,
        [TransactionDate]          DATETIME2     NOT NULL,
        [ReferenceNo]              VARCHAR(50)   NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_ReferenceNo DEFAULT (''),
        [GrossValue]               DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_GrossValue DEFAULT (0),
        [TaxableValue]             DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_TaxableValue DEFAULT (0),
        [TaxAmount]                DECIMAL(18,2) NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_TaxAmount DEFAULT (0),
        [DateTime]                 DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]           DATETIME2     NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]                BIT           NOT NULL CONSTRAINT DF_TaxInvoiceAnnexureLines_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxInvoiceAnnexureLines] PRIMARY KEY ([TaxInvoiceAnnexureLineID]),
        CONSTRAINT [FK_TaxInvoiceAnnexureLines_TaxInvoiceHeaders] FOREIGN KEY ([TaxInvoiceHeaderID])
            REFERENCES dbo.TaxInvoiceHeaders ([TaxInvoiceHeaderID])
    );
    PRINT 'Created table dbo.TaxInvoiceAnnexureLines';
END
ELSE PRINT 'dbo.TaxInvoiceAnnexureLines already exists - skipped.';

/* ── 5. IRN log ────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.EInvoiceLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EInvoiceLogs (
        [EInvoiceLogID]            INT           NOT NULL IDENTITY(1,1),
        [TaxInvoiceHeaderID]       INT           NOT NULL,
        [Status]                   VARCHAR(20)   NOT NULL CONSTRAINT DF_EInvoiceLogs_Status DEFAULT ('Pending'),
        [IRN]                      VARCHAR(70)   NOT NULL CONSTRAINT DF_EInvoiceLogs_IRN DEFAULT (''),
        [AckNo]                    VARCHAR(30)   NOT NULL CONSTRAINT DF_EInvoiceLogs_AckNo DEFAULT (''),
        [AckDate]                  DATETIME2     NULL,
        [SignedQRCode]             VARCHAR(MAX)  NOT NULL CONSTRAINT DF_EInvoiceLogs_SignedQRCode DEFAULT (''),
        [RequestPayload]           VARCHAR(MAX)  NOT NULL CONSTRAINT DF_EInvoiceLogs_RequestPayload DEFAULT (''),
        [ResponsePayload]          VARCHAR(MAX)  NOT NULL CONSTRAINT DF_EInvoiceLogs_ResponsePayload DEFAULT (''),
        [ErrorCode]                VARCHAR(50)   NOT NULL CONSTRAINT DF_EInvoiceLogs_ErrorCode DEFAULT (''),
        [ErrorMessage]             VARCHAR(1000) NOT NULL CONSTRAINT DF_EInvoiceLogs_ErrorMessage DEFAULT (''),
        [AttemptCount]             INT           NOT NULL CONSTRAINT DF_EInvoiceLogs_AttemptCount DEFAULT (0),
        [LastAttemptOn]            DATETIME2     NULL,
        -- Document date + 30 days: past this the IRP will not issue an IRN at all, and the only
        -- remaining route is a credit note plus a fresh invoice. Drives a daily alert.
        [ReportingDeadline]        DATETIME2     NULL,
        -- An IRN can only be cancelled within 24 hours of issue.
        [CancellationWindowEnds]   DATETIME2     NULL,
        [DateTime]                 DATETIME2     NOT NULL CONSTRAINT DF_EInvoiceLogs_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]           DATETIME2     NOT NULL CONSTRAINT DF_EInvoiceLogs_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]                BIT           NOT NULL CONSTRAINT DF_EInvoiceLogs_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_EInvoiceLogs] PRIMARY KEY ([EInvoiceLogID]),
        CONSTRAINT [FK_EInvoiceLogs_TaxInvoiceHeaders] FOREIGN KEY ([TaxInvoiceHeaderID])
            REFERENCES dbo.TaxInvoiceHeaders ([TaxInvoiceHeaderID])
    );
    PRINT 'Created table dbo.EInvoiceLogs';
END
ELSE PRINT 'dbo.EInvoiceLogs already exists - skipped.';

/* ── 6. Exchange legs ──────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.ExchangePartnerLegs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExchangePartnerLegs (
        [ExchangePartnerLegID] INT           NOT NULL IDENTITY(1,1),
        [PartnerUserID]        INT           NOT NULL,
        -- OUTWARD = we credited them (our sale). INWARD = they credited us (their invoice, which
        -- we have to obtain). Under GST s.7(1)(a) barter is supply and BOTH legs are taxable, so
        -- the turnover is the sum of the two, not the difference - which nets to zero.
        [Direction]            VARCHAR(10)   NOT NULL,
        [FundTransactionID]    INT           NOT NULL,
        [WalletID]             INT           NOT NULL CONSTRAINT DF_ExchangePartnerLegs_WalletID DEFAULT (0),
        [LegDate]              DATETIME2     NOT NULL,
        [Amount]               DECIMAL(18,2) NOT NULL CONSTRAINT DF_ExchangePartnerLegs_Amount DEFAULT (0),
        [TaxInvoiceHeaderID]   INT           NULL,
        -- AwaitingDocument IS the queue. A leg with no document is the gap this table exists to
        -- make visible while it can still be fixed.
        [DocumentStatus]       VARCHAR(20)   NOT NULL CONSTRAINT DF_ExchangePartnerLegs_DocumentStatus DEFAULT ('AwaitingDocument'),
        [PeriodKey]            VARCHAR(8)    NOT NULL CONSTRAINT DF_ExchangePartnerLegs_PeriodKey DEFAULT (''),
        [Remark]               VARCHAR(500)  NOT NULL CONSTRAINT DF_ExchangePartnerLegs_Remark DEFAULT (''),
        [DateTime]             DATETIME2     NOT NULL CONSTRAINT DF_ExchangePartnerLegs_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]       DATETIME2     NOT NULL CONSTRAINT DF_ExchangePartnerLegs_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]            BIT           NOT NULL CONSTRAINT DF_ExchangePartnerLegs_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_ExchangePartnerLegs] PRIMARY KEY ([ExchangePartnerLegID]),
        CONSTRAINT [FK_ExchangePartnerLegs_UserMasters] FOREIGN KEY ([PartnerUserID])
            REFERENCES dbo.UserMasters ([UserMasterID]),
        CONSTRAINT [FK_ExchangePartnerLegs_TaxInvoiceHeaders] FOREIGN KEY ([TaxInvoiceHeaderID])
            REFERENCES dbo.TaxInvoiceHeaders ([TaxInvoiceHeaderID])
    );
    PRINT 'Created table dbo.ExchangePartnerLegs';
END
ELSE PRINT 'dbo.ExchangePartnerLegs already exists - skipped.';

/* ── 7. Indexes ────────────────────────────────────────────────────────────────
   Created SEPARATELY from the tables, each guarded on its own.

   The first version of this script created each index inside its table's IF block. A filtered
   index then failed on QUOTED_IDENTIFIER, the CREATE TABLE had already committed, and the re-run
   skipped the whole block because the table now existed - leaving a permanently un-indexed table
   and no error to say so. The unique indexes here ARE the controls against duplicate invoice
   numbers and double-entered supplier bills, so silently not having them is the worst outcome
   available. Guarding each one separately means a re-run repairs a partial failure.
   ──────────────────────────────────────────────────────────────────────────── */

-- One live counter per series per registration per year. A second row for the same three would
-- be a second counter issuing the same numbers.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_TaxDocumentSeries_Code_GSTIN_FY' AND object_id = OBJECT_ID(N'dbo.TaxDocumentSeries'))
BEGIN
    CREATE UNIQUE INDEX UX_TaxDocumentSeries_Code_GSTIN_FY
        ON dbo.TaxDocumentSeries ([SeriesCode], [CompanyGSTIN], [FinancialYear]);
    PRINT '  + UX_TaxDocumentSeries_Code_GSTIN_FY';
END

-- No duplicate sale number under one registration - the most expensive GST error available.
-- Filtered to sales: a purchase carries the SUPPLIER's number and two suppliers may both
-- legitimately have an "INV-001".
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_TaxInvoiceHeaders_Sale_DocumentNo' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceHeaders'))
BEGIN
    CREATE UNIQUE INDEX UX_TaxInvoiceHeaders_Sale_DocumentNo
        ON dbo.TaxInvoiceHeaders ([CompanyGSTIN], [DocumentType], [DocumentNo])
        WHERE [Direction] = 'SALE' AND [IsDeleted] = 0;
    PRINT '  + UX_TaxInvoiceHeaders_Sale_DocumentNo';
END

-- The same supplier bill cannot be entered twice - the control that stops a purchase register
-- inflating, which is half of what the 1 crore question was about.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_TaxInvoiceHeaders_Purchase_PartyDocumentNo' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceHeaders'))
BEGIN
    CREATE UNIQUE INDEX UX_TaxInvoiceHeaders_Purchase_PartyDocumentNo
        ON dbo.TaxInvoiceHeaders ([PartyUserID], [DocumentNo])
        WHERE [Direction] = 'PURCHASE' AND [IsDeleted] = 0;
    PRINT '  + UX_TaxInvoiceHeaders_Purchase_PartyDocumentNo';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaxInvoiceHeaders_Direction_PeriodKey_Status' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceHeaders'))
BEGIN
    CREATE INDEX IX_TaxInvoiceHeaders_Direction_PeriodKey_Status
        ON dbo.TaxInvoiceHeaders ([Direction], [PeriodKey], [Status]);
    PRINT '  + IX_TaxInvoiceHeaders_Direction_PeriodKey_Status';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaxInvoiceLines_TaxInvoiceHeaderID' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceLines'))
BEGIN
    CREATE INDEX IX_TaxInvoiceLines_TaxInvoiceHeaderID ON dbo.TaxInvoiceLines ([TaxInvoiceHeaderID]);
    PRINT '  + IX_TaxInvoiceLines_TaxInvoiceHeaderID';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaxInvoiceAnnexureLines_HeaderID' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines'))
BEGIN
    CREATE INDEX IX_TaxInvoiceAnnexureLines_HeaderID ON dbo.TaxInvoiceAnnexureLines ([TaxInvoiceHeaderID]);
    PRINT '  + IX_TaxInvoiceAnnexureLines_HeaderID';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaxInvoiceAnnexureLines_TransactionMasterID' AND object_id = OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines'))
BEGIN
    CREATE INDEX IX_TaxInvoiceAnnexureLines_TransactionMasterID ON dbo.TaxInvoiceAnnexureLines ([TransactionMasterID]);
    PRINT '  + IX_TaxInvoiceAnnexureLines_TransactionMasterID';
END

-- Drives the daily "IRN still pending and the 30-day limit is approaching" alert.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EInvoiceLogs_Status_ReportingDeadline' AND object_id = OBJECT_ID(N'dbo.EInvoiceLogs'))
BEGIN
    CREATE INDEX IX_EInvoiceLogs_Status_ReportingDeadline ON dbo.EInvoiceLogs ([Status], [ReportingDeadline]);
    PRINT '  + IX_EInvoiceLogs_Status_ReportingDeadline';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EInvoiceLogs_TaxInvoiceHeaderID' AND object_id = OBJECT_ID(N'dbo.EInvoiceLogs'))
BEGIN
    CREATE INDEX IX_EInvoiceLogs_TaxInvoiceHeaderID ON dbo.EInvoiceLogs ([TaxInvoiceHeaderID]);
    PRINT '  + IX_EInvoiceLogs_TaxInvoiceHeaderID';
END

-- One leg per wallet movement per direction, so replaying the swap detector cannot double-count
-- a barter. Double-counting turnover is as wrong as missing it.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ExchangePartnerLegs_FundTransaction_Direction' AND object_id = OBJECT_ID(N'dbo.ExchangePartnerLegs'))
BEGIN
    CREATE UNIQUE INDEX UX_ExchangePartnerLegs_FundTransaction_Direction
        ON dbo.ExchangePartnerLegs ([FundTransactionID], [Direction]);
    PRINT '  + UX_ExchangePartnerLegs_FundTransaction_Direction';
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExchangePartnerLegs_DocumentStatus_PeriodKey' AND object_id = OBJECT_ID(N'dbo.ExchangePartnerLegs'))
BEGIN
    CREATE INDEX IX_ExchangePartnerLegs_DocumentStatus_PeriodKey
        ON dbo.ExchangePartnerLegs ([DocumentStatus], [PeriodKey]);
    PRINT '  + IX_ExchangePartnerLegs_DocumentStatus_PeriodKey';
END
