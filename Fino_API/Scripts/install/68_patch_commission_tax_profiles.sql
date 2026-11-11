/*
    68 - Commission tax treatment: inclusive or exclusive, on what, and whether TDS applies.

    WHY THIS EXISTS
    The tax engine can already EXPRESS every one of these answers - BillingBasis says what tax is
    charged on, IsTaxInclusive says whether a figure already contains it, TDSSectionMasters holds
    the rates. What was missing is anywhere to RECORD, per provider and per slab, which answer is
    the right one. So the callers hardcoded it: every invoice path passes IsTaxInclusive = true,
    and TDS is worked out and stamped but never deducted from anybody.

    That is why the accounting "works" and is still wrong. A provider who pays commission EXCLUSIVE
    of GST and one who pays it INCLUSIVE produce the same number in this system today, and the net
    profit differs by the tax on every rupee of it.

    ONE TABLE, FOUR SCOPES
    Rather than a column on SlabDetail (already 30-odd columns, one set per tier) and another on
    ProviderConfigurationAdminCommission, the treatment lives once and is matched most-specific
    first, exactly as TaxRuleMasters is:

        GLOBAL    - the house default; one row, so a fresh install behaves predictably
        SERVICE   - everything on this service (ScopeID = ServiceID)
        PROVIDER  - money we receive FROM this provider (ScopeID = ProviderID)
        SLAB      - money we pay OUT on this slab (ScopeID = SlabMasterID)

    A PROVIDER row answers the first question in the brief: is the provider's commission inclusive
    or exclusive, and is the GST on the commission or on the whole amount. A SLAB row answers the
    third: whether the retailer's slab commission already contains GST.

    PerTransactionCharge covers the DMT case where the charge is per transaction rather than a
    percentage - "jitna account hai utna charge" - and carries its own inclusive/exclusive mode,
    because a flat Rs 15 charge to a retailer is normally quoted inclusive while a percentage is
    normally quoted exclusive.

    TDSEnabled is the switch the brief asks for. OFF by default and off on the GLOBAL seed: turning
    TDS on starts deducting real money from real people, and that is not a thing to inherit from a
    default. When it is on, TDS is computed on the COMMISSION, which is what Section 194H reaches -
    never on the transaction amount.

    Every DateTime / UpdatedateTime / IsDeleted carries an explicit DEFAULT - the live database has
    none on those columns while dev does, and an INSERT that omits them passes every dev test and
    fails in production with "Cannot insert the value NULL".

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.CommissionTaxProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CommissionTaxProfiles (
        [CommissionTaxProfileID] INT          NOT NULL IDENTITY(1,1),

        -- GLOBAL / SERVICE / PROVIDER / SLAB. Most specific wins; see Priority below.
        [Scope]              VARCHAR(20)  NOT NULL,
        -- ServiceID, ProviderID or SlabMasterID depending on Scope. 0 for GLOBAL.
        [ScopeID]            INT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_ScopeID DEFAULT (0),

        -- Optional further narrowing, so one provider can be treated differently per service.
        [ServiceID]          INT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_ServiceID DEFAULT (0),
        [OperatorID]         INT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_OperatorID DEFAULT (0),

        -- INCLUSIVE: the commission figure already contains GST, so it is backed out of it.
        -- EXCLUSIVE: GST is added on top of the commission figure.
        -- NONE:      no GST on this commission at all.
        [TaxMode]            VARCHAR(20)  NOT NULL CONSTRAINT DF_CommissionTaxProfiles_TaxMode DEFAULT ('INCLUSIVE'),

        -- COMMISSION: GST applies to the commission earned (agency - DMT, AEPS).
        -- GROSS:      GST applies to the whole transaction amount (trading - talktime resold).
        [TaxBase]            VARCHAR(20)  NOT NULL CONSTRAINT DF_CommissionTaxProfiles_TaxBase DEFAULT ('COMMISSION'),

        -- 0 means "ask the tax engine", which resolves the rate from TaxRuleMasters as of the
        -- transaction's own date. A non-zero value here pins it, for the provider whose contract
        -- says a number that is not the statutory one.
        [GSTRatePercent]     DECIMAL(9,4) NOT NULL CONSTRAINT DF_CommissionTaxProfiles_GSTRate DEFAULT (0),

        -- The per-transaction charge, for services billed by count rather than by value.
        [PerTransactionCharge] DECIMAL(18,4) NOT NULL CONSTRAINT DF_CommissionTaxProfiles_PerTxn DEFAULT (0),
        [PerTransactionChargeTaxMode] VARCHAR(20) NOT NULL CONSTRAINT DF_CommissionTaxProfiles_PerTxnMode DEFAULT ('INCLUSIVE'),

        -- The switch. OFF everywhere until somebody turns it on deliberately.
        [TDSEnabled]         BIT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_TDSEnabled DEFAULT (0),
        -- Blank resolves the section from the party's relationship (194H for an agent, 194O for an
        -- e-commerce operator) through the tax engine, the same way the rate does.
        [TDSSectionCode]     VARCHAR(20)  NOT NULL CONSTRAINT DF_CommissionTaxProfiles_TDSSection DEFAULT (''),
        [TDSRatePercent]     DECIMAL(9,4) NOT NULL CONSTRAINT DF_CommissionTaxProfiles_TDSRate DEFAULT (0),

        -- Effective dating, because a rate that changes must not restate last year's books.
        [EffectiveFrom]      DATE         NOT NULL CONSTRAINT DF_CommissionTaxProfiles_From DEFAULT ('2000-01-01'),
        [EffectiveTo]        DATE         NULL,

        -- Higher wins among rows of the same scope. Left 0, the service derives it from how
        -- specific the row is, so an admin never has to reason about numbers.
        [Priority]           INT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_Priority DEFAULT (0),

        [Remark]             VARCHAR(300) NOT NULL CONSTRAINT DF_CommissionTaxProfiles_Remark DEFAULT (''),
        [IsActive]           BIT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_IsActive DEFAULT (1),
        [CreatedByUserID]    INT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_CreatedBy DEFAULT (0),

        [DateTime]           DATETIME2    NOT NULL CONSTRAINT DF_CommissionTaxProfiles_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2    NOT NULL CONSTRAINT DF_CommissionTaxProfiles_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT          NOT NULL CONSTRAINT DF_CommissionTaxProfiles_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_CommissionTaxProfiles] PRIMARY KEY ([CommissionTaxProfileID])
    );
    PRINT 'Created table dbo.CommissionTaxProfiles';
END
GO

IF OBJECT_ID(N'dbo.CommissionTaxProfiles', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CommissionTaxProfiles_Lookup'
                                               AND object_id = OBJECT_ID(N'dbo.CommissionTaxProfiles'))
BEGIN
    CREATE INDEX [IX_CommissionTaxProfiles_Lookup]
        ON dbo.CommissionTaxProfiles ([Scope], [ScopeID], [ServiceID], [OperatorID])
        INCLUDE ([TaxMode], [TaxBase], [IsActive], [EffectiveFrom], [EffectiveTo]);
    PRINT 'Created index IX_CommissionTaxProfiles_Lookup';
END
GO

/* ── The TDS actually deducted, per transaction per party ─────────────────────────────────
   A column on TransactionMaster already exists for the retailer / distributor / super tiers, but
   it records only the AMOUNT. What a deduction has to be able to answer later is "under which
   section, at what rate, and against which commission" - that is what a TDS certificate is - so
   the detail is kept here and the totals stay on the transaction for the reports that read them.  */
IF OBJECT_ID(N'dbo.TransactionTDSDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TransactionTDSDetails (
        [TransactionTDSDetailID] INT      NOT NULL IDENTITY(1,1),
        [TransactionMasterID] INT         NOT NULL,
        [PartyUserID]        INT          NOT NULL,
        -- Retailer / Distributor / SuperDistributor - which tier's commission this is.
        [Tier]               VARCHAR(20)  NOT NULL CONSTRAINT DF_TransactionTDSDetails_Tier DEFAULT (''),
        [SectionCode]        VARCHAR(20)  NOT NULL CONSTRAINT DF_TransactionTDSDetails_Section DEFAULT (''),
        [RatePercent]        DECIMAL(9,4) NOT NULL CONSTRAINT DF_TransactionTDSDetails_Rate DEFAULT (0),
        -- The commission the deduction was computed ON, after GST was backed out of it where the
        -- commission was inclusive. TDS is on income, not on the tax collected with it.
        [CommissionBase]     DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTDSDetails_Base DEFAULT (0),
        [TDSAmount]          DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTDSDetails_Amount DEFAULT (0),
        -- True when the party had no PAN and the higher no-PAN rate was applied.
        [NoPANRateApplied]   BIT          NOT NULL CONSTRAINT DF_TransactionTDSDetails_NoPAN DEFAULT (0),
        [PeriodKey]          VARCHAR(8)   NOT NULL CONSTRAINT DF_TransactionTDSDetails_Period DEFAULT (''),
        [DateTime]           DATETIME2    NOT NULL CONSTRAINT DF_TransactionTDSDetails_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2    NOT NULL CONSTRAINT DF_TransactionTDSDetails_Updated DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT          NOT NULL CONSTRAINT DF_TransactionTDSDetails_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TransactionTDSDetails] PRIMARY KEY ([TransactionTDSDetailID])
    );
    PRINT 'Created table dbo.TransactionTDSDetails';
END
GO

-- One deduction per transaction per tier. A second one would double-deduct from a real person.
IF OBJECT_ID(N'dbo.TransactionTDSDetails', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TransactionTDSDetails_TxnTier'
                                               AND object_id = OBJECT_ID(N'dbo.TransactionTDSDetails'))
BEGIN
    CREATE UNIQUE INDEX [UX_TransactionTDSDetails_TxnTier]
        ON dbo.TransactionTDSDetails ([TransactionMasterID], [Tier]) WHERE [IsDeleted] = 0;
    PRINT 'Created index UX_TransactionTDSDetails_TxnTier';
END
GO

IF OBJECT_ID(N'dbo.TransactionTDSDetails', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TransactionTDSDetails_Party'
                                               AND object_id = OBJECT_ID(N'dbo.TransactionTDSDetails'))
BEGIN
    CREATE INDEX [IX_TransactionTDSDetails_Party]
        ON dbo.TransactionTDSDetails ([PartyUserID], [PeriodKey]) INCLUDE ([TDSAmount], [SectionCode]);
    PRINT 'Created index IX_TransactionTDSDetails_Party';
END
GO

/* ── The house default ────────────────────────────────────────────────────────────────────
   INCLUSIVE / COMMISSION with TDS OFF. That is what this install has been doing implicitly all
   along, so seeding it changes nothing on day one - it simply makes the assumption visible and
   editable, which is the whole point.                                                          */
IF OBJECT_ID(N'dbo.CommissionTaxProfiles', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.CommissionTaxProfiles WHERE [Scope] = 'GLOBAL' AND [IsDeleted] = 0)
BEGIN
    INSERT INTO dbo.CommissionTaxProfiles
        ([Scope], [ScopeID], [TaxMode], [TaxBase], [TDSEnabled], [Priority], [Remark])
    VALUES
        ('GLOBAL', 0, 'INCLUSIVE', 'COMMISSION', 0, 0,
         'House default: commission figures are quoted inclusive of GST and TDS is off. Override per service, provider or slab.');
    PRINT 'Seeded the GLOBAL commission tax profile (INCLUSIVE / COMMISSION, TDS off)';
END
GO

IF OBJECT_ID(N'dbo.CommissionTaxProfiles', N'U') IS NOT NULL
    PRINT 'Commission tax profile schema is in place.';
GO
