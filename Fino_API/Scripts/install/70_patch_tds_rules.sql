/*
    70 - Dynamic TDS: rules per party, and what was done with each month of deductions.

    WHY THIS EXISTS
    68_ gave TDS a switch - on or off per service, provider or slab, at the section's statutory
    rate. The business does not work that way. "0.01% on this service for every retailer, except
    this one distributor who is on a flat Rs 2" is an ordinary instruction in this trade, and there
    was nowhere to write it down: the only dials were a boolean and one rate, scoped to a service.

    So TDS was configurable in the way a developer would find reasonable and not in the way the
    desk actually needs. TdsRules fixes that with one row per decision, matched MOST SPECIFIC
    FIRST, exactly as the tax rules and the commission tax profiles are matched:

        USER_SERVICE       one user, on one service          (the narrowest thing anyone can say)
        USER               one user, every service
        USERTYPE_SERVICE   a whole role, on one service
        USERTYPE           a whole role, every service
        SERVICE            one service, whoever earns on it
        GLOBAL             the house default

    Each row carries whether the figure is a PERCENT or a FIX (flat rupees per transaction), what
    it is computed on (the commission NET of GST, which is the correct default because TDS is a tax
    on income - or the commission as quoted, where a contract says so), a minimum commission below
    which nothing is deducted, a per-transaction cap, the section, and whether the section's higher
    no-PAN rate applies.

    ChargeValue is decimal(18,4) on purpose: the brief's own example is 0.01%, and a rate like
    0.0125% is a real thing. Note for anyone reading a row: 0.01 means 0.01 PERCENT, not 1%.

    THE MASTER SWITCH IS A KEY SETTING, NOT A COLUMN
    'IsTDSEnabled' is seeded into SettingMaster below, OFF. While it is off no rule is read and
    nothing is deducted, so this patch and the rules written after it change nothing until somebody
    deliberately turns TDS on - which matters, because turning it on starts withholding real money
    from real people's commissions.

    TDS IS NOW ACTUALLY WITHHELD
    Until now it was computed and recorded but never came out of anybody's wallet: the platform
    reported a liability it had not collected. It is now debited from the same wallet the commission
    was credited to, through the same posting service (locked, with a ledger row and a line on the
    user's statement). A debit that cannot be funded records Withheld = 0 instead of failing the
    settlement - which is why that column exists, and why the monthly report keeps those apart.
    Depositing money the platform never collected is paying it twice.

    AND THEN ONE OF TWO THINGS HAPPENS TO IT
    TdsSettlements records, once per (month, party): DEPOSITED with the government against its
    challan reference, or WALLET_SETTLED - given back to the party as wallet balance. The unique
    index is what stops a month being settled twice, because settling twice either pays the
    government the same money twice or credits the same wallet twice.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does, and an INSERT that omits them passes every dev test and
    fails in production with "Cannot insert the value NULL".

    Idempotent - safe to re-run.
*/
-- QUOTED_IDENTIFIER has to be ON in every batch that ALTERs a table carrying a filtered index:
-- SQL Server refuses the ALTER otherwise. The API's installer connection has it on; sqlcmd does
-- not, so a DBA applying this by hand would hit a wall the installer never does.
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== 70: dynamic TDS rules + monthly settlement ===';

/* ──────────────────────────────────────────────────────────────────────────────
   1. The rules.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[TdsRules]', N'U') IS NULL
BEGIN
    CREATE TABLE [TdsRules] (
        [TdsRuleID]            int IDENTITY(1,1) NOT NULL,
        -- USER_SERVICE / USER / USERTYPE_SERVICE / USERTYPE / SERVICE / GLOBAL
        [Scope]                varchar(20)    NOT NULL CONSTRAINT [DF_TdsRules_Scope] DEFAULT ('GLOBAL'),
        -- 0 = any. A scope only matches when the thing it names is filled in, so a USER rule with
        -- UserID 0 matches nobody rather than everybody.
        [UserID]               int            NOT NULL CONSTRAINT [DF_TdsRules_UserID] DEFAULT (0),
        [UserTypeID]           int            NOT NULL CONSTRAINT [DF_TdsRules_UserTypeID] DEFAULT (0),
        [ServiceID]            int            NOT NULL CONSTRAINT [DF_TdsRules_ServiceID] DEFAULT (0),
        [OperatorID]           int            NOT NULL CONSTRAINT [DF_TdsRules_OperatorID] DEFAULT (0),
        -- PERCENT or FIX
        [ChargeType]           varchar(20)    NOT NULL CONSTRAINT [DF_TdsRules_ChargeType] DEFAULT ('PERCENT'),
        -- A percentage (0.01 = 0.01%) or rupees per transaction.
        [ChargeValue]          decimal(18,4)  NOT NULL CONSTRAINT [DF_TdsRules_ChargeValue] DEFAULT (0),
        -- COMMISSION_NET (GST backed out - correct, TDS is on income) or COMMISSION_GROSS
        [ApplyOn]              varchar(20)    NOT NULL CONSTRAINT [DF_TdsRules_ApplyOn] DEFAULT ('COMMISSION_NET'),
        [MinCommission]        decimal(18,4)  NOT NULL CONSTRAINT [DF_TdsRules_MinCommission] DEFAULT (0),
        [MaxTDSPerTransaction] decimal(18,4)  NOT NULL CONSTRAINT [DF_TdsRules_MaxTDS] DEFAULT (0),
        -- Blank = 194H, commission or brokerage, which is what this trade's commissions are.
        [SectionCode]          varchar(20)    NOT NULL CONSTRAINT [DF_TdsRules_SectionCode] DEFAULT (''),
        [ApplyNoPANRate]       bit            NOT NULL CONSTRAINT [DF_TdsRules_NoPAN] DEFAULT (1),
        [EffectiveFrom]        date           NOT NULL CONSTRAINT [DF_TdsRules_EffFrom] DEFAULT ('2000-01-01'),
        [EffectiveTo]          date           NULL,
        -- 0 lets the scope decide, so nobody has to invent numbers for the obvious behaviour.
        [Priority]             int            NOT NULL CONSTRAINT [DF_TdsRules_Priority] DEFAULT (0),
        [Remark]               varchar(300)   NOT NULL CONSTRAINT [DF_TdsRules_Remark] DEFAULT (''),
        [IsActive]             bit            NOT NULL CONSTRAINT [DF_TdsRules_IsActive] DEFAULT (1),
        [CreatedByUserID]      int            NOT NULL CONSTRAINT [DF_TdsRules_CreatedBy] DEFAULT (0),
        [DateTime]             datetime2      NOT NULL CONSTRAINT [DF_TdsRules_DateTime] DEFAULT (GETDATE()),
        [UpdatedateTime]       datetime2      NOT NULL CONSTRAINT [DF_TdsRules_Updated] DEFAULT (GETDATE()),
        [IsDeleted]            bit            NOT NULL CONSTRAINT [DF_TdsRules_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_TdsRules] PRIMARY KEY ([TdsRuleID])
    );
    PRINT '  created TdsRules';
END
ELSE
    PRINT '  TdsRules already present';

IF OBJECT_ID(N'[TdsRules]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE [name] = N'IX_TdsRules_Scope_IsActive_EffectiveFrom'
                     AND [object_id] = OBJECT_ID(N'[TdsRules]'))
BEGIN
    -- Resolving a rule happens on every settled transaction, per tier.
    CREATE NONCLUSTERED INDEX [IX_TdsRules_Scope_IsActive_EffectiveFrom]
        ON dbo.TdsRules ([Scope], [IsActive], [EffectiveFrom]);
    PRINT '  index IX_TdsRules_Scope_IsActive_EffectiveFrom created';
END
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   2. What was done with each month of deductions, per party.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[TdsSettlements]', N'U') IS NULL
BEGIN
    CREATE TABLE [TdsSettlements] (
        [TdsSettlementID]  int IDENTITY(1,1) NOT NULL,
        [PeriodKey]        varchar(8)     NOT NULL CONSTRAINT [DF_TdsSettlements_Period] DEFAULT (''),
        [PartyUserID]      int            NOT NULL CONSTRAINT [DF_TdsSettlements_Party] DEFAULT (0),
        -- DEPOSITED (paid to the government) or WALLET_SETTLED (given back to the party)
        [Mode]             varchar(20)    NOT NULL CONSTRAINT [DF_TdsSettlements_Mode] DEFAULT (''),
        [TDSAmount]        decimal(18,2)  NOT NULL CONSTRAINT [DF_TdsSettlements_Amount] DEFAULT (0),
        [DeductionCount]   int            NOT NULL CONSTRAINT [DF_TdsSettlements_Count] DEFAULT (0),
        -- The challan / BSR / UTR for a deposit. A TDS certificate is issued against it.
        [Reference]        varchar(100)   NOT NULL CONSTRAINT [DF_TdsSettlements_Reference] DEFAULT (''),
        [ReferenceDate]    date           NULL,
        [WalletID]         int            NOT NULL CONSTRAINT [DF_TdsSettlements_Wallet] DEFAULT (0),
        [Remark]           varchar(300)   NOT NULL CONSTRAINT [DF_TdsSettlements_Remark] DEFAULT (''),
        [SettledOn]        datetime2      NOT NULL CONSTRAINT [DF_TdsSettlements_SettledOn] DEFAULT (GETDATE()),
        [SettledByUserID]  int            NOT NULL CONSTRAINT [DF_TdsSettlements_SettledBy] DEFAULT (0),
        [DateTime]         datetime2      NOT NULL CONSTRAINT [DF_TdsSettlements_DateTime] DEFAULT (GETDATE()),
        [UpdatedateTime]   datetime2      NOT NULL CONSTRAINT [DF_TdsSettlements_Updated] DEFAULT (GETDATE()),
        [IsDeleted]        bit            NOT NULL CONSTRAINT [DF_TdsSettlements_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_TdsSettlements] PRIMARY KEY ([TdsSettlementID])
    );
    PRINT '  created TdsSettlements';
END
ELSE
    PRINT '  TdsSettlements already present';

IF OBJECT_ID(N'[TdsSettlements]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE [name] = N'IX_TdsSettlements_PeriodKey_PartyUserID'
                     AND [object_id] = OBJECT_ID(N'[TdsSettlements]'))
BEGIN
    -- One settlement per party per month. This index is not an optimisation: it is what stops a
    -- month being paid to the government twice, or credited to a wallet twice, when two desks
    -- press the button at the same moment. Soft-deleted rows are excluded so a cancelled
    -- settlement can be replaced.
    CREATE UNIQUE NONCLUSTERED INDEX [IX_TdsSettlements_PeriodKey_PartyUserID]
        ON dbo.TdsSettlements ([PeriodKey], [PartyUserID])
        WHERE [IsDeleted] = 0;
    PRINT '  unique index IX_TdsSettlements_PeriodKey_PartyUserID created';
END
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   3. The deduction row gains the rule that decided it, and whether the money was
      actually collected.
   ────────────────────────────────────────────────────────────────────────────── */
DECLARE @cols TABLE (ColumnName SYSNAME, Definition NVARCHAR(200));
INSERT INTO @cols VALUES
    ('TdsRuleID',  'int NOT NULL CONSTRAINT [DF_TTD_TdsRuleID] DEFAULT (0)'),
    ('ChargeType', 'varchar(20) NOT NULL CONSTRAINT [DF_TTD_ChargeType] DEFAULT ('''')'),
    -- Existing rows predate withholding: they were recorded but never taken out of a wallet, so
    -- 0 is the truthful default and the monthly report will show them as not withheld.
    ('Withheld',   'bit NOT NULL CONSTRAINT [DF_TTD_Withheld] DEFAULT (0)'),
    ('DecidedBy',  'varchar(200) NOT NULL CONSTRAINT [DF_TTD_DecidedBy] DEFAULT ('''')');

DECLARE @c SYSNAME, @d NVARCHAR(200), @sql NVARCHAR(MAX), @added INT = 0, @skipped INT = 0;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT ColumnName, Definition FROM @cols;
OPEN cur;
FETCH NEXT FROM cur INTO @c, @d;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'[TransactionTDSDetails]', N'U') IS NULL
    BEGIN
        PRINT '  SKIP: TransactionTDSDetails does not exist (apply 68_ first)';
        SET @skipped = @skipped + 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns
                    WHERE [object_id] = OBJECT_ID(N'[TransactionTDSDetails]') AND [name] = @c)
        SET @skipped = @skipped + 1;
    ELSE
    BEGIN
        SET @sql = N'ALTER TABLE [TransactionTDSDetails] ADD ' + QUOTENAME(@c) + N' ' + @d + N';';
        EXEC sp_executesql @sql;
        PRINT '  added TransactionTDSDetails.' + @c;
        SET @added = @added + 1;
    END
    FETCH NEXT FROM cur INTO @c, @d;
END
CLOSE cur; DEALLOCATE cur;
PRINT '  deduction columns added: ' + CAST(@added AS varchar(10)) + ', already present: ' + CAST(@skipped AS varchar(10));
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   4. The master switch, OFF.

      It is seeded rather than left absent so the Key Setting screen shows the row
      with a state somebody chose, instead of an empty control that reads as
      "not implemented". Never overwritten on re-run: if an operator has turned
      TDS on, re-running the installer must not turn it off underneath them.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[SettingMasters]', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'IsTDSEnabled' AND [IsDeleted] = 0)
    BEGIN
        -- Every column is listed because SettingMasters has NO defaults on live - not on
        -- IsPassword, not on IsDeleted, not even on DateTime. An INSERT that omits one passes in
        -- dev, where the model supplies them, and fails in production with "Cannot insert the
        -- value NULL". This script hit exactly that on the first dry run.
        INSERT INTO dbo.SettingMasters
            ([Key], [Value], [IsOn], [IsSecure], [IsPassword], [Description], [DateTime], [UpdatedateTime], [IsDeleted])
        VALUES ('IsTDSEnabled', 'false', 0, 0, 0,
                'Deduct TDS from commissions. OFF until the TDS rules are set: turning it on starts withholding real money from real people. Rules live on Accountant -> TDS Setting.',
                GETDATE(), GETDATE(), 0);
        PRINT '  seeded Key Setting IsTDSEnabled = OFF';
    END
    ELSE
        PRINT '  Key Setting IsTDSEnabled already present - left exactly as it is';
END
GO
SET QUOTED_IDENTIFIER ON;

PRINT '';
PRINT '=== TDS state now ===';
IF OBJECT_ID(N'[TdsRules]', N'U') IS NOT NULL
    SELECT 'rules' AS [What], COUNT(*) AS [Count] FROM dbo.TdsRules WHERE IsDeleted = 0;
IF OBJECT_ID(N'[SettingMasters]', N'U') IS NOT NULL
    SELECT 'master switch' AS [What], [Value] AS [State] FROM dbo.SettingMasters WHERE [Key] = 'IsTDSEnabled' AND IsDeleted = 0;
PRINT 'No rule and the switch off means nothing is deducted - which is exactly what happened before.';
PRINT '=== 70 complete ===';
