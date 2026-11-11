using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// How ONE bank lays its statement out, saved once instead of described at every upload.
    ///
    /// The importer detects the common shapes on its own — it knows that HDFC writes "Withdrawal
    /// Amt." where ICICI writes "Debit" — and for a CSV or an Excel export that is usually enough.
    /// It is not enough for two cases, and those are exactly the ones an operator meets:
    ///
    ///   * a bank whose export has no header row, or a header the synonyms do not cover;
    ///   * a PDF, which has no columns at all — only text on a page — so the line has to be split
    ///     by a pattern that is particular to that bank.
    ///
    /// Describing that at every upload is a per-import manual step, which on a white-label install
    /// means a defect: the person doing it is not the person who knows what a column map is. So it
    /// is configuration, set once per bank, and the upload screen just picks the file.
    /// </summary>
    public class BankStatementFormat : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankStatementFormatID { get; set; }

        /// <summary>The bank this format belongs to. One bank may have several — HDFC's Excel
        /// export and HDFC's PDF are different shapes of the same account.</summary>
        public int BankAccountID { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string Name { get; set; } = string.Empty;

        /// <summary>CSV / XLSX / PDF — see <see cref="StaticModels.BankStatementSource"/>.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Source { get; set; } = StaticModels.BankStatementSource.Csv;

        /// <summary>
        /// 0-based row holding the column names. -1 means "detect it", which is the default and
        /// right for most files; a positive number is for a statement with a preamble.
        /// </summary>
        public int HeaderRow { get; set; } = -1;

        /// <summary>Rows to ignore at the top before anything is read. 0 for most files.</summary>
        public int SkipRows { get; set; }

        /// <summary>
        /// A date format to try FIRST, for a bank whose dates are ambiguous — 03/04/2026 is the
        /// third of April in India and the fourth of March in a US export, and no parser can tell
        /// them apart. Blank uses the built-in list.
        /// </summary>
        [Column(TypeName = "varchar(40)")]
        public string DateFormat { get; set; } = string.Empty;

        // ── The column map. Each value is a HEADER NAME or a 0-based index; blank = detect. ───
        [Column(TypeName = "varchar(80)")] public string TxnDateColumn { get; set; } = string.Empty;
        [Column(TypeName = "varchar(80)")] public string ValueDateColumn { get; set; } = string.Empty;
        [Column(TypeName = "varchar(80)")] public string DescriptionColumn { get; set; } = string.Empty;
        [Column(TypeName = "varchar(80)")] public string ReferenceColumn { get; set; } = string.Empty;
        [Column(TypeName = "varchar(80)")] public string DebitColumn { get; set; } = string.Empty;
        [Column(TypeName = "varchar(80)")] public string CreditColumn { get; set; } = string.Empty;

        /// <summary>For a statement with ONE amount column where the sign (or a Dr/Cr suffix)
        /// carries the direction.</summary>
        [Column(TypeName = "varchar(80)")] public string AmountColumn { get; set; } = string.Empty;

        [Column(TypeName = "varchar(80)")] public string BalanceColumn { get; set; } = string.Empty;

        // ── PDF only ──────────────────────────────────────────────────────────────────────
        /// <summary>
        /// A regular expression with NAMED GROUPS that turns one line of extracted text into
        /// columns: (?&lt;date&gt;…) (?&lt;desc&gt;…) (?&lt;ref&gt;…) (?&lt;debit&gt;…)
        /// (?&lt;credit&gt;…) (?&lt;amount&gt;…) (?&lt;balance&gt;…). Blank uses a general pattern
        /// that reads "date … text … amounts at the end", which covers most Indian statements.
        /// </summary>
        [Column(TypeName = "varchar(600)")]
        public string PdfLinePattern { get; set; } = string.Empty;

        /// <summary>
        /// Lines to throw away before parsing — page headers, footers, the bank's address. A
        /// regular expression; blank keeps everything and lets the date test do the filtering.
        /// </summary>
        [Column(TypeName = "varchar(400)")]
        public string PdfSkipPattern { get; set; } = string.Empty;

        /// <summary>
        /// Join a row that the PDF wrapped onto the next line back onto its predecessor. Most
        /// statements wrap a long narration, and without this the wrapped half is read as a row
        /// with no date and silently dropped — taking the end of the narration with it.
        /// </summary>
        public bool PdfJoinWrappedLines { get; set; } = true;

        /// <summary>Set when the format was last used, so a dead format is visible as dead.</summary>
        public DateTime? LastUsedOn { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(300)")]
        public string Remark { get; set; } = string.Empty;

        public int CreatedByUserID { get; set; }
    }

    /// <summary>
    /// What a statement line IS — purchase, a user's deposit, an expense — decided by matching the
    /// narration the bank wrote.
    ///
    /// Reconciliation proves a line happened by pairing it with a voucher. It cannot say what the
    /// line was FOR, and the bank's own narration is the only evidence of that: "NEFT DR-HDFC-JAIJUI
    /// SOLUTIONS" is a provider purchase and "UPI/9876543210/RAHUL STORE" is a retailer's deposit.
    /// One rule per phrase the operator recognises, matched most specific first.
    /// </summary>
    public class BankLineCategoryRule : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankLineCategoryRuleID { get; set; }

        /// <summary>0 = every bank. A rule pinned to one bank wins over a general one.</summary>
        public int BankAccountID { get; set; }

        /// <summary>DEBIT / CREDIT / ANY — the side this rule may fire on.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Direction { get; set; } = StaticModels.BankLineDirection.Any;

        /// <summary>CONTAINS / STARTS_WITH / REGEX.</summary>
        [Column(TypeName = "varchar(20)")]
        public string MatchType { get; set; } = StaticModels.BankLineMatchType.Contains;

        [Column(TypeName = "varchar(300)")]
        public string Pattern { get; set; } = string.Empty;

        /// <summary>Only fire between these amounts. 0 and 0 mean any amount.</summary>
        [Column(TypeName = "decimal(18,2)")] public decimal MinAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal MaxAmount { get; set; }

        /// <summary>What the line is — see <see cref="StaticModels.BankLineCategory"/>.</summary>
        [Column(TypeName = "varchar(30)")]
        public string Category { get; set; } = StaticModels.BankLineCategory.Uncategorised;

        /// <summary>Where it belongs in the books, when the operator knows. Blank leaves it to the
        /// posting rules; it is a hint for the ledger, never a posting on its own.</summary>
        [Column(TypeName = "varchar(20)")]
        public string LedgerAccountCode { get; set; } = string.Empty;

        /// <summary>Higher wins. 0 derives it: a bank-specific rule beats a general one, and a
        /// longer pattern beats a shorter one, so nobody has to invent numbers.</summary>
        public int Priority { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(300)")]
        public string Remark { get; set; } = string.Empty;

        public int CreatedByUserID { get; set; }
    }
}
