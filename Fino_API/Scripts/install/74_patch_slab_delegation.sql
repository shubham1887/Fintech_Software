/*
    Hierarchy-based commission slab delegation.

    A commission slab used to be one thing: a rate card the platform operator wrote and handed to
    whoever they liked. This patch gives a slab an OWNER and a PARENT, so a super-distributor or a
    distributor can cut their own card out of the one they were given and resell inside the margin
    it allows, without anybody being able to hand out more than they hold.

    Three parts:

      * SlabMasters gains ParentSlabID / OwnerUserID / OwnerUserType / CreatedByUserID /
        IsDelegatedSlab / Status. Every default is "the platform operator's own card", so each
        existing row reads exactly as it did before and nothing about today's commissions changes.

      * SlabDetails gains the parent-band link, a snapshot of the ceiling the band was saved
        under, and a per-band Status. The snapshot carries NO weight in the commission
        calculation - the engine still reads the tier columns and nothing else. It exists so a
        refusal can name the figure it measured against, and so an audit can say what the ceiling
        WAS at the time rather than what it is today. Status is the one column the engine does
        read: a band marked INVALID (the parent card was later lowered past it) is refused rather
        than paid, because falling through to the parent's card would pay MORE than the owner now
        allows.

      * SlabCommissionAudits - one row per commission figure that actually moved, on any slab, by
        anybody, with the ceiling it was measured against. Append-only: nothing in the application
        updates or deletes a row here.

    Idempotent - safe to re-run. Nothing is back-filled, because the correct value for every
    existing row is the default.
*/

-- ── SlabMasters: who owns this card, and which card it was cut from ────────────────────────
IF COL_LENGTH(N'dbo.SlabMasters', N'ParentSlabID') IS NULL
BEGIN
    ALTER TABLE dbo.SlabMasters ADD [ParentSlabID] INT NULL;
END
GO

IF COL_LENGTH(N'dbo.SlabMasters', N'OwnerUserID') IS NULL
BEGIN
    -- NULL = the platform operator's own card. Deliberately nullable rather than defaulting to
    -- the admin's user id: an install's admin id is not a constant, and "nobody below owns this"
    -- is the fact being recorded.
    ALTER TABLE dbo.SlabMasters ADD [OwnerUserID] INT NULL;
END
GO

IF COL_LENGTH(N'dbo.SlabMasters', N'OwnerUserType') IS NULL
BEGIN
    ALTER TABLE dbo.SlabMasters
        ADD [OwnerUserType] INT NOT NULL
            CONSTRAINT DF_SlabMasters_OwnerUserType DEFAULT (0);
END
GO

IF COL_LENGTH(N'dbo.SlabMasters', N'CreatedByUserID') IS NULL
BEGIN
    ALTER TABLE dbo.SlabMasters ADD [CreatedByUserID] INT NULL;
END
GO

IF COL_LENGTH(N'dbo.SlabMasters', N'IsDelegatedSlab') IS NULL
BEGIN
    ALTER TABLE dbo.SlabMasters
        ADD [IsDelegatedSlab] BIT NOT NULL
            CONSTRAINT DF_SlabMasters_IsDelegatedSlab DEFAULT (0);
END
GO

IF COL_LENGTH(N'dbo.SlabMasters', N'Status') IS NULL
BEGIN
    ALTER TABLE dbo.SlabMasters
        ADD [Status] VARCHAR(20) NOT NULL
            CONSTRAINT DF_SlabMasters_Status DEFAULT ('ACTIVE');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabMasters_OwnerUserID' AND object_id = OBJECT_ID(N'dbo.SlabMasters'))
    CREATE INDEX IX_SlabMasters_OwnerUserID ON dbo.SlabMasters ([OwnerUserID]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabMasters_ParentSlabID' AND object_id = OBJECT_ID(N'dbo.SlabMasters'))
    CREATE INDEX IX_SlabMasters_ParentSlabID ON dbo.SlabMasters ([ParentSlabID]);
GO

-- ── SlabDetails: the ceiling this band was saved under, and whether it still fits ──────────
IF COL_LENGTH(N'dbo.SlabDetails', N'ParentSlabDetailID') IS NULL
BEGIN
    ALTER TABLE dbo.SlabDetails ADD [ParentSlabDetailID] INT NULL;
END
GO

IF COL_LENGTH(N'dbo.SlabDetails', N'ParentRetailerComm') IS NULL
BEGIN
    ALTER TABLE dbo.SlabDetails ADD
        [ParentRetailerComm]         DECIMAL(18,5) NOT NULL CONSTRAINT DF_SlabDetails_ParentRetailerComm DEFAULT (0),
        [ParentDistributorComm]      DECIMAL(18,5) NOT NULL CONSTRAINT DF_SlabDetails_ParentDistributorComm DEFAULT (0),
        [ParentSuperDistributorComm] DECIMAL(18,5) NOT NULL CONSTRAINT DF_SlabDetails_ParentSuperDistributorComm DEFAULT (0),
        [ParentClientComm]           DECIMAL(18,5) NOT NULL CONSTRAINT DF_SlabDetails_ParentClientComm DEFAULT (0);
END
GO

IF COL_LENGTH(N'dbo.SlabDetails', N'ParentCommType') IS NULL
BEGIN
    ALTER TABLE dbo.SlabDetails ADD
        [ParentCommType]       VARCHAR(100) NOT NULL CONSTRAINT DF_SlabDetails_ParentCommType DEFAULT (''),
        [ParentGSTType]        VARCHAR(20)  NOT NULL CONSTRAINT DF_SlabDetails_ParentGSTType DEFAULT (''),
        [ParentGSTRatePercent] DECIMAL(9,4) NOT NULL CONSTRAINT DF_SlabDetails_ParentGSTRatePercent DEFAULT (0);
END
GO

IF COL_LENGTH(N'dbo.SlabDetails', N'Status') IS NULL
BEGIN
    ALTER TABLE dbo.SlabDetails
        ADD [Status] VARCHAR(20) NOT NULL
            CONSTRAINT DF_SlabDetails_Status DEFAULT ('ACTIVE');
END
GO

-- The commission engine's band match, which runs on every transaction. It was matching on an
-- unindexed (SlabMasterID, ServiceID, OperatorID, amount) and reading the whole table out of the
-- in-process cache; delegation multiplies the number of rows by the number of reseller cards, so
-- the index earns its keep now whether or not it did before.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabDetails_Band' AND object_id = OBJECT_ID(N'dbo.SlabDetails'))
    CREATE INDEX IX_SlabDetails_Band ON dbo.SlabDetails ([SlabMasterID], [ServiceID], [OperatorID], [FromAmount]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabDetails_ParentSlabDetailID' AND object_id = OBJECT_ID(N'dbo.SlabDetails'))
    CREATE INDEX IX_SlabDetails_ParentSlabDetailID ON dbo.SlabDetails ([ParentSlabDetailID]);
GO

-- ── The audit trail ───────────────────────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.SlabCommissionAudits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SlabCommissionAudits
    (
        [SlabCommissionAuditID] BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_SlabCommissionAudits PRIMARY KEY,
        [SlabMasterID]      INT            NOT NULL,
        -- 0 for a slab-level act (created / assigned / invalidated) with no one band.
        [SlabDetailID]      INT            NOT NULL CONSTRAINT DF_SlabCommissionAudits_SlabDetailID DEFAULT (0),
        [OwnerUserID]       INT            NULL,
        [ParentUserID]      INT            NULL,
        [UpdatedByUserID]   INT            NOT NULL,
        [ServiceID]         INT            NOT NULL CONSTRAINT DF_SlabCommissionAudits_ServiceID DEFAULT (0),
        [OperatorID]        INT            NOT NULL CONSTRAINT DF_SlabCommissionAudits_OperatorID DEFAULT (0),
        [FromAmount]        DECIMAL(18,5)  NOT NULL CONSTRAINT DF_SlabCommissionAudits_FromAmount DEFAULT (0),
        [ToAmount]          DECIMAL(18,5)  NOT NULL CONSTRAINT DF_SlabCommissionAudits_ToAmount DEFAULT (0),
        -- Retailer / Distributor / SuperDistributor / Client, or CREATE / ASSIGN / UNASSIGN /
        -- INVALIDATE / REVALIDATE.
        [Tier]              VARCHAR(40)    NOT NULL CONSTRAINT DF_SlabCommissionAudits_Tier DEFAULT (''),
        [OldCommission]     DECIMAL(18,5)  NOT NULL CONSTRAINT DF_SlabCommissionAudits_OldCommission DEFAULT (0),
        [NewCommission]     DECIMAL(18,5)  NOT NULL CONSTRAINT DF_SlabCommissionAudits_NewCommission DEFAULT (0),
        [OldCommType]       VARCHAR(100)   NOT NULL CONSTRAINT DF_SlabCommissionAudits_OldCommType DEFAULT (''),
        [NewCommType]       VARCHAR(100)   NOT NULL CONSTRAINT DF_SlabCommissionAudits_NewCommType DEFAULT (''),
        -- The ceiling the new figure was measured against, at save time.
        [ParentCommission]  DECIMAL(18,5)  NOT NULL CONSTRAINT DF_SlabCommissionAudits_ParentCommission DEFAULT (0),
        [Remark]            VARCHAR(400)   NOT NULL CONSTRAINT DF_SlabCommissionAudits_Remark DEFAULT (''),
        [IPAddress]         VARCHAR(64)    NOT NULL CONSTRAINT DF_SlabCommissionAudits_IPAddress DEFAULT (''),
        [DateTime]          DATETIME2      NOT NULL CONSTRAINT DF_SlabCommissionAudits_DateTime DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabCommissionAudits_Slab' AND object_id = OBJECT_ID(N'dbo.SlabCommissionAudits'))
    CREATE INDEX IX_SlabCommissionAudits_Slab ON dbo.SlabCommissionAudits ([SlabMasterID], [DateTime]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SlabCommissionAudits_Owner' AND object_id = OBJECT_ID(N'dbo.SlabCommissionAudits'))
    CREATE INDEX IX_SlabCommissionAudits_Owner ON dbo.SlabCommissionAudits ([OwnerUserID], [DateTime]);
GO

/*
    No menu rows here on purpose. The sidebar is defined in code (Services/MenuService.cs), not in
    a table, so the "My Commission Slabs" and "Commission Audit Trail" entries ship with the API
    build rather than with this script. Nothing to seed, and nothing for an installer to get
    out of step with.
*/

