using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class TransactionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; } = null!;

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } = null!;

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; } = null!;

        [Required]
        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(250)")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int ProviderID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderOpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderCommission { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProviderClosingBalance { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ProviderRequestID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string ReferenceNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string APIUserRequestID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string ProviderDOCNo { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string ProviderRefNo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerGST { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Bill_CustomerMessageMobileNo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        [DefaultValue(0)]
        public decimal CashBack { get; set; }

        public int HitBackCount { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ErrorCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional1 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional2 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional3 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional4 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional5 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional6 { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_BeneficiaryID { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string DMT_BeneficiaryName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_Bank { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_IFSC { get; set; } = string.Empty;

        [Column(TypeName = "varchar(1000)")]
        public string DMT_BeneficaryNameByBank { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_PaymentType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DMT_BillNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal DMT_OriginalAmount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_StatusCode { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsDispute { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string SimCardDetails { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Through { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string IPAddress { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Circle { get; set; } = string.Empty;

        public int DistributerUserID { get; set; }

        public int SuperDistributerUserID { get; set; }

        // ── Who funded each tier's commission ────────────────────────────────────────────────
        // 0 = the platform operator (the admin ancestor), which is every row written before
        // delegation existed and every row on an install that does not delegate.
        //
        // Stamped at initiation and never recomputed, because the refund path has to reverse the
        // postings that were actually made. Reading the rate card again at refund time would read
        // whatever it says THEN: a distributor who re-priced a retailer in between would be
        // credited back money that was debited from someone else, which is how a balanced pair
        // becomes an unbalanced one.
        public int RetailerCommissionFunderID { get; set; }

        public int DistributerCommissionFunderID { get; set; }

        public int SuperDistributerCommissionFunderID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ComplaintStatus { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminCommission { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CardType { get; set; } = string.Empty;

        public int StateID { get; set; }

        [DefaultValue(false)]
        public bool IsROffer { get; set; }

        [DefaultValue(true)]
        public bool IsOnlineDMT { get; set; }

        [DefaultValue(false)]
        public bool IsProviderLowBalance { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ChangeStatus { get; set; } = string.Empty;

        public DateTime ChangeStatusDOC { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AEPSResponseJSON { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string PendingProviderStatus { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminProfit { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ResponseType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ROfferCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerCommissionHold { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerCommissionHold { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerCommissionHold { get; set; }

        [DefaultValue(false)]
        public bool SettlementAtProviderEnd { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string BBPS_CustomerName { get; set; } = string.Empty;

        [DefaultValue(false)]
        public bool IsCCF { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal NetCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        // ── Charge / commission GST breakdown ────────────────────────────────────────────────
        //
        // Stamped ONCE, at the single settlement choke point, and only for a transaction that
        // SUCCEEDED. A failed transaction is refunded in full — commissions and all — so it earns
        // nothing and owes no tax: every column below stays 0 and ChargeGSTStampedOn stays null.
        //
        // The figures are SIGNED the same way every commission in this system is signed: positive
        // = earned, negative = paid away (a surcharge, or a provider charge such as DMT). GST and
        // base carry the sign of the charge they were split out of, so a report can sum them
        // without knowing which rail it is looking at. Base + GST = the charge as quoted when it
        // was INCLUSIVE; charge + GST = what is actually payable when it was EXCLUSIVE, which is
        // why both ProviderBaseCharge and ProviderTotalCharge are kept rather than derived.
        //
        // Reports read these columns instead of recomputing: the rate card can be edited tomorrow,
        // and a settled transaction must keep telling the truth about the card it was priced on.

        /// <summary>FLAT or PERCENTAGE — what the matched provider band charged on.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ProviderChargeType { get; set; } = string.Empty;

        /// <summary>INCLUSIVE / EXCLUSIVE / NONE, as resolved for this transaction.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ProviderGSTType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal ProviderGSTPercent { get; set; }

        /// <summary>The GST inside (INCLUSIVE) or on top of (EXCLUSIVE) the provider charge.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderGSTAmount { get; set; }

        /// <summary>The provider charge without GST. Input credit is claimed on the GST above it.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderBaseCharge { get; set; }

        /// <summary>What the provider leg actually settles at: base + GST.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderTotalCharge { get; set; }

        /// <summary>Which layer decided the provider GST — the band, a profile, or the fallback.
        /// Written so support can answer "why 0.90?" without re-deriving a card that has since
        /// been edited.</summary>
        [Column(TypeName = "varchar(200)")]
        public string ProviderGSTSource { get; set; } = string.Empty;

        /// <summary>FLAT or PERCENTAGE — what the matched slab band charged on.</summary>
        [Column(TypeName = "varchar(20)")]
        public string RetailerChargeType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string RetailerGSTType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal RetailerGSTPercent { get; set; }

        /// <summary>The retailer's commission/charge without GST. RetailerGST is the tax on it.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerBaseCharge { get; set; }

        /// <summary>What the retailer leg actually settles at: base + GST.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerTotalCharge { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerBaseCharge { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerBaseCharge { get; set; }

        /// <summary>Which layer decided the retailer-side GST.</summary>
        [Column(TypeName = "varchar(200)")]
        public string RetailerGSTSource { get; set; } = string.Empty;

        /// <summary>
        /// When the breakdown was stamped. Null = never stamped, which is the truthful state for a
        /// pending or failed transaction and for everything settled before this shipped. It is also
        /// the idempotency guard: a status-check sweep, a reconciliation run and a manual settle can
        /// all land on the same transaction, and the stamp must not move once written.
        /// </summary>
        public DateTime? ChargeGSTStampedOn { get; set; }
    }
}
