/*
    64 - Phase 4: the double-entry ledger.

    WHY THIS EXISTS
    The wallets and the provider stock are operational balances; they are not books. "18 crore
    against 19 crore" was two running totals nobody could tie to a third, independent record. A
    ledger is that third record: every movement posts a balanced voucher, and a daily check proves
    the ledger still agrees with the wallets and the provider stock. When it stops agreeing, the
    day it stops is visible instead of a year's drift.

    THE PIECES
      - AcctAccounts        the chart of accounts (seeded by 65)
      - AcctPostingRules    event -> which accounts to debit and credit (seeded by 65)
      - AcctPostingOutbox   events waiting to post; a failure is a visible row, never silent
      - AcctVouchers / Lines the balanced postings themselves
      - AcctPeriodLocks     a closed month no voucher may post into

    CONTROLS (unique indexes), each in its own guarded block so a failure after a CREATE TABLE
    cannot leave a table permanently un-indexed:
      - one account per Code;
      - one outbox row per EventKey, so a producer's retry cannot double-post;
      - one lock per period.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does. Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

/* ── 1. Chart of accounts ───────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.AcctAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctAccounts (
        [AcctAccountID]   INT          NOT NULL IDENTITY(1,1),
        [Code]            VARCHAR(20)  NOT NULL,
        [Name]            VARCHAR(150) NOT NULL,
        [AccountType]     VARCHAR(20)  NOT NULL,
        [NormalBalance]   VARCHAR(2)   NOT NULL CONSTRAINT DF_AcctAccounts_Normal DEFAULT ('Dr'),
        [IsControl]       BIT          NOT NULL CONSTRAINT DF_AcctAccounts_Control DEFAULT (0),
        [ParentAccountID] INT          NULL,
        [IsActive]        BIT          NOT NULL CONSTRAINT DF_AcctAccounts_Active DEFAULT (1),
        [DateTime]        DATETIME2    NOT NULL CONSTRAINT DF_AcctAccounts_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]  DATETIME2    NOT NULL CONSTRAINT DF_AcctAccounts_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]       BIT          NOT NULL CONSTRAINT DF_AcctAccounts_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctAccounts] PRIMARY KEY ([AcctAccountID])
    );
    PRINT 'Created table dbo.AcctAccounts';
END
GO
IF OBJECT_ID(N'dbo.AcctAccounts', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AcctAccounts_Code' AND object_id = OBJECT_ID(N'dbo.AcctAccounts'))
BEGIN
    CREATE UNIQUE INDEX UX_AcctAccounts_Code ON dbo.AcctAccounts (Code) WHERE IsDeleted = 0;
    PRINT 'Created index UX_AcctAccounts_Code';
END
GO

/* ── 2. Posting rules ───────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.AcctPostingRules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctPostingRules (
        [AcctPostingRuleID] INT         NOT NULL IDENTITY(1,1),
        [EventType]         VARCHAR(40) NOT NULL,
        [LineNo]            INT         NOT NULL CONSTRAINT DF_AcctRules_LineNo DEFAULT (1),
        [DebitAccountCode]  VARCHAR(20) NOT NULL,
        [CreditAccountCode] VARCHAR(20) NOT NULL,
        [AmountField]       VARCHAR(40) NOT NULL,
        [Narration]         VARCHAR(200) NOT NULL CONSTRAINT DF_AcctRules_Narration DEFAULT (''),
        [IsActive]          BIT         NOT NULL CONSTRAINT DF_AcctRules_Active DEFAULT (1),
        [DateTime]          DATETIME2   NOT NULL CONSTRAINT DF_AcctRules_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]    DATETIME2   NOT NULL CONSTRAINT DF_AcctRules_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]         BIT         NOT NULL CONSTRAINT DF_AcctRules_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctPostingRules] PRIMARY KEY ([AcctPostingRuleID])
    );
    PRINT 'Created table dbo.AcctPostingRules';
END
GO
IF OBJECT_ID(N'dbo.AcctPostingRules', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AcctPostingRules_Event' AND object_id = OBJECT_ID(N'dbo.AcctPostingRules'))
BEGIN
    CREATE INDEX IX_AcctPostingRules_Event ON dbo.AcctPostingRules (EventType, [LineNo]);
    PRINT 'Created index IX_AcctPostingRules_Event';
END
GO

/* ── 3. Posting outbox ──────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.AcctPostingOutbox', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctPostingOutbox (
        [AcctPostingOutboxID] INT          NOT NULL IDENTITY(1,1),
        [EventType]           VARCHAR(40)  NOT NULL,
        [EventKey]            VARCHAR(100) NOT NULL,
        [EventDate]           DATETIME2    NOT NULL,
        [PayloadJson]         VARCHAR(MAX) NOT NULL CONSTRAINT DF_AcctOutbox_Payload DEFAULT ('{}'),
        [Narration]           VARCHAR(300) NOT NULL CONSTRAINT DF_AcctOutbox_Narration DEFAULT (''),
        [Status]              VARCHAR(20)  NOT NULL CONSTRAINT DF_AcctOutbox_Status DEFAULT ('Pending'),
        [AttemptCount]        INT          NOT NULL CONSTRAINT DF_AcctOutbox_Attempts DEFAULT (0),
        [LastAttemptOn]       DATETIME2    NULL,
        [LastError]           VARCHAR(1000) NOT NULL CONSTRAINT DF_AcctOutbox_Error DEFAULT (''),
        [PostedVoucherID]     INT          NOT NULL CONSTRAINT DF_AcctOutbox_Voucher DEFAULT (0),
        [DateTime]            DATETIME2    NOT NULL CONSTRAINT DF_AcctOutbox_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2    NOT NULL CONSTRAINT DF_AcctOutbox_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT          NOT NULL CONSTRAINT DF_AcctOutbox_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctPostingOutbox] PRIMARY KEY ([AcctPostingOutboxID])
    );
    PRINT 'Created table dbo.AcctPostingOutbox';
END
GO
-- The idempotency control: one row per real-world event, so a producer retry cannot double-post.
IF OBJECT_ID(N'dbo.AcctPostingOutbox', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AcctPostingOutbox_EventKey' AND object_id = OBJECT_ID(N'dbo.AcctPostingOutbox'))
BEGIN
    CREATE UNIQUE INDEX UX_AcctPostingOutbox_EventKey ON dbo.AcctPostingOutbox (EventKey) WHERE IsDeleted = 0;
    PRINT 'Created index UX_AcctPostingOutbox_EventKey';
END
GO
-- The engine pulls Pending (and retryable Failed) oldest-first.
IF OBJECT_ID(N'dbo.AcctPostingOutbox', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AcctPostingOutbox_Status' AND object_id = OBJECT_ID(N'dbo.AcctPostingOutbox'))
BEGIN
    CREATE INDEX IX_AcctPostingOutbox_Status ON dbo.AcctPostingOutbox (Status, AcctPostingOutboxID);
    PRINT 'Created index IX_AcctPostingOutbox_Status';
END
GO

/* ── 4. Vouchers and lines ──────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.AcctVouchers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctVouchers (
        [AcctVoucherID]       INT          NOT NULL IDENTITY(1,1),
        [VoucherNo]           VARCHAR(30)  NOT NULL CONSTRAINT DF_AcctVouchers_No DEFAULT (''),
        [VoucherDate]         DATETIME2    NOT NULL,
        [PeriodKey]           VARCHAR(8)   NOT NULL,
        [VoucherType]         VARCHAR(40)  NOT NULL,
        [Narration]           VARCHAR(300) NOT NULL CONSTRAINT DF_AcctVouchers_Narration DEFAULT (''),
        [SourceOutboxID]      INT          NOT NULL CONSTRAINT DF_AcctVouchers_Outbox DEFAULT (0),
        [TotalDebit]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_AcctVouchers_Dr DEFAULT (0),
        [TotalCredit]         DECIMAL(18,2) NOT NULL CONSTRAINT DF_AcctVouchers_Cr DEFAULT (0),
        [Status]              VARCHAR(20)  NOT NULL CONSTRAINT DF_AcctVouchers_Status DEFAULT ('Posted'),
        [ReversalOfVoucherID] INT          NULL,
        [CreatedByUserID]     INT          NOT NULL CONSTRAINT DF_AcctVouchers_By DEFAULT (0),
        [DateTime]            DATETIME2    NOT NULL CONSTRAINT DF_AcctVouchers_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2    NOT NULL CONSTRAINT DF_AcctVouchers_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT          NOT NULL CONSTRAINT DF_AcctVouchers_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctVouchers] PRIMARY KEY ([AcctVoucherID])
    );
    PRINT 'Created table dbo.AcctVouchers';
END
GO
IF OBJECT_ID(N'dbo.AcctVouchers', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AcctVouchers_Period' AND object_id = OBJECT_ID(N'dbo.AcctVouchers'))
BEGIN
    CREATE INDEX IX_AcctVouchers_Period ON dbo.AcctVouchers (PeriodKey, Status);
    PRINT 'Created index IX_AcctVouchers_Period';
END
GO

IF OBJECT_ID(N'dbo.AcctVoucherLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctVoucherLines (
        [AcctVoucherLineID] INT          NOT NULL IDENTITY(1,1),
        [AcctVoucherID]     INT          NOT NULL,
        [AcctAccountID]     INT          NOT NULL,
        [AccountCode]       VARCHAR(20)  NOT NULL CONSTRAINT DF_AcctLines_Code DEFAULT (''),
        [LineNo]            INT          NOT NULL CONSTRAINT DF_AcctLines_LineNo DEFAULT (1),
        [Debit]             DECIMAL(18,2) NOT NULL CONSTRAINT DF_AcctLines_Dr DEFAULT (0),
        [Credit]            DECIMAL(18,2) NOT NULL CONSTRAINT DF_AcctLines_Cr DEFAULT (0),
        [Narration]         VARCHAR(300) NOT NULL CONSTRAINT DF_AcctLines_Narration DEFAULT (''),
        [DateTime]          DATETIME2    NOT NULL CONSTRAINT DF_AcctLines_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]    DATETIME2    NOT NULL CONSTRAINT DF_AcctLines_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]         BIT          NOT NULL CONSTRAINT DF_AcctLines_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctVoucherLines] PRIMARY KEY ([AcctVoucherLineID]),
        CONSTRAINT [FK_AcctVoucherLines_Voucher] FOREIGN KEY ([AcctVoucherID]) REFERENCES dbo.AcctVouchers ([AcctVoucherID])
    );
    PRINT 'Created table dbo.AcctVoucherLines';
END
GO
IF OBJECT_ID(N'dbo.AcctVoucherLines', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AcctVoucherLines_Account' AND object_id = OBJECT_ID(N'dbo.AcctVoucherLines'))
BEGIN
    CREATE INDEX IX_AcctVoucherLines_Account ON dbo.AcctVoucherLines (AcctAccountID, AcctVoucherID);
    PRINT 'Created index IX_AcctVoucherLines_Account';
END
GO

/* ── 5. Period locks ────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.AcctPeriodLocks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AcctPeriodLocks (
        [AcctPeriodLockID] INT          NOT NULL IDENTITY(1,1),
        [PeriodKey]        VARCHAR(8)   NOT NULL,
        [LockedOn]         DATETIME2    NOT NULL,
        [LockedByUserID]   INT          NOT NULL CONSTRAINT DF_AcctLocks_By DEFAULT (0),
        [Note]             VARCHAR(300) NOT NULL CONSTRAINT DF_AcctLocks_Note DEFAULT (''),
        [DateTime]         DATETIME2    NOT NULL CONSTRAINT DF_AcctLocks_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]   DATETIME2    NOT NULL CONSTRAINT DF_AcctLocks_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]        BIT          NOT NULL CONSTRAINT DF_AcctLocks_Deleted DEFAULT (0),
        CONSTRAINT [PK_AcctPeriodLocks] PRIMARY KEY ([AcctPeriodLockID])
    );
    PRINT 'Created table dbo.AcctPeriodLocks';
END
GO
IF OBJECT_ID(N'dbo.AcctPeriodLocks', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_AcctPeriodLocks_Period' AND object_id = OBJECT_ID(N'dbo.AcctPeriodLocks'))
BEGIN
    CREATE UNIQUE INDEX UX_AcctPeriodLocks_Period ON dbo.AcctPeriodLocks (PeriodKey) WHERE IsDeleted = 0;
    PRINT 'Created index UX_AcctPeriodLocks_Period';
END
GO

PRINT 'Phase 4 accounting ledger schema is in place.';
