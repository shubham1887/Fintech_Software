-- ============================================================================
-- Service Subscription lifecycle — schema + master toggle seed (idempotent).
-- Run once against the target DB, then RESTART the API (in-memory caches).
-- ============================================================================

-- 1) Plan validity window (days). 0 = lifetime.
IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[ServiceSubscriptionCreatePlanMasters]') AND name = 'ValidityDays')
BEGIN
    ALTER TABLE [ServiceSubscriptionCreatePlanMasters] ADD [ValidityDays] int NOT NULL DEFAULT 0;
END;
GO

-- 2) Permanent-free exemption flag on the user.
IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[UserMasters]') AND name = 'IsSubscriptionExempt')
BEGIN
    ALTER TABLE [UserMasters] ADD [IsSubscriptionExempt] bit NOT NULL DEFAULT 0;
END;
GO

-- 3) Per-user subscription records (one row per subscribe / renew).
IF OBJECT_ID(N'[UserSubscriptions]') IS NULL
BEGIN
    CREATE TABLE [UserSubscriptions] (
        [UserSubscriptionID] int NOT NULL IDENTITY,
        [UserID] int NOT NULL,
        [PlanID] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL DEFAULT 0,
        [StartDate] datetime2 NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DateTime] datetime2 NOT NULL,
        [UpdatedateTime] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserSubscriptions] PRIMARY KEY ([UserSubscriptionID])
    );
    CREATE INDEX [IX_UserSubscriptions_UserID] ON [UserSubscriptions] ([UserID]);
END;
GO

-- The master toggle (SettingMaster key "IsSubscriptionEnabled") is NOT seeded here — an
-- absent row reads as OFF, which is the intended default. Turn the feature on from the
-- Key Settings screen ("Service Subscription"), which creates the row with all columns set.

