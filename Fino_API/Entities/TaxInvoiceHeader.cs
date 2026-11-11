using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One GST document — sale, purchase, credit note, debit note, bill of supply or self-invoice.
    ///
    /// Sale and purchase share a table on purpose. Every reconciliation this subsystem exists for
    /// compares the two sides, and the comparison is only honest when both carry the same columns:
    /// a gross value AND a taxable value AND the basis between them. Two tables drift, and the
    /// 18cr-against-19cr mismatch is what drift looks like.
    ///
    /// Documents are never edited once issued. A mistake is corrected with a credit note and a
    /// fresh invoice, because a number that has gone into GSTR-1 cannot change its content
    /// without the return and the books disagreeing from then on.
    /// </summary>
    public class TaxInvoiceHeader : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxInvoiceHeaderID { get; set; }

        /// <summary>SALE or PURCHASE.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>TaxInvoice / CreditNote / DebitNote / BillOfSupply / SelfInvoice.</summary>
        [Column(TypeName = "varchar(20)")]
        public string DocumentType { get; set; } = string.Empty;

        public int? TaxDocumentSeriesID { get; set; }

        [ForeignKey(nameof(TaxDocumentSeriesID))]
        public virtual TaxDocumentSeries? Series { get; set; }

        /// <summary>Our number for a sale; the supplier's number for a purchase.</summary>
        [Column(TypeName = "varchar(20)")]
        public string DocumentNo { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// For a purchase, when the provider wallet was actually credited.
        ///
        /// Both dates are kept because they differ and the difference decides which month the
        /// purchase falls in: Ezytm and Erech both dated their invoices 30-Sep but took their IRN
        /// on 5-Oct. Which date governs is a question for the CA (blueprint #11); the system
        /// stores both so either answer can be reported without re-entering anything.
        /// </summary>
        public DateTime? WalletCreditDate { get; set; }

        // ── Parties ────────────────────────────────────────────────────────────────────
        public int PartyUserID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string PartyName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string PartyGSTIN { get; set; } = string.Empty;

        /// <summary>REGISTERED / UNREGISTERED / COMPOSITION / SEZ / OVERSEAS — picks the GSTR-1
        /// table this document lands in (B2B, B2CL, B2CS...).</summary>
        [Column(TypeName = "varchar(20)")]
        public string PartyRegistrationStatus { get; set; } = string.Empty;

        [Column(TypeName = "varchar(15)")]
        public string CompanyGSTIN { get; set; } = string.Empty;

        [Column(TypeName = "varchar(2)")]
        public string SupplierStateCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(2)")]
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string SupplyNature { get; set; } = string.Empty;

        // ── Money. Both bases, always ──────────────────────────────────────────────────

        /// <summary>Total throughput behind this document.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CGSTTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SGSTTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGSTTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessTotal { get; set; }

        /// <summary>Its own line, and its own ledger account, so the document total always equals
        /// the money that moved. Both suppliers' invoices carry one.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RoundOff { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        // ── Credit / debit notes ───────────────────────────────────────────────────────

        /// <summary>The document being corrected. Set only on a note.</summary>
        public int? OriginalInvoiceHeaderID { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string NoteReason { get; set; } = string.Empty;

        /// <summary>
        /// The last date this note can still carry tax: 30 November of the year after the original
        /// supply's financial year, or the annual return, whichever is earlier. Past it, reversing
        /// the supply no longer recovers the tax, so a dispute settled late costs the full amount.
        /// Stored rather than computed so the deadline can be reported on and warned about.
        /// </summary>
        public DateTime? NoteTimeLimitDate { get; set; }

        // ── State ──────────────────────────────────────────────────────────────────────

        /// <summary>Draft / Issued / Cancelled. Only an Issued document counts as turnover.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        /// <summary>Which accounting period it belongs to, for the period lock.</summary>
        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>True when this stands for many transactions rolled up — a month's commission
        /// or a day's B2C supplies. The detail is in TaxInvoiceAnnexureLines.</summary>
        public bool IsConsolidated { get; set; }

        /// <summary>Set on both legs of a reciprocal wallet swap, so a barter can never be read
        /// as an ordinary sale and lose its other half.</summary>
        public int? ExchangePartnerLegID { get; set; }

        /// <summary>Purchase side only: the provider and wallet this supplier bill funded, so the
        /// three-way reconciliation can line up what we PAID the provider (the wallet top-up)
        /// against what the supplier BILLED us (this document) against what they FILED (2B). Null
        /// on a sale, and on a purchase that is not a provider wallet top-up.</summary>
        public int? ProviderID { get; set; }
        public int? WalletID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;

        public int CreatedByUserID { get; set; }
        public int ApprovedByUserID { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
