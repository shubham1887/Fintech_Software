-- Item 6 — MPIN (transaction / app-open PIN).
-- Idempotent: adds the hashed-MPIN columns to UserMasters and seeds the global IsMpinEnabled
-- feature flag in SettingMaster (Key Setting) as OFF. Shipped as a script (matching the
-- Scripts/*.sql convention) rather than an EF migration.

IF COL_LENGTH('dbo.UserMasters', 'MpinHash') IS NULL
    ALTER TABLE dbo.UserMasters ADD MpinHash varchar(128) NOT NULL CONSTRAINT DF_UserMasters_MpinHash DEFAULT('');
GO

IF COL_LENGTH('dbo.UserMasters', 'IsMpinEnabled') IS NULL
    ALTER TABLE dbo.UserMasters ADD IsMpinEnabled bit NOT NULL CONSTRAINT DF_UserMasters_IsMpinEnabled DEFAULT(0);
GO

-- Global toggle shown in Key Setting. Off by default: nobody is forced to set an MPIN until the
-- admin turns this on (or an individual user opts in via Change MPIN).
IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'IsMpinEnabled')
    INSERT INTO dbo.SettingMasters ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES ('IsMpinEnabled', 'false', 0, 0, 'Require an MPIN for transactions and on opening the app (true/false).', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
GO
