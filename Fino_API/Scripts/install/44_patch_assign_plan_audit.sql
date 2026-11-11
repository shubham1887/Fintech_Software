-- ============================================================================
-- Admin "Assign Plan" — audit columns on UserSubscriptions (idempotent).
-- Run once against the target DB, then RESTART the API (in-memory caches).
--
-- Requires subscription_lifecycle.sql to have created [UserSubscriptions] first.
-- ============================================================================

IF OBJECT_ID(N'[UserSubscriptions]') IS NULL
BEGIN
    RAISERROR (N'[UserSubscriptions] does not exist. Run subscription_lifecycle.sql first.', 16, 1);
END;
GO

-- 1) Which admin granted this subscription window (0 = the user subscribed themselves).
IF OBJECT_ID(N'[UserSubscriptions]') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[UserSubscriptions]') AND name = 'AssignedByUserID')
BEGIN
    ALTER TABLE [UserSubscriptions] ADD [AssignedByUserID] int NOT NULL DEFAULT 0;
END;
GO

-- 2) Granted free of charge (no wallet movement) — goodwill / migration / trial.
IF OBJECT_ID(N'[UserSubscriptions]') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[UserSubscriptions]') AND name = 'IsComplimentary')
BEGIN
    ALTER TABLE [UserSubscriptions] ADD [IsComplimentary] bit NOT NULL DEFAULT 0;
END;
GO

-- 3) Why it was granted / revoked. Shown in the per-user audit trail.
IF OBJECT_ID(N'[UserSubscriptions]') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[UserSubscriptions]') AND name = 'Remark')
BEGIN
    ALTER TABLE [UserSubscriptions] ADD [Remark] varchar(500) NOT NULL DEFAULT '';
END;
GO
