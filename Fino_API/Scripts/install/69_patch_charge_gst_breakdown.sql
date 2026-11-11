/*
    69 - Per-band GST on a charge, and the breakdown stamped on the transaction.

    WHY THIS EXISTS
    68_ made the commission tax treatment configurable, but only per provider, per service, per slab
    or for the house. A real rate card is not that uniform. JaiJui's card prices DMT in amount
    bands, and a retailer slab can read "500-1000 inclusive, 1001-5000 exclusive, 5001-25000 no
    GST" - three different answers inside one slab, which nothing above the band can express. An
    install whose contracts happen to be uniform is unaffected; one whose aren't had nowhere to put
    the truth.

    The second half is worse, and it is what this patch really fixes: the GST on a charge was never
    RECORDED anywhere per transaction. The provider side was not split at all, so the GST inside a
    DMT charge - which is input credit, real money back - existed in no column. And the three GST
    columns the transaction has carried since the beginning (RetailerGST, DistributerGST,
    SuperDistributerGST) were read by four reports and written by NOTHING: every one of them summed
    zeros and showed a GST total of 0.00 next to crores of commission.

    WHAT IS ADDED
      ProviderConfigurationAdminCommissions   GSTType, GSTRatePercent   - per provider band
      SlabDetails                             GSTType, GSTRatePercent   - per slab band
      TransactionMasters                      the breakdown, both sides, as settled

    GSTType ships as 'DEFAULT' on every existing row, which means "this band does not decide - fall
    through to the commission tax profile". That is deliberate and it is the whole reason this is
    safe to apply to a live database: on the day it runs, not one figure moves. A band only starts
    deciding when somebody opens the screen and sets it.

    GSTRatePercent of 0 means "ask the tax engine", which resolves the service's rate as of the
    transaction's own date, so a future rate change stays effective-dated in one place.

    ON THE TRANSACTION
    The breakdown is stamped once, at the single settlement choke point, and only for a transaction
    that SUCCEEDED - a failed one is refunded in full, commissions and all, so it earns nothing and
    owes no tax; its columns stay 0 and ChargeGSTStampedOn stays NULL. Reports read the stamp rather
    than recomputing, because the rate card can be edited tomorrow and a settled transaction has to
    keep telling the truth about the card it was priced on.

    Figures are SIGNED as every commission in this system is signed: positive = earned,
    negative = paid away (a surcharge, or a provider charge such as DMT). GST and base carry the
    sign of the charge they came out of.

    Every column is added with an explicit DEFAULT and NOT NULL except the timestamp - the live
    database has no defaults on DateTime/IsDeleted where dev does, and a row inserted without them
    fails in production with "Cannot insert the value NULL" having passed every test in dev.

    Idempotent - safe to re-run.
*/
-- QUOTED_IDENTIFIER has to be ON in every batch that ALTERs a table carrying a filtered
-- index: SQL Server refuses the ALTER otherwise. The API's own installer connection has it on,
-- but sqlcmd does not, so a DBA applying this by hand would hit a wall the installer never does.
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== 69: per-band GST + transaction charge breakdown ===';

/* ──────────────────────────────────────────────────────────────────────────────
   One helper, used for every column below: add it only if the table has it not.
   Written as dynamic SQL because ALTER TABLE cannot be parameterised.
   ────────────────────────────────────────────────────────────────────────────── */
DECLARE @cols TABLE (TableName SYSNAME, ColumnName SYSNAME, Definition NVARCHAR(200));

INSERT INTO @cols (TableName, ColumnName, Definition) VALUES
    -- Per-band GST decision on a provider's rate card.
    ('ProviderConfigurationAdminCommissions', 'GSTType',        'varchar(20) NOT NULL CONSTRAINT [DF_PCAC_GSTType] DEFAULT (''DEFAULT'')'),
    ('ProviderConfigurationAdminCommissions', 'GSTRatePercent', 'decimal(9,4) NOT NULL CONSTRAINT [DF_PCAC_GSTRatePercent] DEFAULT (0)'),

    -- Per-band GST decision on a retailer slab. Applies to every tier on the row.
    ('SlabDetails', 'GSTType',        'varchar(20) NOT NULL CONSTRAINT [DF_SlabDetails_GSTType] DEFAULT (''DEFAULT'')'),
    ('SlabDetails', 'GSTRatePercent', 'decimal(9,4) NOT NULL CONSTRAINT [DF_SlabDetails_GSTRatePercent] DEFAULT (0)'),

    -- What the provider charged us, split.
    ('TransactionMasters', 'ProviderChargeType',   'varchar(20) NOT NULL CONSTRAINT [DF_TM_ProviderChargeType] DEFAULT ('''')'),
    ('TransactionMasters', 'ProviderGSTType',      'varchar(20) NOT NULL CONSTRAINT [DF_TM_ProviderGSTType] DEFAULT ('''')'),
    ('TransactionMasters', 'ProviderGSTPercent',   'decimal(9,4) NOT NULL CONSTRAINT [DF_TM_ProviderGSTPercent] DEFAULT (0)'),
    ('TransactionMasters', 'ProviderGSTAmount',    'decimal(18,5) NOT NULL CONSTRAINT [DF_TM_ProviderGSTAmount] DEFAULT (0)'),
    ('TransactionMasters', 'ProviderBaseCharge',   'decimal(18,5) NOT NULL CONSTRAINT [DF_TM_ProviderBaseCharge] DEFAULT (0)'),
    ('TransactionMasters', 'ProviderTotalCharge',  'decimal(18,5) NOT NULL CONSTRAINT [DF_TM_ProviderTotalCharge] DEFAULT (0)'),
    ('TransactionMasters', 'ProviderGSTSource',    'varchar(200) NOT NULL CONSTRAINT [DF_TM_ProviderGSTSource] DEFAULT ('''')'),

    -- What the downline earned, split. RetailerGST / DistributerGST / SuperDistributerGST already
    -- exist and are finally written; these are the parts that had nowhere to go.
    ('TransactionMasters', 'RetailerChargeType',   'varchar(20) NOT NULL CONSTRAINT [DF_TM_RetailerChargeType] DEFAULT ('''')'),
    ('TransactionMasters', 'RetailerGSTType',      'varchar(20) NOT NULL CONSTRAINT [DF_TM_RetailerGSTType] DEFAULT ('''')'),
    ('TransactionMasters', 'RetailerGSTPercent',   'decimal(9,4) NOT NULL CONSTRAINT [DF_TM_RetailerGSTPercent] DEFAULT (0)'),
    ('TransactionMasters', 'RetailerBaseCharge',   'decimal(18,5) NOT NULL CONSTRAINT [DF_TM_RetailerBaseCharge] DEFAULT (0)'),
    ('TransactionMasters', 'RetailerTotalCharge',  'decimal(18,5) NOT NULL CONSTRAINT [DF_TM_RetailerTotalCharge] DEFAULT (0)'),
    ('TransactionMasters', 'RetailerGSTSource',    'varchar(200) NOT NULL CONSTRAINT [DF_TM_RetailerGSTSource] DEFAULT ('''')'),
    ('TransactionMasters', 'DistributerBaseCharge','decimal(18,5) NOT NULL CONSTRAINT [DF_TM_DistributerBaseCharge] DEFAULT (0)'),
    ('TransactionMasters', 'SuperDistributerBaseCharge','decimal(18,5) NOT NULL CONSTRAINT [DF_TM_SuperDistributerBaseCharge] DEFAULT (0)'),

    -- NULL on purpose: "never stamped" is the truthful state for a pending transaction, a failed
    -- one, and everything that settled before this shipped. It is also the idempotency guard.
    ('TransactionMasters', 'ChargeGSTStampedOn',   'datetime2 NULL');

DECLARE @t SYSNAME, @c SYSNAME, @d NVARCHAR(200), @sql NVARCHAR(MAX), @added INT = 0, @skipped INT = 0;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, ColumnName, Definition FROM @cols;
OPEN cur;
FETCH NEXT FROM cur INTO @t, @c, @d;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(@t), N'U') IS NULL
    BEGIN
        PRINT '  SKIP (no such table): ' + @t;
        SET @skipped = @skipped + 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns
                    WHERE [object_id] = OBJECT_ID(QUOTENAME(@t)) AND [name] = @c)
    BEGIN
        SET @skipped = @skipped + 1;
    END
    ELSE
    BEGIN
        SET @sql = N'ALTER TABLE ' + QUOTENAME(@t) + N' ADD ' + QUOTENAME(@c) + N' ' + @d + N';';
        EXEC sp_executesql @sql;
        PRINT '  added ' + @t + '.' + @c;
        SET @added = @added + 1;
    END
    FETCH NEXT FROM cur INTO @t, @c, @d;
END
CLOSE cur; DEALLOCATE cur;

PRINT '  columns added: ' + CAST(@added AS VARCHAR(10)) + ', already present: ' + CAST(@skipped AS VARCHAR(10));
GO
SET QUOTED_IDENTIFIER ON;
-- The statements below reference the columns just added, so they need their own batch: a
-- column name is resolved when the batch is COMPILED, not when it runs, so leaving them here
-- would fail the whole script on a database that does not have the columns yet - which is every
-- database this is meant for.

/* ──────────────────────────────────────────────────────────────────────────────
   Any row that predates the column, or that a screen wrote blank, reads as
   DEFAULT. Normalising it means the fall-through is explicit in the data rather
   than inferred from an empty string in the code.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[ProviderConfigurationAdminCommissions]', N'U') IS NOT NULL
    UPDATE dbo.ProviderConfigurationAdminCommissions
       SET GSTType = 'DEFAULT'
     WHERE GSTType IS NULL OR LTRIM(RTRIM(GSTType)) = '';

IF OBJECT_ID(N'[SlabDetails]', N'U') IS NOT NULL
    UPDATE dbo.SlabDetails
       SET GSTType = 'DEFAULT'
     WHERE GSTType IS NULL OR LTRIM(RTRIM(GSTType)) = '';

/* ──────────────────────────────────────────────────────────────────────────────
   Finding a transaction whose breakdown is missing has to be cheap: the daybook,
   the income report and the GST reconciliation all filter on "stamped".
   A filtered index keeps it to the stamped rows only, which is the small side.
   ────────────────────────────────────────────────────────────────────────────── */
GO
SET QUOTED_IDENTIFIER ON;
IF OBJECT_ID(N'[TransactionMasters]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE [name] = N'IX_TransactionMasters_ChargeGSTStampedOn'
                     AND [object_id] = OBJECT_ID(N'[TransactionMasters]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TransactionMasters_ChargeGSTStampedOn]
        ON dbo.TransactionMasters ([ChargeGSTStampedOn])
        INCLUDE ([ServiceID], [ProviderID], [ProviderGSTAmount], [RetailerGST])
        WHERE [ChargeGSTStampedOn] IS NOT NULL;
    PRINT '  index IX_TransactionMasters_ChargeGSTStampedOn created';
END

GO
SET QUOTED_IDENTIFIER ON;
PRINT '';
PRINT '=== Per-band GST config now in place ===';
IF OBJECT_ID(N'[SlabDetails]', N'U') IS NOT NULL
    SELECT 'SlabDetails' AS [Table], GSTType, COUNT(*) AS Rows_
      FROM dbo.SlabDetails WHERE IsDeleted = 0 GROUP BY GSTType;
IF OBJECT_ID(N'[ProviderConfigurationAdminCommissions]', N'U') IS NOT NULL
    SELECT 'ProviderBands' AS [Table], GSTType, COUNT(*) AS Rows_
      FROM dbo.ProviderConfigurationAdminCommissions WHERE IsDeleted = 0 GROUP BY GSTType;
PRINT 'Every band reads DEFAULT until somebody sets one, so nothing is taxed differently today.';
PRINT '=== 69 complete ===';
