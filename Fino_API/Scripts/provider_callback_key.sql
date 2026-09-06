/* ============================================================================
   One shared inbound callback address for every provider
   ----------------------------------------------------------------------------
   Adds ProviderMaster.CallbackKey — the public, opaque id that tells the single
   endpoint

        https://<our-host>/api/Callback/p/{CallbackKey}

   WHICH upstream is pushing. Before this, every provider needed its own route
   (/api/Callback/recharge, /api/Callback/pay1, ...). Now a new integration is a
   provider row plus this key; the endpoint, the verification, the settlement and
   the logging are shared.

   The key is NOT a credential — it travels in a URL and sits in the provider's
   dashboard. Authenticity is proved separately: Pay1's hash_code, and the IP
   allowlist in ProviderMaster.IPAddress (set it for any provider that signs
   nothing — leave it blank and the key alone is the only lock).

   Safe to re-run: the column, the backfill and the index are each guarded.
   After running, RESTART the API — ProviderMasterRepository caches the provider
   list in memory, so a direct SQL change is invisible until it does.
   ============================================================================ */

/* ── Session settings — REQUIRED, and required FIRST ──────────────────────────
   The filtered index in STEP 3 cannot be created under QUOTED_IDENTIFIER OFF, and
   once it exists NO UPDATE on the table is allowed under OFF either — so STEP 2
   fails too on any later run. sqlcmd connects with it OFF by default (SSMS has it
   ON), which is exactly how a "safe to re-run" script stops being safe to re-run.
   Set once here; SET options persist across GO for the whole connection.         */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ── STEP 0 — refuse to run against the wrong database ─────────────────────────
   Running this in the SSMS default context puts the column in `master` while the
   application keeps failing on its own database — the failure looks identical, so
   the wasted hour is spent looking at the wrong place. This guard turns that into
   a loud message instead: SET NOEXEC ON survives GO, so every batch below is
   parsed and skipped, and nothing is changed.
   Pick the application database in the SSMS dropdown (or add USE [<db>]) and re-run.
   Not sure which one it is? Run Scripts/find_app_database.sql first.            */
IF DB_NAME() IN ('master', 'model', 'msdb', 'tempdb') OR OBJECT_ID('dbo.ProviderMasters') IS NULL
BEGIN
    PRINT '*** WRONG DATABASE: currently in [' + DB_NAME() + ']. Nothing was changed.';
    PRINT '*** Select the application database (the one holding dbo.ProviderMasters) and run again.';
    SET NOEXEC ON;
END
GO

/* ── STEP 1 — the column, with a self-generating default ──────────────────────
   The default is NOT cosmetic: NEWID() is evaluated per row, so ANY insert that
   does not name the column still gets its own key — the API's own Create path,
   the provider seed scripts (jaijui_provider_seed.sql, pay1_bbps_seed.sql) and
   anything added by hand in SSMS. That is what makes "every provider has a
   callback address" true going forward instead of a step someone must remember. */
IF NOT EXISTS (SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.ProviderMasters') AND name = 'CallbackKey')
BEGIN
    ALTER TABLE dbo.ProviderMasters ADD CallbackKey varchar(64) NOT NULL
        CONSTRAINT DF_ProviderMasters_CallbackKey
        DEFAULT ('pv_' + LOWER(LEFT(REPLACE(CONVERT(varchar(64), NEWID()), '-', ''), 24)));
    PRINT 'CallbackKey column added (self-generating default).';
END
ELSE
    PRINT 'CallbackKey column already present.';
GO

/* ── STEP 2 — a key for every existing provider ────────────────────────────────
   'pv_' + 24 random hex characters, the same shape the application generates for
   new providers. Row-by-row on purpose: NEWID() inside a set-based UPDATE would
   be evaluated once and hand every provider the SAME key.                        */
WHILE EXISTS (SELECT 1 FROM dbo.ProviderMasters WHERE ISNULL(CallbackKey, '') = '')
BEGIN
    UPDATE TOP (1) dbo.ProviderMasters
       SET CallbackKey = 'pv_' + LOWER(LEFT(REPLACE(CONVERT(varchar(64), NEWID()), '-', ''), 24))
     WHERE ISNULL(CallbackKey, '') = '';
END
GO

/* ── STEP 3 — no two providers may share a key (filtered index) ────────────── */
IF NOT EXISTS (SELECT 1 FROM sys.indexes
                WHERE object_id = OBJECT_ID('dbo.ProviderMasters') AND name = 'UX_ProviderMasters_CallbackKey')
BEGIN
    CREATE UNIQUE INDEX UX_ProviderMasters_CallbackKey
        ON dbo.ProviderMasters (CallbackKey)
        WHERE CallbackKey <> '';
    PRINT 'Unique index created.';
END
GO

/* ── STEP 4 — the addresses to hand out ───────────────────────────────────────
   Replace the host with the API's public host before sending these to anyone.   */
SELECT  ProviderID,
        ProviderName,
        CallbackKey,
        'https://api.ripay.co.in/api/Callback/p/' + CallbackKey AS CallbackURL,
        CASE WHEN ISNULL(IPAddress, '') = ''
             THEN 'NOT SET - key is the only lock; add the provider''s source IP'
             ELSE IPAddress END                                 AS AllowedSourceIP
  FROM  dbo.ProviderMasters
 WHERE  ISNULL(IsDelete, 0) = 0 AND ISNULL(IsDeleted, 0) = 0
 ORDER BY ProviderName;

/* ── STEP 5 — release the guard ────────────────────────────────────────────── */
SET NOEXEC OFF;
GO
