namespace FintechApp.Requests.TaxEngine
{
    /// <summary>
    /// A rule to create, or a replacement for one being superseded.
    ///
    /// Every dimension is optional. 0 on an id and an empty string on a text field both mean ANY —
    /// the rule does not care about that dimension — and the more a rule pins, the more specific
    /// it is and the higher it ranks.
    /// </summary>
    public class SaveTaxRuleRequest
    {
        /// <summary>On a supersede, the rule being replaced. Ignored on a create.</summary>
        public int TaxRuleMasterID { get; set; }

        public string RuleName { get; set; } = string.Empty;

        /// <summary>SALE or PURCHASE.</summary>
        public string Direction { get; set; } = string.Empty;

        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int UserTypeID { get; set; }
        public int PartyUserID { get; set; }
        public string PartyRegistrationStatus { get; set; } = string.Empty;
        public string SupplyNature { get; set; } = string.Empty;

        /// <summary>GROSS / COMMISSION / CHARGE / EXEMPT / NONGST. The field that decides what tax
        /// is charged ON, and the one worth getting right.</summary>
        public string BillingBasis { get; set; } = string.Empty;

        public int? TaxRateMasterID { get; set; }
        public int? HSNSACMasterID { get; set; }
        public int? TDSSectionMasterID { get; set; }

        public bool ReverseCharge { get; set; }
        public bool ITCEligible { get; set; } = true;

        /// <summary>Higher wins. Leave 0 and the service derives it from how specific the rule is.</summary>
        public int Priority { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public string Remark { get; set; } = string.Empty;

        /// <summary>Why this change is being made. Required — a rate change nobody can explain is a
        /// rate change nobody can defend.</summary>
        public string Reason { get; set; } = string.Empty;
    }
}
