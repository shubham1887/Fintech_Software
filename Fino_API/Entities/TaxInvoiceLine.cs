using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One line of a GST document — one SAC at one rate.
    ///
    /// A line carries the gross value, the taxable value and the basis that got from one to the
    /// other. All three, on every line, because that is the only way a purchase register and a
    /// sales register can be compared: JaiJui's own invoice shows 1,506.00 of transaction value
    /// against 15.00 of taxable amount on a single line, and a system that stored only the second
    /// cannot tell you what moved.
    ///
    /// TaxRuleMasterID records which rule produced the figures, so a line can be traced back to
    /// the decision rather than re-derived from masters that may since have been superseded.
    /// </summary>
    public class TaxInvoiceLine : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxInvoiceLineID { get; set; }

        public int TaxInvoiceHeaderID { get; set; }

        [ForeignKey(nameof(TaxInvoiceHeaderID))]
        public virtual TaxInvoiceHeader? Header { get; set; }

        public int LineNo { get; set; }

        public int ServiceID { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string SACCode { get; set; } = string.Empty;

        /// <summary>Count of underlying transactions on a rolled-up line; 1 on a single one.
        /// JaiJui prints this as "Count".</summary>
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "varchar(20)")]
        public string BillingBasis { get; set; } = string.Empty;

        /// <summary>Throughput behind the line.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableValue { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        /// <summary>Which rule decided this line.</summary>
        public int TaxRuleMasterID { get; set; }

        /// <summary>Purchase side: whether the credit on this line may be claimed.</summary>
        public bool ITCEligible { get; set; }

        public bool ReverseCharge { get; set; }
    }
}
