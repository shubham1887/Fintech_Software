/*
    User change history + the fund minimum / maximum Key Settings.

    ── UserChangeHistorys ──────────────────────────────────────────────────────────────────────
    Every change the panel makes to a user record: what changed, from what to what, who did it,
    why, and from which IP / channel.

    The system already had three half-answers to that question and no whole one.
    UserActionHistorys records an action against a user but not WHO performed it, so it cannot
    distinguish "the retailer did this" from "an admin did this to the retailer".
    LastChangeUserDetailss covers onboarding drafts only, with no actor and no timestamp. The
    per-feature logs (WalletHoldHistorys, PasswordChangeHistorys) each answer for their own
    feature. So "who deactivated this retailer, when, and on whose instruction" was not
    answerable for any change made from the Users screen.

    Append-only by design: a correction is its own row, never an update, so the table reads as a
    ledger. One edit that touched several fields writes one row per field — that is what lets the
    report answer "what exactly changed" instead of "something changed".

    Nothing is cascaded from UserMasters: a user is soft-deleted (IsDeleted = 1), never removed,
    and their history has to outlive the deletion in any case — the deletion is itself one of the
    rows. There is therefore no foreign key, which also keeps the audit write from ever being the
    reason a legitimate change fails.

    ── FundMinAmount / FundMaxAmount ───────────────────────────────────────────────────────────
    Two SettingMasters rows that bound what a fund request or a fund transfer may carry. Each is
    honoured only while IsOn = 1 and Value is a positive number, so a row left at 0 means "no
    bound on that side" — the same rule ShowLimitInDMT follows. Seeded Off with 0, so an install
    that never touches them behaves exactly as before.

    Idempotent — safe to re-run.
*/

IF OBJECT_ID(N'dbo.UserChangeHistorys', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserChangeHistorys (
        [UserChangeHistoryID] INT            NOT NULL IDENTITY(1,1),
        [UserID]              INT            NOT NULL,
        [ChangedByUserID]     INT            NOT NULL,
        [Action]              VARCHAR(40)    NOT NULL,
        [FieldName]           VARCHAR(100)   NOT NULL CONSTRAINT DF_UserChangeHistorys_FieldName DEFAULT (''),
        [OldValue]            VARCHAR(1000)  NOT NULL CONSTRAINT DF_UserChangeHistorys_OldValue  DEFAULT (''),
        [NewValue]            VARCHAR(1000)  NOT NULL CONSTRAINT DF_UserChangeHistorys_NewValue  DEFAULT (''),
        [Reason]              VARCHAR(500)   NOT NULL CONSTRAINT DF_UserChangeHistorys_Reason    DEFAULT (''),
        [IPAddress]           VARCHAR(64)    NOT NULL CONSTRAINT DF_UserChangeHistorys_IPAddress DEFAULT (''),
        [Platform]            VARCHAR(20)    NOT NULL CONSTRAINT DF_UserChangeHistorys_Platform  DEFAULT (''),
        [DateTime]            DATETIME2      NOT NULL CONSTRAINT DF_UserChangeHistorys_DateTime  DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      DATETIME2      NOT NULL CONSTRAINT DF_UserChangeHistorys_UpdDate   DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           BIT            NOT NULL CONSTRAINT DF_UserChangeHistorys_IsDeleted DEFAULT (0),
        CONSTRAINT PK_UserChangeHistorys PRIMARY KEY CLUSTERED ([UserChangeHistoryID])
    );
END;
GO

-- The report is read two ways: everything done TO one user, and everything done BY one operator,
-- both newest-first within a date range. One index each; the table is append-only, so they cost
-- nothing worth measuring on the write side.
IF OBJECT_ID(N'dbo.UserChangeHistorys', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_UserChangeHistorys_UserID_DateTime'
                     AND object_id = OBJECT_ID(N'dbo.UserChangeHistorys'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserChangeHistorys_UserID_DateTime
        ON dbo.UserChangeHistorys ([UserID], [DateTime]);
END;
GO

IF OBJECT_ID(N'dbo.UserChangeHistorys', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_UserChangeHistorys_ChangedByUserID_DateTime'
                     AND object_id = OBJECT_ID(N'dbo.UserChangeHistorys'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_UserChangeHistorys_ChangedByUserID_DateTime
        ON dbo.UserChangeHistorys ([ChangedByUserID], [DateTime]);
END;
GO

-- Fund amount bounds. Inserted Off / 0 so nothing is restricted until the operator sets them on
-- Setting -> Key Setting. The Description is what that screen shows under each row.
IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'FundMinAmount')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], [Value], [IsOn], [IsSecure], [Description], [IsPassword], [DateTime], [UpdatedateTime], [IsDeleted])
    VALUES ('FundMinAmount', '0', 0, 0,
            'Smallest amount a fund request or fund transfer may carry. Off, or 0, means no minimum.',
            0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
END;
GO

IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'FundMaxAmount')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], [Value], [IsOn], [IsSecure], [Description], [IsPassword], [DateTime], [UpdatedateTime], [IsDeleted])
    VALUES ('FundMaxAmount', '0', 0, 0,
            'Largest amount a fund request or fund transfer may carry. Off, or 0, means no maximum.',
            0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
END;
GO
