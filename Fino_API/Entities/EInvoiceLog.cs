using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One attempt to register a document with the Invoice Registration Portal, and its IRN.
    ///
    /// At this turnover e-invoicing is not optional: the Rs 5 crore AATO threshold is crossed, and
    /// above Rs 10 crore a document must reach the IRP within 30 days of its date or the portal
    /// refuses to issue an IRN at all. A notified invoice without a valid IRN is not a valid tax
    /// invoice, and the buyer's credit on it is at risk. Both of this platform's own suppliers
    /// already carry IRNs on their invoices.
    ///
    /// A queue rather than an inline call, for two reasons. The IRP goes down, and an invoice must
    /// not fail to exist because a government portal was unavailable — the document is issued
    /// locally and registered after. And the 30-day limit is a deadline worth watching: a
    /// document still Pending as the limit approaches needs an alert while something can still be
    /// done about it, not a discovery at the end of the month.
    /// </summary>
    public class EInvoiceLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EInvoiceLogID { get; set; }

        public int TaxInvoiceHeaderID { get; set; }

        [ForeignKey(nameof(TaxInvoiceHeaderID))]
        public virtual TaxInvoiceHeader? Header { get; set; }

        /// <summary>Pending / Registered / Failed / Cancelled / NotApplicable.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        /// <summary>The 64-character Invoice Reference Number, once the IRP issues it.</summary>
        [Column(TypeName = "varchar(70)")]
        public string IRN { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string AckNo { get; set; } = string.Empty;

        public DateTime? AckDate { get; set; }

        /// <summary>The signed QR the invoice must print.</summary>
        [Column(TypeName = "varchar(max)")]
        public string SignedQRCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string RequestPayload { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string ResponsePayload { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string ErrorCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string ErrorMessage { get; set; } = string.Empty;

        public int AttemptCount { get; set; }

        public DateTime? LastAttemptOn { get; set; }

        /// <summary>
        /// Document date + 30 days: the last day the IRP will accept it. Past this there is no IRN
        /// to be had and the only remaining route is a credit note and a fresh invoice, so this
        /// date drives a daily alert rather than sitting as a field nobody reads.
        /// </summary>
        public DateTime? ReportingDeadline { get; set; }

        /// <summary>An IRN can only be cancelled within 24 hours of being issued; after that a
        /// credit note is the only correction. Stored so the system can decide which route is
        /// still open instead of asking a person to remember.</summary>
        public DateTime? CancellationWindowEnds { get; set; }
    }
}
