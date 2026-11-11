namespace FintechApp.Responses.CommissionTax
{
    /// <summary>
    /// The treatment that applies to one commission, and WHERE each answer came from. The source
    /// matters: an admin looking at a wrong figure needs to know which row to edit, not just that
    /// the figure is wrong.
    /// </summary>
    public class CommissionTaxTreatment
    {
        public int CommissionTaxProfileID { get; set; }

        /// <summary>GLOBAL / SERVICE / PROVIDER / SLAB — the level this was decided at.</summary>
        public string Scope { get; set; } = string.Empty;
        public string ScopeLabel { get; set; } = string.Empty;

        public string TaxMode { get; set; } = string.Empty;
        public string TaxBase { get; set; } = string.Empty;

        /// <summary>The rate actually used, after the engine filled in a 0 on the profile.</summary>
        public decimal GSTRatePercent { get; set; }

        /// <summary>True when the rate came from the tax engine rather than being pinned here.</summary>
        public bool RateFromTaxEngine { get; set; }

        public decimal PerTransactionCharge { get; set; }
        public string PerTransactionChargeTaxMode { get; set; } = string.Empty;

        public bool TDSEnabled { get; set; }
        public string TDSSectionCode { get; set; } = string.Empty;
        public decimal TDSRatePercent { get; set; }

        /// <summary>Set when no profile matched and the built-in fallback was used — which is
        /// itself worth reporting, because it means nobody has configured this yet.</summary>
        public string Warning { get; set; } = string.Empty;
    }

    /// <summary>A commission figure split into what it actually is.</summary>
    public class CommissionSplit
    {
        /// <summary>The figure as configured — what the slab or the provider contract says.</summary>
        public decimal CommissionAsQuoted { get; set; }

        /// <summary>The income, with GST removed where the figure was inclusive.</summary>
        public decimal NetCommission { get; set; }

        /// <summary>The GST on it. Added on top when exclusive, carved out when inclusive.</summary>
        public decimal GST { get; set; }

        /// <summary>What actually changes hands: net + GST. For an INCLUSIVE figure this equals
        /// the quoted one; for an EXCLUSIVE figure it is larger.</summary>
        public decimal Payable { get; set; }

        /// <summary>What the GST was computed on — the commission, or the whole amount.</summary>
        public decimal TaxableBase { get; set; }

        public string TaxMode { get; set; } = string.Empty;
        public string TaxBase { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
    }

    /// <summary>One configured profile, as the screen lists it.</summary>
    public class CommissionTaxProfileRow
    {
        public int CommissionTaxProfileID { get; set; }
        public string Scope { get; set; } = string.Empty;
        public int ScopeID { get; set; }
        /// <summary>The provider / service / slab's own name, so the list is readable.</summary>
        public string ScopeName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string TaxMode { get; set; } = string.Empty;
        public string TaxBase { get; set; } = string.Empty;
        public decimal GSTRatePercent { get; set; }
        public decimal PerTransactionCharge { get; set; }
        public string PerTransactionChargeTaxMode { get; set; } = string.Empty;
        public bool TDSEnabled { get; set; }
        public string TDSSectionCode { get; set; } = string.Empty;
        public decimal TDSRatePercent { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int Priority { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>A worked example, step by step, for the admin to check before trusting it.</summary>
    public class CommissionTaxPreview
    {
        public CommissionTaxTreatment Treatment { get; set; } = new();
        public CommissionSplit Split { get; set; } = new();

        /// <summary>The TDS that would be deducted from this commission, if TDS is on.</summary>
        public decimal TDSAmount { get; set; }
        public string TDSSectionCode { get; set; } = string.Empty;

        /// <summary>What the party actually receives: payable − TDS.</summary>
        public decimal NetToParty { get; set; }

        /// <summary>Each step in words, so the arithmetic can be followed rather than trusted.</summary>
        public List<string> Steps { get; set; } = new();
    }

    /// <summary>What a TDS run deducted, per tier.</summary>
    public class TdsDeductionRow
    {
        public int PartyUserID { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public decimal RatePercent { get; set; }
        public decimal CommissionBase { get; set; }
        public decimal TDSAmount { get; set; }
        public bool NoPANRateApplied { get; set; }

        /// <summary>PERCENT or FIX — a flat deduction has no meaningful rate to show.</summary>
        public string ChargeType { get; set; } = string.Empty;

        /// <summary>Whether the money actually came out of the party's wallet.</summary>
        public bool Withheld { get; set; }

        /// <summary>Which rule decided, or why nothing was deducted. Never a silent zero.</summary>
        public string Note { get; set; } = string.Empty;
    }

    public class TdsDeductionResult
    {
        public int TransactionMasterID { get; set; }
        public bool TDSEnabled { get; set; }
        public decimal TotalDeducted { get; set; }
        public List<TdsDeductionRow> Rows { get; set; } = new();
        /// <summary>Why nothing was deducted, when nothing was. Never silent.</summary>
        public string Note { get; set; } = string.Empty;
    }
}
