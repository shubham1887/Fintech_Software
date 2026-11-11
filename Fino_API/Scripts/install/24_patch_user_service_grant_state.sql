/*
    Three-state service grants for the Add / Edit User "Services & access" grid.

    UserServiceMappings only had IsActive, which can hold two states. The grid has three:
        On   = IsGranted 1, IsActive 1   -> usable
        Lock = IsGranted 1, IsActive 0   -> listed but suspended, transactions refused
        Off  = IsGranted 0, IsActive 0   -> hidden from the catalog / menu, transactions refused

    "Off" used to be stored by deleting the row, which is indistinguishable from a user who was
    never put through the grid at all - and that case has to mean "unrestricted", otherwise every
    account created before this feature (and every self-signup approved through
    PendingRegistration, which grants no services) would be locked out of the whole app. So an
    admin who switched the entire grid Off ended up saving no rows, which read back as
    unrestricted and left every service visible. IsGranted removes that ambiguity.

    Existing rows are all currently "granted" (an ungranted service had no row), so the column
    defaults to 1 and no backfill beyond the default is needed.

    Idempotent - safe to re-run. RESTART THE API AFTERWARDS: the repository caches the mapping
    list in a static in-memory cache.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.UserServiceMappings') AND name = N'IsGranted'
)
BEGIN
    ALTER TABLE dbo.UserServiceMappings
        ADD IsGranted BIT NOT NULL CONSTRAINT DF_UserServiceMappings_IsGranted DEFAULT (1);

    PRINT 'Added UserServiceMappings.IsGranted (default 1).';
END
ELSE
BEGIN
    PRINT 'UserServiceMappings.IsGranted already present - nothing to do.';
END
GO

-- A locked row is granted; only an explicit Off row is not. Any pre-existing row is granted.
UPDATE dbo.UserServiceMappings SET IsGranted = 1 WHERE IsGranted IS NULL;
GO
