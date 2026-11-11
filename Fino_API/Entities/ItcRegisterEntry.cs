using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// The input-tax-credit ledger: one row per purchase document whose lines carry claimable
    /// credit. It is the answer to "how much ITC may we take, how much have we taken, and how much
    /// must we give back, and by when".
    ///
    /// Two deadlines live on every row because both can take the credit away and neither announces
    /// itself:
    ///   - Section 16(4) — a credit not claimed by 30 November of the year after the supply's
    ///     financial year is lost. This is the deadline on an UNCLAIMED entry.
    ///   - Rule 37 — a credit claimed but not paid to the supplier within 180 days must be
    ///     reversed (and re-claimed only once paid). This is the deadline on a CLAIMED, UNPAID entry.
    /// One field cannot carry both, because which one bites depends on whether the entry has been
    /// claimed and whether the supplier has been paid — so both dates are stored and the alert
    /// queries decide which applies.
    /// </summary>
    public class ItcRegisterEntry : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItcRegisterEntryID { get; set; }

        /// <summary>The purchase document this credit arises from.</summary>
        public int TaxInvoiceHeaderID { get; set; }

        [ForeignKey(nameof(TaxInvoiceHeaderID))]
        public virtual TaxInvoiceHeader? Header { get; set; }

        /// <summary>The period the purchase falls in (yyyyMM).</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string SupplierGSTIN { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string SupplierName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SGST { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cess { get; set; }

        /// <summary>The credit on the lines that are ITC-eligible — may be less than the total tax
        /// on the document when some lines are blocked (Section 17(5)) or the supplier is composition.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal EligibleITC { get; set; }

        /// <summary>Eligible / Claimed / Reversed / Ineligible.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        /// <summary>The 4-bucket recon result this entry is tied to (Matched / MissingIn2B / etc.).
        /// A credit is only safe to claim once it is Matched, so the register shows the bucket.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ReconBucket { get; set; } = string.Empty;

        /// <summary>The 2B line this matched, when it did.</summary>
        public int? Matched2BLineID { get; set; }

        // ── Claim ────────────────────────────────────────────────────────────────────────
        [Column(TypeName = "varchar(8)")]
        public string ClaimedInPeriod { get; set; } = string.Empty;
        public DateTime? ClaimedOn { get; set; }
        public int ClaimedByUserID { get; set; }

        // ── Reversal ───────────────────────────────────────────────────────────────────
        public DateTime? ReversedOn { get; set; }
        public int ReversedByUserID { get; set; }

        /// <summary>Rule37 / Section16_4 / Manual — why the credit was given back.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ReversalReason { get; set; } = string.Empty;

        // ── Deadlines and supplier payment ───────────────────────────────────────────────

        /// <summary>Document date + 180 days: the last day to pay the supplier before the claimed
        /// credit must be reversed under Rule 37.</summary>
        public DateTime Rule37DueDate { get; set; }

        /// <summary>30 November of the year after the supply's financial year: the last day the
        /// credit can be claimed at all (Section 16(4)).</summary>
        public DateTime Section16_4Deadline { get; set; }

        public bool SupplierPaid { get; set; }
        public DateTime? SupplierPaidOn { get; set; }
    }
}
