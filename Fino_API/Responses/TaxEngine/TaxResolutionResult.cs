namespace FintechApp.Responses.TaxEngine
{
    /// <summary>
    /// What the engine decided. These fields are what gets STAMPED on the transaction
    /// (TransactionTaxDetail) and what an invoice line is built from — reports read the stamp and
    /// never recompute, so this shape is the contract the whole subsystem rests on.
    /// </summary>
    public class TaxResolutionResult
    {
        public int TaxRuleMasterID { get; set; }
        public string RuleName { get; set; } = string.Empty;

        public string Direction { get; set; } = string.Empty;
        public string BillingBasis { get; set; } = string.Empty;

        /// <summary>The whole throughput, carried through unchanged.</summary>
        public decimal GrossValue { get; set; }

        /// <summary>What tax was charged on, after the basis picked a value and — when the value
        /// was tax-inclusive — after backing the tax out of it.</summary>
        public decimal TaxableValue { get; set; }

        public string SupplyNature { get; set; } = string.Empty;
        public string SupplierStateCode { get; set; } = string.Empty;
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;
        public string SACCode { get; set; } = string.Empty;

        public decimal CGSTRate { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CessAmount { get; set; }

        /// <summary>CGST + SGST + IGST + cess.</summary>
        public decimal TotalTax { get; set; }

        /// <summary>Taxable + tax. On a tax-inclusive line this should equal the value that came
        /// in, give or take RoundOff.</summary>
        public decimal InvoiceValue { get; set; }

        /// <summary>
        /// The paise that rounding leaves behind. Ezytm's own invoice carries exactly this:
        /// 4,66,101.69 + 83,898.30 = 5,49,999.99 against a 5,50,000.00 total, so the invoice shows
        /// "Round Off 0.01". Reporting it rather than absorbing it is what keeps the invoice total
        /// equal to the money that actually moved.
        /// </summary>
        public decimal RoundOff { get; set; }

        public string TDSSectionCode { get; set; } = string.Empty;
        public decimal TDSRate { get; set; }
        public decimal TDSAmount { get; set; }
        public string PartyRelationship { get; set; } = string.Empty;

        public bool ITCEligible { get; set; }
        public bool ReverseCharge { get; set; }

        /// <summary>
        /// Empty when the resolution was clean. Set when no rule matched, when two matched at the
        /// same priority, or when a TDS decision could not be made because the relationship is
        /// unknown. Stamped on the row and surfaced on a dashboard tile — these are the
        /// configuration backlog, and a warning in a file the app pool cannot write to is a
        /// warning nobody reads.
        /// </summary>
        public string Warning { get; set; } = string.Empty;

        /// <summary>Every rule that matched, highest priority first — what the simulator shows so
        /// a non-technical admin can see WHY a rule won, not just which one did.</summary>
        public List<TaxRuleMatch> Candidates { get; set; } = new();

        public DateTime ResolvedOn { get; set; }
    }

    /// <summary>One rule that matched the context, for the simulator's explanation.</summary>
    public class TaxRuleMatch
    {
        public int TaxRuleMasterID { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string BillingBasis { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public string SACCode { get; set; } = string.Empty;

        /// <summary>Which dimensions this rule pinned, e.g. "service, party" — the reason it
        /// ranks where it does.</summary>
        public string MatchedOn { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
