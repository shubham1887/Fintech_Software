namespace FintechApp.Responses.BankConfig
{
    /// <summary>One bank we reconcile, as the configuration screen shows it.</summary>
    public class BankConfigRow
    {
        public int BankAccountID { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string LedgerAccountCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>The Payment Mode sub-mode this bank came from. 0 = standalone.</summary>
        public int SubModeID { get; set; }
        public int ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public string SubModeName { get; set; } = string.Empty;

        /// <summary>True when the sub-mode it is linked to has gone, so the link is stale.</summary>
        public bool SubModeMissing { get; set; }

        public int FormatCount { get; set; }
        public int ImportCount { get; set; }
        public DateTime? LastImportOn { get; set; }

        /// <summary>What is missing before this bank can be reconciled, in words. Empty when ready.</summary>
        public string Gap { get; set; } = string.Empty;
    }

    public class BankConfigView
    {
        public List<BankConfigRow> Banks { get; set; } = new();

        /// <summary>Sub-modes under a Bank-like mode that are NOT linked to any bank yet — the
        /// list the Sync button would create from.</summary>
        public List<UnlinkedSubMode> UnlinkedSubModes { get; set; } = new();

        public string Note { get; set; } = string.Empty;
    }

    public class UnlinkedSubMode
    {
        public int SubModeID { get; set; }
        public int ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public string SubModeName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;

        /// <summary>What the sub-mode is missing before it can be a reconcilable bank.</summary>
        public string Gap { get; set; } = string.Empty;
    }

    public class BankSyncResult
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Notes { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class BankStatementFormatRow
    {
        public int BankStatementFormatID { get; set; }
        public int BankAccountID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int HeaderRow { get; set; }
        public int SkipRows { get; set; }
        public string DateFormat { get; set; } = string.Empty;

        public string TxnDateColumn { get; set; } = string.Empty;
        public string ValueDateColumn { get; set; } = string.Empty;
        public string DescriptionColumn { get; set; } = string.Empty;
        public string ReferenceColumn { get; set; } = string.Empty;
        public string DebitColumn { get; set; } = string.Empty;
        public string CreditColumn { get; set; } = string.Empty;
        public string AmountColumn { get; set; } = string.Empty;
        public string BalanceColumn { get; set; } = string.Empty;

        public string PdfLinePattern { get; set; } = string.Empty;
        public string PdfSkipPattern { get; set; } = string.Empty;
        public bool PdfJoinWrappedLines { get; set; }

        public bool IsActive { get; set; }
        public string Remark { get; set; } = string.Empty;
        public DateTime? LastUsedOn { get; set; }

        /// <summary>What this format does, in one sentence.</summary>
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>One row as a format would read it, for the try-before-you-save screen.</summary>
    public class BankFormatPreviewRow
    {
        public int RowNo { get; set; }
        public string TxnDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Balance { get; set; } = string.Empty;

        /// <summary>The raw text the row came from — the only way to see WHY a pattern missed.</summary>
        public string Raw { get; set; } = string.Empty;

        public bool Read { get; set; }
        public string Problem { get; set; } = string.Empty;
    }

    public class BankFormatPreview
    {
        public string Source { get; set; } = string.Empty;
        public int TotalRowsSeen { get; set; }
        public int RowsRead { get; set; }
        public int RowsSkipped { get; set; }

        /// <summary>Which column was read as what, in words.</summary>
        public string DetectedColumns { get; set; } = string.Empty;

        public List<BankFormatPreviewRow> Rows { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string Verdict { get; set; } = string.Empty;
    }

    public class BankLineCategoryRuleRow
    {
        public int BankLineCategoryRuleID { get; set; }
        public int BankAccountID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string MatchType { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string LedgerAccountCode { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; } = string.Empty;

        /// <summary>How many statement lines this rule has decided so far.</summary>
        public int MatchedLines { get; set; }

        public string Summary { get; set; } = string.Empty;
    }

    public class BankClassifyResult
    {
        public int LinesSeen { get; set; }
        public int Categorised { get; set; }
        public int LeftAlone { get; set; }
        public int DuplicatesFlagged { get; set; }
        public List<CategoryTally> ByCategory { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class CategoryTally
    {
        public string Category { get; set; } = string.Empty;
        public int Lines { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    /// <summary>One deposit claimed by more than one fund request.</summary>
    public class DuplicateFundClaim
    {
        public int FundTransactionID { get; set; }
        public DateTime DateTime { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;

        /// <summary>True when this one actually moved money into a wallet.</summary>
        public bool Approved { get; set; }
    }

    public class DuplicateFundGroup
    {
        /// <summary>The reference the requests share — a UTR, usually.</summary>
        public string Reference { get; set; } = string.Empty;

        public int ClaimCount { get; set; }
        public int ApprovedCount { get; set; }

        /// <summary>What was credited in total against this one reference.</summary>
        public decimal ApprovedAmount { get; set; }

        /// <summary>What a single deposit of this reference was worth — the first claim's amount.
        /// Anything above it is the exposure.</summary>
        public decimal SingleAmount { get; set; }

        /// <summary>Approved total less one genuine deposit: the money at risk.</summary>
        public decimal Exposure { get; set; }

        /// <summary>True when a bank statement line with this reference has been imported, so the
        /// bank's own side can be seen. False means nobody has checked it against a statement.</summary>
        public bool SeenOnStatement { get; set; }

        /// <summary>What the bank actually credited against it, where a statement was imported.</summary>
        public decimal BankCredited { get; set; }

        public List<DuplicateFundClaim> Claims { get; set; } = new();
        public string Verdict { get; set; } = string.Empty;
    }

    public class DuplicateFundReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int GroupCount { get; set; }
        public decimal TotalExposure { get; set; }
        public List<DuplicateFundGroup> Groups { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }
}
