using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One underlying transaction behind a rolled-up invoice line.
    ///
    /// A month's DMT commission or a day's B2C supplies cannot print a thousand lines on the
    /// invoice, but the audit trail has to survive anyway. The invoice carries one line per SAC;
    /// the annexure carries one row per transaction, so "which transactions make up this
    /// 1,48,230.55?" has an answer that does not depend on re-running a report whose rules may
    /// have changed since.
    /// </summary>
    public class TaxInvoiceAnnexureLine : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxInvoiceAnnexureLineID { get; set; }

        public int TaxInvoiceHeaderID { get; set; }

        [ForeignKey(nameof(TaxInvoiceHeaderID))]
        public virtual TaxInvoiceHeader? Header { get; set; }

        /// <summary>The invoice line this rolls into.</summary>
        public int TaxInvoiceLineID { get; set; }

        public int TransactionMasterID { get; set; }

        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ReferenceNo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
    }
}
