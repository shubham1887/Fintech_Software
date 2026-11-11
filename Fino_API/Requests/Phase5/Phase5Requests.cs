namespace FintechApp.Requests.Phase5
{
    /// <summary>
    /// Upload a statement. The file arrives as text (CSV) or base64 (XLSX) in the encrypted body,
    /// the same way the 2B and e-invoice imports take their JSON — there is no multipart path
    /// through the AES envelope, and inventing one for this alone would be a second way in.
    /// </summary>
    public class BankStatementImportRequest
    {
        public int BankAccountID { get; set; }

        /// <summary>CSV text, or the .xlsx bytes as base64. <see cref="Source"/> says which.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>CSV or XLSX. Blank is read as CSV.</summary>
        public string Source { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Optional column mapping, by header name or 0-based index, for a bank whose headers the
        /// parser cannot recognise. Left empty the parser detects them — which is the normal case,
        /// and the point: a non-technical operator should not have to describe their statement.
        /// </summary>
        public BankColumnMap? Columns { get; set; }

        public int ImportedByUserID { get; set; }

        /// <summary>
        /// The bank's saved statement format to read this file with. 0 picks the bank's active
        /// format for the kind of file being uploaded, which is the point of saving one: the
        /// person uploading a statement should only have to choose the file.
        /// </summary>
        public int BankStatementFormatID { get; set; }
    }

    /// <summary>Which column holds what. Each value is a header name or a 0-based index.</summary>
    public class BankColumnMap
    {
        public string TxnDate { get; set; } = string.Empty;
        public string ValueDate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string Debit { get; set; } = string.Empty;
        public string Credit { get; set; } = string.Empty;
        /// <summary>A single signed amount column, for banks that do not split debit and credit.</summary>
        public string Amount { get; set; } = string.Empty;
        public string RunningBalance { get; set; } = string.Empty;
    }

    /// <summary>Run or commit a bank-vs-books reconciliation for one account over a date range.</summary>
    public class BankReconRequest
    {
        public int BankAccountID { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }

        /// <summary>Amount tolerance in rupees. 0 falls back to the house default (0.05).</summary>
        public decimal Tolerance { get; set; }

        /// <summary>Days either side a date may differ. 0 falls back to the house default (3).</summary>
        public int DateWindowDays { get; set; }

        public int CreatedByUserID { get; set; }
    }

    /// <summary>Pair one statement line to one ledger line by hand, when the match could not.</summary>
    public class BankManualMatchRequest
    {
        public int BankStatementLineID { get; set; }
        public int AcctVoucherLineID { get; set; }
        public int CreatedByUserID { get; set; }
    }

    /// <summary>Break a pairing that was made wrongly.</summary>
    public class BankUnmatchRequest
    {
        public int BankStatementLineID { get; set; }
        public int CreatedByUserID { get; set; }
    }

    /// <summary>Read filter for the statement lines and the unreconciled register.</summary>
    public class BankStatementQueryRequest
    {
        public int BankAccountID { get; set; }
        public int BankStatementImportID { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }

        /// <summary>Matched / Mismatch / Unmatched, or blank for all.</summary>
        public string MatchStatus { get; set; } = string.Empty;
    }

    /// <summary>Create or update a bank account.</summary>
    public class SaveBankAccountRequest
    {
        public int BankAccountID { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string LedgerAccountCode { get; set; } = "1000";
        public bool IsActive { get; set; } = true;
    }
}
