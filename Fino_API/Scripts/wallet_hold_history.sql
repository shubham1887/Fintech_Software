/*
  WalletHoldHistories — the audit behind the hold report.

  A hold freezes balance a user can see but cannot spend, so "who froze it, when, on which wallet,
  and why" has to be answerable afterwards. Until this table the hold was a bare number on
  UserWalletMasters and the remark the admin typed was discarded.

  Rows are append-only: a release is its own row with a negative Amount, so the history reads as a
  ledger rather than being overwritten.

  Shipped as a script rather than an EF migration deliberately — the model snapshot in this repo has
  drifted from the entities, so `dotnet ef migrations add` would bundle unrelated column changes into
  the same migration. This creates exactly the one table, and is safe to run more than once.

  Run against the application database (the one in ConnectionStrings:DefaultConnection).
*/

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
BEGIN
    PRINT 'WalletHoldHistories already exists — nothing to do.';
END
GO
