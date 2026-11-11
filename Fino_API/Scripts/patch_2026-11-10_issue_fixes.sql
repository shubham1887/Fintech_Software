-- Issue-fix patch, 10 Nov 2026. Idempotent: safe to run twice.
--   1. SentSMS.Request varchar(500) -> varchar(max): the WhatsApp gateway URL overran 500 chars and
--      every send that logged it threw "String or binary data would be truncated" (event log, 8/day).
--   2. Nightly maintenance window 23:50-00:00 (was 23:45-06:30): the DailySnapshotJob runs inside it.
--   3. DailyBalanceSnapshots: the job creates it too, but open it here so the first night needs no DDL rights.
-- Run: sqlcmd -S localhost\SQLEXPRESS -d finocore_db -E -C -I -i <this file>
SET QUOTED_IDENTIFIER ON;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SentSMS') AND name = 'Request' AND max_length <> -1)
    ALTER TABLE [SentSMS] ALTER COLUMN [Request] varchar(max) NOT NULL;
GO
UPDATE SettingMasters SET [Value] = '23:50', IsOn = 1, UpdatedateTime = GETDATE() WHERE [Key] = 'SystemDownFrom';
UPDATE SettingMasters SET [Value] = '00:00', IsOn = 1, UpdatedateTime = GETDATE() WHERE [Key] = 'SystemDownTo';
GO
IF OBJECT_ID(N'[DailyBalanceSnapshots]', N'U') IS NULL
BEGIN
CREATE TABLE [DailyBalanceSnapshots] (
    [DailyBalanceSnapshotID] int NOT NULL IDENTITY,
    [SnapshotDate] date NOT NULL,
    [Kind] varchar(10) NOT NULL,
    [OwnerID] int NOT NULL,
    [WalletID] int NOT NULL,
    [Balance] decimal(18,5) NOT NULL,
    [HoldBalance] decimal(18,5) NOT NULL,
    [TakenAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DailyBalanceSnapshots] PRIMARY KEY ([DailyBalanceSnapshotID])
);
CREATE UNIQUE INDEX [UX_DailyBalanceSnapshots_Day] ON [DailyBalanceSnapshots] ([SnapshotDate], [Kind], [OwnerID], [WalletID]);
END;
GO
SELECT [Key], [Value], IsOn FROM SettingMasters WHERE [Key] IN ('IsSystemDown', 'SystemDownFrom', 'SystemDownTo');
SELECT name, max_length AS RequestMaxLength FROM sys.columns WHERE object_id = OBJECT_ID('SentSMS') AND name = 'Request';
SELECT COUNT(*) AS SnapshotTableExists FROM sys.tables WHERE name = 'DailyBalanceSnapshots';
GO
