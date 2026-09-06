/* ============================================================================
   JaiJui DMT — provider + linked DMT-gateway seed
   ----------------------------------------------------------------------------
   The code changes need NO schema migration: JaiJui's credentials reuse existing
   columns — ProviderMasters.ProviderUniqueID holds the AuthCode, and
   DMTSettings.ProviderCode holds the link (= the provider's ProviderName).

   This script only SEEDS the two data rows (idempotent — safe to re-run):
     1. ProviderMasters row  "JaiJui DMT"  (credential source of truth / "Add API")
        - LoginID = Mobile, Token = APIToken, ProviderUniqueID = AuthCode
        - left BLANK here: enter the real secrets in Add API → Edit, or via the
          UPDATE at the bottom of this file. (Secrets are never stored in source.)
     2. DMTSettings row      "JaiJuiDMT"   (links to the provider via ProviderCode)
        - created INACTIVE so it cannot route live until you add credentials and
          tick Active in Settings → DMT Settings.

   Run against the target DB (e.g. FintechApp) in SSMS / sqlcmd.
   ============================================================================ */

SET NOCOUNT ON;

/* ---- 1. Provider gateway (Add API / ProviderMasters) --------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ProviderMasters
               WHERE ProviderName = 'JaiJui DMT' AND IsDeleted = 0)
BEGIN
    INSERT INTO dbo.ProviderMasters
        (ProviderName, ProviderUrl, ProviderUniqueID, IPAddress,
         IsActive, IsDelete, IsDeleted, IsHLR, IsRealAPI,
         RechargeBalance, DMTBalance, ResponseGroupID, LowBalanceAmount,
         LoginID, Token, SupportNumber, [DateTime], UpdatedateTime)
    VALUES
        ('JaiJui DMT',                               -- ProviderName (the link key)
         'https://api.jaijuirecharge.in/DMT/v1',     -- ProviderUrl  = JaiJui BaseUrl
         '',                                          -- ProviderUniqueID = AuthCode  <-- fill in
         '',                                          -- IPAddress (optional whitelist)
         1,                                           -- IsActive
         0, 0, 0, 0,                                  -- IsDelete, IsDeleted, IsHLR, IsRealAPI
         0, 0, 0, 0,                                  -- RechargeBalance, DMTBalance, ResponseGroupID, LowBalanceAmount
         '',                                          -- LoginID = registered Mobile  <-- fill in
         '',                                          -- Token   = APIToken           <-- fill in
         '',                                          -- SupportNumber
         SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Inserted ProviderMasters: JaiJui DMT';
END
ELSE
    PRINT 'ProviderMasters "JaiJui DMT" already exists — skipped.';

/* ---- 2. Linked DMT gateway (Settings -> DMT Settings / DMTSettings) ------- */
IF NOT EXISTS (SELECT 1 FROM dbo.DMTSettings WHERE Name = 'JaiJuiDMT')
BEGIN
    INSERT INTO dbo.DMTSettings
        (Name, DisplayName, ProviderCode,
         IsActive, IsActiveForWebsite, IsNeft, IsCCF, IsPayoutDMT, IsExpressDMT,
         MinAmount, SplitAmount, SenderLimit, Sequence, [Mode],
         UserName, Token, WalletID, CallBackURL)
    VALUES
        ('JaiJuiDMT',                 -- Name (internal key)
         'JaiJui Money Transfer',     -- DisplayName
         'JaiJui DMT',                -- ProviderCode = ProviderMasters.ProviderName (the LINK)
         0,                           -- IsActive  = INACTIVE until credentials are set
         1,                           -- IsActiveForWebsite
         0, 0, 0, 0,                  -- IsNeft, IsCCF, IsPayoutDMT, IsExpressDMT
         0, 0, 500000, 0,             -- MinAmount, SplitAmount, SenderLimit, Sequence
         '',                          -- Mode ('' | 'IMPS' | 'NEFT' | 'IMPS/NEFT')
         '', '', '', '');             -- UserName, Token, WalletID, CallBackURL (fallback only; creds come from the linked provider)
    PRINT 'Inserted DMTSettings: JaiJuiDMT (inactive)';
END
ELSE
    PRINT 'DMTSettings "JaiJuiDMT" already exists — skipped.';
GO

/* ---- 3. (Optional) set the real credentials by script instead of the UI ---
   Uncomment and fill in, then run. Prefer doing this in Add API → Edit so the
   secrets are typed by you and not saved in a file.

UPDATE dbo.ProviderMasters
   SET LoginID          = '<registered mobile>',
       Token            = '<APIToken>',
       ProviderUniqueID = '<AuthCode>'
 WHERE ProviderName = 'JaiJui DMT' AND IsDeleted = 0;

-- Then activate the DMT gateway once credentials are verified:
-- UPDATE dbo.DMTSettings SET IsActive = 1 WHERE Name = 'JaiJuiDMT';
--------------------------------------------------------------------------- */

/* ---- 4. Verify ----------------------------------------------------------- */
SELECT ProviderID, ProviderName, ProviderUrl,
       CASE WHEN LoginID = '' THEN '(empty)' ELSE '(set)' END  AS Mobile,
       CASE WHEN Token   = '' THEN '(empty)' ELSE '(set)' END  AS APIToken,
       CASE WHEN ProviderUniqueID = '' THEN '(empty)' ELSE '(set)' END AS AuthCode,
       IsActive
  FROM dbo.ProviderMasters
 WHERE ProviderName = 'JaiJui DMT' AND IsDeleted = 0;

SELECT DMTSettingID, Name, DisplayName, ProviderCode, IsActive, SenderLimit
  FROM dbo.DMTSettings
 WHERE Name = 'JaiJuiDMT';
GO
