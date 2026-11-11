/* ============================================================================
   Pay1 Utility (BBPS) - provider + biller routing seed
   ----------------------------------------------------------------------------
   Pay1 is configured exactly the way Payin is: as DATA. No schema migration.

     ProviderMasters        the gateway + its credentials ("API Providers" screen)
                              ProviderUrl      = https://recharges.pay1.in
                              LoginID          = partner_id / account id
                              Token            = API key   <-- fill in, see part 1b
                              ProviderUniqueID = partner id shown in the Pay1 panel (31)
     ProviderConfigurations  one row per biller: which operator Pay1 serves, with the
                             Pay1 product_id carried in OpCode (same as Payin's operator code)
     OperatorSequences       the switching order - WITHOUT this row the gateway treats the
                             operator as "no provider authorised" and never dispatches

   Pay1 signs every call with sha1(partner_id + trans_id + api_key), which a
   ProviderDetails URL template cannot produce, so this provider talks through the
   Fino.Pay1Api library instead of endpoint/response rows (the same arrangement
   JaiJui uses for DMT). That is why this script seeds NO ProviderDetails and NO
   ProviderResponseMasters rows - there is nothing for them to do.

   Run against the target DB (e.g. FintechApp) in SSMS / sqlcmd. Idempotent.
   ============================================================================ */

SET NOCOUNT ON;

/* ---- 1. The provider gateway ("API Providers" / ProviderMasters) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ProviderMasters WHERE ProviderName = 'Pay1 BBPS' AND IsDeleted = 0)
BEGIN
    INSERT INTO dbo.ProviderMasters
        (ProviderName, ProviderUrl, ProviderUniqueID, IPAddress,
         IsActive, IsDelete, IsDeleted, IsHLR, IsRealAPI,
         RechargeBalance, DMTBalance, ResponseGroupID, LowBalanceAmount,
         -- CallbackKey arrived with 29_patch_provider_callback_key and is NOT NULL. Without it
         -- named here the insert fails outright; the gateway fills a real one when the client
         -- generates a callback URL.
         LoginID, Token, SupportNumber, CallbackKey, [DateTime], UpdatedateTime)
    VALUES
        ('Pay1 BBPS',                 -- ProviderName: must contain "Pay1" - that is how the
                                      --               gateway recognises the protocol
         'https://recharges.pay1.in', -- ProviderUrl  = host ("/apis/receiveApi" is appended)
         '',                          -- ProviderUniqueID = THIS client's Pay1 partner id <-- fill in
         '',                          -- IPAddress (Pay1 whitelists OUR server IP at their end)
         0,                           -- IsActive: off until the credentials below are filled in
         0, 0, 0, 0,                  -- IsDelete, IsDeleted, IsHLR, IsRealAPI
         0, 0, 0, 0,                  -- RechargeBalance, DMTBalance, ResponseGroupID, LowBalanceAmount
         '',                          -- LoginID = THIS client's partner_id / account id <-- fill in
         '',                          -- Token   = API KEY                               <-- fill in
         '',                          -- SupportNumber
         '',                          -- CallbackKey (issued when one is needed)
         SYSUTCDATETIME(), SYSUTCDATETIME());
    PRINT 'Inserted ProviderMasters: Pay1 BBPS';
END
ELSE
    PRINT 'ProviderMasters "Pay1 BBPS" already exists - skipped.';
GO

/* ---- 1b. The API key ------------------------------------------------------
   Prefer typing it in the UI (API Providers -> Pay1 BBPS -> Edit -> Token) so the
   secret never lands in a file. If you must script it, uncomment and fill in:

UPDATE dbo.ProviderMasters
   SET Token = '<Pay1 API key>'
 WHERE ProviderName = 'Pay1 BBPS' AND IsDeleted = 0;

   Until the key is set, every Pay1 call fails fast with
   "Pay1 credentials are incomplete" and no transaction is dispatched.
   --------------------------------------------------------------------------- */

/* ---- 2. Biller routing ----------------------------------------------------
   One row per biller you want Pay1 to serve. OpCode = Pay1's product_id, which you
   read from the biller catalogue (operation_type 7). Scripts\pay1_biller_list.ps1
   fetches that catalogue and prints these VALUES rows ready to paste.

   Fill @Map with (our operator Opcode, Pay1 product_id) pairs. Everything below it
   is generic - it creates the route and the switching sequence for each pair, and
   re-running only updates what changed.
   --------------------------------------------------------------------------- */
DECLARE @ProviderID INT =
    (SELECT TOP 1 ProviderID FROM dbo.ProviderMasters WHERE ProviderName = 'Pay1 BBPS' AND IsDeleted = 0);

IF @ProviderID IS NULL
BEGIN
    RAISERROR('Pay1 provider row not found - run part 1 first.', 16, 1);
    RETURN;
END

DECLARE @Map TABLE (Opcode VARCHAR(50), ProductID VARCHAR(20));

INSERT INTO @Map (Opcode, ProductID) VALUES
    -- Our OperatorMasters.Opcode  ,  Pay1 product_id
    ('ADANIE', '45');               -- Adani Electricity Mumbai  (product_id 45, from the Pay1 document)
    -- ('TATAPO', '<product_id>'),  -- Tata Power
    -- ('BSESRA', '<product_id>'),  -- BSES Rajdhani
    -- ('MSEDCL', '<product_id>'),  -- MSEDCL (Mahadiscom)
    -- ... paste the rest from pay1_biller_list.ps1

/* -- routes ---------------------------------------------------------------- */
UPDATE pc
   SET OpCode = m.ProductID,
       IsActive = 1,
       UpdatedateTime = SYSUTCDATETIME()
  FROM dbo.ProviderConfigurations pc
  JOIN dbo.OperatorMasters o ON o.OperatorID = pc.OperatorID
  JOIN @Map m ON m.Opcode = o.Opcode
 WHERE pc.ProviderID = @ProviderID AND pc.IsDeleted = 0;

INSERT INTO dbo.ProviderConfigurations
    (OperatorID, ProviderID, ServiceID, OpCode, [Type], Lapu, Quantity,
     AdminComm, AdminCommType, CommissionCategory,
     PendingCount, PendingInMinute, FailCount, FailInMinute,
     IsActive, IsDeleted, [DateTime], UpdatedateTime)
SELECT o.OperatorID, @ProviderID, o.ServiceID, m.ProductID, '', '', '',
       0, 'Fix', '',
       0, 0, 0, 0,
       1, 0, SYSUTCDATETIME(), SYSUTCDATETIME()
  FROM dbo.OperatorMasters o
  JOIN @Map m ON m.Opcode = o.Opcode
 WHERE o.IsDeleted = 0 AND ISNULL(o.IsDelete, 0) = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.ProviderConfigurations pc
                    WHERE pc.OperatorID = o.OperatorID AND pc.ProviderID = @ProviderID AND pc.IsDeleted = 0);

DECLARE @Routes INT = (SELECT COUNT(*) FROM dbo.ProviderConfigurations WHERE ProviderID = @ProviderID AND IsDeleted = 0);
PRINT CONCAT('Routes now mapped to Pay1: ', @Routes);

/* -- switching sequence ----------------------------------------------------
   Without a sequence row the gateway refuses to dispatch the operator at all.

   [Count] is the priority and it sorts ASCENDING - the lowest number is tried
   first. So Pay1 is added at the BACK of whatever the biller already has
   (max existing Count + 10). A biller that is live on another provider keeps
   using it; Pay1 only serves billers that had nobody, and only takes over once
   you deliberately promote it in Settings -> Provider Sequence. Never seed this
   as 1 on a live database - that silently diverts working traffic.             */
INSERT INTO dbo.OperatorSequences
    (ServiceID, OperatorID, ProviderID, [Count], IsDeleted, [DateTime], UpdatedateTime)
SELECT o.ServiceID, o.OperatorID, @ProviderID,
       ISNULL((SELECT MAX(s2.[Count]) FROM dbo.OperatorSequences s2
                WHERE s2.OperatorID = o.OperatorID AND s2.IsDeleted = 0), 0) + 10,
       0, SYSUTCDATETIME(), SYSUTCDATETIME()
  FROM dbo.OperatorMasters o
  JOIN @Map m ON m.Opcode = o.Opcode
 WHERE o.IsDeleted = 0 AND ISNULL(o.IsDelete, 0) = 0
   AND NOT EXISTS (SELECT 1 FROM dbo.OperatorSequences s
                    WHERE s.OperatorID = o.OperatorID AND s.ProviderID = @ProviderID AND s.IsDeleted = 0);

DECLARE @Sequences INT = (SELECT COUNT(*) FROM dbo.OperatorSequences WHERE ProviderID = @ProviderID AND IsDeleted = 0);
PRINT CONCAT('Sequence entries for Pay1: ', @Sequences);

/* -- billers must be flagged as BBPS bill-fetch billers to get the bill screen */
UPDATE o
   SET IsBBPS = 1,
       UpdatedateTime = SYSUTCDATETIME()
  FROM dbo.OperatorMasters o
  JOIN @Map m ON m.Opcode = o.Opcode
 WHERE ISNULL(o.IsBBPS, 0) = 0;
GO

/* ---- 3. Check what is now wired ------------------------------------------ */
SELECT p.ProviderName,
       p.LoginID                                   AS PartnerID,
       CASE WHEN LEN(ISNULL(p.Token, '')) > 0 THEN 'set' ELSE 'MISSING' END AS ApiKey,
       o.OperatorName,
       o.Opcode                                    AS OurCode,
       pc.OpCode                                   AS Pay1ProductID,
       s.Name                                      AS Service,
       pc.IsActive                                 AS RouteActive,
       CASE WHEN seq.OperatorSequenceID IS NULL THEN 'MISSING' ELSE 'ok' END AS Sequence
  FROM dbo.ProviderMasters p
  JOIN dbo.ProviderConfigurations pc ON pc.ProviderID = p.ProviderID AND pc.IsDeleted = 0
  JOIN dbo.OperatorMasters o ON o.OperatorID = pc.OperatorID
  JOIN dbo.ServiceMasters s ON s.ServiceID = o.ServiceID
  LEFT JOIN dbo.OperatorSequences seq
         ON seq.OperatorID = o.OperatorID AND seq.ProviderID = p.ProviderID AND seq.IsDeleted = 0
 WHERE p.ProviderName = 'Pay1 BBPS' AND p.IsDeleted = 0
 ORDER BY s.Name, o.OperatorName;

/* ---- 4. Still to do outside this script -----------------------------------
   a) API key           API Providers -> Pay1 BBPS -> Edit -> Token
   b) Our server IP     give it to Pay1 - they only accept whitelisted IPs
   c) Status update URL give Pay1:  https://<our-host>/api/Callback/pay1
                        (signed push; it also carries their reversals)
   d) Biller inputs     each biller's extra fields (billing unit, cycle) are the
                        existing BBPSConfiguration rows for that operator - field 1
                        is the consumer number, fields 2 and 3 become Pay1's
                        "param" and "param1"
   --------------------------------------------------------------------------- */
