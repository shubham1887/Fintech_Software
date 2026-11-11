/* ============================================================================
   Payin - repair the recharge response rules on an EXISTING install
   ----------------------------------------------------------------------------
   Run this on an install where Payin recharges sit in the pending queue for
   hours and the status-check log shows the provider answering clearly
   ("status":"SUCCESS" / "FAILURE" / "NotFound") while the transaction never
   moves. That is the signature of ProviderResponseMasters having no usable rule
   for Payin: the gateway holds every unmatched reply as PENDING on purpose, so
   with no rules nothing ever settles - a recharge Payin has already delivered
   stays pending and the agent's money stays frozen.

   This script touches ONE table, dbo.ProviderResponseMasters, and only the rows
   belonging to the provider named below. Endpoints, operator routes, switching
   sequences and credentials are left exactly as the client set them.

   It is the same rule set that payin_provider_seed.sql installs on a fresh
   database, so running either leaves the install in the same state. Idempotent
   and re-runnable.

   Run:  sqlcmd -S "SERVER\INSTANCE" -d YourDb -E -C -I -i payin_response_rules_repair.sql
   ============================================================================ */

SET NOCOUNT ON;

DECLARE @now      datetime2(7) = SYSUTCDATETIME();
DECLARE @provider varchar(150) = 'Payin';

DECLARE @providerId int =
    (SELECT TOP 1 ProviderID FROM dbo.ProviderMasters WHERE ProviderName = @provider AND IsDeleted = 0);

IF @providerId IS NULL
BEGIN
    PRINT CONCAT('No provider named "', @provider, '" on this database - nothing to repair.');
    RETURN;
END

/* Placeholder rules from the first cut of the seed: they match a reply shape
   Payin never sends, so they could only ever leave a transaction unmatched.
   Matched on their exact text, so a rule the client wrote is never touched. */
DELETE FROM dbo.ProviderResponseMasters
WHERE  ProviderID = @providerId
  AND  TextMustexits IN (N'"state":"done","orderRef"', N'"state":"rejected"');

IF @@ROWCOUNT > 0 PRINT 'Placeholder rules removed.';

/* SUCCESS finalises and pays commission, FAILURE refunds, PROCESS is held for
   the next status-check sweep, and NotFound means Payin never accepted the
   request id - it rejects duplicates, so an id it does not know was never
   taken, and the wallet must go back. */
;WITH src (TextMustexits, Type, Status, Response) AS (
  SELECT * FROM (VALUES
    (N'"status":"SUCCESS"', N'TRANSACTION', N'SUCCESS',
     N'"message":"[Message]","status":"SUCCESS","transactionID":"[RefID]"'),
    (N'"status":"FAILURE"', N'TRANSACTION', N'FAILED',
     N'"message":"[Message]","status":"FAILURE"'),
    (N'"status":"PROCESS"', N'TRANSACTION', N'PENDING',
     N'"message":"[Message]","status":"PROCESS"'),

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

PRINT CONCAT('Rules added: ', @@ROWCOUNT);

/* A rule that is present but switched off reads the same as a missing one. */
UPDATE dbo.ProviderResponseMasters
SET    IsActive = 1, UpdatedateTime = @now
WHERE  ProviderID = @providerId
  AND  IsDeleted = 0
  AND  IsActive = 0
  AND  TextMustexits IN (N'"status":"SUCCESS"', N'"status":"FAILURE"',
                         N'"status":"PROCESS"', N'"status":"NotFound"');

IF @@ROWCOUNT > 0 PRINT CONCAT('Rules re-activated: ', @@ROWCOUNT);

SELECT ProviderResponseMasterID, Type, Status, TextMustexits, IsActive
FROM   dbo.ProviderResponseMasters
WHERE  ProviderID = @providerId AND IsDeleted = 0
ORDER  BY Type, Status, TextMustexits;

/* ============================================================================
   After this runs, the API picks the rules up on its next provider-cache read
   (restart the app pool to be certain), and the recharge pending queue settles
   on the next status-check sweep - SUCCESS finalised, NotFound refunded.
   ============================================================================ */
