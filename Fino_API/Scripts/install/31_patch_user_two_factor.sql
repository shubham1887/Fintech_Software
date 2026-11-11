/*
    Authenticator-app ("Google Authenticator") enrolments - one row per user.

    Backs Entities/UserTwoFactor. Deliberately its own table rather than columns on UserMaster:
    SecretBase32 is a credential - anyone holding it can generate that user's codes forever -
    and UserMaster is selected in bulk, cached in memory and mapped into a dozen response DTOs,
    so a secret living there is one careless projection away from being sent to a browser.

    The unique index on UserID is what makes re-enrolling safe: starting setup again REPLACES the
    row instead of adding a second secret. Two live secrets for one user would mean a phone the
    user believed they had revoked still produced working codes.

    Note there is no index filtered on IsDeleted. The repository hard-deletes these rows for the
    same reason - a soft-deleted secret is still a working secret.

    Idempotent - safe to re-run.
*/

IF OBJECT_ID(N'dbo.UserTwoFactors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserTwoFactors (
        [UserTwoFactorID]    INT            NOT NULL IDENTITY(1,1),
        [UserID]             INT            NOT NULL,
        -- Base32, 20 random bytes => 32 characters. varchar(64) leaves room for a longer secret
        -- without a schema change.
        [SecretBase32]       VARCHAR(64)    NOT NULL,
        -- False between "show me the QR" and "here is a code from my phone". An unconfirmed row
        -- must never gate a login, or scanning a QR and closing the page would lock the user out.
        [IsConfirmed]        BIT            NOT NULL CONSTRAINT DF_UserTwoFactors_IsConfirmed DEFAULT (0),
        [ConfirmedOn]        DATETIME2      NULL,
        -- The last 30-second step a code was accepted for, so the same six digits cannot be
        -- replayed inside their own 90-second validity window.
        [LastUsedStep]       BIGINT         NOT NULL CONSTRAINT DF_UserTwoFactors_LastUsedStep DEFAULT (0),
        -- JSON array of SHA-256 hashes of the UNUSED recovery codes. Hashed because they are
        -- single-use passwords; each is removed from the array as it is spent.
        [RecoveryCodeHashes] VARCHAR(MAX)   NOT NULL CONSTRAINT DF_UserTwoFactors_RecoveryCodeHashes DEFAULT (''),
        [DateTime]           DATETIME2      NOT NULL CONSTRAINT DF_UserTwoFactors_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2      NOT NULL CONSTRAINT DF_UserTwoFactors_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT            NOT NULL CONSTRAINT DF_UserTwoFactors_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_UserTwoFactors] PRIMARY KEY ([UserTwoFactorID])
    );

    PRINT 'Created table UserTwoFactors.';
END
ELSE
BEGIN
    PRINT 'UserTwoFactors already present - nothing to do.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.UserTwoFactors') AND name = N'IX_UserTwoFactors_UserID'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserTwoFactors_UserID] ON dbo.UserTwoFactors ([UserID]);
    PRINT 'Created unique index IX_UserTwoFactors_UserID.';
END
GO

/*
    Repair pass for any install where IsTwoFA was switched on by hand (or by an older build) while
    no confirmed enrolment exists. Such an account demands a code that nothing can produce, which
    locks the user out permanently. Clearing the flag returns them to password-only sign-in; they
    can enrol properly from the Security screen.

    (UserMasters.IsTwoFA itself is created by 10_schema.sql - it is the flag the login path reads
    to decide whether to demand a second step. Only its VALUE needs repairing here.)
*/
UPDATE u
   SET u.IsTwoFA = 0
  FROM dbo.UserMasters u
 WHERE u.IsTwoFA = 1
   AND NOT EXISTS (
        SELECT 1 FROM dbo.UserTwoFactors t
         WHERE t.UserID = u.UserMasterID AND t.IsConfirmed = 1 AND t.IsDeleted = 0
   );

IF @@ROWCOUNT > 0 PRINT 'Cleared IsTwoFA on accounts with no confirmed authenticator enrolment.';
GO
