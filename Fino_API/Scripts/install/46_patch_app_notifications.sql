-- ============================================================================
-- Notification bell + push delivery + the admin-selectable mobile app layout.
--
-- Creates three tables and seeds the UI settings that drive the phone layout.
-- Idempotent: every statement is guarded, so a re-run is a no-op.
--
--  [UserDevices]                 one FCM token per handset/browser. Replaces the
--                                single UserMasters.AppFCMToken column, which was
--                                never written to (UpdateFCMToken was a stub) and
--                                could only ever hold one device anyway.
--  [AppNotifications]            the alert itself, with its audience on the row.
--  [AppNotificationRecipients]   one row per user: the unread flag the bell reads,
--                                and whether the push actually left.
--
-- After running this, RESTART the API (in-memory caches).
-- ============================================================================
SET NOCOUNT ON;
GO

/* ------------------------------------------------------------- UserDevices */
IF OBJECT_ID(N'[UserDevices]') IS NULL
BEGIN
    CREATE TABLE [UserDevices] (
        [UserDeviceID]        int IDENTITY(1,1) NOT NULL,
        [UserID]              int            NOT NULL,
        [Platform]            varchar(20)    NOT NULL CONSTRAINT [DF_UserDevices_Platform] DEFAULT (''),
        [FcmToken]            varchar(500)   NOT NULL,
        [DeviceName]          nvarchar(200)  NOT NULL CONSTRAINT [DF_UserDevices_DeviceName] DEFAULT (N''),
        [AppVersion]          varchar(50)    NOT NULL CONSTRAINT [DF_UserDevices_AppVersion] DEFAULT (''),
        [IsActive]            bit            NOT NULL CONSTRAINT [DF_UserDevices_IsActive] DEFAULT (1),
        [LastSeenDateTime]    datetime2      NOT NULL CONSTRAINT [DF_UserDevices_LastSeen] DEFAULT (SYSUTCDATETIME()),
        [DeactivatedReason]   varchar(200)   NOT NULL CONSTRAINT [DF_UserDevices_Reason] DEFAULT (''),
        [DateTime]            datetime2      NOT NULL CONSTRAINT [DF_UserDevices_DateTime] DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      datetime2      NOT NULL CONSTRAINT [DF_UserDevices_Updated] DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           bit            NOT NULL CONSTRAINT [DF_UserDevices_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_UserDevices] PRIMARY KEY CLUSTERED ([UserDeviceID] ASC)
    );
END;
GO

-- One row per token: the same handset signing in again updates its row instead of
-- adding a second one, so a reinstalled app is not pushed to twice.
IF OBJECT_ID(N'[UserDevices]') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserDevices_FcmToken'
                   AND object_id = OBJECT_ID(N'[UserDevices]'))
BEGIN
    CREATE UNIQUE INDEX [IX_UserDevices_FcmToken] ON [UserDevices] ([FcmToken] ASC);
END;
GO

IF OBJECT_ID(N'[UserDevices]') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserDevices_UserID_IsActive'
                   AND object_id = OBJECT_ID(N'[UserDevices]'))
BEGIN
    CREATE INDEX [IX_UserDevices_UserID_IsActive] ON [UserDevices] ([UserID] ASC, [IsActive] ASC);
END;
GO

/* --------------------------------------------------------- AppNotifications */
IF OBJECT_ID(N'[AppNotifications]') IS NULL
BEGIN
    CREATE TABLE [AppNotifications] (
        [AppNotificationID]   int IDENTITY(1,1) NOT NULL,
        [Title]               nvarchar(200)  NOT NULL,
        [Body]                nvarchar(1000) NOT NULL,
        [ImageUrl]            varchar(500)   NOT NULL CONSTRAINT [DF_AppNotifications_ImageUrl] DEFAULT (''),
        [ActionUrl]           varchar(300)   NOT NULL CONSTRAINT [DF_AppNotifications_ActionUrl] DEFAULT (''),
        [Kind]                varchar(30)    NOT NULL CONSTRAINT [DF_AppNotifications_Kind] DEFAULT ('Info'),
        [Audience]            varchar(20)    NOT NULL CONSTRAINT [DF_AppNotifications_Audience] DEFAULT ('All'),
        [AudienceRoles]       varchar(100)   NOT NULL CONSTRAINT [DF_AppNotifications_Roles] DEFAULT (''),
        [CreatedByUserID]     int            NOT NULL CONSTRAINT [DF_AppNotifications_CreatedBy] DEFAULT (0),
        [RecipientCount]      int            NOT NULL CONSTRAINT [DF_AppNotifications_Recipients] DEFAULT (0),
        [PushedCount]         int            NOT NULL CONSTRAINT [DF_AppNotifications_Pushed] DEFAULT (0),
        [DateTime]            datetime2      NOT NULL CONSTRAINT [DF_AppNotifications_DateTime] DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      datetime2      NOT NULL CONSTRAINT [DF_AppNotifications_Updated] DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           bit            NOT NULL CONSTRAINT [DF_AppNotifications_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_AppNotifications] PRIMARY KEY CLUSTERED ([AppNotificationID] ASC)
    );
END;
GO

/* ------------------------------------------------ AppNotificationRecipients */
IF OBJECT_ID(N'[AppNotificationRecipients]') IS NULL
BEGIN
    CREATE TABLE [AppNotificationRecipients] (
        [AppNotificationRecipientID] int IDENTITY(1,1) NOT NULL,
        [AppNotificationID]   int            NOT NULL,
        [UserID]              int            NOT NULL,
        [IsRead]              bit            NOT NULL CONSTRAINT [DF_AppNotifRcp_IsRead] DEFAULT (0),
        [ReadDateTime]        datetime2      NULL,
        [PushSent]            bit            NOT NULL CONSTRAINT [DF_AppNotifRcp_PushSent] DEFAULT (0),
        [PushDateTime]        datetime2      NULL,
        [DateTime]            datetime2      NOT NULL CONSTRAINT [DF_AppNotifRcp_DateTime] DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]      datetime2      NOT NULL CONSTRAINT [DF_AppNotifRcp_Updated] DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]           bit            NOT NULL CONSTRAINT [DF_AppNotifRcp_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_AppNotificationRecipients] PRIMARY KEY CLUSTERED ([AppNotificationRecipientID] ASC),
        CONSTRAINT [FK_AppNotificationRecipients_AppNotifications]
            FOREIGN KEY ([AppNotificationID]) REFERENCES [AppNotifications] ([AppNotificationID]) ON DELETE CASCADE
    );
END;
GO

-- The bell asks exactly one question: "my unread, newest first".
IF OBJECT_ID(N'[AppNotificationRecipients]') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AppNotificationRecipients_UserID_IsRead'
                   AND object_id = OBJECT_ID(N'[AppNotificationRecipients]'))
BEGIN
    CREATE INDEX [IX_AppNotificationRecipients_UserID_IsRead]
        ON [AppNotificationRecipients] ([UserID] ASC, [IsRead] ASC);
END;
GO

IF OBJECT_ID(N'[AppNotificationRecipients]') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AppNotificationRecipients_AppNotificationID'
                   AND object_id = OBJECT_ID(N'[AppNotificationRecipients]'))
BEGIN
    CREATE INDEX [IX_AppNotificationRecipients_AppNotificationID]
        ON [AppNotificationRecipients] ([AppNotificationID] ASC);
END;
GO

/* ------------------------------------------------- mobile layout UI settings
   The Android app is the web console in a WebView, so its look is set here and
   takes effect on every phone at the next launch - no APK rebuild, no store
   review. The admin picks these from Settings -> Appearance.
   --------------------------------------------------------------------------- */
IF OBJECT_ID(N'[UISettingMasters]') IS NOT NULL
BEGIN
    ;WITH seed([Key], [Value], [Description]) AS (
        SELECT 'MobileLayout',      'classic',
               'Phone/app home layout preset: classic | compact | vibrant | minimal | business'
        UNION ALL SELECT 'MobileHeaderStyle', 'solid',
               'App bar treatment on the phone: solid | gradient | glass'
        UNION ALL SELECT 'MobileTileShape',   'rounded',
               'Service tile shape on the phone home grid: rounded | circle | square | card'
        UNION ALL SELECT 'MobileGridColumns', '4',
               'Service tiles per row on the phone home grid (3, 4 or 5)'
        UNION ALL SELECT 'MobileShowBalanceCard', 'true',
               'Show the large balance card on the phone home screen (true | false)'
        UNION ALL SELECT 'MobileShowTodayStats',  'true',
               'Show the today figures strip on the phone home screen (true | false)'
        UNION ALL SELECT 'MobileTabStyle',   'labelled',
               'Bottom tab bar on the phone: labelled | icons | floating'
    )
    INSERT INTO [UISettingMasters] ([Key], [Value], [Description], [DateTime], [UpdatedateTime], [IsDeleted])
    SELECT s.[Key], s.[Value], s.[Description], SYSUTCDATETIME(), SYSUTCDATETIME(), 0
    FROM seed s
    WHERE NOT EXISTS (
        SELECT 1 FROM [UISettingMasters] u WHERE u.[Key] = s.[Key] AND u.[IsDeleted] = 0
    );
END;
GO
