/*
    Outstanding (cash collection) — the ledger behind "what this user owes their upline".

    The two tables already existed and are left where they are. What was missing was everything
    that makes the figure trustworthy:

      • a way back from a statement line to the fund movement that caused it. Without it the
        statement could say "credit 50,000" and nothing could answer "when did I take it, how did
        I take it" — which is the question the page exists for.

      • one row per user, enforced. Every read of a user's outstanding takes the FIRST row it
        finds, so a second row for the same user does not split the balance, it hides half of it
        and makes the figure change depending on which row was written last. Nothing stopped that
        from happening.

      • an index the statement can actually be read through. CashCollectionDetails grows by a row
        for every credit and every collection on the network, for ever, and the screen filters it
        by user, by counterparty and by date.

    Also seeds the feature switch and the three message templates, so a fresh install has the
    whole thing working rather than working except for the texts.
*/

-------------------------------------------------------------------------------
-- 1. Trace a statement line back to the fund movement behind it.
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.CashCollectionDetails', 'FundTransactionID') IS NULL
    ALTER TABLE dbo.CashCollectionDetails ADD FundTransactionID INT NULL;
GO

-- Which wallet the funds landed in. The debt itself is wallet-agnostic on purpose - recharge,
-- AEPS and DMT credit all roll into one balance, because the debt is to a person - but "which
-- wallet did I take it into" is the first thing anybody asks about a line on their statement.
IF COL_LENGTH('dbo.CashCollectionDetails', 'WalletID') IS NULL
    ALTER TABLE dbo.CashCollectionDetails ADD WalletID INT NULL;
GO

-------------------------------------------------------------------------------
-- 2. One outstanding row per user.
--
-- Deliberately NOT a blind CREATE UNIQUE INDEX: an install that already has duplicates would
-- fail the whole script and take the API's first boot with it. Duplicates are collapsed first -
-- the balances are SUMMED into the surviving row and the extras soft-deleted, because the money
-- in them is real and silently dropping it would understate a debt somebody is owed.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.CashCollectionMasters', N'U') IS NOT NULL
BEGIN
    DECLARE @dupes INT = 0;

    ;WITH ranked AS (
        SELECT CashCollectionMasterID, UserID,
               ROW_NUMBER() OVER (PARTITION BY UserID ORDER BY CashCollectionMasterID) AS rn
        FROM dbo.CashCollectionMasters
        WHERE IsDeleted = 0
    )
    SELECT @dupes = COUNT(*) FROM ranked WHERE rn > 1;

    IF @dupes > 0
    BEGIN
        -- Fold the extra rows' balances and the HIGHEST limit into the keeper.
        ;WITH ranked AS (
            SELECT CashCollectionMasterID, UserID, Outstanding, MaxOutstanding,
                   ROW_NUMBER() OVER (PARTITION BY UserID ORDER BY CashCollectionMasterID) AS rn
            FROM dbo.CashCollectionMasters
            WHERE IsDeleted = 0
        ),
        totals AS (
            SELECT UserID,
                   SUM(Outstanding) AS TotalOutstanding,
                   MAX(MaxOutstanding) AS TopLimit
            FROM ranked
            GROUP BY UserID
            HAVING COUNT(*) > 1
        )
        UPDATE m
        SET m.Outstanding   = t.TotalOutstanding,
            m.MaxOutstanding = t.TopLimit,
            m.UpdatedateTime = SYSUTCDATETIME()
        FROM dbo.CashCollectionMasters m
        JOIN totals t ON t.UserID = m.UserID
        WHERE m.IsDeleted = 0
          AND m.CashCollectionMasterID = (
                SELECT MIN(CashCollectionMasterID) FROM dbo.CashCollectionMasters
                WHERE UserID = m.UserID AND IsDeleted = 0);

        ;WITH ranked AS (
            SELECT CashCollectionMasterID,
                   ROW_NUMBER() OVER (PARTITION BY UserID ORDER BY CashCollectionMasterID) AS rn
            FROM dbo.CashCollectionMasters
            WHERE IsDeleted = 0
        )
        UPDATE m
        SET m.IsDeleted = 1, m.UpdatedateTime = SYSUTCDATETIME()
        FROM dbo.CashCollectionMasters m
        JOIN ranked r ON r.CashCollectionMasterID = m.CashCollectionMasterID
        WHERE r.rn > 1;

        PRINT 'Outstanding: collapsed ' + CAST(@dupes AS varchar(10))
            + ' duplicate cash-collection row(s); their balances were summed into the row that was kept.';
    END
END
GO

-- Filtered, so the constraint applies to live rows only and a soft-deleted duplicate can sit
-- beside its replacement for ever without blocking anything.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CashCollectionMasters_UserID_Live'
               AND object_id = OBJECT_ID('dbo.CashCollectionMasters'))
    CREATE UNIQUE INDEX UX_CashCollectionMasters_UserID_Live
        ON dbo.CashCollectionMasters (UserID)
        WHERE IsDeleted = 0;
GO

-------------------------------------------------------------------------------
-- 3. The index the statement is read through.
--
-- Column order follows the query: pinned to a user, usually narrowed to one counterparty, then
-- ordered newest-first by date. DateTime is in the key rather than included so the ORDER BY is
-- served by the index instead of a sort over the whole user's history.
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CashCollectionDetails_User_Parent_Date'
               AND object_id = OBJECT_ID('dbo.CashCollectionDetails'))
    CREATE INDEX IX_CashCollectionDetails_User_Parent_Date
        ON dbo.CashCollectionDetails (UserID, ParentID, [DateTime] DESC)
        INCLUDE (OutstandingOpening, OutstandingAmount, OutstandingClosing, Credit, Debit,
                 [Type], PayMode, UTRNumber, ParentRemark, FundTransactionID, WalletID);
GO

-- The daily reminder sweep and the Outstanding tile both ask "who has a balance above zero".
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CashCollectionMasters_Outstanding'
               AND object_id = OBJECT_ID('dbo.CashCollectionMasters'))
    CREATE INDEX IX_CashCollectionMasters_Outstanding
        ON dbo.CashCollectionMasters (Outstanding)
        INCLUDE (UserID, MaxOutstanding)
        WHERE IsDeleted = 0;
GO

-------------------------------------------------------------------------------
-- 4. The feature switch.
--
-- Seeded ON, because the tables already exist and installs may already be carrying balances;
-- a switch that defaulted to off would make existing debts vanish from every screen on upgrade.
-- Turning it off stops NEW credit and stops the morning reminders; it does not hide or forgive
-- what is already owed.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'CashCollection')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('CashCollection', 'true', 1, 0,
         'Cash collection / outstanding. When ON, a fund request or transfer sent with the Credit payment mode funds the wallet AND records the amount as outstanding against the receiver, and everyone carrying a balance gets a daily reminder. When OFF, no new credit can be taken; balances already recorded stay visible and collectable.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: CashCollection (ON).';
END
GO

-------------------------------------------------------------------------------
-- 5. Message templates for the three outstanding events.
--
-- Matched the same way the message service matches at run time - case and punctuation
-- insensitive - so an install whose operator has already renamed or edited one of these keeps
-- their wording instead of gaining a second, competing row.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.MessageSettings', N'U') IS NOT NULL
BEGIN
    DECLARE @seed TABLE ([Type] varchar(200), IsSMS bit, IsWhatsUp bit, IsOTP bit,
                         SmsMessage nvarchar(max), WhatsUpMessage nvarchar(max));

    INSERT INTO @seed ([Type], IsSMS, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage) VALUES
    ('Outstanding Credited', 1, 1, 0,
     'Rs.{Amount} credit given by {ParentName}. Your total outstanding is now Rs.{Outstanding}. - {AppName}',
     '*Credit given*\n\nAmount: *Rs.{Amount}*\nFrom: {ParentName}\nDate: {DateTime}\nRemark: {Remark}\n\nTotal outstanding: *Rs.{Outstanding}*\n\n- {AppName}'),

    ('Outstanding Debited', 1, 1, 0,
     'Rs.{Amount} received against your outstanding. Balance outstanding Rs.{Outstanding}. - {AppName}',
     '*Payment received*\n\nAmount: *Rs.{Amount}*\nReceived by: {ParentName}\nMode: {PayMode}\nDate: {DateTime}\n\nBalance outstanding: *Rs.{Outstanding}*\n\nKeep this message as your receipt.\n\n- {AppName}'),

    ('Outstanding Reminder', 1, 1, 0,
     'Reminder: Rs.{Outstanding} is outstanding on your account with {ParentName}. Please clear it at the earliest. - {AppName}',
     '*Outstanding reminder*\n\nAmount outstanding: *Rs.{Outstanding}*\nPayable to: {ParentName}\n\nPlease clear your outstanding at the earliest to keep your credit limit available.\n\n- {AppName}');

    DECLARE @inserted INT = 0;

    INSERT INTO dbo.MessageSettings
        ([Type], IsSMS, IsMail, IsWhatsUp, WhatsUpMessage, SmsMessage, EmailMessage, TemplateID,
         IsOTP, [DateTime], UpdatedateTime, IsDeleted)
    SELECT s.[Type], s.IsSMS, 0, s.IsWhatsUp, s.WhatsUpMessage, s.SmsMessage, s.SmsMessage, '',
           s.IsOTP, SYSUTCDATETIME(), SYSUTCDATETIME(), 0
    FROM @seed s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.MessageSettings ms
        WHERE ms.IsDeleted = 0
          AND LOWER(REPLACE(REPLACE(REPLACE(ms.[Type], ' ', ''), '-', ''), '_', ''))
            = LOWER(REPLACE(REPLACE(REPLACE(s.[Type],  ' ', ''), '-', ''), '_', '')));

    SET @inserted = @@ROWCOUNT;
    PRINT 'Outstanding message templates: ' + CAST(@inserted AS varchar(10)) + ' event(s) added.';

    IF @inserted > 0
    BEGIN
        PRINT 'ACTION REQUIRED: paste the registered DLT content-template id for each new event into';
        PRINT '  Settings -> Message Settings -> Template ID. Indian SMS gateways accept the API';
        PRINT '  call without it and then do not deliver.';
    END
END
GO

-------------------------------------------------------------------------------
-- 6. Make sure the Credit payment mode exists.
--
-- It is seeded by 40_seed_fund_mode_submode.sql, but the whole feature hangs off a Mode row
-- whose NAME is "Credit" - that is what the fund flows match on, because the id differs per
-- install - so it is worth being certain rather than leaving the feature quietly unreachable.
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.Modes', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.Modes WHERE ModeName = 'Credit' AND IsDelete = 0)
BEGIN
    INSERT INTO dbo.Modes (ModeName, IsActive, IsDelete, DisplayOrder, CreatedDate, CreatedBy, UpdatedBy)
    VALUES ('Credit', 1, 0, 5, SYSDATETIME(), 1, 0);
    PRINT 'Inserted Mode: Credit (the payment mode that puts a fund request onto outstanding).';
END
GO
