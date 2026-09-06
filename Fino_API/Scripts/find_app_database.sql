/* ============================================================================
   Which database is the application ACTUALLY using?
   ----------------------------------------------------------------------------
   Run this on the SQL SERVER the API connects to. It answers the question the
   repo's appsettings.json cannot: a connection string can live in an environment
   variable, in web.config, in an appsettings.<Environment>.json on the server —
   and if it carries no "Initial Catalog", the connection silently lands in the
   login's DEFAULT database (usually master).

   Run it from any database; it reads server-level views only and changes nothing.
   ============================================================================ */

/* ── 1. The database the live app connections are sitting in ────────────────── */
SELECT  DB_NAME(s.database_id)  AS AppDatabase,
        s.login_name,
        s.host_name,
        s.program_name,
        COUNT(*)                AS Connections
  FROM  sys.dm_exec_sessions s
 WHERE  s.is_user_process = 1
   AND (s.program_name LIKE '%SqlClient%' OR s.program_name LIKE '%.Net%' OR s.program_name LIKE '%Core%')
 GROUP BY DB_NAME(s.database_id), s.login_name, s.host_name, s.program_name
 ORDER BY Connections DESC;
/* Empty result = the API is stopped. Start it, hit any provider screen, re-run. */

/* ── 2. Every database on this server that holds ProviderMasters ─────────────
   The one with real provider rows is the application database. That is where
   provider_callback_key.sql must run — and HasCallbackKey tells you whether it
   already did.                                                                 */
DECLARE @sql nvarchar(max) = N'';

SELECT @sql = @sql + N'
SELECT ' + QUOTENAME(name, '''') + N' AS DbName,
       (SELECT COUNT(*) FROM ' + QUOTENAME(name) + N'.sys.tables WHERE name = ''ProviderMasters'') AS HasTable,
       (SELECT COUNT(*) FROM ' + QUOTENAME(name) + N'.sys.columns c
          JOIN ' + QUOTENAME(name) + N'.sys.tables t ON t.object_id = c.object_id
         WHERE t.name = ''ProviderMasters'' AND c.name = ''CallbackKey'') AS HasCallbackKey
 WHERE EXISTS (SELECT 1 FROM ' + QUOTENAME(name) + N'.sys.tables WHERE name = ''ProviderMasters'')
UNION ALL'
  FROM sys.databases
 WHERE state = 0                                  -- online only
   AND name NOT IN ('tempdb', 'model', 'msdb');

SET @sql = @sql + N'
SELECT ''-'' AS DbName, 0 AS HasTable, 0 AS HasCallbackKey WHERE 1 = 0;';

EXEC sys.sp_executesql @sql;

/* ── 3. Sanity check on the winner ────────────────────────────────────────────
   Replace <AppDatabase> with the database from step 1/2 and confirm it really is
   the live one — recent transactions mean live traffic, not an old copy.
   ---------------------------------------------------------------------------
   USE [<AppDatabase>];
   SELECT COUNT(*) AS Providers FROM dbo.ProviderMasters;
   SELECT TOP 5 TransactionMasterID, DateTime FROM dbo.TxnMasters ORDER BY TransactionMasterID DESC;
   --------------------------------------------------------------------------- */
