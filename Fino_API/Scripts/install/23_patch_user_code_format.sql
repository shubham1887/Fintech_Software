/* ============================================================================
   User Type based auto User-Code generation — schema + seed + uniqueness
   ----------------------------------------------------------------------------
   Gives every user type its own configurable code (AD / RD / DS ...) and its own
   running sequence, so a new user is coded automatically:

       AD000001, AD000002 ...   independent of
       RD000001, RD000002 ...   independent of
       DS000001, DS000002 ...

   The generated code is stored on the user as AliasName (the Alias Name field is
   no longer typed in). The shape is a business setting, not code — the Pattern
   column takes {CODE}, {SEQ}, {YYYY}, {YY}, {MM} plus any literal text, so
   prefix / postfix / inline / year / month styles are all reachable:

       {CODE}{SEQ}              -> RD000001
       {CODE}{YYYY}{SEQ}        -> RD2026000001
       USR-{CODE}-{YYYY}-{SEQ}  -> USR-RD-2026-000001

   What this script does
     1. Creates dbo.UserCodeFormats (one row per user type, unique on UserTypeID).
     2. Seeds a row per user type — AD / SD / DS / RD / AP for the standard tiers,
        else a short code derived from the type itself. All of it editable after.
     3. Seeds each running sequence from the codes already in use, so generation
        CONTINUES an existing series instead of colliding with it.
     4. Adds the unique index on UserMasters.AliasName that makes duplicate user
        codes impossible at the database level. If duplicates already exist the
        index is skipped and the offending rows are listed — clean them, re-run.

   Idempotent — safe to run repeatedly. Nothing here overwrites an existing
   configuration row or an existing user code.
   ============================================================================ */
/* The filtered unique index below needs QUOTED_IDENTIFIER ON, which sqlcmd leaves
   off by default — set it in its own batch so the rest of the script inherits it. */
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;

/* ── 1. Table ───────────────────────────────────────────────────────────────── */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserCodeFormats')
BEGIN
    CREATE TABLE dbo.UserCodeFormats
    (
        UserCodeFormatID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_UserCodeFormats PRIMARY KEY,
        UserTypeID       INT          NOT NULL,
        Code             VARCHAR(10)  NOT NULL CONSTRAINT DF_UCF_Code           DEFAULT (''),
        Pattern          VARCHAR(100) NOT NULL CONSTRAINT DF_UCF_Pattern        DEFAULT ('{CODE}{SEQ}'),
        SequenceLength   INT          NOT NULL CONSTRAINT DF_UCF_SequenceLength DEFAULT (6),
        StartNumber      INT          NOT NULL CONSTRAINT DF_UCF_StartNumber    DEFAULT (1),
        LastSequence     INT          NOT NULL CONSTRAINT DF_UCF_LastSequence   DEFAULT (0),
        ResetPolicy      VARCHAR(10)  NOT NULL CONSTRAINT DF_UCF_ResetPolicy    DEFAULT ('None'),
        SequencePeriod   VARCHAR(10)  NOT NULL CONSTRAINT DF_UCF_SequencePeriod DEFAULT (''),
        IsActive         BIT          NOT NULL CONSTRAINT DF_UCF_IsActive       DEFAULT (1),
        [DateTime]       DATETIME2    NOT NULL CONSTRAINT DF_UCF_DateTime       DEFAULT (SYSUTCDATETIME()),
        UpdatedateTime   DATETIME2    NOT NULL CONSTRAINT DF_UCF_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        IsDeleted        BIT          NOT NULL CONSTRAINT DF_UCF_IsDeleted      DEFAULT (0),
        CONSTRAINT FK_UserCodeFormats_UserTypeMasters
            FOREIGN KEY (UserTypeID) REFERENCES dbo.UserTypeMasters (UserTypeID)
    );

    /* One format per user type — the sequence lives on this row, so a second row
       for the same type would be a second (conflicting) counter. */
    CREATE UNIQUE INDEX UX_UserCodeFormats_UserTypeID ON dbo.UserCodeFormats (UserTypeID);
    PRINT 'Created table dbo.UserCodeFormats';
END
ELSE
    PRINT 'Table dbo.UserCodeFormats already exists - skipped.';

/* ── 2. Seed one format per user type ───────────────────────────────────────
   The standard tiers get the short codes the specification uses; anything else
   takes the code already on the type master when that is short enough to be one,
   else the first two letters of the type name. All of it is editable afterwards
   from Users -> User Alias.                                                    */
INSERT INTO dbo.UserCodeFormats
    (UserTypeID, Code, Pattern, SequenceLength, StartNumber, LastSequence, ResetPolicy, SequencePeriod, IsActive)
SELECT
    t.UserTypeID,
    CASE t.UserTypeID
        WHEN 1 THEN 'AD'   /* Admin             */
        WHEN 2 THEN 'SD'   /* Super Distributor */
        WHEN 3 THEN 'DS'   /* Distributor       */
        WHEN 4 THEN 'RD'   /* Retailer          */
        WHEN 5 THEN 'AP'   /* API User          */
        ELSE
            CASE
                WHEN LEN(LTRIM(RTRIM(ISNULL(t.AliasName, '')))) BETWEEN 1 AND 4
                    THEN UPPER(LTRIM(RTRIM(t.AliasName)))
                ELSE UPPER(LEFT(REPLACE(LTRIM(RTRIM(ISNULL(t.UserType, 'USR'))), ' ', ''), 2))
            END
    END COLLATE DATABASE_DEFAULT,
    '{CODE}{SEQ}', 6, 1, 0, 'None', '', 1
FROM dbo.UserTypeMasters t
WHERE t.IsDeleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.UserCodeFormats f WHERE f.UserTypeID = t.UserTypeID);

PRINT 'Seeded missing user-type code formats.';

/* ── 3. Continue the series already in use ──────────────────────────────────
   For each type, find the highest number among the codes its users already
   carry in the configured shape, and park the counter there. Generation then
   picks up at the next number rather than re-issuing an existing code.        */
;WITH used AS
(
    SELECT f.UserTypeID,
           TRY_CONVERT(INT, SUBSTRING(u.AliasName, LEN(f.Code) + 1, 20)) AS Sequence
    FROM dbo.UserCodeFormats f
    JOIN dbo.UserMasters u
      ON u.UserTypeID = f.UserTypeID
     AND u.AliasName IS NOT NULL
     AND u.AliasName <> ''
     AND f.Code <> ''
     AND u.AliasName LIKE f.Code + '%'
     AND LEN(u.AliasName) > LEN(f.Code)
     AND SUBSTRING(u.AliasName, LEN(f.Code) + 1, 20) NOT LIKE '%[^0-9]%'
    WHERE f.Pattern = '{CODE}{SEQ}'
)
UPDATE f
SET f.LastSequence = x.MaxSequence,
    f.UpdatedateTime = SYSUTCDATETIME()
FROM dbo.UserCodeFormats f
JOIN (SELECT UserTypeID, MAX(Sequence) AS MaxSequence FROM used WHERE Sequence IS NOT NULL GROUP BY UserTypeID) x
  ON x.UserTypeID = f.UserTypeID
WHERE x.MaxSequence > f.LastSequence;

PRINT 'Sequences aligned with the codes already in use.';

/* ── 4. Duplicate user codes must be impossible ─────────────────────────────── */
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_UserMasters_AliasName' AND object_id = OBJECT_ID('dbo.UserMasters'))
BEGIN
    PRINT 'Unique index UX_UserMasters_AliasName already exists - skipped.';
END
ELSE IF EXISTS (
    SELECT 1 FROM dbo.UserMasters
    WHERE AliasName IS NOT NULL AND AliasName <> ''
    GROUP BY AliasName HAVING COUNT(*) > 1)
BEGIN
    PRINT '*** DUPLICATE user codes found - unique index NOT created. Fix these rows and re-run: ***';
    SELECT u.AliasName, COUNT(*) AS UserCount,
           STRING_AGG(CONVERT(VARCHAR(20), u.UserMasterID), ', ') AS UserMasterIDs
    FROM dbo.UserMasters u
    WHERE u.AliasName IS NOT NULL AND u.AliasName <> ''
    GROUP BY u.AliasName
    HAVING COUNT(*) > 1
    ORDER BY COUNT(*) DESC;
END
ELSE
BEGIN
    /* Filtered: legacy rows with no code yet stay legal, duplicates never are. */
    CREATE UNIQUE INDEX UX_UserMasters_AliasName
        ON dbo.UserMasters (AliasName)
        WHERE AliasName IS NOT NULL AND AliasName <> '';
    PRINT 'Created unique index UX_UserMasters_AliasName.';
END

/* ── Result ─────────────────────────────────────────────────────────────────── */
SELECT f.UserCodeFormatID, f.UserTypeID, t.UserType, f.Code, f.Pattern,
       f.SequenceLength, f.StartNumber, f.LastSequence, f.ResetPolicy, f.IsActive
FROM dbo.UserCodeFormats f
JOIN dbo.UserTypeMasters t ON t.UserTypeID = f.UserTypeID
WHERE f.IsDeleted = 0
ORDER BY f.UserTypeID;
GO
