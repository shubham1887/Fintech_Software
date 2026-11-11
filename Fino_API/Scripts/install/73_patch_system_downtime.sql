/*
    System Down - a scheduled maintenance window during which the platform refuses financial
    transactions, with a named set of services allowed to keep running.

    Two parts:

      * ServiceMasters.IsAllowedWhenSystemDown - per service, "this one still works while we are
        down". Default 0, so switching the window on stops everything until the operator chooses
        the exceptions. A window that let a service through by default would be worse than having
        no window at all.

      * Four SettingMasters rows, which is where the Key Settings screen reads and writes its
        curated toggles. IsSystemDown is the switch; SystemDownFrom / SystemDownTo are clock times
        in HH:mm; SystemDownMessage is what the user is told. A window whose From is later than its
        To wraps over midnight (22:00 to 02:00), which is when maintenance usually runs.

    Seeded OFF with an empty window, so applying this changes nothing until an operator turns it
    on. Inserted only when missing, so re-running never overwrites a live configuration.

    Idempotent - safe to re-run.
*/

IF COL_LENGTH(N'dbo.ServiceMasters', N'IsAllowedWhenSystemDown') IS NULL
BEGIN
    ALTER TABLE dbo.ServiceMasters
        ADD [IsAllowedWhenSystemDown] BIT NOT NULL
            CONSTRAINT DF_ServiceMasters_IsAllowedWhenSystemDown DEFAULT (0);
END
GO

INSERT INTO dbo.SettingMasters ([Key], [Value], [IsOn], [IsSecure], [IsPassword], [Description], [DateTime], [UpdatedateTime], [IsDeleted])
SELECT v.[Key], v.[Value], v.[IsOn], 0, 0, v.[Description], SYSUTCDATETIME(), SYSUTCDATETIME(), 0
FROM (VALUES
    (N'IsSystemDown',      N'false', CAST(0 AS BIT), N'Master switch for the scheduled system-down window. On = financial transactions are refused between the From and To times below, except on services marked "works while system is down".'),
    (N'SystemDownFrom',    N'',      CAST(0 AS BIT), N'Start of the daily system-down window, as a 24-hour clock time (HH:mm).'),
    (N'SystemDownTo',      N'',      CAST(0 AS BIT), N'End of the daily system-down window, as a 24-hour clock time (HH:mm). Earlier than From means the window crosses midnight.'),
    (N'SystemDownMessage', N'',      CAST(0 AS BIT), N'Message shown to users while the system is down. Blank falls back to a standard wording.')
) AS v([Key], [Value], [IsOn], [Description])
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.SettingMasters s
    WHERE s.[Key] = v.[Key] AND s.IsDeleted = 0
);
GO
