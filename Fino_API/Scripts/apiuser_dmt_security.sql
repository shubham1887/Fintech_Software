/* ============================================================================
   B2B DMT API (/api/v1/dmt) — supporting indexes
   ----------------------------------------------------------------------------
   The DMT endpoints reuse the transaction pipeline and therefore the idempotency
   constraint already created by apiuser_requestid_idempotency.sql:

       UX_TransactionMasters_User_ApiUserRequestID

   RUN THAT SCRIPT FIRST if it has not been run. Without it, two simultaneous
   retries of the same requestid can both pass the service-level duplicate check
   and create two transfers — a real double-send, at DMT ticket sizes.

   This script adds the one index the DMT path needs on top of it.

   Safe to re-run: every step is guarded by an existence check.
   ============================================================================ */

/* ── The per-sender monthly cap ───────────────────────────────────────────────
   ApiUserDmtGatewayService sums a remitter's committed transfers for the current
   calendar month on the hot path of EVERY transfer, before the wallet is touched:

       WHERE DMT_SenderNumber = @mobile
         AND DateTime        >= @monthStart
         AND ServiceID IN (<the DMT services>)
         AND Status NOT IN ('FAILED','FAILURE','REFUND','REFUNDED','REVERSED')

   Without a supporting index that is a scan of TransactionMasters on the
   money-moving path, and it gets slower every month the table grows. The leading
   column is the remitter's mobile because it is the equality predicate; DateTime
   follows so the month range is a seek, not a filter.

   Amount and Status are INCLUDEd so the aggregate is answered entirely from the
   index — the sum never touches the base table. ServiceID is included for the
   same reason: the DMT service ids are resolved in the application (they are
   admin-created rows, not fixed ids), so the filter is applied here.
   ---------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'IX_TransactionMasters_DMTSender_DateTime'
                 AND object_id = OBJECT_ID('dbo.TransactionMasters'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TransactionMasters_DMTSender_DateTime
        ON dbo.TransactionMasters (DMT_SenderNumber, [DateTime])
        INCLUDE (Amount, Status, ServiceID)
        WHERE DMT_SenderNumber <> '' AND IsDeleted = 0;

    PRINT 'Created IX_TransactionMasters_DMTSender_DateTime.';
END
ELSE
    PRINT 'IX_TransactionMasters_DMTSender_DateTime already exists.';
GO

/* ── Verification ─────────────────────────────────────────────────────────────
   Both indexes must be present before the DMT API carries production traffic.
   Expect exactly two rows.
   ---------------------------------------------------------------------------- */
SELECT  i.name          AS IndexName,
        i.is_unique     AS IsUnique,
        i.filter_definition
FROM    sys.indexes i
WHERE   i.object_id = OBJECT_ID('dbo.TransactionMasters')
  AND   i.name IN ('UX_TransactionMasters_User_ApiUserRequestID',
                   'IX_TransactionMasters_DMTSender_DateTime');
GO
