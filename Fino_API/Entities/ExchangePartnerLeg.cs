using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One side of a reciprocal wallet swap with an exchange partner.
    ///
    /// Two platforms give each other API access and each credits the other's wallet — "he puts 50k
    /// in mine, I put 50k in his". The party balance nets to zero, which is why it feels like an
    /// adjustment, but under GST Section 7(1)(a) barter and exchange ARE supply: each side is a
    /// taxable supply at open market value, and the turnover is the sum of both legs, not the
    /// difference. Five partners swapping Rs 50 lakh a month is Rs 3 crore of turnover that leaves
    /// the books entirely if only the incoming credit is recorded.
    ///
    /// Each leg is a row, and each row is expected to end up with a document against it:
    ///   OUTWARD — we credited them   -> our sale invoice
    ///   INWARD  — they credited us   -> their purchase invoice, which we have to obtain
    ///
    /// A leg with no document is not an error to be swallowed. It is the exchange register's whole
    /// purpose: an inward leg sits in AwaitingDocument until the partner's invoice arrives, and
    /// that queue is what makes the gap visible while it can still be fixed.
    /// </summary>
    public class ExchangePartnerLeg : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExchangePartnerLegID { get; set; }

        public int PartnerUserID { get; set; }

        [ForeignKey(nameof(PartnerUserID))]
        public virtual UserMaster? Partner { get; set; }

        /// <summary>OUTWARD (we credited them) or INWARD (they credited us).</summary>
        [Column(TypeName = "varchar(10)")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>The wallet movement this leg accounts for.</summary>
        public int FundTransactionID { get; set; }

        public int WalletID { get; set; }

        public DateTime LegDate { get; set; }

        /// <summary>Open market value of the supply — the credited amount.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Our sale document on an outward leg; their purchase document once entered on
        /// an inward one. Null while still awaited.</summary>
        public int? TaxInvoiceHeaderID { get; set; }

        [ForeignKey(nameof(TaxInvoiceHeaderID))]
        public virtual TaxInvoiceHeader? Document { get; set; }

        /// <summary>Invoiced / AwaitingDocument / WrittenOff. AwaitingDocument is the queue.</summary>
        [Column(TypeName = "varchar(20)")]
        public string DocumentStatus { get; set; } = string.Empty;

        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;
    }
}
