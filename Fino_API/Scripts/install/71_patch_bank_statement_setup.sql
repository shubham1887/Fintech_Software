/*
    71 - Bank reconciliation setup: our banks from Payment Modes, a saved statement format per
         bank (PDF included), and what a statement line MEANS.

    THREE THINGS THAT WERE MISSING, AND WHY EACH ONE MATTERS

    1. WHICH BANKS ARE OURS - in two places.
       The company's bank accounts already exist under Payment Modes, as the sub-modes a retailer
       deposits into: SubModes carries BankName, AccountNumber, IFSC and BeneficiaryName. The
       reconciliation side kept its own BankAccounts list beside it, so the same account was typed
       twice and the two could disagree about a digit - and the one that disagrees is the one a
       statement fails to match. BankAccounts now carries SubModeID / ModeID and is filled from
       there. Nothing is ever deleted by the sync: a bank with statements imported against it stays,
       whatever later happens to the sub-mode.

    2. HOW EACH BANK WRITES A STATEMENT - asked at every upload.
       The importer detects the usual shapes (it knows HDFC's "Withdrawal Amt." is ICICI's "Debit"),
       and for most CSV and Excel exports that is enough. It is not enough for a bank whose export
       has no header, and it is never enough for a PDF: a PDF has no columns, only glyphs at
       coordinates. Describing that at every upload is a per-client manual step, which on a
       white-label install is a defect - the person uploading the statement is not the person who
       knows what a column map is. BankStatementFormats holds it once per bank: the column map, the
       rows to skip, the date format, and for PDFs a line pattern and a skip pattern.

    3. WHAT A LINE IS - nowhere at all.
       Matching a statement line to a voucher proves the money moved. It cannot say what the money
       was FOR, and that is what an operator actually writes down: stock bought from a provider, a
       retailer's deposit, an expense, a transfer between our own accounts. The bank's narration is
       the only evidence, so BankLineCategoryRules reads it - "NEFT DR-JAIJUI" is a purchase,
       "UPI/.../RAHUL STORE" is a user's funding - and the answer is stored on the line rather than
       re-derived, because a category that moves when somebody edits a rule is not a record.

    AND THE ONE THAT COSTS MONEY
    BankStatementLines gains DuplicateOfLineID / DuplicateReason. Two cases are flagged: the same
    reference and amount twice on one statement (usually a statement uploaded over itself), and -
    the expensive one - a credit that MORE THAN ONE APPROVED FUND REQUEST claims. The bank credited
    once; the wallet was funded twice. That is visible nowhere else, because each request looks
    perfectly ordinary on its own.

    CounterpartyUserID / CounterpartyName answer "who is this line about" for a credit that matches
    a known deposit, so the register reads as names rather than as UTRs.

    Every column is added with an explicit DEFAULT - the live database has no defaults where dev
    does, and an INSERT that omits one passes every test and fails in production with "Cannot insert
    the value NULL".

    Idempotent - safe to re-run.
*/
-- QUOTED_IDENTIFIER must be ON in any batch that ALTERs a table carrying a filtered index.
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT '=== 71: bank statement setup (banks from modes, formats, line categories) ===';

/* ──────────────────────────────────────────────────────────────────────────────
   1. How each bank writes its statement.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[BankStatementFormats]', N'U') IS NULL
BEGIN
    CREATE TABLE [BankStatementFormats] (
        [BankStatementFormatID] int IDENTITY(1,1) NOT NULL,
        [BankAccountID]      int           NOT NULL CONSTRAINT [DF_BSF_Bank] DEFAULT (0),
        [Name]               varchar(120)  NOT NULL CONSTRAINT [DF_BSF_Name] DEFAULT (''),
        -- CSV / XLSX / PDF
        [Source]             varchar(10)   NOT NULL CONSTRAINT [DF_BSF_Source] DEFAULT ('CSV'),
        -- -1 = detect the header row, which is right for most files.
        [HeaderRow]          int           NOT NULL CONSTRAINT [DF_BSF_HeaderRow] DEFAULT (-1),
        [SkipRows]           int           NOT NULL CONSTRAINT [DF_BSF_SkipRows] DEFAULT (0),
        [DateFormat]         varchar(40)   NOT NULL CONSTRAINT [DF_BSF_DateFormat] DEFAULT (''),
        -- Each column is a HEADER NAME or a 0-based index; blank means "detect it".
        [TxnDateColumn]      varchar(80)   NOT NULL CONSTRAINT [DF_BSF_TxnDate] DEFAULT (''),
        [ValueDateColumn]    varchar(80)   NOT NULL CONSTRAINT [DF_BSF_ValueDate] DEFAULT (''),
        [DescriptionColumn]  varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Desc] DEFAULT (''),
        [ReferenceColumn]    varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Ref] DEFAULT (''),
        [DebitColumn]        varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Debit] DEFAULT (''),
        [CreditColumn]       varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Credit] DEFAULT (''),
        [AmountColumn]       varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Amount] DEFAULT (''),
        [BalanceColumn]      varchar(80)   NOT NULL CONSTRAINT [DF_BSF_Balance] DEFAULT (''),
        -- PDF only: a regex with named groups (date, desc, ref, debit, credit, amount, balance).
        [PdfLinePattern]     varchar(600)  NOT NULL CONSTRAINT [DF_BSF_PdfLine] DEFAULT (''),
        [PdfSkipPattern]     varchar(400)  NOT NULL CONSTRAINT [DF_BSF_PdfSkip] DEFAULT (''),
        -- A wrapped narration joined back onto its row; without it the tail is silently dropped,
        -- and the tail is where the counterparty's name usually is.
        [PdfJoinWrappedLines] bit          NOT NULL CONSTRAINT [DF_BSF_PdfJoin] DEFAULT (1),
        [LastUsedOn]         datetime2     NULL,
        [IsActive]           bit           NOT NULL CONSTRAINT [DF_BSF_IsActive] DEFAULT (1),
        [Remark]             varchar(300)  NOT NULL CONSTRAINT [DF_BSF_Remark] DEFAULT (''),
        [CreatedByUserID]    int           NOT NULL CONSTRAINT [DF_BSF_CreatedBy] DEFAULT (0),
        [DateTime]           datetime2     NOT NULL CONSTRAINT [DF_BSF_DateTime] DEFAULT (GETDATE()),
        [UpdatedateTime]     datetime2     NOT NULL CONSTRAINT [DF_BSF_Updated] DEFAULT (GETDATE()),
        [IsDeleted]          bit           NOT NULL CONSTRAINT [DF_BSF_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_BankStatementFormats] PRIMARY KEY ([BankStatementFormatID])
    );
    PRINT '  created BankStatementFormats';
END
ELSE
    PRINT '  BankStatementFormats already present';

IF OBJECT_ID(N'[BankStatementFormats]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_BankStatementFormats_Bank_Source'
                     AND [object_id] = OBJECT_ID(N'[BankStatementFormats]'))
BEGIN
    -- The importer picks a format by (bank, file type) on every upload.
    CREATE NONCLUSTERED INDEX [IX_BankStatementFormats_Bank_Source]
        ON dbo.BankStatementFormats ([BankAccountID], [Source], [IsActive]);
    PRINT '  index IX_BankStatementFormats_Bank_Source created';
END
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   2. What a statement line means.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[BankLineCategoryRules]', N'U') IS NULL
BEGIN
    CREATE TABLE [BankLineCategoryRules] (
        [BankLineCategoryRuleID] int IDENTITY(1,1) NOT NULL,
        -- 0 = every bank. A rule pinned to one bank wins over a general one.
        [BankAccountID]      int           NOT NULL CONSTRAINT [DF_BLCR_Bank] DEFAULT (0),
        -- DEBIT / CREDIT / ANY
        [Direction]          varchar(10)   NOT NULL CONSTRAINT [DF_BLCR_Direction] DEFAULT ('ANY'),
        -- CONTAINS / STARTS_WITH / REGEX
        [MatchType]          varchar(20)   NOT NULL CONSTRAINT [DF_BLCR_MatchType] DEFAULT ('CONTAINS'),
        [Pattern]            varchar(300)  NOT NULL CONSTRAINT [DF_BLCR_Pattern] DEFAULT (''),
        [MinAmount]          decimal(18,2) NOT NULL CONSTRAINT [DF_BLCR_Min] DEFAULT (0),
        [MaxAmount]          decimal(18,2) NOT NULL CONSTRAINT [DF_BLCR_Max] DEFAULT (0),
        -- PURCHASE / USER_FUND / EXPENSE / TRANSFER / BANK_CHARGES / INTEREST / SETTLEMENT / REFUND
        [Category]           varchar(30)   NOT NULL CONSTRAINT [DF_BLCR_Category] DEFAULT ('UNCATEGORISED'),
        [LedgerAccountCode]  varchar(20)   NOT NULL CONSTRAINT [DF_BLCR_Account] DEFAULT (''),
        [Priority]           int           NOT NULL CONSTRAINT [DF_BLCR_Priority] DEFAULT (0),
        [IsActive]           bit           NOT NULL CONSTRAINT [DF_BLCR_IsActive] DEFAULT (1),
        [Remark]             varchar(300)  NOT NULL CONSTRAINT [DF_BLCR_Remark] DEFAULT (''),
        [CreatedByUserID]    int           NOT NULL CONSTRAINT [DF_BLCR_CreatedBy] DEFAULT (0),
        [DateTime]           datetime2     NOT NULL CONSTRAINT [DF_BLCR_DateTime] DEFAULT (GETDATE()),
        [UpdatedateTime]     datetime2     NOT NULL CONSTRAINT [DF_BLCR_Updated] DEFAULT (GETDATE()),
        [IsDeleted]          bit           NOT NULL CONSTRAINT [DF_BLCR_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_BankLineCategoryRules] PRIMARY KEY ([BankLineCategoryRuleID])
    );
    PRINT '  created BankLineCategoryRules';
END
ELSE
    PRINT '  BankLineCategoryRules already present';
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   3. The new columns on what already exists.
   ────────────────────────────────────────────────────────────────────────────── */
DECLARE @cols TABLE (TableName SYSNAME, ColumnName SYSNAME, Definition NVARCHAR(200));

INSERT INTO @cols VALUES
    -- Where this bank came from: the Payment Mode sub-mode the operator already maintains.
    ('BankAccounts', 'SubModeID', 'int NOT NULL CONSTRAINT [DF_BA_SubModeID] DEFAULT (0)'),
    ('BankAccounts', 'ModeID',    'int NOT NULL CONSTRAINT [DF_BA_ModeID] DEFAULT (0)'),

    -- What the line is, and who decided.
    ('BankStatementLines', 'Category',          'varchar(30) NOT NULL CONSTRAINT [DF_BSL_Category] DEFAULT (''UNCATEGORISED'')'),
    ('BankStatementLines', 'CategoryRuleID',    'int NOT NULL CONSTRAINT [DF_BSL_CategoryRule] DEFAULT (0)'),
    -- A person''s decision is final: no later rule run overwrites it.
    ('BankStatementLines', 'CategorySetByUser', 'bit NOT NULL CONSTRAINT [DF_BSL_CategoryByUser] DEFAULT (0)'),

    -- Who the line is about, resolved once at import so the register reads as names.
    ('BankStatementLines', 'CounterpartyUserID', 'int NOT NULL CONSTRAINT [DF_BSL_Counterparty] DEFAULT (0)'),
    ('BankStatementLines', 'CounterpartyName',   'varchar(150) NOT NULL CONSTRAINT [DF_BSL_CounterpartyName] DEFAULT ('''')'),

    -- The same money twice.
    ('BankStatementLines', 'DuplicateOfLineID', 'int NOT NULL CONSTRAINT [DF_BSL_DuplicateOf] DEFAULT (0)'),
    ('BankStatementLines', 'DuplicateReason',   'varchar(250) NOT NULL CONSTRAINT [DF_BSL_DuplicateReason] DEFAULT ('''')');

DECLARE @t SYSNAME, @c SYSNAME, @d NVARCHAR(200), @sql NVARCHAR(MAX), @added INT = 0, @skipped INT = 0;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT TableName, ColumnName, Definition FROM @cols;
OPEN cur;
FETCH NEXT FROM cur INTO @t, @c, @d;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(@t), N'U') IS NULL
    BEGIN
        PRINT '  SKIP (no such table): ' + @t + ' - apply 66_ first';
        SET @skipped = @skipped + 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE [object_id] = OBJECT_ID(QUOTENAME(@t)) AND [name] = @c)
        SET @skipped = @skipped + 1;
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
PRINT '  columns added: ' + CAST(@added AS varchar(10)) + ', already present: ' + CAST(@skipped AS varchar(10));
GO
SET QUOTED_IDENTIFIER ON;

/* ──────────────────────────────────────────────────────────────────────────────
   4. A starter set of category rules.

      Deliberately small, and only the phrases that mean the same thing at every Indian bank. The
      operator's own providers and expenses are named differently per install, so guessing them
      here would file real money under a label nobody chose. Everything unmatched reads
      UNCATEGORISED, which is honest, and the Bank Setup screen is where the rest get written.
   ────────────────────────────────────────────────────────────────────────────── */
IF OBJECT_ID(N'[BankLineCategoryRules]', N'U') IS NOT NULL
BEGIN
    DECLARE @seed TABLE (Direction VARCHAR(10), MatchType VARCHAR(20), Pattern VARCHAR(300),
                         Category VARCHAR(30), Priority INT, Remark VARCHAR(300));

    INSERT INTO @seed VALUES
      ('DEBIT',  'CONTAINS', 'BANK CHARGE',  'BANK_CHARGES', 50, 'What the bank itself takes.'),
      ('DEBIT',  'CONTAINS', 'SERVICE CHARGE','BANK_CHARGES', 50, 'What the bank itself takes.'),
      ('DEBIT',  'CONTAINS', 'CHRG',         'BANK_CHARGES', 40, 'Abbreviated bank charge on most statements.'),
      ('DEBIT',  'CONTAINS', 'GST ON',       'BANK_CHARGES', 55, 'GST the bank charges on its own fees.'),
      ('CREDIT', 'CONTAINS', 'INT.PD',       'INTEREST',     50, 'Interest paid by the bank.'),
      ('CREDIT', 'CONTAINS', 'INTEREST',     'INTEREST',     45, 'Interest credited.'),
      ('CREDIT', 'CONTAINS', 'SETTLEMENT',   'SETTLEMENT',   45, 'A gateway or aggregator settling a batch.'),
      ('CREDIT', 'CONTAINS', 'REVERSAL',     'REFUND',       45, 'Money coming back.'),
      ('CREDIT', 'CONTAINS', 'RETURN',       'REFUND',       40, 'A failed transfer returned.');

    DECLARE @dir VARCHAR(10), @mt VARCHAR(20), @pat VARCHAR(300), @cat VARCHAR(30), @pri INT, @rem VARCHAR(300);
    DECLARE s CURSOR LOCAL FAST_FORWARD FOR SELECT Direction, MatchType, Pattern, Category, Priority, Remark FROM @seed;
    OPEN s;
    FETCH NEXT FROM s INTO @dir, @mt, @pat, @cat, @pri, @rem;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.BankLineCategoryRules
                       WHERE [BankAccountID] = 0 AND [Pattern] = @pat AND [IsDeleted] = 0)
        BEGIN
            INSERT INTO dbo.BankLineCategoryRules
                ([BankAccountID],[Direction],[MatchType],[Pattern],[MinAmount],[MaxAmount],
                 [Category],[LedgerAccountCode],[Priority],[IsActive],[Remark],[CreatedByUserID],
                 [DateTime],[UpdatedateTime],[IsDeleted])
            VALUES (0, @dir, @mt, @pat, 0, 0, @cat, '', @pri, 1, @rem, 0, GETDATE(), GETDATE(), 0);
            PRINT '  seeded rule: ' + @pat + ' -> ' + @cat;
        END
        FETCH NEXT FROM s INTO @dir, @mt, @pat, @cat, @pri, @rem;
    END
    CLOSE s; DEALLOCATE s;
END
GO
SET QUOTED_IDENTIFIER ON;

PRINT '';
PRINT '=== Bank setup now ===';
IF OBJECT_ID(N'[BankAccounts]', N'U') IS NOT NULL
    SELECT 'banks' AS [What], COUNT(*) AS [N],
           SUM(CASE WHEN SubModeID > 0 THEN 1 ELSE 0 END) AS LinkedToAModeSubMode
      FROM dbo.BankAccounts WHERE IsDeleted = 0;
IF OBJECT_ID(N'[BankStatementFormats]', N'U') IS NOT NULL
    SELECT 'statement formats' AS [What], COUNT(*) AS [N] FROM dbo.BankStatementFormats WHERE IsDeleted = 0;
IF OBJECT_ID(N'[BankLineCategoryRules]', N'U') IS NOT NULL
    SELECT 'category rules' AS [What], COUNT(*) AS [N] FROM dbo.BankLineCategoryRules WHERE IsDeleted = 0;
PRINT 'Open Accountant -> Bank Setup and press Sync to bring the banks in from Payment Modes.';
PRINT '=== 71 complete ===';
