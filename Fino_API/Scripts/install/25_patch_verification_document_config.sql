/* ============================================================================
   Verification Document Configuration — schema + seed
   ----------------------------------------------------------------------------
   Database-driven document master for Manual (non-eKYC) verification. Admin
   manages this list from Settings → Verification Documents; the user's Manual
   Verification screen renders whatever is Active. Adding a new regulatory
   document later needs NO code change — just a new row here.

   Also seeds the Key Setting flag `eVerification`:
     TRUE  ⇒ user verified via E-KYC / E-Verification (E-KYC Settings).
     FALSE ⇒ user redirected to Manual Verification (these documents).

   Idempotent — safe to run repeatedly.
   NOTE: repositories cache in memory — RESTART the API after running this.
   ============================================================================ */
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'VerificationDocumentConfigs')
BEGIN
    CREATE TABLE dbo.VerificationDocumentConfigs
    (
        VerificationDocumentConfigID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_VerificationDocumentConfigs PRIMARY KEY,
        DocumentName  VARCHAR(150) NOT NULL CONSTRAINT DF_VDC_DocumentName DEFAULT (''),
        DocumentCode  VARCHAR(50)  NOT NULL CONSTRAINT DF_VDC_DocumentCode DEFAULT (''),
        DocumentType  VARCHAR(50)  NOT NULL CONSTRAINT DF_VDC_DocumentType DEFAULT (''),
        IsRequired    BIT          NOT NULL CONSTRAINT DF_VDC_IsRequired   DEFAULT (0),
        IsActive      BIT          NOT NULL CONSTRAINT DF_VDC_IsActive     DEFAULT (1),
        DisplayOrder  INT          NOT NULL CONSTRAINT DF_VDC_DisplayOrder DEFAULT (0),
        CreatedBy     INT          NOT NULL CONSTRAINT DF_VDC_CreatedBy    DEFAULT (0),
        CreatedDate   DATETIME2    NOT NULL CONSTRAINT DF_VDC_CreatedDate  DEFAULT (SYSUTCDATETIME()),
        UpdatedBy     INT          NULL,
        UpdatedDate   DATETIME2    NULL
    );
    PRINT 'Created table dbo.VerificationDocumentConfigs';
END
ELSE
    PRINT 'Table dbo.VerificationDocumentConfigs already exists — skipped.';

/* Seed a sensible starter set (PAN required, others optional). */
IF NOT EXISTS (SELECT 1 FROM dbo.VerificationDocumentConfigs)
BEGIN
    INSERT INTO dbo.VerificationDocumentConfigs
        (DocumentName, DocumentCode, DocumentType, IsRequired, IsActive, DisplayOrder, CreatedBy, CreatedDate)
    VALUES
        ('PAN Card',          'PAN',     'Identity', 1, 1, 1, 0, SYSUTCDATETIME()),
        ('Aadhaar Card',      'AADHAAR', 'Identity', 1, 1, 2, 0, SYSUTCDATETIME()),
        ('GST Certificate',   'GST',     'Business', 0, 1, 3, 0, SYSUTCDATETIME()),
        ('Shop / Business Photo', 'SHOP_PHOTO', 'Business', 0, 1, 4, 0, SYSUTCDATETIME());
    PRINT 'Seeded starter verification documents.';
END
ELSE
    PRINT 'VerificationDocumentConfigs already has rows — seed skipped.';

/* Key Setting: eVerification (defaults OFF ⇒ Manual Verification). */
IF NOT EXISTS (SELECT 1 FROM dbo.SettingMasters WHERE [Key] = 'eVerification')
BEGIN
    INSERT INTO dbo.SettingMasters ([Key], Value, IsOn, IsSecure, Description, IsPassword, [DateTime], UpdatedateTime, IsDeleted)
    VALUES ('eVerification', 'false', 0, 0, 'When ON, users are verified via E-KYC / E-Verification. When OFF, users complete Manual Verification by uploading the admin-configured documents.', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
    PRINT 'Inserted SettingMaster: eVerification = false';
END
ELSE
    PRINT 'SettingMaster "eVerification" already exists — skipped.';

SELECT VerificationDocumentConfigID, DocumentName, DocumentCode, IsRequired, IsActive, DisplayOrder
FROM dbo.VerificationDocumentConfigs ORDER BY DisplayOrder;
GO
