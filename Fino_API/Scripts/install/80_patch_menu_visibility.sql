/*
    Menu visibility — the admin's answer to "we do not use that screen".

    The menu definition ships with the product and lists every screen the product has. Any one
    install uses a fraction of them: a network that does not sell AEPS has no use for four AEPS
    reports, and an operator reading a thirty-row Reports group to find the two they use is the
    complaint this answers. Deleting rows from the shipped definition is not the fix — the next
    install needs them, and so does this one on the day it switches that service on.

    So the decision becomes data, and the table holds EXCEPTIONS ONLY. No row = the entry behaves
    exactly as shipped, which is what makes this safe to apply to a running install: a fresh table
    changes nothing on anybody's screen until an admin hides something.

    MenuKey is the leaf's URL, lower-cased (e.g. '/aepsreport'), or 'group:<name>' for a whole
    group, which has no URL of its own. The URL rather than the label, because a renamed screen is
    still the same screen and the decision "we do not use it" should survive the rename.

    UserTypeID 0 means every role. A role-specific row wins over the all-roles row, so "hide it
    everywhere except admin" is expressible in two rows rather than nine.

    This is PRESENTATION, not permission. Hiding an entry takes it off the sidebar; it does not
    revoke access to the endpoint behind it, which stays ApiAccessPolicy's job. A menu switch that
    quietly doubled as a security control would be a security control nobody could audit.

    Idempotent: safe to run twice.
*/

IF OBJECT_ID(N'dbo.MenuVisibilitySettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuVisibilitySettings
    (
        MenuVisibilitySettingID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_MenuVisibilitySettings PRIMARY KEY,
        MenuKey                 VARCHAR(200)  NOT NULL,
        MenuName                VARCHAR(150)  NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_MenuName DEFAULT(''),
        UserTypeID              INT           NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_UserTypeID DEFAULT(0),
        IsVisible               BIT           NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_IsVisible DEFAULT(1),
        UpdatedByUserID         INT           NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_UpdatedBy DEFAULT(0),
        [DateTime]              DATETIME2(7)  NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_DateTime DEFAULT(SYSUTCDATETIME()),
        UpdatedateTime          DATETIME2(7)  NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_Updated DEFAULT(SYSUTCDATETIME()),
        IsDeleted               BIT           NOT NULL
            CONSTRAINT DF_MenuVisibilitySettings_IsDeleted DEFAULT(0)
    );
    PRINT 'Created dbo.MenuVisibilitySettings (empty - every menu entry behaves as shipped).';
END
ELSE
    PRINT 'dbo.MenuVisibilitySettings already exists - left alone.';
GO

/*
    One live decision per (entry, role). The filtered index keeps a soft-deleted row from
    blocking a fresh decision about the same entry.
*/
IF OBJECT_ID(N'dbo.MenuVisibilitySettings', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'UX_MenuVisibilitySettings_Key_Role'
                     AND object_id = OBJECT_ID(N'dbo.MenuVisibilitySettings'))
BEGIN
    CREATE UNIQUE INDEX UX_MenuVisibilitySettings_Key_Role
        ON dbo.MenuVisibilitySettings (MenuKey, UserTypeID)
        WHERE IsDeleted = 0;
    PRINT 'Created UX_MenuVisibilitySettings_Key_Role.';
END
GO
