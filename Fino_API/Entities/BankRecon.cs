using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// A real bank account we hold, mapped to the LEDGER account its movements post to.
    ///
    /// The mapping is by account CODE rather than id, the same rule the posting rules follow:
    /// codes are stable across installs, ids are not. Today the seeded chart has one Bank account
    /// (1000) and one row here points at it; the day a second current account is opened, this
    /// mapping is the only thing that changes.
    /// </summary>
    public class BankAccount : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankAccountID { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string AccountName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(150)")]
        public string BankName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(40)")]
        public string AccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string IFSC { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string LedgerAccountCode { get; set; } = "1000";

        /// <summary>
        /// The Payment Mode sub-mode this bank IS, where it came from there — which it should,
        /// because that is where the operator already keeps the company's bank accounts (the ones
        /// a retailer deposits into on a fund request: name, account number, IFSC). Maintaining a
        /// second list beside it is how two lists end up disagreeing about an account number.
        ///
        /// 0 means a bank that exists only for reconciliation — an account nobody funds into.
        /// </summary>
        public int SubModeID { get; set; }

        /// <summary>The mode the sub-mode belongs to (Bank, UPI, …). 0 when unlinked.</summary>
        public int ModeID { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// One statement upload. Active / Superseded like a 2B snapshot, and for the same reason: a
    /// bank can restate a line after it clears, so a re-upload replaces rather than merges.
    /// </summary>
    public class BankStatementImport : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankStatementImportID { get; set; }

        public int BankAccountID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime PeriodTo { get; set; }

        [Column(TypeName = "varchar(260)")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>CSV or XLSX — kept so an odd parse can be traced to the format it came from.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Source { get; set; } = "CSV";

        public int LineCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCredit { get; set; }

        /// <summary>The bank's own closing balance when the file carried a running balance. The one
        /// figure that proves the parser dropped no line.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ClosingBalance { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = StaticModels.BankStatementStatus.Active;

        public int ImportedByUserID { get; set; }
    }

    /// <summary>One row of a statement, as the bank gave it, plus where its match got to.</summary>
    public class BankStatementLine : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankStatementLineID { get; set; }

        public int BankStatementImportID { get; set; }
        public int BankAccountID { get; set; }

        [Column(TypeName = "date")]
        public DateTime TxnDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ValueDate { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; } = string.Empty;

        /// <summary>The UTR / cheque number / RRN as the bank printed it.</summary>
        [Column(TypeName = "varchar(100)")]
        public string ReferenceNo { get; set; } = string.Empty;

        /// <summary>The same reference stripped to letters and digits and upper-cased. This is what
        /// the match keys on, so "UTR: SBIN-123" and "SBIN123" are one reference, not two.</summary>
        [Column(TypeName = "varchar(100)")]
        public string ReferenceKey { get; set; } = string.Empty;

        /// <summary>Money OUT of our account. In the ledger this is a CREDIT to Bank.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        /// <summary>Money INTO our account. In the ledger this is a DEBIT to Bank.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RunningBalance { get; set; }

        /// <summary>Row number in the uploaded file, so a complaint about "line 48" can be found.</summary>
        public int RowNo { get; set; }

        /// <summary>Denormalised from <see cref="BankReconMatch"/> so the register filters without
        /// a join. Matched / Mismatch / Unmatched.</summary>
        [Column(TypeName = "varchar(20)")]
        public string MatchStatus { get; set; } = "Unmatched";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Variance { get; set; }

        // ── What the line IS, as against whether it happened ──────────────────────────────
        //
        // Matching proves a line happened by pairing it with a voucher. It cannot say what the
        // money was FOR, and that is the half the operator actually needs written down: stock
        // bought from a provider, a retailer's deposit, an expense, a transfer between our own
        // accounts. The bank's narration is the only evidence of it, so a rule reads the narration
        // and the answer is stored here rather than re-derived every time a screen opens.

        /// <summary>PURCHASE / USER_FUND / EXPENSE / … — see <see cref="StaticModels.BankLineCategory"/>.</summary>
        [Column(TypeName = "varchar(30)")]
        public string Category { get; set; } = StaticModels.BankLineCategory.Uncategorised;

        /// <summary>The rule that decided it. 0 = nothing matched, or a human set it by hand.</summary>
        public int CategoryRuleID { get; set; }

        /// <summary>True when a person set the category, so a later re-run never overwrites it.</summary>
        public bool CategorySetByUser { get; set; }

        /// <summary>
        /// The user this line is about, where one could be identified — the retailer whose fund
        /// request this deposit answers. 0 when the narration names nobody we know.
        /// </summary>
        public int CounterpartyUserID { get; set; }

        /// <summary>Their name, resolved once at import so a register does not join on every read.</summary>
        [Column(TypeName = "varchar(150)")]
        public string CounterpartyName { get; set; } = string.Empty;

        /// <summary>
        /// Set when this line looks like money already credited once — the same reference and
        /// amount on another line, or a fund request already settled against it. Points at the
        /// earlier line; 0 when there is nothing to suspect.
        /// </summary>
        public int DuplicateOfLineID { get; set; }

        /// <summary>Why it was flagged, in words. Empty when it was not.</summary>
        [Column(TypeName = "varchar(250)")]
        public string DuplicateReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// The pairing: one bank line to one ledger voucher line.
    ///
    /// A table rather than a column on either side, because a ledger line must stay
    /// known-reconciled across future runs and future statements. Stamping only the bank line would
    /// make every past payment reappear as unreconciled the next time a later month is uploaded.
    /// </summary>
    public class BankReconMatch : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankReconMatchID { get; set; }

        public int BankStatementLineID { get; set; }
        public int AcctVoucherLineID { get; set; }
        public int BankAccountID { get; set; }

        /// <summary>Reference / AmountAndDate / Manual — see <see cref="StaticModels.BankMatchMethod"/>.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Method { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Variance { get; set; }

        public int MatchedByUserID { get; set; }
    }
}
