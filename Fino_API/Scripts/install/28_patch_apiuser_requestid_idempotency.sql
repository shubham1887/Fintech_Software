/* ============================================================================
   B2B API (/api/v1) — request-id idempotency hardening
   ----------------------------------------------------------------------------
   ApiUserGatewayService.Recharge already replays an existing transaction when it
   sees a requestid it has served before. That check is a read followed by a
   write, so two *simultaneous* retries of the same requestid can both pass it
   and create two transactions — a real double-charge under a client that fires
   parallel retries.

   This filtered unique index closes that window in the database: the second
   concurrent insert fails, the recharge is not created, and the caller gets a
   clean error instead of a duplicate charge. The filter keeps every non-API
   transaction (empty APIUserRequestID) out of the constraint.

   RUN ORDER
     1. Run STEP 1 and confirm it returns no rows.
     2. If it returns rows, clean them up (see the note) before continuing.
     3. Run STEP 2.
   Safe to re-run: STEP 2 is guarded by an existence check.
   ============================================================================ */

/* ── STEP 1 — existing duplicates. Must return ZERO rows before STEP 2. ────── */
SELECT  UserID,
        APIUserRequestID,
        COUNT(*)                AS Duplicates,
        MIN(TransactionMasterID) AS FirstTxnID,
        MAX(TransactionMasterID) AS LastTxnID
FROM    TransactionMasters
WHERE   ISNULL(APIUserRequestID, '') <> ''
  AND   IsDeleted = 0
GROUP BY UserID, APIUserRequestID
HAVING  COUNT(*) > 1
ORDER BY Duplicates DESC;

/* If STEP 1 returns rows, each group is a historical double-charge. Do NOT
   delete them — they are real money. Investigate and refund/settle through the
   admin screens first, then blank the request id on the transactions that must
   not hold the key, e.g.:

     UPDATE TransactionMasters
     SET    APIUserRequestID = APIUserRequestID + '-DUP-' + CAST(TransactionMasterID AS varchar(20))
     WHERE  TransactionMasterID IN (<the ids to release>);
*/

/* ── STEP 2 — the constraint ────────────────────────────────────────────────── */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_TransactionMasters_User_ApiUserRequestID'
                 AND object_id = OBJECT_ID('dbo.TransactionMasters'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_TransactionMasters_User_ApiUserRequestID
        ON dbo.TransactionMasters (UserID, APIUserRequestID)
        WHERE APIUserRequestID <> '' AND IsDeleted = 0;

    PRINT 'Created UX_TransactionMasters_User_ApiUserRequestID.';
END
ELSE
    PRINT 'UX_TransactionMasters_User_ApiUserRequestID already exists.';
GO

/* This index also serves the two hot lookups of the B2B API — the idempotency
   check on /api/v1/recharge and the resolve step of /api/v1/status — so it pays
   for itself in read performance as well as correctness. */
