/*
    66 - Phase 5: bank statement import and bank-vs-books reconciliation.

    WHY THIS EXISTS
    Phase 4 gave the ledger a Bank account (1000) that every real movement of money is supposed to
    pass through. Nothing has ever proved that it does. A bank reconciliation is the proof: it puts
    the bank's own statement beside the ledger's Bank lines and sorts every movement into four
    buckets, exactly as the 2B reconciliation does for purchases.

    The bucket that earns the feature is MissingInBooks - money that actually moved and that the
    books know nothing about. Bank charges, interest credited, a direct transfer nobody recorded:
    all of it is invisible until a statement is compared, and all of it is real.

    FOUR TABLES, AND WHY EACH IS SEPARATE

      1. dbo.BankAccounts       - the real bank accounts we hold, each mapped to the LEDGER account
                                  code its movements post to (1000 by default). The ledger has one
                                  "Bank" account today; the day a second current account is opened,
                                  this mapping is the only thing that has to change, instead of
                                  every statement suddenly reconciling against the wrong balance.
      2. dbo.BankStatementImports - one upload. Active / Superseded, like a 2B snapshot: a bank can
                                  restate a line after it clears, so a re-upload REPLACES rather
                                  than merges, and the old one stays readable for history.
      3. dbo.BankStatementLines - the statement rows as the bank gave them, plus where the match
                                  got to. MatchStatus is denormalised onto the line so the register
                                  can filter without joining.
      4. dbo.BankReconMatches   - the pairing itself, bank line <-> ledger voucher line. A table of
                                  its own, NOT a column on either side, for one reason: a ledger
                                  line has to stay known-reconciled across future runs and future
                                  statements. Stamping only the bank line would make every past
                                  payment reappear as unreconciled the next time a statement is
                                  uploaded for a later month.

    THE DIRECTION RULE, which is the thing most easily got backwards:
    a bank statement CREDIT is money arriving in our account, and in the ledger that is a DEBIT to
    Bank (an asset rising). A statement DEBIT is a ledger CREDIT. The service matches on signed
    amount, so a sign error here would pair every receipt with a payment of the same size.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does, and an INSERT that omits them passes every dev test and
    fails in production with "Cannot insert the value NULL".

    The unique indexes are controls, not tidiness:
      - one active import per bank account per date range, so two uploads cannot both be live;
      - one match per bank line and one per ledger line, so a single movement cannot be
        reconciled twice and make both sides look complete.
    Each is created in its OWN guarded block, never inside the CREATE TABLE's IF - a failure after
    the table commits would otherwise leave a permanently un-indexed table the re-run skips.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
-- Required for the FILTERED unique indexes below, and for any later INSERT/UPDATE on these tables.
SET QUOTED_IDENTIFIER ON;

/* ── 1. The bank accounts we reconcile ──────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.BankAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankAccounts (
        [BankAccountID]    INT          NOT NULL IDENTITY(1,1),
        [AccountName]      VARCHAR(150) NOT NULL,
        [BankName]         VARCHAR(150) NOT NULL CONSTRAINT DF_BankAccounts_BankName DEFAULT (''),
        [AccountNumber]    VARCHAR(40)  NOT NULL CONSTRAINT DF_BankAccounts_AccountNumber DEFAULT (''),
        [IFSC]             VARCHAR(15)  NOT NULL CONSTRAINT DF_BankAccounts_IFSC DEFAULT (''),
        -- The LEDGER account its movements post to. Code, never id: codes are stable across
        -- installs, ids are not (the same rule the posting rules follow).
        [LedgerAccountCode] VARCHAR(20) NOT NULL CONSTRAINT DF_BankAccounts_LedgerAccountCode DEFAULT ('1000'),
        [IsActive]         BIT          NOT NULL CONSTRAINT DF_BankAccounts_IsActive DEFAULT (1),
        [DateTime]         DATETIME2    NOT NULL CONSTRAINT DF_BankAccounts_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]   DATETIME2    NOT NULL CONSTRAINT DF_BankAccounts_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]        BIT          NOT NULL CONSTRAINT DF_BankAccounts_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_BankAccounts] PRIMARY KEY ([BankAccountID])
    );
    PRINT 'Created table dbo.BankAccounts';
END
GO

/* ── 2. One statement upload ────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.BankStatementImports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankStatementImports (
        [BankStatementImportID] INT          NOT NULL IDENTITY(1,1),
        [BankAccountID]      INT          NOT NULL,
        [PeriodFrom]         DATE         NOT NULL,
        [PeriodTo]           DATE         NOT NULL,
        [FileName]           VARCHAR(260) NOT NULL CONSTRAINT DF_BankStatementImports_FileName DEFAULT (''),
        -- CSV or XLSX. Kept so an odd parse can be traced to the format it came from.
        [Source]             VARCHAR(20)  NOT NULL CONSTRAINT DF_BankStatementImports_Source DEFAULT ('CSV'),
        [LineCount]          INT          NOT NULL CONSTRAINT DF_BankStatementImports_LineCount DEFAULT (0),
        [TotalDebit]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankStatementImports_TotalDebit DEFAULT (0),
        [TotalCredit]        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankStatementImports_TotalCredit DEFAULT (0),
        -- The bank's own closing balance, when the file carried a running balance. Worth storing:
        -- it is the one figure that proves no line was dropped by the parser.
        [ClosingBalance]     DECIMAL(18,2) NULL,
        [Status]             VARCHAR(20)  NOT NULL CONSTRAINT DF_BankStatementImports_Status DEFAULT ('Active'),
        [ImportedByUserID]   INT          NOT NULL CONSTRAINT DF_BankStatementImports_ImportedBy DEFAULT (0),
        [DateTime]           DATETIME2    NOT NULL CONSTRAINT DF_BankStatementImports_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2    NOT NULL CONSTRAINT DF_BankStatementImports_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT          NOT NULL CONSTRAINT DF_BankStatementImports_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_BankStatementImports] PRIMARY KEY ([BankStatementImportID])
    );
    PRINT 'Created table dbo.BankStatementImports';
END
GO

-- At most one ACTIVE import per account per exact date range. Filtered on Status so a superseded
-- upload stays for history without blocking the replacement.
IF OBJECT_ID(N'dbo.BankStatementImports', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BankStatementImports_ActiveRange'
                                               AND object_id = OBJECT_ID(N'dbo.BankStatementImports'))
BEGIN
    CREATE UNIQUE INDEX [UX_BankStatementImports_ActiveRange]
        ON dbo.BankStatementImports ([BankAccountID], [PeriodFrom], [PeriodTo])
        WHERE [Status] = 'Active' AND [IsDeleted] = 0;
    PRINT 'Created index UX_BankStatementImports_ActiveRange';
END
GO

/* ── 3. The statement lines ─────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.BankStatementLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankStatementLines (
        [BankStatementLineID] INT          NOT NULL IDENTITY(1,1),
        [BankStatementImportID] INT        NOT NULL,
        [BankAccountID]      INT          NOT NULL,
        [TxnDate]            DATE         NOT NULL,
        [ValueDate]          DATE         NULL,
        [Description]        VARCHAR(500) NOT NULL CONSTRAINT DF_BankStatementLines_Description DEFAULT (''),
        -- UTR / cheque number / RRN as the bank printed it, and the same stripped to letters and
        -- digits. The normalised copy is what the match keys on, so "UTR: SBIN-123" and
        -- "SBIN123" are one reference rather than two.
        [ReferenceNo]        VARCHAR(100) NOT NULL CONSTRAINT DF_BankStatementLines_ReferenceNo DEFAULT (''),
        [ReferenceKey]       VARCHAR(100) NOT NULL CONSTRAINT DF_BankStatementLines_ReferenceKey DEFAULT (''),
        [Debit]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankStatementLines_Debit DEFAULT (0),
        [Credit]             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankStatementLines_Credit DEFAULT (0),
        [RunningBalance]     DECIMAL(18,2) NULL,
        [RowNo]              INT          NOT NULL CONSTRAINT DF_BankStatementLines_RowNo DEFAULT (0),
        -- Denormalised from BankReconMatches so the register filters without a join.
        [MatchStatus]        VARCHAR(20)  NOT NULL CONSTRAINT DF_BankStatementLines_MatchStatus DEFAULT ('Unmatched'),
        [Variance]           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankStatementLines_Variance DEFAULT (0),
        [DateTime]           DATETIME2    NOT NULL CONSTRAINT DF_BankStatementLines_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2    NOT NULL CONSTRAINT DF_BankStatementLines_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT          NOT NULL CONSTRAINT DF_BankStatementLines_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_BankStatementLines] PRIMARY KEY ([BankStatementLineID])
    );
    PRINT 'Created table dbo.BankStatementLines';
END
GO

IF OBJECT_ID(N'dbo.BankStatementLines', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankStatementLines_Import'
                                               AND object_id = OBJECT_ID(N'dbo.BankStatementLines'))
BEGIN
    CREATE INDEX [IX_BankStatementLines_Import]
        ON dbo.BankStatementLines ([BankStatementImportID], [MatchStatus]) INCLUDE ([TxnDate], [Debit], [Credit]);
    PRINT 'Created index IX_BankStatementLines_Import';
END
GO

IF OBJECT_ID(N'dbo.BankStatementLines', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankStatementLines_RefKey'
                                               AND object_id = OBJECT_ID(N'dbo.BankStatementLines'))
BEGIN
    CREATE INDEX [IX_BankStatementLines_RefKey]
        ON dbo.BankStatementLines ([BankAccountID], [ReferenceKey]);
    PRINT 'Created index IX_BankStatementLines_RefKey';
END
GO

/* ── 4. The pairing ─────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.BankReconMatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BankReconMatches (
        [BankReconMatchID]   INT          NOT NULL IDENTITY(1,1),
        [BankStatementLineID] INT         NOT NULL,
        [AcctVoucherLineID]  INT          NOT NULL,
        [BankAccountID]      INT          NOT NULL,
        -- Reference / AmountAndDate / Manual. Recorded so a reviewer can tell a strong pairing
        -- from a circumstantial one without re-running the match.
        [Method]             VARCHAR(20)  NOT NULL CONSTRAINT DF_BankReconMatches_Method DEFAULT ('Reference'),
        [Variance]           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BankReconMatches_Variance DEFAULT (0),
        [MatchedByUserID]    INT          NOT NULL CONSTRAINT DF_BankReconMatches_MatchedBy DEFAULT (0),
        [DateTime]           DATETIME2    NOT NULL CONSTRAINT DF_BankReconMatches_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2    NOT NULL CONSTRAINT DF_BankReconMatches_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT          NOT NULL CONSTRAINT DF_BankReconMatches_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_BankReconMatches] PRIMARY KEY ([BankReconMatchID])
    );
    PRINT 'Created table dbo.BankReconMatches';
END
GO

-- One live match per bank line, and one per ledger line. Without these a movement could be
-- reconciled twice and both sides would look complete while the difference stayed real.
IF OBJECT_ID(N'dbo.BankReconMatches', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BankReconMatches_StatementLine'
                                               AND object_id = OBJECT_ID(N'dbo.BankReconMatches'))
BEGIN
    CREATE UNIQUE INDEX [UX_BankReconMatches_StatementLine]
        ON dbo.BankReconMatches ([BankStatementLineID]) WHERE [IsDeleted] = 0;
    PRINT 'Created index UX_BankReconMatches_StatementLine';
END
GO

IF OBJECT_ID(N'dbo.BankReconMatches', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BankReconMatches_VoucherLine'
                                               AND object_id = OBJECT_ID(N'dbo.BankReconMatches'))
BEGIN
    CREATE UNIQUE INDEX [UX_BankReconMatches_VoucherLine]
        ON dbo.BankReconMatches ([AcctVoucherLineID]) WHERE [IsDeleted] = 0;
    PRINT 'Created index UX_BankReconMatches_VoucherLine';
END
GO

/* ── 5. A default bank account, so the screen is usable before anyone configures anything ── */
-- Mapped to ledger 1000, which is the only Bank account the seeded chart has. The admin renames it
-- and fills the account number on the screen; a second account is added there too.
IF OBJECT_ID(N'dbo.BankAccounts', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.BankAccounts WHERE [IsDeleted] = 0)
BEGIN
    INSERT INTO dbo.BankAccounts ([AccountName], [BankName], [AccountNumber], [IFSC], [LedgerAccountCode])
    VALUES ('Primary current account', '', '', '', '1000');
    PRINT 'Seeded the default bank account (ledger 1000)';
END
GO

IF OBJECT_ID(N'dbo.BankAccounts', N'U') IS NOT NULL
    PRINT 'Phase 5 bank reconciliation schema is in place.';
GO
