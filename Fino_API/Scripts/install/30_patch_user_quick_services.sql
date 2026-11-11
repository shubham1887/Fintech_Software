/*
    "Quick Services" - the shortcut tiles each user pins for themselves.

    Separate from ServiceMaster.UISequence on purpose. UISequence is the ADMIN's running order for
    the whole catalogue (every user sees the grid in that order); this table is one USER's own
    selection out of that catalogue, in the order they chose. A retailer who only sells recharge
    and DMT all day pins those two and stops scrolling, without the admin having to rearrange the
    catalogue for everyone.

    It lives server-side rather than in browser storage so the same shortcuts follow the user from
    the web panel into the Android app and onto a new device.

    The unique index on (UserID, ServiceID) is what makes the save idempotent: saving the list
    replaces it wholesale, so a double submit or two open tabs cannot leave the same tile twice.
    Rows are hard-deleted on replace (the index does not know about IsDeleted, so a soft-deleted
    row would permanently block re-pinning that service), which is why there is no audit concern -
    a shortcut list is a preference, not a record.

    Idempotent - safe to re-run.
*/

IF OBJECT_ID(N'dbo.UserQuickServices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserQuickServices (
        [UserQuickServiceID] INT           NOT NULL IDENTITY(1,1),
        [UserID]             INT           NOT NULL,
        [ServiceID]          INT           NOT NULL,
        [UISequence]         INT           NOT NULL CONSTRAINT DF_UserQuickServices_UISequence DEFAULT (0),
        [DateTime]           DATETIME2     NOT NULL CONSTRAINT DF_UserQuickServices_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2     NOT NULL CONSTRAINT DF_UserQuickServices_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT           NOT NULL CONSTRAINT DF_UserQuickServices_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_UserQuickServices] PRIMARY KEY ([UserQuickServiceID])
    );

    PRINT 'Created table UserQuickServices.';
END
ELSE
BEGIN
    PRINT 'UserQuickServices already present - nothing to do.';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.UserQuickServices') AND name = N'IX_UserQuickServices_UserID_ServiceID'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserQuickServices_UserID_ServiceID]
        ON dbo.UserQuickServices ([UserID], [ServiceID]);

    PRINT 'Created unique index IX_UserQuickServices_UserID_ServiceID.';
END
GO
