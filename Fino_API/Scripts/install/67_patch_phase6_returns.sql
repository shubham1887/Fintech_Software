/*
    67 - Phase 6: the record of a GST return that was generated or filed.

    WHY THIS EXISTS
    GSTR-1, GSTR-3B and GSTR-9 are all DERIVED - every figure comes from documents the system
    already holds, and nothing in Phase 6 is entered twice. So why store anything at all?

    Because a return, once filed, stops being a derivation and becomes a fact. Two things depend
    on that:

      1. GSTR-9 asks for the annual figures BOTH "as per books" and "as declared in the returns
         filed". Without a record of what was actually filed, the only honest answer to the second
         is the first one recomputed - which is not an answer, it is an assumption, and the whole
         purpose of the annual return is to surface where the two differ.
      2. A rule or a rate changed after a month was filed restates that month on every rebuild.
         A snapshot of the JSON as it went to the portal is the only way to show, later, what was
         declared rather than what today's configuration would declare.

    ONE TABLE. Status is Draft / Filed / Superseded, and a regeneration supersedes the previous
    snapshot of the same return and period rather than overwriting it - the same rule the 2B and
    bank-statement imports follow, and for the same reason: the old one is evidence.

    PayloadJson holds the return in the portal's own shape. It is nvarchar(max) because a busy
    month's GSTR-1 is comfortably past 8,000 characters, and because trimming a filed return to
    fit a column would be the one corruption nobody would notice.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does, and an INSERT that omits them passes every dev test and
    fails in production with "Cannot insert the value NULL".

    The unique index is a control: at most one LIVE (Draft or Filed) snapshot per return per period
    per registration, so "what did we file for October" has exactly one answer. It is created in
    its OWN guarded block, never inside the CREATE TABLE's IF - a failure after the table commits
    would otherwise leave a permanently un-indexed table the re-run skips.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
-- Required for the FILTERED unique index below, and for any later INSERT/UPDATE on this table.
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.GstReturnFilings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GstReturnFilings (
        [GstReturnFilingID] INT          NOT NULL IDENTITY(1,1),
        [ReturnType]        VARCHAR(10)  NOT NULL,
        -- yyyyMM for GSTR-1 and GSTR-3B; yyyy0000 + the FY start year for GSTR-9 (e.g. 20260000
        -- means FY 2026-27), so one column orders and groups every return the same way.
        [PeriodKey]         VARCHAR(8)   NOT NULL,
        [CompanyGSTIN]      VARCHAR(15)  NOT NULL,

        -- The headline figures, denormalised so the list screen needs no JSON parsing.
        [TotalTaxableValue] DECIMAL(18,2) NOT NULL CONSTRAINT DF_GstReturnFilings_Taxable DEFAULT (0),
        [TotalIGST]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_GstReturnFilings_IGST DEFAULT (0),
        [TotalCGST]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_GstReturnFilings_CGST DEFAULT (0),
        [TotalSGST]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_GstReturnFilings_SGST DEFAULT (0),
        [TotalCess]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_GstReturnFilings_Cess DEFAULT (0),
        [DocumentCount]     INT          NOT NULL CONSTRAINT DF_GstReturnFilings_DocCount DEFAULT (0),

        -- The return as it went to the portal. See the note above on why this is kept.
        [PayloadJson]       NVARCHAR(MAX) NOT NULL CONSTRAINT DF_GstReturnFilings_Payload DEFAULT (''),

        [Status]            VARCHAR(20)  NOT NULL CONSTRAINT DF_GstReturnFilings_Status DEFAULT ('Draft'),
        -- Set only when the return is declared filed, with the portal's acknowledgement if there
        -- is one. A filed return with no reference is still filed; the column is not required.
        [FiledOn]           DATETIME2    NULL,
        [AcknowledgementNo] VARCHAR(50)  NOT NULL CONSTRAINT DF_GstReturnFilings_Ack DEFAULT (''),
        [Note]              VARCHAR(300) NOT NULL CONSTRAINT DF_GstReturnFilings_Note DEFAULT (''),
        [CreatedByUserID]   INT          NOT NULL CONSTRAINT DF_GstReturnFilings_CreatedBy DEFAULT (0),

        [DateTime]          DATETIME2    NOT NULL CONSTRAINT DF_GstReturnFilings_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]    DATETIME2    NOT NULL CONSTRAINT DF_GstReturnFilings_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]         BIT          NOT NULL CONSTRAINT DF_GstReturnFilings_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_GstReturnFilings] PRIMARY KEY ([GstReturnFilingID])
    );
    PRINT 'Created table dbo.GstReturnFilings';
END
GO

-- At most one LIVE snapshot per return per period per registration. Filtered on Status so every
-- superseded generation stays readable for history without blocking the replacement.
IF OBJECT_ID(N'dbo.GstReturnFilings', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_GstReturnFilings_Live'
                                               AND object_id = OBJECT_ID(N'dbo.GstReturnFilings'))
BEGIN
    CREATE UNIQUE INDEX [UX_GstReturnFilings_Live]
        ON dbo.GstReturnFilings ([ReturnType], [PeriodKey], [CompanyGSTIN])
        WHERE [Status] <> 'Superseded' AND [IsDeleted] = 0;
    PRINT 'Created index UX_GstReturnFilings_Live';
END
GO

IF OBJECT_ID(N'dbo.GstReturnFilings', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GstReturnFilings_Period'
                                               AND object_id = OBJECT_ID(N'dbo.GstReturnFilings'))
BEGIN
    CREATE INDEX [IX_GstReturnFilings_Period]
        ON dbo.GstReturnFilings ([CompanyGSTIN], [PeriodKey], [ReturnType]) INCLUDE ([Status]);
    PRINT 'Created index IX_GstReturnFilings_Period';
END
GO

IF OBJECT_ID(N'dbo.GstReturnFilings', N'U') IS NOT NULL
    PRINT 'Phase 6 GST return schema is in place.';
GO
