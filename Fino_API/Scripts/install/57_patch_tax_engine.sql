/*
    Dynamic tax engine - masters and the per-transaction stamp. Phase 1.

    WHY THIS EXISTS
    TransactionMasters already carries AdminGST, RetailerGST/TDS, DistributerGST/TDS and so on, so
    the AMOUNTS have always been stored. What was never stored is where they came from: no rate,
    no rule, no effective date, no basis. So the system cannot answer "which rate applied on 12
    August", and changing a rate silently restates every past month - at which point the books
    stop agreeing with the returns already filed, and nobody can tell a real error from the
    software having changed its mind.

    These tables fix that in three moves:
      1. Rates, SAC codes and TDS sections become effective-dated ROWS, never edited in place.
      2. TaxRuleMasters decides, per direction/service/party, what the taxable value IS -
         gross, commission or charge. That one field is the 18cr-sales against 19cr-purchase
         mismatch: the suppliers bill recharge on the whole top-up (998413) and DMT on the charge
         alone (997112/997159), and a register that adds both is comparing two quantities.
      3. TransactionTaxDetails STAMPS the resolved answer on the transaction. Reports read the
         stamp; they never recompute.

    NOTE ON DEFAULTS
    Every DateTime / UpdatedateTime / IsDeleted column below carries an explicit DEFAULT. The
    live database was found with those columns NOT NULL and no default while dev had defaults, so
    an INSERT that omitted them passed every dev test and failed in production with
    "Cannot insert the value NULL into column 'IsDeleted'". Defaults here, named columns in the
    seeds - both, because either alone has already failed once.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;

/* ── 1. Tax rates ──────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxRateMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxRateMasters (
        [TaxRateMasterID] INT           NOT NULL IDENTITY(1,1),
        [TaxType]         VARCHAR(10)   NOT NULL,
        [Code]            VARCHAR(30)   NOT NULL,
        [Description]     VARCHAR(200)  NOT NULL,
        -- Percent, not a fraction: 18 means 18%.
        [Rate]            DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TaxRateMasters_Rate DEFAULT (0),
        [CessRate]        DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TaxRateMasters_CessRate DEFAULT (0),
        [EffectiveFrom]   DATETIME2     NOT NULL,
        -- NULL = still current. A change closes this row and opens a new one; Rate is never
        -- updated, because updating it rewrites history.
        [EffectiveTo]     DATETIME2     NULL,
        [NotificationRef] VARCHAR(200)  NOT NULL CONSTRAINT DF_TaxRateMasters_NotificationRef DEFAULT (''),
        [IsActive]        BIT           NOT NULL CONSTRAINT DF_TaxRateMasters_IsActive DEFAULT (1),
        [DateTime]        DATETIME2     NOT NULL CONSTRAINT DF_TaxRateMasters_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]  DATETIME2     NOT NULL CONSTRAINT DF_TaxRateMasters_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]       BIT           NOT NULL CONSTRAINT DF_TaxRateMasters_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxRateMasters] PRIMARY KEY ([TaxRateMasterID])
    );
    CREATE INDEX IX_TaxRateMasters_TaxType_Code_EffectiveFrom
        ON dbo.TaxRateMasters ([TaxType], [Code], [EffectiveFrom]);
    PRINT 'Created table dbo.TaxRateMasters';
END
ELSE PRINT 'dbo.TaxRateMasters already exists - skipped.';

/* ── 2. HSN / SAC codes ────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.HSNSACMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HSNSACMasters (
        [HSNSACMasterID]         INT           NOT NULL IDENTITY(1,1),
        [Code]                   VARCHAR(10)   NOT NULL,
        [Description]            VARCHAR(300)  NOT NULL,
        [DefaultTaxRateMasterID] INT           NULL,
        [EffectiveFrom]          DATETIME2     NOT NULL,
        [EffectiveTo]            DATETIME2     NULL,
        [IsActive]               BIT           NOT NULL CONSTRAINT DF_HSNSACMasters_IsActive DEFAULT (1),
        [DateTime]               DATETIME2     NOT NULL CONSTRAINT DF_HSNSACMasters_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]         DATETIME2     NOT NULL CONSTRAINT DF_HSNSACMasters_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]              BIT           NOT NULL CONSTRAINT DF_HSNSACMasters_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_HSNSACMasters] PRIMARY KEY ([HSNSACMasterID]),
        CONSTRAINT [FK_HSNSACMasters_TaxRateMasters] FOREIGN KEY ([DefaultTaxRateMasterID])
            REFERENCES dbo.TaxRateMasters ([TaxRateMasterID])
    );
    CREATE INDEX IX_HSNSACMasters_Code ON dbo.HSNSACMasters ([Code]);
    PRINT 'Created table dbo.HSNSACMasters';
END
ELSE PRINT 'dbo.HSNSACMasters already exists - skipped.';

/* ── 3. TDS / TCS sections ─────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TDSSectionMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TDSSectionMasters (
        [TDSSectionMasterID] INT           NOT NULL IDENTITY(1,1),
        [SectionCode]        VARCHAR(20)   NOT NULL,
        [Description]        VARCHAR(200)  NOT NULL,
        [Rate]               DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TDSSectionMasters_Rate DEFAULT (0),
        -- Annual on purpose: 194H's Rs 20,000 is tested on the YEAR's aggregate per party, not
        -- per payment, so the engine needs a running year-to-date total.
        [ThresholdAnnual]    DECIMAL(18,2) NOT NULL CONSTRAINT DF_TDSSectionMasters_ThresholdAnnual DEFAULT (0),
        [NoPANRate]          DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TDSSectionMasters_NoPANRate DEFAULT (0),
        [NonFilerRate]       DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TDSSectionMasters_NonFilerRate DEFAULT (0),
        [EffectiveFrom]      DATETIME2     NOT NULL,
        [EffectiveTo]        DATETIME2     NULL,
        [IsActive]           BIT           NOT NULL CONSTRAINT DF_TDSSectionMasters_IsActive DEFAULT (1),
        [DateTime]           DATETIME2     NOT NULL CONSTRAINT DF_TDSSectionMasters_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]     DATETIME2     NOT NULL CONSTRAINT DF_TDSSectionMasters_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT           NOT NULL CONSTRAINT DF_TDSSectionMasters_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TDSSectionMasters] PRIMARY KEY ([TDSSectionMasterID])
    );
    CREATE INDEX IX_TDSSectionMasters_SectionCode_EffectiveFrom
        ON dbo.TDSSectionMasters ([SectionCode], [EffectiveFrom]);
    PRINT 'Created table dbo.TDSSectionMasters';
END
ELSE PRINT 'dbo.TDSSectionMasters already exists - skipped.';

/* ── 4. The rules ──────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxRuleMasters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxRuleMasters (
        [TaxRuleMasterID]         INT           NOT NULL IDENTITY(1,1),
        [RuleName]                VARCHAR(120)  NOT NULL,
        [Direction]               VARCHAR(10)   NOT NULL,
        -- Every dimension is optional. 0 on an id and '' on a string mean ANY: the rule does not
        -- care about that dimension.
        [ServiceID]               INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_ServiceID DEFAULT (0),
        [OperatorID]              INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_OperatorID DEFAULT (0),
        [ProviderID]              INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_ProviderID DEFAULT (0),
        [UserTypeID]              INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_UserTypeID DEFAULT (0),
        [PartyUserID]             INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_PartyUserID DEFAULT (0),
        [PartyRegistrationStatus] VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxRuleMasters_PartyRegistrationStatus DEFAULT (''),
        [SupplyNature]            VARCHAR(20)   NOT NULL CONSTRAINT DF_TaxRuleMasters_SupplyNature DEFAULT (''),
        [BillingBasis]            VARCHAR(20)   NOT NULL,
        [TaxRateMasterID]         INT           NULL,
        [HSNSACMasterID]          INT           NULL,
        [TDSSectionMasterID]      INT           NULL,
        [ReverseCharge]           BIT           NOT NULL CONSTRAINT DF_TaxRuleMasters_ReverseCharge DEFAULT (0),
        [ITCEligible]             BIT           NOT NULL CONSTRAINT DF_TaxRuleMasters_ITCEligible DEFAULT (1),
        -- Higher wins. Set from how specific the rule is; two matches at the SAME priority are
        -- raised as an error rather than resolved by a coin toss.
        [Priority]                INT           NOT NULL CONSTRAINT DF_TaxRuleMasters_Priority DEFAULT (0),
        [Remark]                  VARCHAR(500)  NOT NULL CONSTRAINT DF_TaxRuleMasters_Remark DEFAULT (''),
        [EffectiveFrom]           DATETIME2     NOT NULL,
        [EffectiveTo]             DATETIME2     NULL,
        [IsActive]                BIT           NOT NULL CONSTRAINT DF_TaxRuleMasters_IsActive DEFAULT (1),
        [DateTime]                DATETIME2     NOT NULL CONSTRAINT DF_TaxRuleMasters_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]          DATETIME2     NOT NULL CONSTRAINT DF_TaxRuleMasters_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]               BIT           NOT NULL CONSTRAINT DF_TaxRuleMasters_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxRuleMasters] PRIMARY KEY ([TaxRuleMasterID]),
        CONSTRAINT [FK_TaxRuleMasters_TaxRateMasters] FOREIGN KEY ([TaxRateMasterID])
            REFERENCES dbo.TaxRateMasters ([TaxRateMasterID]),
        CONSTRAINT [FK_TaxRuleMasters_HSNSACMasters] FOREIGN KEY ([HSNSACMasterID])
            REFERENCES dbo.HSNSACMasters ([HSNSACMasterID]),
        CONSTRAINT [FK_TaxRuleMasters_TDSSectionMasters] FOREIGN KEY ([TDSSectionMasterID])
            REFERENCES dbo.TDSSectionMasters ([TDSSectionMasterID])
    );
    CREATE INDEX IX_TaxRuleMasters_Direction_ServiceID_EffectiveFrom
        ON dbo.TaxRuleMasters ([Direction], [ServiceID], [EffectiveFrom]);
    PRINT 'Created table dbo.TaxRuleMasters';
END
ELSE PRINT 'dbo.TaxRuleMasters already exists - skipped.';

/* ── 5. Party tax profile ──────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.PartyTaxProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PartyTaxProfiles (
        [PartyTaxProfileID]       INT           NOT NULL IDENTITY(1,1),
        [UserID]                  INT           NOT NULL,
        [GSTIN]                   VARCHAR(15)   NOT NULL CONSTRAINT DF_PartyTaxProfiles_GSTIN DEFAULT (''),
        [RegistrationStatus]      VARCHAR(20)   NOT NULL CONSTRAINT DF_PartyTaxProfiles_RegistrationStatus DEFAULT (''),
        [GSTINVerifiedOn]         DATETIME2     NULL,
        [PAN]                     VARCHAR(10)   NOT NULL CONSTRAINT DF_PartyTaxProfiles_PAN DEFAULT (''),
        [PANVerifiedOn]           DATETIME2     NULL,
        [Is206ABFlagged]          BIT           NOT NULL CONSTRAINT DF_PartyTaxProfiles_Is206ABFlagged DEFAULT (0),
        [ComplianceCheckedOn]     DATETIME2     NULL,
        -- A Section 197 certificate is capped by BOTH a date and an amount; deducting at the
        -- certificate rate past either is a short deduction recovered with interest.
        [LowerDeductionCertNo]    VARCHAR(30)   NOT NULL CONSTRAINT DF_PartyTaxProfiles_LowerDeductionCertNo DEFAULT (''),
        [LDCRate]                 DECIMAL(9,4)  NOT NULL CONSTRAINT DF_PartyTaxProfiles_LDCRate DEFAULT (0),
        [LDCValidFrom]            DATETIME2     NULL,
        [LDCValidTill]            DATETIME2     NULL,
        [LDCAmountCap]            DECIMAL(18,2) NOT NULL CONSTRAINT DF_PartyTaxProfiles_LDCAmountCap DEFAULT (0),
        [PlaceOfSupplyStateCode]  VARCHAR(2)    NOT NULL CONSTRAINT DF_PartyTaxProfiles_PlaceOfSupplyStateCode DEFAULT (''),
        -- Both legs of a reciprocal wallet swap must be invoiced: under GST s.7(1)(a) barter is
        -- a supply and each side is taxable.
        [IsExchangePartner]       BIT           NOT NULL CONSTRAINT DF_PartyTaxProfiles_IsExchangePartner DEFAULT (0),
        [ExchangeAgreementRef]    VARCHAR(200)  NOT NULL CONSTRAINT DF_PartyTaxProfiles_ExchangeAgreementRef DEFAULT (''),
        [Remark]                  VARCHAR(500)  NOT NULL CONSTRAINT DF_PartyTaxProfiles_Remark DEFAULT (''),
        [DateTime]                DATETIME2     NOT NULL CONSTRAINT DF_PartyTaxProfiles_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]          DATETIME2     NOT NULL CONSTRAINT DF_PartyTaxProfiles_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]               BIT           NOT NULL CONSTRAINT DF_PartyTaxProfiles_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_PartyTaxProfiles] PRIMARY KEY ([PartyTaxProfileID]),
        CONSTRAINT [FK_PartyTaxProfiles_UserMasters] FOREIGN KEY ([UserID])
            REFERENCES dbo.UserMasters ([UserMasterID])
    );
    CREATE UNIQUE INDEX UX_PartyTaxProfiles_UserID ON dbo.PartyTaxProfiles ([UserID]);
    PRINT 'Created table dbo.PartyTaxProfiles';
END
ELSE PRINT 'dbo.PartyTaxProfiles already exists - skipped.';

/* ── 6. Party relationship per service (the Bharti Cellular switch) ─────────── */
IF OBJECT_ID(N'dbo.PartyServiceRelationships', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PartyServiceRelationships (
        [PartyServiceRelationshipID] INT          NOT NULL IDENTITY(1,1),
        [UserID]                     INT          NOT NULL,
        -- 0 = every service.
        [ServiceID]                  INT          NOT NULL CONSTRAINT DF_PartyServiceRelationships_ServiceID DEFAULT (0),
        -- P2P = a discount, no 194H. AGENT = commission, 194H at 2%.
        [RelationshipType]           VARCHAR(10)  NOT NULL,
        [AgreementRef]               VARCHAR(200) NOT NULL CONSTRAINT DF_PartyServiceRelationships_AgreementRef DEFAULT (''),
        [EffectiveFrom]              DATETIME2    NOT NULL,
        [EffectiveTo]                DATETIME2    NULL,
        [IsActive]                   BIT          NOT NULL CONSTRAINT DF_PartyServiceRelationships_IsActive DEFAULT (1),
        [DateTime]                   DATETIME2    NOT NULL CONSTRAINT DF_PartyServiceRelationships_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]             DATETIME2    NOT NULL CONSTRAINT DF_PartyServiceRelationships_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]                  BIT          NOT NULL CONSTRAINT DF_PartyServiceRelationships_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_PartyServiceRelationships] PRIMARY KEY ([PartyServiceRelationshipID]),
        CONSTRAINT [FK_PartyServiceRelationships_UserMasters] FOREIGN KEY ([UserID])
            REFERENCES dbo.UserMasters ([UserMasterID])
    );
    CREATE INDEX IX_PartyServiceRelationships_UserID_ServiceID_EffectiveFrom
        ON dbo.PartyServiceRelationships ([UserID], [ServiceID], [EffectiveFrom]);
    PRINT 'Created table dbo.PartyServiceRelationships';
END
ELSE PRINT 'dbo.PartyServiceRelationships already exists - skipped.';

/* ── 7. Audit of every master change ───────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TaxRuleAudits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxRuleAudits (
        [TaxRuleAuditID]   INT           NOT NULL IDENTITY(1,1),
        [EntityName]       VARCHAR(40)   NOT NULL,
        [EntityID]         INT           NOT NULL,
        [Action]           VARCHAR(20)   NOT NULL,
        [OldValueJson]     VARCHAR(MAX)  NOT NULL CONSTRAINT DF_TaxRuleAudits_OldValueJson DEFAULT (''),
        [NewValueJson]     VARCHAR(MAX)  NOT NULL CONSTRAINT DF_TaxRuleAudits_NewValueJson DEFAULT (''),
        [Reason]           VARCHAR(500)  NOT NULL CONSTRAINT DF_TaxRuleAudits_Reason DEFAULT (''),
        [ChangedByUserID]  INT           NOT NULL CONSTRAINT DF_TaxRuleAudits_ChangedByUserID DEFAULT (0),
        [ChangedOn]        DATETIME2     NOT NULL CONSTRAINT DF_TaxRuleAudits_ChangedOn DEFAULT (SYSUTCDATETIME()),
        [DateTime]         DATETIME2     NOT NULL CONSTRAINT DF_TaxRuleAudits_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]   DATETIME2     NOT NULL CONSTRAINT DF_TaxRuleAudits_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]        BIT           NOT NULL CONSTRAINT DF_TaxRuleAudits_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TaxRuleAudits] PRIMARY KEY ([TaxRuleAuditID])
    );
    CREATE INDEX IX_TaxRuleAudits_EntityName_EntityID_ChangedOn
        ON dbo.TaxRuleAudits ([EntityName], [EntityID], [ChangedOn]);
    PRINT 'Created table dbo.TaxRuleAudits';
END
ELSE PRINT 'dbo.TaxRuleAudits already exists - skipped.';

/* ── 8. The stamp ──────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'dbo.TransactionTaxDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TransactionTaxDetails (
        [TransactionTaxDetailID]  INT           NOT NULL IDENTITY(1,1),
        [TransactionMasterID]     INT           NOT NULL,
        [Direction]               VARCHAR(10)   NOT NULL,
        -- 0 when no rule matched and the fallback was used - itself worth reporting on.
        [TaxRuleMasterID]         INT           NOT NULL CONSTRAINT DF_TransactionTaxDetails_TaxRuleMasterID DEFAULT (0),
        [BillingBasis]            VARCHAR(20)   NOT NULL,
        [GrossValue]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_GrossValue DEFAULT (0),
        [TaxableValue]            DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_TaxableValue DEFAULT (0),
        [SupplyNature]            VARCHAR(20)   NOT NULL CONSTRAINT DF_TransactionTaxDetails_SupplyNature DEFAULT (''),
        [SupplierStateCode]       VARCHAR(2)    NOT NULL CONSTRAINT DF_TransactionTaxDetails_SupplierStateCode DEFAULT (''),
        [PlaceOfSupplyStateCode]  VARCHAR(2)    NOT NULL CONSTRAINT DF_TransactionTaxDetails_PlaceOfSupplyStateCode DEFAULT (''),
        [SACCode]                 VARCHAR(10)   NOT NULL CONSTRAINT DF_TransactionTaxDetails_SACCode DEFAULT (''),
        [CGSTRate]                DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TransactionTaxDetails_CGSTRate DEFAULT (0),
        [CGSTAmount]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_CGSTAmount DEFAULT (0),
        [SGSTRate]                DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TransactionTaxDetails_SGSTRate DEFAULT (0),
        [SGSTAmount]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_SGSTAmount DEFAULT (0),
        [IGSTRate]                DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TransactionTaxDetails_IGSTRate DEFAULT (0),
        [IGSTAmount]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_IGSTAmount DEFAULT (0),
        [CessAmount]              DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_CessAmount DEFAULT (0),
        [TDSSectionCode]          VARCHAR(20)   NOT NULL CONSTRAINT DF_TransactionTaxDetails_TDSSectionCode DEFAULT (''),
        [TDSRate]                 DECIMAL(9,4)  NOT NULL CONSTRAINT DF_TransactionTaxDetails_TDSRate DEFAULT (0),
        [TDSAmount]               DECIMAL(18,2) NOT NULL CONSTRAINT DF_TransactionTaxDetails_TDSAmount DEFAULT (0),
        [PartyRelationship]       VARCHAR(10)   NOT NULL CONSTRAINT DF_TransactionTaxDetails_PartyRelationship DEFAULT (''),
        [ITCEligible]             BIT           NOT NULL CONSTRAINT DF_TransactionTaxDetails_ITCEligible DEFAULT (0),
        [ReverseCharge]           BIT           NOT NULL CONSTRAINT DF_TransactionTaxDetails_ReverseCharge DEFAULT (0),
        [ResolutionWarning]       VARCHAR(300)  NOT NULL CONSTRAINT DF_TransactionTaxDetails_ResolutionWarning DEFAULT (''),
        [ResolvedOn]              DATETIME2     NOT NULL CONSTRAINT DF_TransactionTaxDetails_ResolvedOn DEFAULT (SYSUTCDATETIME()),
        [DateTime]                DATETIME2     NOT NULL CONSTRAINT DF_TransactionTaxDetails_DateTime DEFAULT (SYSUTCDATETIME()),
        [UpdatedateTime]          DATETIME2     NOT NULL CONSTRAINT DF_TransactionTaxDetails_UpdatedateTime DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]               BIT           NOT NULL CONSTRAINT DF_TransactionTaxDetails_IsDeleted DEFAULT (0),
        CONSTRAINT [PK_TransactionTaxDetails] PRIMARY KEY ([TransactionTaxDetailID]),
        CONSTRAINT [FK_TransactionTaxDetails_TransactionMasters] FOREIGN KEY ([TransactionMasterID])
            REFERENCES dbo.TransactionMasters ([TransactionMasterID])
    );
    -- One stamp per transaction, enforced. Two stamps would mean two different answers on record
    -- for one transaction and no way to say which the return was filed on. This index is also
    -- what makes a retry of the stamping job safe.
    CREATE UNIQUE INDEX UX_TransactionTaxDetails_TransactionMasterID
        ON dbo.TransactionTaxDetails ([TransactionMasterID]);
    PRINT 'Created table dbo.TransactionTaxDetails';
END
ELSE PRINT 'dbo.TransactionTaxDetails already exists - skipped.';
