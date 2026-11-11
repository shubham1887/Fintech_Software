namespace FintechApp.Interfaces.Services
{
    /// <summary>One party's TDS decision for one transaction, with the reason attached.</summary>
    public class TdsDecision
    {
        /// <summary>True when a rule matched. False means no rule covers this party/service, and
        /// the caller falls back to the commission tax profile exactly as it did before rules
        /// existed — so adding this subsystem changes nothing until a rule is written.</summary>
        public bool RuleFound { get; set; }

        public int TdsRuleID { get; set; }
        public string Scope { get; set; } = string.Empty;

        /// <summary>PERCENT or FIX.</summary>
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>The configured figure: a percentage, or rupees per transaction.</summary>
        public decimal ChargeValue { get; set; }

        /// <summary>COMMISSION_NET or COMMISSION_GROSS — what the percentage is taken of.</summary>
        public string ApplyOn { get; set; } = string.Empty;

        public decimal MinCommission { get; set; }
        public decimal MaxTDSPerTransaction { get; set; }

        public string SectionCode { get; set; } = string.Empty;
        public bool ApplyNoPANRate { get; set; }

        /// <summary>Which rule won, in words. Stored on the deduction so support can answer
        /// "why ₹0.13?" without re-deriving a rule that may since have been edited.</summary>
        public string DecidedBy { get; set; } = string.Empty;
    }

    /// <summary>The computed deduction for one commission under one decision.</summary>
    public class TdsComputation
    {
        /// <summary>The commission as it was earned, before anything was taken off.</summary>
        public decimal CommissionAsEarned { get; set; }

        /// <summary>What the deduction was computed on, after GST was backed out where the rule
        /// says NET. This is the figure a TDS certificate states.</summary>
        public decimal Base { get; set; }

        public decimal RatePercent { get; set; }
        public decimal TDSAmount { get; set; }

        public string ChargeType { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public bool NoPANRateApplied { get; set; }

        /// <summary>Why nothing was deducted, when nothing was. Never silently zero.</summary>
        public string SkipReason { get; set; } = string.Empty;

        public string DecidedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Which TDS rule applies to a commission, and what it comes to.
    ///
    /// Six scopes, most specific first — one user on one service, that user everywhere, a whole
    /// role on one service, that role everywhere, a service for everyone, then the house default.
    /// Percentage or flat, on the commission net of GST or as quoted, with a floor and a cap.
    /// </summary>
    public interface ITdsRuleResolver
    {
        /// <summary>The rule that applies, or RuleFound = false when none does.</summary>
        Task<TdsDecision> Resolve(int userID, int userTypeID, int serviceID, int operatorID, DateTime asOf);

        /// <summary>
        /// Work a commission through a decision. <paramref name="commissionNet"/> is the commission
        /// with GST backed out (the charge GST breakdown already carries it) and
        /// <paramref name="commissionGross"/> is the figure as quoted.
        /// </summary>
        Task<TdsComputation> Compute(TdsDecision decision, int userID, decimal commissionGross, decimal commissionNet);

        /// <summary>Is TDS switched on at all? The Key Setting master toggle, read in one place.</summary>
        Task<bool> IsMasterSwitchOn();
    }
}
