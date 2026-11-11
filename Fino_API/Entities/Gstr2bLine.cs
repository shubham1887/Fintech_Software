using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One supplier document as it appears in a GSTR-2B snapshot — one row per invoice / note the
    /// supplier filed against us.
    ///
    /// The match result is kept on the row (MatchStatus / MatchedHeaderID) rather than only in a
    /// report, so "which of our purchases does this 2B line correspond to" survives a page reload
    /// and an auditor's later question. The recon writes it when it is committed.
    /// </summary>
    public class Gstr2bLine : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Gstr2bLineID { get; set; }

        public int Gstr2bImportID { get; set; }

        [ForeignKey(nameof(Gstr2bImportID))]
        public virtual Gstr2bImport? Import { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string SupplierGSTIN { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string SupplierName { get; set; } = string.Empty;

        /// <summary>INV / CRN / DBN — the 2B document kind.</summary>
        [Column(TypeName = "varchar(10)")]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>The supplier's own number, exactly as filed. The match normalises it for
        /// comparison but the raw value is kept, because "INV/26-27/00412" and "INV-26-27-00412"
        /// are the same bill filed two ways and the auditor wants to see what was actually filed.</summary>
        [Column(TypeName = "varchar(30)")]
        public string DocumentNo { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }

        [Column(TypeName = "varchar(2)")]
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        public bool ReverseCharge { get; set; }

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

        /// <summary>The portal's own ITC-available flag (itcavl = Y/N). A line can be in 2B and
        /// still carry no claimable credit — a supplier filing late, or an ineligible category.</summary>
        public bool ItcAvailable { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ItcUnavailableReason { get; set; } = string.Empty;

        // ── Recon result (written when a reconciliation is committed) ────────────────────

        /// <summary>Unmatched / Matched / Mismatch — the bucket this 2B line fell into last time
        /// the recon was committed. Unmatched until then.</summary>
        [Column(TypeName = "varchar(20)")]
        public string MatchStatus { get; set; } = string.Empty;

        /// <summary>The purchase document (TaxInvoiceHeaders, Direction=PURCHASE) this matched.</summary>
        public int? MatchedHeaderID { get; set; }

        /// <summary>The larger of the per-head tax differences at the last commit, so a near-miss
        /// can be told from a gross mismatch without recomputing.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxVariance { get; set; }
    }
}
