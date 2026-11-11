/* ============================================================================
   KYC gate + user lifecycle — schema & settings (idempotent)
   ----------------------------------------------------------------------------
   Ships with:
     * Users → KYC Approvals  (admin review queue + approve / reject + policy)
     * Users → Inactive Users (activate, admin only)
     * Users → Deleted Users  (soft-deleted only, restore, admin only)

   1) ServiceMasters.IsAllowedWithoutKYC
        Per-service KYC exemption. When the gate is on, a user whose KYC is not
        approved is refused EVERY service except the ones flagged here. The
        admin sets these on the KYC Approvals → Policy tab.

   2) SettingMaster "IsKYCEnforced"
        The master switch for the gate. Seeded OFF so running this script never
        changes behaviour on its own — the admin turns it on deliberately, from
        the Policy tab or Settings → Key Setting.

   3) Message templates for the account status + KYC events, if the message
      master is present and doesn't have them yet.

   Safe to run repeatedly.
   NOTE: repositories cache in memory — RESTART the API after running this.
   ============================================================================ */
SET NOCOUNT ON;

/* ---- 1) Per-service KYC exemption ---------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[ServiceMasters]') AND name = 'IsAllowedWithoutKYC')
BEGIN
    ALTER TABLE [ServiceMasters] ADD [IsAllowedWithoutKYC] bit NOT NULL
        CONSTRAINT DF_ServiceMasters_IsAllowedWithoutKYC DEFAULT (0);
    PRINT 'Added ServiceMasters.IsAllowedWithoutKYC (default 0 = requires an approved KYC).';
END
ELSE
    PRINT 'ServiceMasters.IsAllowedWithoutKYC already exists — skipped.';
GO

/* ---- 2) Master switch for the KYC gate ----------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'IsKYCEnforced')
BEGIN
    INSERT INTO dbo.SettingMasters
        ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES
        ('IsKYCEnforced', 'false', 0, 0,
         'When ON, a user whose KYC is not approved cannot use any service except the ones marked as allowed without KYC on the KYC Approvals policy tab.',
         0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: IsKYCEnforced = false (turn it on from the KYC Approvals policy tab).';
END
ELSE
    PRINT 'SettingMaster "IsKYCEnforced" already exists — skipped.';
GO

/* ---- 3) Message templates for the new events ------------------------------
   The KYC Approved / KYC Rejected templates already ship with the message
   master defaults; the account status ones are new. Only inserted when the
   message table exists and the row is absent, so a deployment that manages its
   own templates is untouched.
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'[MessageSettings]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.MessageSettings WHERE [Type] = 'Account Activated')
        INSERT INTO dbo.MessageSettings
            ([Type], IsSMS, IsMail, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage, EmailMessage, TemplateID, [DateTime], UpdatedateTime, IsDeleted)
        VALUES ('Account Activated', 1, 0, 1, 0,
                'Dear {CustomerName}, your account has been ACTIVATED. You can sign in and transact again.',
                'Dear {CustomerName}, your account has been ACTIVATED. You can sign in and transact again.',
                '', '', SYSUTCDATETIME(), SYSUTCDATETIME(), 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.MessageSettings WHERE [Type] = 'Account Deactivated')
        INSERT INTO dbo.MessageSettings
            ([Type], IsSMS, IsMail, IsWhatsUp, IsOTP, SmsMessage, WhatsUpMessage, EmailMessage, TemplateID, [DateTime], UpdatedateTime, IsDeleted)
        VALUES ('Account Deactivated', 1, 0, 0, 0,
                'Dear {CustomerName}, your account has been DEACTIVATED. Please contact your service provider.',
                '', '', '', SYSUTCDATETIME(), SYSUTCDATETIME(), 0);

    PRINT 'Account status message templates checked.';
END
ELSE
    PRINT 'MessageSettings table not found — template seed skipped (messages fall back to built-in text).';
GO

/* ---- Verify -------------------------------------------------------------- */
SELECT ServiceID, Name, IsActive, IsAllowedWithoutKYC
FROM dbo.ServiceMasters WHERE IsDeleted = 0 ORDER BY UISequence, Name;

SELECT [Key], Value, IsOn FROM dbo.SettingMasters
WHERE [Key] IN ('IsKYCEnforced', 'eVerification');
GO
