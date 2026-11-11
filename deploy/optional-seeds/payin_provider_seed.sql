/* ============================================================================
   Payin - recharge provider seed (prepaid + DTH)
   ----------------------------------------------------------------------------
   Payin is configured as DATA, the same way the other gateways are. No schema
   migration.

     ProviderMasters         the gateway and its credentials ("API Providers")
                               ProviderUrl = https://api.payin.live
                               LoginID     = the Mobile registered with Payin
                               Token       = the APIToken from the Payin panel
     ProviderDetails         one row per endpoint (recharge / balance / status).
                               The URLs carry [UserName] and [Token] placeholders,
                               which the gateway fills from ProviderMasters at
                               call time - so the URLs hold no secret.
     ProviderResponseMasters how Payin's replies map to SUCCESS / FAILED.
     ProviderConfigurations  which operator Payin serves, and under what OpCode.

   CREDENTIALS ARE LEFT BLANK. They belong to one client's contract with Payin,
   never to this repository. Fill them in Add API -> Edit after installing, or
   with the UPDATE at the bottom of this file. Until they are set the provider
   simply never routes - nothing breaks.

   Idempotent and re-runnable. Everything is matched on natural keys (provider
   name, operator name, endpoint type), never on identity values, because
   ProviderID / ServiceID / OperatorID differ between databases.

   Needs 47_seed_service_and_operator_catalogue.sql to have run first - that is
   where the operators named below come from.

   Run:  sqlcmd -S "SERVER\INSTANCE" -d YourDb -E -C -I -i payin_provider_seed.sql
   ============================================================================ */

SET NOCOUNT ON;

DECLARE @now datetime2(7) = SYSUTCDATETIME();
DECLARE @provider varchar(150) = 'Payin';

/* ------------------------------------------------------- 1. the gateway row */
IF NOT EXISTS (SELECT 1 FROM dbo.ProviderMasters WHERE ProviderName = @provider AND IsDeleted = 0)
BEGIN
    INSERT INTO dbo.ProviderMasters
          (ProviderName, ProviderUrl, ProviderUniqueID, IPAddress, IsDelete, IsActive, IsHLR,
           RechargeBalance, DMTBalance, ResponseGroupID, IsRealAPI, LoginID, Token, SupportNumber,
           LowBalanceAmount, [DateTime], UpdatedateTime, IsDeleted, CallbackKey)
    VALUES (@provider, 'https://api.payin.live', '', '', 0, 0, 0,
            0, 0, 0, 1, '', '', '', 100, @now, @now, 0, '');

    PRINT 'Payin provider row created (inactive, no credentials).';
END
ELSE
    PRINT 'Payin provider row already present - left as it is.';

DECLARE @providerId int =
    (SELECT TOP 1 ProviderID FROM dbo.ProviderMasters WHERE ProviderName = @provider AND IsDeleted = 0);

/* ----------------------------------------------------- 2. the endpoint rows */
;WITH src (ServiceName, ServiceCategory, UrlType, ProviderURL, MethodType, DateFormat) AS (
  SELECT * FROM (VALUES
    (N'Prepaid Recharge', N'Mobile Services', N'TRANSACTION',
     N'https://api.payin.live/RechargeAPI/v1/GetRecharge?Mobile=[UserName]&APIToken=[Token]&OperatorID=[OperatorCode]&APIUserRequestID=[RequestID]&ConsumerNumber=[Number]&Amount=[Amount]&Pin=&Param1=&Param2=&Param3=&Param4=&Param5=',
     N'GET', N''),
    (N'Prepaid Recharge', N'Mobile Services', N'BALANCE',
     N'https://api.payin.live/RechargeAPI/v1/GetBalance?Mobile=[UserName]&APIToken=[Token]',
     N'GET', N''),
    (N'Prepaid Recharge', N'Mobile Services', N'STATUS',
     N'https://api.payin.live/RechargeAPI/v1/GetStatusCheck?Mobile=[UserName]&APIToken=[Token]&APIUserRequestID=[RequestID]&DateInteger=[Date]',
     N'GET', N'yyyyMMdd'),
    (N'DTH Recharge', N'DTH Services', N'TRANSACTION',
     N'https://api.payin.live/RechargeAPI/v1/GetRecharge?Mobile=[UserName]&APIToken=[Token]&OperatorID=[OperatorCode]&APIUserRequestID=[RequestID]&ConsumerNumber=[Number]&Amount=[Amount]&Pin=&Param1=&Param2=&Param3=&Param4=&Param5=',
     N'GET', N'')
  ) v (ServiceName, ServiceCategory, UrlType, ProviderURL, MethodType, DateFormat)
)
INSERT INTO dbo.ProviderDetails
      (ProviderID, ServiceID, UrlType, ProviderURL, MethodType, Balance, HeaderJson, DateFormat,
       IsActive, TransactionIDPrefix, [DateTime], UpdatedateTime, IsDeleted)
SELECT @providerId, s.ServiceID, src.UrlType, src.ProviderURL, src.MethodType, 0, '', src.DateFormat,
       1, '', @now, @now, 0
FROM   src
JOIN   dbo.ServiceMasters s
       ON s.Name = src.ServiceName AND s.ServiceCategory = src.ServiceCategory AND s.IsDeleted = 0
WHERE  NOT EXISTS (
    SELECT 1 FROM dbo.ProviderDetails d
    WHERE  d.ProviderID = @providerId AND d.ServiceID = s.ServiceID
      AND  d.UrlType = src.UrlType AND d.IsDeleted = 0);

PRINT CONCAT('Payin endpoints added: ', @@ROWCOUNT);

/* --------------------------------------------- 3. how to read Payin's reply */
/* Payin answers EVERY call - the recharge and the status enquiry alike - with a
   flat JSON object carrying a "status" of SUCCESS / FAILURE / PROCESS, and the
   status enquiry adds a fourth value, NotFound. These rules are what turn that
   one word into a settlement:

       SUCCESS   -> the transaction is finalised and commission is paid
       FAILURE   -> the wallet is refunded
       PROCESS   -> held, and re-enquired by the status-check sweep
       NotFound  -> Payin never accepted this request id. It rejects duplicate
                    ids, so an id it does not know was never taken -> refund.

   A reply that matches NO rule is deliberately held as PENDING by the gateway.
   That is the safe default for one odd reply, but with no rules at all NOTHING
   ever settles: a recharge Payin has already delivered sits pending for ever
   and the agent's money stays frozen. The STATUS rules matter as much as the
   TRANSACTION ones - they are the half that closes the pending queue.

   The match text is a comma-separated list of fragments that must ALL appear in
   the reply, so no fragment may itself contain a comma. Whitespace around the
   JSON separators does not matter. Values the template does not pick up
   (balances, commission, description) are read out of the reply by the
   gateway's own JSON backfill, so the templates stay short on purpose.        */

/* The first cut of this seed carried placeholder rules that matched a reply
   shape Payin never sends ("state":"done" / "state":"rejected"), so every Payin
   transaction went unmatched and stayed pending. Drop them - matched on their
   exact text, so a rule the client wrote themselves is never touched.         */
DELETE FROM dbo.ProviderResponseMasters
WHERE  ProviderID = @providerId
  AND  TextMustexits IN (N'"state":"done","orderRef"', N'"state":"rejected"');

IF @@ROWCOUNT > 0 PRINT 'Payin placeholder response rules removed.';

;WITH src (TextMustexits, Type, Status, Response) AS (
  SELECT * FROM (VALUES
    /* ---- the recharge call ---------------------------------------------- */
    (N'"status":"SUCCESS"', N'TRANSACTION', N'SUCCESS',
     N'"message":"[Message]","status":"SUCCESS","transactionID":"[RefID]"'),
    (N'"status":"FAILURE"', N'TRANSACTION', N'FAILED',
     N'"message":"[Message]","status":"FAILURE"'),
    (N'"status":"PROCESS"', N'TRANSACTION', N'PENDING',
     N'"message":"[Message]","status":"PROCESS"'),

    /* ---- the status enquiry --------------------------------------------- */
    (N'"status":"SUCCESS"', N'STATUS', N'SUCCESS',
     N'"status":"SUCCESS","apiUserRequestID":"[SkipRequestID]","referenceNo":"[RefID]"'),
    (N'"status":"FAILURE"', N'STATUS', N'FAILED',
     N'"status":"FAILURE","apiUserRequestID":"[SkipRequestID]","referenceNo":"[RefID]"'),
    (N'"status":"PROCESS"', N'STATUS', N'PENDING',
     N'"status":"PROCESS"'),
    (N'"status":"NotFound"', N'STATUS', N'FAILED',
     N'"status":"NotFound"')
  ) v (TextMustexits, Type, Status, Response)
)
INSERT INTO dbo.ProviderResponseMasters
      (ProviderID, TextMustexits, Type, Status, Response, IsReference, IsActive,
       [DateTime], UpdatedateTime, IsDeleted)
SELECT @providerId, src.TextMustexits, src.Type, src.Status, src.Response, 0, 1, @now, @now, 0
FROM   src
WHERE  NOT EXISTS (
    SELECT 1 FROM dbo.ProviderResponseMasters r
    WHERE  r.ProviderID = @providerId AND r.TextMustexits = src.TextMustexits
      AND  r.Type = src.Type AND r.IsDeleted = 0);

PRINT CONCAT('Payin response rules added: ', @@ROWCOUNT);

/* ------------------------------------- 4. which operators Payin serves, how */
/* OpCode is Payin's own operator id. Without a row here the gateway treats the
   operator as "no provider authorised" and never dispatches it.               */
;WITH src (ServiceName, ServiceCategory, OperatorName, OpCode) AS (
  SELECT * FROM (VALUES
    (N'Prepaid Recharge', N'Mobile Services', N'Airtel',             N'14'),
    (N'Prepaid Recharge', N'Mobile Services', N'BSNL',               N'15'),
    (N'Prepaid Recharge', N'Mobile Services', N'Vodafone Idea (Vi)', N'16'),
    (N'Prepaid Recharge', N'Mobile Services', N'Reliance Jio',       N'17')
  ) v (ServiceName, ServiceCategory, OperatorName, OpCode)
)
INSERT INTO dbo.ProviderConfigurations
      (OperatorID, ProviderID, ServiceID, OpCode, Type, Lapu, Quantity, AdminComm, AdminCommType,
       IsActive, CommissionCategory, PendingCount, PendingInMinute, FailCount, FailInMinute,
       [DateTime], UpdatedateTime, IsDeleted)
SELECT o.OperatorID, @providerId, s.ServiceID, src.OpCode, '', '', 0, 0, 'Fix',
       1, '', 0, 0, 0, 0, @now, @now, 0
FROM   src
JOIN   dbo.ServiceMasters s
       ON s.Name = src.ServiceName AND s.ServiceCategory = src.ServiceCategory AND s.IsDeleted = 0
JOIN   dbo.OperatorMasters o
       ON o.ServiceID = s.ServiceID AND o.OperatorName = src.OperatorName AND o.IsDeleted = 0
WHERE  NOT EXISTS (
    SELECT 1 FROM dbo.ProviderConfigurations c
    WHERE  c.ProviderID = @providerId AND c.OperatorID = o.OperatorID AND c.IsDeleted = 0);

PRINT CONCAT('Payin operator routes added: ', @@ROWCOUNT);

/* ------------------------------------- 5. the switching order per operator */
/* OperatorSequences decides which provider is tried first. One row per
   operator Payin serves, so a fresh install dispatches instead of reporting
   "no provider authorised".                                                  */
INSERT INTO dbo.OperatorSequences (ServiceID, OperatorID, ProviderID, [Count], [DateTime], UpdatedateTime, IsDeleted)
SELECT c.ServiceID, c.OperatorID, c.ProviderID, 1, @now, @now, 0
FROM   dbo.ProviderConfigurations c
WHERE  c.ProviderID = @providerId AND c.IsDeleted = 0
  AND  NOT EXISTS (
    SELECT 1 FROM dbo.OperatorSequences q
    WHERE  q.OperatorID = c.OperatorID AND q.ProviderID = c.ProviderID AND q.IsDeleted = 0);

PRINT CONCAT('Payin switching rows added: ', @@ROWCOUNT);

/* ============================================================================
   LAST STEP - the client's own credentials, then switch it on.

   Do this in the panel (Add API -> Payin -> Edit), or here:

       UPDATE dbo.ProviderMasters
       SET    LoginID  = '<the Mobile registered with Payin>',
              Token    = '<the APIToken from the Payin panel>',
              IsActive = 1
       WHERE  ProviderName = 'Payin' AND IsDeleted = 0;

   Leave IsActive = 0 until the credentials are in: an active provider with no
   token fails every recharge it is handed.
   ============================================================================ */
