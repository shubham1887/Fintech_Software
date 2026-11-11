/* ============================================================================
   DMT Key Settings — SettingMaster seed
   ----------------------------------------------------------------------------
   Two admin-managed keys surfaced in Settings → Key Setting:

     IsBeneficiaryOTP  (bool)   Value 'true'/'false'. true  ⇒ adding a DMT
                                beneficiary requires OTP verification.
                                Read by DMTBeneficiaryFlowService; falls back to
                                the legacy 'DMTOTP' key when this row is absent.

     ShowLimitInDMT    (number) A per-transfer cap (₹) shown & enforced on the
                                retailer DMT page. Honoured only when IsOn = 1 and
                                Value is a positive number; otherwise no extra cap.
                                Read by DMTSenderFlowService (SenderDetails.DisplayLimit).

   Idempotent — safe to run repeatedly. Edit the Value/IsOn later in Key Setting.
   NOTE: SettingMasterRepository caches GetAll in memory — restart the API (or
   re-save a row through Key Setting, which busts the cache) after a direct seed.
   ============================================================================ */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'IsBeneficiaryOTP')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES ('IsBeneficiaryOTP', 'true', 1, 0, 'Require OTP verification when a retailer adds a DMT beneficiary (true/false).', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: IsBeneficiaryOTP = true';
END
ELSE
    PRINT 'SettingMaster "IsBeneficiaryOTP" already exists — skipped.';

IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'ShowLimitInDMT')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES ('ShowLimitInDMT', '25000', 1, 0, 'Maximum amount (₹) per DMT transfer shown & enforced on the money-transfer page. Turn the row Off to disable the cap.', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: ShowLimitInDMT = 25000';
END
ELSE
    PRINT 'SettingMaster "ShowLimitInDMT" already exists — skipped.';

SELECT SettingMasterID, [Key], Value, IsOn FROM dbo.SettingMasters
WHERE [Key] IN ('IsBeneficiaryOTP', 'ShowLimitInDMT');
GO
