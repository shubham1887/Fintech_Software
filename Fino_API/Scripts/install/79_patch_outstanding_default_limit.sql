/*
    Three additions to the outstanding feature, all driven by how it behaved in practice.

    1. A DEFAULT credit limit, as a Key Setting.

       Every account starts at MaxOutstanding = 0, and 0 means "no credit at all". That is the safe
       default, but it made the feature unusable out of the box: every Credit-mode fund transfer was
       refused until an admin had visited each account one at a time. A default limit makes the
       feature work the moment it is switched on, while a per-account limit still overrides it.

       Which forces a distinction the single column could not make: "nobody has set a limit for
       this account, so use the default" versus "an admin has deliberately set this account to
       zero, so it gets NO credit even though a default exists". Hence HasExplicitLimit - without
       it, deliberately blocking one retailer would be indistinguishable from never having
       configured them, and the default would silently re-open their credit.

    2. The funded wallet's own opening and closing balance on each statement line.

       The statement already showed the DEBT going from X to Y. It could not answer "which wallet
       did this land in, and what did that wallet go from and to" - which is the first thing
       somebody checks a credit line against.

    3. Back-fill, so nothing already configured changes meaning.
*/

-------------------------------------------------------------------------------
-- 1. "Has somebody actually set a limit for this account?"
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.CashCollectionMasters', 'HasExplicitLimit') IS NULL
    ALTER TABLE dbo.CashCollectionMasters
        ADD HasExplicitLimit BIT NOT NULL CONSTRAINT DF_CashCollectionMasters_HasExplicitLimit DEFAULT(0);
GO

/*
    Back-fill: a row with a POSITIVE limit can only have got it from somebody setting it (every
    row is created at zero), so it is an explicit limit and must keep behaving as one.

    A row sitting at zero is left as NOT explicit, which is the honest reading - it is the
    create-time default, not a decision. Those accounts therefore pick up the new default limit
    once one is configured, which is the point of the feature. An admin who wants a specific
    account to have no credit at all sets its limit to 0 from the screen, and that WILL be
    recorded as explicit.
*/
UPDATE dbo.CashCollectionMasters
SET HasExplicitLimit = 1
WHERE MaxOutstanding > 0
  AND HasExplicitLimit = 0
  AND IsDeleted = 0;
GO

-------------------------------------------------------------------------------
-- 2. The funded wallet's before/after on the statement line.
--    Nullable: a cash collection moves no wallet at all, and a null there means exactly that
--    rather than a misleading 0.00.
-------------------------------------------------------------------------------
IF COL_LENGTH('dbo.CashCollectionDetails', 'WalletOpening') IS NULL
    ALTER TABLE dbo.CashCollectionDetails ADD WalletOpening DECIMAL(18,5) NULL;
GO

IF COL_LENGTH('dbo.CashCollectionDetails', 'WalletClosing') IS NULL
    ALTER TABLE dbo.CashCollectionDetails ADD WalletClosing DECIMAL(18,5) NULL;
GO

-------------------------------------------------------------------------------
-- 3. The default limit itself.
--
-- Seeded OFF with an empty value on purpose. A credit limit is a commercial decision about real
-- money, and shipping an install that silently extends (say) 50,000 of credit to every account
-- the moment it upgrades would be indefensible. The operator turns it on and types the number,
-- which is also exactly how the other amount-bearing Key Settings behave (FundMinAmount,
-- FundMaxAmount, ShowLimitInDMT).
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.SettingMasters', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'DefaultCreditLimit')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('DefaultCreditLimit', '0', 0, 0,
         'Default credit limit for accounts that have not been given one of their own. When ON, any account whose limit has never been set may take funds on credit up to this amount. A limit set for a specific account under Funds -> Outstanding always wins, including a deliberate 0 (that account gets no credit even while this default is on). OFF means no credit until each account is given a limit individually.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: DefaultCreditLimit (OFF, 0 - set the amount and switch it on to use it).';
END
GO
