using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// What the tax engine decided for one transaction, written once and never recomputed.
    ///
    /// This is the most important rule in the subsystem: reports READ this row, they do not work
    /// the tax out again. The moment a rate or a rule changes, a report that recalculates would
    /// restate every past month and stop agreeing with the returns already filed — and then
    /// nobody can tell whether a difference is a real error or just the software having changed
    /// its mind. Resolving once and stamping the answer is what makes the books defensible.
    ///
    /// A separate table rather than more columns on TransactionMaster, which is past a hundred
    /// columns, is selected in bulk by every report, and is cached.
    ///
    /// Rates are stored ALONGSIDE the rule id on purpose. The rule id says which decision was
    /// taken; the rates say what it produced. Keeping both means a stamped row can be read back
    /// without joining to masters that may since have been closed and superseded.
    /// </summary>
    public class TransactionTaxDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionTaxDetailID { get; set; }

        public int TransactionMasterID { get; set; }

        [ForeignKey(nameof(TransactionMasterID))]
        public virtual TransactionMaster? TransactionMaster { get; set; }

        /// <summary>SALE or PURCHASE.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>Which rule produced this. 0 when no rule matched and the fallback was used,
        /// which is itself worth reporting on.</summary>
        public int TaxRuleMasterID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string BillingBasis { get; set; } = string.Empty;

        /// <summary>The whole throughput — what moved.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossValue { get; set; }

        /// <summary>What tax was actually charged on, per the basis.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableValue { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string SupplyNature { get; set; } = string.Empty;

        /// <summary>Kept so a wrong tax head can be traced to the state codes that caused it.</summary>
        [Column(TypeName = "varchar(2)")]
        public string SupplierStateCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(2)")]
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string SACCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal CGSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CGSTAmount { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal SGSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SGSTAmount { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal IGSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessAmount { get; set; }

        /// <summary>Set only where a TDS treatment applied. Empty is the common case for a sale.</summary>
        [Column(TypeName = "varchar(20)")]
        public string TDSSectionCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal TDSRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TDSAmount { get; set; }

        /// <summary>P2P or AGENT at the time — the Bharti Cellular distinction, frozen. Whether
        /// 194H applied is a question about the relationship as it stood that day, and the
        /// agreement may be renegotiated later.</summary>
        [Column(TypeName = "varchar(10)")]
        public string PartyRelationship { get; set; } = string.Empty;

        public bool ITCEligible { get; set; }

        public bool ReverseCharge { get; set; }

        /// <summary>Set when no rule matched, or when two matched at the same priority. These
        /// rows are the configuration backlog and belong on a dashboard tile, not in a log file
        /// the app pool cannot write to.</summary>
        [Column(TypeName = "varchar(300)")]
        public string ResolutionWarning { get; set; } = string.Empty;

        public DateTime ResolvedOn { get; set; }
    }
}
