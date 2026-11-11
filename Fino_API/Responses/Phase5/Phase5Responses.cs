namespace FintechApp.Responses.Phase5
{
    /// <summary>What a statement upload produced.</summary>
    public class BankStatementImportResult
    {
        public int BankStatementImportID { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int LinesImported { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal? ClosingBalance { get; set; }

        /// <summary>True when a previous ACTIVE import for the same account and range was
        /// superseded by this one.</summary>
        public bool SupersededPrevious { get; set; }

        /// <summary>Which column the parser decided was which — shown back on the screen, because
        /// a statement parsed into the wrong columns otherwise reconciles to nonsense quietly.</summary>
        public string DetectedColumns { get; set; } = string.Empty;

        /// <summary>Rows that could not be read, with the row number and the reason. These are not
        /// failures of the import — the rest still landed — they are the list to look at.</summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>How many lines the category rules decided. The rest read UNCATEGORISED and
        /// are shown as such — a guess would be worse than a blank.</summary>
        public int Categorised { get; set; }

        /// <summary>Lines that look like the same money twice: repeated on the statement, or
        /// claimed by more than one fund request.</summary>
        public int DuplicatesFlagged { get; set; }
    }

    /// <summary>One movement in a reconciliation bucket. Both sides' figures are carried so a
    /// mismatch shows WHAT disagrees, not just that something does.</summary>
    public class BankReconRow
    {
        public string Bucket { get; set; } = string.Empty;

        // The statement side, or zero/empty when the movement is only in the ledger.
        public int? BankStatementLineID { get; set; }
        public DateTime? BankDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public decimal BankDebit { get; set; }
        public decimal BankCredit { get; set; }

        /// <summary>What the statement line IS — purchase, a user's deposit, an expense. Matching
        /// proves it happened; this says what it was, and the two are different questions.</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>Who the line is about, where the narration named somebody we know.</summary>
        public string Counterparty { get; set; } = string.Empty;

        /// <summary>Set when the line looks like the same money twice, with the reason.</summary>
        public string DuplicateReason { get; set; } = string.Empty;

        // The ledger side, or zero/empty when the movement is only on the statement.
        public int? AcctVoucherLineID { get; set; }
        public int? AcctVoucherID { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public DateTime? VoucherDate { get; set; }
        public string Narration { get; set; } = string.Empty;
        public decimal LedgerDebit { get; set; }
        public decimal LedgerCredit { get; set; }

        /// <summary>Signed bank amount − signed ledger amount, where + is money in. Zero (within
        /// tolerance) in the Matched bucket.</summary>
        public decimal Variance { get; set; }

        /// <summary>Reference / AmountAndDate / Manual — empty when the row is unmatched.</summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>Days between the two dates, for a pairing made on amount alone. A match seven
        /// days apart is worth a second look even when the amount is exact.</summary>
        public int DateGapDays { get; set; }

        /// <summary>
        /// For a statement line the LEDGER does not know about: whether the operator's own records
        /// explain it anyway. "Resolved" when it pairs with an approved fund request or a recorded
        /// provider purchase, or a person categorised it as a charge / expense / transfer;
        /// "Pending" when it still needs an entry or a category. Empty on matched and ledger-only rows.
        /// </summary>
        public string Resolution { get; set; } = string.Empty;
        /// <summary>Why — "Fund request #41 from Sharma Traders", "Provider purchase not recorded yet".</summary>
        public string ResolutionNote { get; set; } = string.Empty;
    }

    public class BankReconBucketSummary
    {
        public string Bucket { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal BankAmount { get; set; }
        public decimal LedgerAmount { get; set; }
    }

    public class BankReconResult
    {
        public int BankAccountID { get; set; }
        public string BankAccountName { get; set; } = string.Empty;
        public string LedgerAccountCode { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal Tolerance { get; set; }
        public int DateWindowDays { get; set; }
        public bool Committed { get; set; }

        /// <summary>True when no active statement covers this range — every ledger movement then
        /// falls into MissingInBank, which is an "upload the statement first" signal, not a gap.</summary>
        public bool NoStatementImported { get; set; }

        /// <summary>
        /// The reconciliation statement proper, and the only figures an accountant will quote:
        /// the bank's closing balance, the ledger's Bank balance, and the difference the four
        /// buckets have to explain between them.
        /// </summary>
        public decimal BankClosingBalance { get; set; }
        public decimal LedgerClosingBalance { get; set; }
        public decimal Difference { get; set; }
        public bool Explained { get; set; }

        public List<BankReconBucketSummary> Summary { get; set; } = new();
        public List<BankReconRow> Rows { get; set; } = new();
    }

    public class BankStatementImportRow
    {
        public int BankStatementImportID { get; set; }
        public int BankAccountID { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ImportedOn { get; set; }

        /// <summary>How much of this import is still unmatched — the one number that says whether
        /// the upload has actually been worked.</summary>
        public int UnmatchedLines { get; set; }
    }
}
