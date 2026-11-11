-- ============================================================================
-- 1. One mobile number, two roles ("linked logins").
-- 2. An expiry on announcements and in-app alerts.
--
-- Idempotent: every statement is guarded, so a re-run is a no-op.
-- After running this, RESTART the API (in-memory user cache).
--
-- ---------------------------------------------------------------------------
-- LINKED LOGINS
--
-- A distributor who also runs a counter, a super-distributor who sells directly,
-- an API client that needs a retailer login to test with — each needed a SECOND
-- MOBILE NUMBER, because sign-in is keyed on the number and [UserMasters] had a
-- unique index on it alone.
--
-- [LinkedPrimaryUserID] makes the pair explicit: the main account carries NULL,
-- the second login points at it. Both are real accounts (own wallet, own slab,
-- own ledger, own user code); they share the mobile number and the password.
--
-- The uniqueness rule moves from (MobileNumber) to (MobileNumber, UserTypeID):
-- one number may hold at most one account PER ROLE, which is exactly the rule
-- the feature needs and still refuses the duplicate-retailer mistake the old
-- index was there to prevent. Application code (UserService.Insert) continues to
-- refuse any duplicate number on ordinary Add User; only the profile's "add a
-- second login" action creates a sibling, and only for the role pairs
-- StaticModels/LinkedProfilePolicy allows.
--
-- ---------------------------------------------------------------------------
-- EXPIRY
--
-- Announcements and alerts are overwhelmingly time-bound ("offer till Sunday",
-- "maintenance tonight") and nothing retired them, so banners and bells filled
-- with notices that had stopped being true. [ExpiresOn] NULL keeps the old
-- behaviour — shown until somebody takes it down.
-- ============================================================================
SET NOCOUNT ON;
-- sqlcmd defaults QUOTED_IDENTIFIER OFF, and SQL Server refuses to create an index
-- while it is off. Stated explicitly so the script behaves the same whether the
-- installer runs it (SqlClient, ON) or an operator runs it by hand.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ------------------------------------------- UserMasters.LinkedPrimaryUserID */
IF COL_LENGTH(N'[UserMasters]', N'LinkedPrimaryUserID') IS NULL
BEGIN
    ALTER TABLE [UserMasters] ADD [LinkedPrimaryUserID] int NULL;
END;
GO

IF COL_LENGTH(N'[UserMasters]', N'LinkedPrimaryUserID') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_UserMasters_LinkedPrimaryUserID'
                     AND object_id = OBJECT_ID(N'[UserMasters]'))
BEGIN
    CREATE INDEX [IX_UserMasters_LinkedPrimaryUserID]
        ON [UserMasters] ([LinkedPrimaryUserID] ASC);
END;
GO

/* ------------------------------- mobile uniqueness: per number -> per number+role */
-- Dropped and replaced in one script so a database is never left with neither:
-- without a uniqueness rule here, two accounts of the SAME role could share a
-- number and sign-in would have no single answer for who the credentials belong to.
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = N'IX_UserMasters_MobileNumber'
             AND object_id = OBJECT_ID(N'[UserMasters]'))
BEGIN
    DROP INDEX [IX_UserMasters_MobileNumber] ON [UserMasters];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_UserMasters_Mobile_UserType'
                 AND object_id = OBJECT_ID(N'[UserMasters]'))
BEGIN
    -- Existing data may already hold a duplicate (number, role) pair from before any
    -- index existed. Creating the unique index would fail outright and abort the whole
    -- install, so it is only attempted when the data can actually carry it; the
    -- non-unique index keeps the lookup fast either way, and the duplicates are
    -- reported rather than silently tolerated.
    IF EXISTS (SELECT 1 FROM [UserMasters]
               GROUP BY [MobileNumber], [UserTypeID]
               HAVING COUNT(*) > 1)
    BEGIN
        CREATE INDEX [UX_UserMasters_Mobile_UserType]
            ON [UserMasters] ([MobileNumber] ASC, [UserTypeID] ASC);

        PRINT 'WARNING: [UserMasters] already contains more than one account with the same';
        PRINT '         mobile number AND the same role, so the UNIQUE index could not be';
        PRINT '         created and a non-unique one was used instead. Linked logins still';
        PRINT '         work. To restore the guarantee, resolve the duplicates with:';
        PRINT '           SELECT MobileNumber, UserTypeID, COUNT(*) FROM UserMasters';
        PRINT '           GROUP BY MobileNumber, UserTypeID HAVING COUNT(*) > 1;';
        PRINT '         then drop UX_UserMasters_Mobile_UserType and re-create it as UNIQUE.';
    END
    ELSE
    BEGIN
        CREATE UNIQUE INDEX [UX_UserMasters_Mobile_UserType]
            ON [UserMasters] ([MobileNumber] ASC, [UserTypeID] ASC);
    END;
END;
GO

/* ---------------------------------------------- NewsMasters.ExpiresOn */
IF OBJECT_ID(N'[NewsMasters]') IS NOT NULL
   AND COL_LENGTH(N'[NewsMasters]', N'ExpiresOn') IS NULL
BEGIN
    ALTER TABLE [NewsMasters] ADD [ExpiresOn] datetime2 NULL;
END;
GO

/* ----------------------------------------- AppNotifications.ExpiresOn */
IF OBJECT_ID(N'[AppNotifications]') IS NOT NULL
   AND COL_LENGTH(N'[AppNotifications]', N'ExpiresOn') IS NULL
BEGIN
    ALTER TABLE [AppNotifications] ADD [ExpiresOn] datetime2 NULL;
END;
GO
