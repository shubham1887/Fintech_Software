/*
    Invoice annexure: the source reference that makes generation safe to re-run.

    WHY THIS EXISTS
    Invoice generation runs in batches over a period. A batch can stop halfway - a connection
    drops, the app pool recycles, somebody runs it twice by accident - and the only safe way to
    recover is to run it again. That is only safe if a source already invoiced cannot be invoiced
    a second time, and the cheapest place to guarantee that is the database.

    So each annexure row names its source, and a source can appear once:
      TransactionMasterID  a commission or charge line
      FundTransactionID    a gross wallet top-up line

    Exactly one of the two is set on any row, which is why both indexes are filtered.

    Without these, a re-run produces a second invoice for the same supply, and double-counted
    turnover is as wrong as missing turnover - it just fails in the other direction, and nobody
    goes looking for money they appear to have made.

    Indexes are created in their own guarded blocks, for the reason 59_patch_invoicing.sql now
    documents: an index created inside a table's IF block is never repaired if it fails after the
    CREATE TABLE commits.

    Idempotent - safe to re-run.
*/

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;  -- required for the filtered indexes below

IF OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines', N'U') IS NULL
BEGIN
    PRINT 'dbo.TaxInvoiceAnnexureLines does not exist - run 59_patch_invoicing.sql first.';
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.TaxInvoiceAnnexureLines', 'FundTransactionID') IS NULL
    BEGIN
        ALTER TABLE dbo.TaxInvoiceAnnexureLines
            ADD [FundTransactionID] INT NOT NULL
                CONSTRAINT DF_TaxInvoiceAnnexureLines_FundTransactionID DEFAULT (0);
        PRINT 'Added dbo.TaxInvoiceAnnexureLines.FundTransactionID';
    END
    ELSE PRINT 'dbo.TaxInvoiceAnnexureLines.FundTransactionID already exists - skipped.';

    -- The non-unique index from 59 is replaced by a unique filtered one. Dropping first, because
    -- two indexes on the same column is waste and the unique one is the control.
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaxInvoiceAnnexureLines_TransactionMasterID'
                                           AND object_id = OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines'))
    BEGIN
        DROP INDEX IX_TaxInvoiceAnnexureLines_TransactionMasterID ON dbo.TaxInvoiceAnnexureLines;
        PRINT '  - IX_TaxInvoiceAnnexureLines_TransactionMasterID (replaced by the unique one)';
    END

    /* The two CREATE INDEX statements go through EXEC on purpose.

       T-SQL compiles a whole batch before executing any of it, so an index naming a column the
       ALTER TABLE above adds in the same batch fails to compile - "Invalid column name
       'FundTransactionID'" - even though the column would exist by the time it ran. Splitting on
       GO would fix it for sqlcmd, but the application's own bootstrap runner executes these files
       and may not split on GO. EXEC defers compilation to run time and works either way. */

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_TaxInvoiceAnnexureLines_TransactionMasterID'
                                               AND object_id = OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines'))
    BEGIN
        EXEC('CREATE UNIQUE INDEX UX_TaxInvoiceAnnexureLines_TransactionMasterID
                  ON dbo.TaxInvoiceAnnexureLines ([TransactionMasterID])
                  WHERE [TransactionMasterID] > 0 AND [IsDeleted] = 0;');
        PRINT '  + UX_TaxInvoiceAnnexureLines_TransactionMasterID';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_TaxInvoiceAnnexureLines_FundTransactionID'
                                               AND object_id = OBJECT_ID(N'dbo.TaxInvoiceAnnexureLines'))
    BEGIN
        EXEC('CREATE UNIQUE INDEX UX_TaxInvoiceAnnexureLines_FundTransactionID
                  ON dbo.TaxInvoiceAnnexureLines ([FundTransactionID])
                  WHERE [FundTransactionID] > 0 AND [IsDeleted] = 0;');
        PRINT '  + UX_TaxInvoiceAnnexureLines_FundTransactionID';
    END
END
