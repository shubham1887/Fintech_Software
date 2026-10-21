using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One tax rule: "for this kind of transaction, the taxable value is X, the rate is Y, the
    /// SAC is Z". The whole engine is a lookup over this table.
    ///
    /// MATCHING. Every dimension below is optional. 0 on an id column and an empty string on a
    /// text column both mean ANY — the rule does not care about that dimension. A rule matches a
    /// transaction when every dimension it DOES set equals the transaction's value, and the
    /// transaction date falls inside the rule's effective window.
    ///
    /// CHOOSING. More than one rule can match. The engine takes the highest Priority, which is
    /// set from how specific a rule is (a rule naming one party beats one naming a user type,
    /// which beats a service default). When two rules match at the SAME priority the engine
    /// raises an error rather than picking one, because a coin-toss between two tax treatments is
    /// worse than a loud failure — the ambiguity is a configuration mistake and has to be seen.
    ///
    /// CHANGING. Rules are effective-dated like rates: close the old row, open a new one. The
    /// resolved rule's id is stamped on the transaction, so a later edit cannot change what was
    /// already charged, and a report never recomputes tax.
    /// </summary>
    public class TaxRuleMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaxRuleMasterID { get; set; }

        [Column(TypeName = "varchar(120)")]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>SALE or PURCHASE. A service is often billed to us on one basis and by us on
        /// another, so this is part of the key rather than a property of the service.</summary>
        [Column(TypeName = "varchar(10)")]
        public string Direction { get; set; } = string.Empty;

        // ── Dimensions. 0 / "" = any ────────────────────────────────────────────────────
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }

        /// <summary>Retailer / Distributor / API User … — see StaticModels.UserTypeCode.</summary>
        public int UserTypeID { get; set; }

        /// <summary>One specific party, for an exchange partner or an API client on its own terms.</summary>
        public int PartyUserID { get; set; }

        /// <summary>REGISTERED / UNREGISTERED / COMPOSITION / SEZ / OVERSEAS.</summary>
        [Column(TypeName = "varchar(20)")]
        public string PartyRegistrationStatus { get; set; } = string.Empty;

        /// <summary>INTRASTATE / INTERSTATE / EXPORT / SEZ. Usually left as any — the engine
        /// derives the nature from the two states and only a special case pins it.</summary>
        [Column(TypeName = "varchar(20)")]
        public string SupplyNature { get; set; } = string.Empty;

        // ── What the rule decides ───────────────────────────────────────────────────────

        /// <summary>GROSS / COMMISSION / CHARGE / EXEMPT / NONGST. The field this subsystem
        /// exists for.</summary>
        [Column(TypeName = "varchar(20)")]
        public string BillingBasis { get; set; } = string.Empty;

        public int? TaxRateMasterID { get; set; }

        [ForeignKey(nameof(TaxRateMasterID))]
        public virtual TaxRateMaster? TaxRate { get; set; }

        public int? HSNSACMasterID { get; set; }

        [ForeignKey(nameof(HSNSACMasterID))]
        public virtual HSNSACMaster? HSNSAC { get; set; }

        /// <summary>Set only on rules that also decide a TDS treatment.</summary>
        public int? TDSSectionMasterID { get; set; }

        [ForeignKey(nameof(TDSSectionMasterID))]
        public virtual TDSSectionMaster? TDSSection { get; set; }

        /// <summary>We pay the tax ourselves and raise a self-invoice
        /// (ServiceMaster.HSNCodeForSelfInvoice is for exactly this).</summary>
        public bool ReverseCharge { get; set; }

        /// <summary>Purchase side: may the input credit be claimed at all.</summary>
        public bool ITCEligible { get; set; } = true;

        /// <summary>Higher wins. Set from specificity; see the class comment.</summary>
        public int Priority { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
