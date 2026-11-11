using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// How a commission is taxed: whether the figure already contains GST, what the GST is charged
    /// on, and whether TDS is deducted from it.
    ///
    /// These are the answers the system was missing. Every invoice path used to pass
    /// "tax inclusive = true" as a literal, and TDS was computed and stamped but never deducted —
    /// so a provider paying commission EXCLUSIVE of GST and one paying it INCLUSIVE produced the
    /// identical number here, and the net profit was out by the tax on every rupee of it.
    ///
    /// One row per decision, matched most specific first (slab, then provider, then service, then
    /// the house default), exactly as a tax rule is matched.
    /// </summary>
    public class CommissionTaxProfile : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommissionTaxProfileID { get; set; }

        /// <summary>GLOBAL / SERVICE / PROVIDER / SLAB — see <see cref="StaticModels.CommissionTaxScope"/>.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>ServiceID, ProviderID or SlabMasterID depending on <see cref="Scope"/>; 0 for GLOBAL.</summary>
        public int ScopeID { get; set; }

        /// <summary>Narrows the row further. 0 means any.</summary>
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }

        /// <summary>INCLUSIVE / EXCLUSIVE / NONE.</summary>
        [Column(TypeName = "varchar(20)")]
        public string TaxMode { get; set; } = StaticModels.CommissionTaxMode.Inclusive;

        /// <summary>COMMISSION / GROSS — what the GST is charged on.</summary>
        [Column(TypeName = "varchar(20)")]
        public string TaxBase { get; set; } = StaticModels.CommissionTaxBase.Commission;

        /// <summary>0 means "ask the tax engine", which resolves the rate as of the transaction's
        /// own date. A value here pins it, for a contract that names a different number.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal GSTRatePercent { get; set; }

        /// <summary>A charge per transaction rather than per rupee — the DMT case where the fee
        /// follows the count of transfers rather than their value.</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal PerTransactionCharge { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string PerTransactionChargeTaxMode { get; set; } = StaticModels.CommissionTaxMode.Inclusive;

        /// <summary>The switch. Off until somebody turns it on: it deducts real money.</summary>
        public bool TDSEnabled { get; set; }

        /// <summary>Blank resolves the section from the party's relationship through the tax
        /// engine — 194H for an agent, 194O for an e-commerce operator.</summary>
        [Column(TypeName = "varchar(20)")]
        public string TDSSectionCode { get; set; } = string.Empty;

        /// <summary>0 takes the section's own rate from TDSSectionMasters.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal TDSRatePercent { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveFrom { get; set; } = new DateTime(2000, 1, 1);

        [Column(TypeName = "date")]
        public DateTime? EffectiveTo { get; set; }

        /// <summary>Higher wins among rows of the same scope. 0 lets the service derive it from
        /// how specific the row is, so nobody has to reason about numbers.</summary>
        public int Priority { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Remark { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int CreatedByUserID { get; set; }
    }

    /// <summary>
    /// One TDS deduction, against one tier's commission on one transaction.
    ///
    /// The transaction already carries the deducted AMOUNT per tier. What a deduction has to be
    /// able to answer later — because that is what a TDS certificate states — is under which
    /// section, at what rate, and against which commission. That detail lives here.
    /// </summary>
    public class TransactionTDSDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionTDSDetailID { get; set; }

        public int TransactionMasterID { get; set; }
        public int PartyUserID { get; set; }

        /// <summary>Retailer / Distributor / SuperDistributor.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Tier { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string SectionCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,4)")]
        public decimal RatePercent { get; set; }

        /// <summary>The commission the deduction was computed on, AFTER backing GST out of it
        /// where the commission was inclusive. TDS is on income, not on the tax collected with it.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionBase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TDSAmount { get; set; }

        /// <summary>True when the party had no PAN and the higher no-PAN rate applied.</summary>
        public bool NoPANRateApplied { get; set; }

        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>The TDS rule that decided this deduction. 0 = none matched and the commission
        /// tax profile's own switch was used, which is how every deduction before the rules worked.</summary>
        public int TdsRuleID { get; set; }

        /// <summary>PERCENT or FIX. A flat deduction has no meaningful rate, so a report that shows
        /// only RatePercent would print 0% beside a real figure and look broken.</summary>
        [Column(TypeName = "varchar(20)")]
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>
        /// Whether the money was actually taken out of the party's wallet. False means the
        /// deduction is recorded but not withheld — the wallet could not fund it — and the monthly
        /// report shows it apart, because depositing money nobody collected is paying it twice.
        /// </summary>
        public bool Withheld { get; set; }

        /// <summary>Which rule decided, in words, for the screen and for support.</summary>
        [Column(TypeName = "varchar(200)")]
        public string DecidedBy { get; set; } = string.Empty;
    }
}
