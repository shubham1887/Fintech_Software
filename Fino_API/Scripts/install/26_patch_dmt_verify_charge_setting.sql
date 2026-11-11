/* ============================================================================
   DMT beneficiary account-verify fee — SettingMaster seed
   ----------------------------------------------------------------------------
   DMTAccountVerifyService reads SettingMaster key 'DMTVerifyCharge' (Value = fee
   in rupees) for the penny-drop verification charge. When the row is absent the
   code falls back to ₹1, so this seed is OPTIONAL — run it to make the fee
   visible / editable in the Settings → Key Setting screen.

   Idempotent. Change the '1' below (or edit it later in Key Setting) to set the fee.
   ============================================================================ */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'DMTVerifyCharge')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES ('DMTVerifyCharge', '1', 1, 0, 'Fee (₹) charged to the retailer wallet for a beneficiary account verification (penny-drop name check).', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: DMTVerifyCharge = 1';
END
ELSE
    PRINT 'SettingMaster "DMTVerifyCharge" already exists — skipped.';

SELECT SettingMasterID, [Key], Value FROM dbo.SettingMasters WHERE [Key] = 'DMTVerifyCharge';
GO
