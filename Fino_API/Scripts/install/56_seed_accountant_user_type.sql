-- ============================================================================
-- Accountant user type (UserTypeID 9).
--
-- WHY THIS EXISTS
-- The code has always known about an accountant. ApiAccessPolicy, MenuService
-- and UserTypeCode all carry `Accountant = 9`, and the Administration menu
-- lists the accounting rows for role 9. But dbo.UserTypeMasters only ever
-- shipped rows 1-5, so there was no user type to pick: the role existed in
-- every layer except the one that lets you create a user with it.
--
-- The symptom was not an error. The "Add User" type dropdown simply had no
-- Accountant option, so the whole accounting side of the panel was unreachable
-- and looked unbuilt. Nothing had failed -- the row was just missing.
--
-- This platform is installed by a non-technical person, one client at a time.
-- Asking that person to insert a row by hand is a per-client manual step, which
-- is a defect. So the row ships.
--
-- WHAT IT DOES
--   1. Inserts UserTypeMasters row 9 = Accountant (IDENTITY_INSERT, because
--      UserTypeID is an identity column and the id is referenced by code).
--   2. Inserts the matching dbo.UserCodeFormats row ('AC'), so that creating the
--      first accountant does not fail on a missing code format. That table has a
--      UNIQUE index on UserTypeID and a FK to UserTypeMasters, so the order
--      matters: the type must exist first.
--
-- Idempotent: every statement is guarded by NOT EXISTS, so a re-run is a no-op
-- and an admin's own rename of the type or edit of the code is never overwritten.
-- ============================================================================

SET NOCOUNT ON;

/* ── 1. The user type ───────────────────────────────────────────────────────
   UserTypeID 9 is not arbitrary: StaticModels/UserTypeCode.cs pins
   Accountant = 9, and MenuService/ApiAccessPolicy both gate on that number.
   The row therefore has to carry id 9 exactly, which needs IDENTITY_INSERT. */
IF NOT EXISTS (SELECT 1 FROM dbo.UserTypeMasters WHERE UserTypeID = 9)
BEGIN
    SET IDENTITY_INSERT dbo.UserTypeMasters ON;

    INSERT INTO dbo.UserTypeMasters
        (UserTypeID, UserType, AliasName, IsActive, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (9, 'Accountant', 'Accountant', 1, SYSDATETIME(), SYSDATETIME(), 0);

    SET IDENTITY_INSERT dbo.UserTypeMasters OFF;

    PRINT 'Seeded dbo.UserTypeMasters row 9 (Accountant).';
END
ELSE
    PRINT 'UserTypeMasters row 9 (Accountant) already present - skipped.';

/* ── 2. Its user-code format ────────────────────────────────────────────────
   23_patch_user_code_format.sql seeds one format per user type, but it runs
   BEFORE this script, so on a fresh install it never sees type 9. Without a
   format row the first accountant cannot be issued a user code.
   'AC' is what that script's own fallback would have produced from the type
   name, so the code is the same whichever path created it. */
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserCodeFormats')
   AND EXISTS (SELECT 1 FROM dbo.UserTypeMasters WHERE UserTypeID = 9)
   AND NOT EXISTS (SELECT 1 FROM dbo.UserCodeFormats WHERE UserTypeID = 9)
BEGIN
    /* DateTime / UpdatedateTime / IsDeleted are listed explicitly on purpose. Some databases
       carry defaults on these BaseEntity columns and some do not -- a live install was found
       with all three NOT NULL and no default, where omitting them failed with
       "Cannot insert the value NULL into column 'IsDeleted'". Naming them works either way. */
    INSERT INTO dbo.UserCodeFormats
        (UserTypeID, Code, Pattern, SequenceLength, StartNumber, LastSequence,
         ResetPolicy, SequencePeriod, IsActive, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        (9, 'AC', '{CODE}{SEQ}', 6, 1, 0, 'None', '', 1, SYSDATETIME(), SYSDATETIME(), 0);

    /* Report what actually landed. A PRINT placed after a failed statement still runs -- the
       error terminates the statement, not the batch -- so an unconditional message here would
       claim success for an insert that did not happen. */
    IF @@ROWCOUNT = 1
        PRINT 'Seeded dbo.UserCodeFormats for user type 9 (AC).';
    ELSE
        PRINT 'WARNING: dbo.UserCodeFormats insert for user type 9 did NOT land - check the error above.';
END
ELSE
    PRINT 'UserCodeFormats for user type 9 already present or table absent - skipped.';
