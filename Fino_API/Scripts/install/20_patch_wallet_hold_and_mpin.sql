/* ============================================================================
   Fino — all pending schema changes, in one script.

   Run this ONCE against the application database
   (ConnectionStrings:DefaultConnection). Every statement is idempotent — running
   the whole file again is safe and does nothing the second time.

   Covers:
     1. WalletHoldHistories  — the Add Hold Balance / Hold Report feature.
     2. UserMasters MPIN cols + IsMpinEnabled Key Setting — the MPIN feature.
   ============================================================================ */

-------------------------------------------------------------------------------
-- 1) WalletHoldHistories — audit behind the hold report.
--    A hold freezes balance a user can see but cannot spend; this records who
--    froze it, when, on which wallet, how much, and why. Rows are append-only
--    (a release is its own row with a negative Amount).
-------------------------------------------------------------------------------
IF OBJECT_ID(N'[WalletHoldHistories]', N'U') IS NULL
BEGIN
    CREATE TABLE [WalletHoldHistories] (
        [WalletHoldHistoryID] int NOT NULL IDENTITY,
        [UserID]              int NOT NULL,
        [WalletID]            int NOT NULL,
        -- Positive = placed, negative = released.
        [Amount]              decimal(18,5) NOT NULL,
        -- Total held in that wallet after this change.
        [HoldBalanceAfter]    decimal(18,5) NOT NULL,
        -- The wallet's spendable balance at the time, for context when reading the row back.
        [WalletBalance]       decimal(18,5) NOT NULL,
        [ActionByUserID]      int NOT NULL,
        -- HOLD or RELEASE.
        [Action]              varchar(20) NOT NULL,
        -- Why. The question this table exists to answer.
        [Remark]              varchar(500) NOT NULL,
        [DateTime]            datetime2 NOT NULL,
        [UpdatedateTime]      datetime2 NOT NULL,
        [IsDeleted]           bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_WalletHoldHistories] PRIMARY KEY ([WalletHoldHistoryID])
    );

    -- The report filters by user and by date, in that order.
    CREATE INDEX [IX_WalletHoldHistories_UserID_DateTime]
        ON [WalletHoldHistories] ([UserID], [DateTime] DESC);

    PRINT 'WalletHoldHistories created.';
END
ELSE
    PRINT 'WalletHoldHistories already exists - skipped.';
GO

-------------------------------------------------------------------------------
-- 2a) UserMasters — hashed MPIN storage.
--     The transaction / app-open PIN, stored hashed (never the digits). The
--     legacy int Mpin column is left untouched; MpinHash is the source of truth.
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.UserMasters', 'MpinHash') IS NULL
BEGIN
    ALTER TABLE dbo.UserMasters
        ADD MpinHash varchar(128) NOT NULL CONSTRAINT DF_UserMasters_MpinHash DEFAULT('');
    PRINT 'UserMasters.MpinHash added.';
END
ELSE
    PRINT 'UserMasters.MpinHash already exists - skipped.';
GO

-- 2b) UserMasters — the user's own MPIN opt-in flag.
IF COL_LENGTH('dbo.UserMasters', 'IsMpinEnabled') IS NULL
BEGIN
    ALTER TABLE dbo.UserMasters
        ADD IsMpinEnabled bit NOT NULL CONSTRAINT DF_UserMasters_IsMpinEnabled DEFAULT(0);
    PRINT 'UserMasters.IsMpinEnabled added.';
END
ELSE
    PRINT 'UserMasters.IsMpinEnabled already exists - skipped.';
GO

-- 2c) Global "Require MPIN" toggle shown in Key Setting. OFF by default: nobody
--     is forced to set an MPIN until the admin turns this on (a user can still
--     opt in for themselves via Account Security -> Change MPIN).
IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'IsMpinEnabled')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('IsMpinEnabled', 'false', 0, 0,
         'Require an MPIN for transactions and on opening the app (true/false).',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'SettingMaster IsMpinEnabled seeded (off).';
END
ELSE
    PRINT 'SettingMaster IsMpinEnabled already exists - skipped.';
GO

PRINT 'All pending migrations applied.';
GO
