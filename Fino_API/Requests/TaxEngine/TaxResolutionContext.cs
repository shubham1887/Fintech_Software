namespace FintechApp.Requests.TaxEngine
{
    /// <summary>
    /// Everything the tax engine needs to decide one transaction. Nothing is looked up from
    /// inside the engine except the masters themselves — the caller supplies the facts.
    ///
    /// That is deliberate. An engine that fetches the user, the service and the company profile
    /// for itself cannot be tested without a database, and a tax engine that cannot be tested is
    /// a tax engine nobody trusts. Everything here is a plain value, so the whole resolution is
    /// exercisable against the real masters with no transaction in sight.
    /// </summary>
    public class TaxResolutionContext
    {
        /// <summary>SALE or PURCHASE — see StaticModels.TaxDirection.</summary>
        public string Direction { get; set; } = string.Empty;

        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }

        /// <summary>The other party's user type, for rules that differ by tier.</summary>
        public int UserTypeID { get; set; }

        /// <summary>The other party. 0 for an anonymous or aggregate line.</summary>
        public int PartyUserID { get; set; }

        /// <summary>REGISTERED / UNREGISTERED / COMPOSITION / SEZ / OVERSEAS. Left empty, the
        /// engine treats the party as unregistered, which is the safe assumption: it routes the
        /// turnover to B2C rather than claiming a credit nobody can support.</summary>
        public string PartyRegistrationStatus { get; set; } = string.Empty;

        /// <summary>The date the rate and rule are resolved AS OF. The transaction's own date,
        /// never "now" — that is what lets a two-year-old row answer with what it was charged.</summary>
        public DateTime TransactionDate { get; set; }

        // ── The three candidate values. Which one becomes taxable depends on the basis ──

        /// <summary>The whole throughput.</summary>
        public decimal GrossValue { get; set; }

        /// <summary>Commission earned on this transaction.</summary>
        public decimal CommissionValue { get; set; }

        /// <summary>Fee charged on this transaction.</summary>
        public decimal ChargeValue { get; set; }

        /// <summary>
        /// True when the value above ALREADY contains the tax, so the engine must work backwards.
        /// This is the normal case here: Ezytm charged Rs 5,50,000 for a Rs 5,50,000 top-up and
        /// showed 4,66,101.69 taxable plus 83,898.30 IGST inside it. Getting this flag wrong adds
        /// 18% on top of a figure that already included it.
        /// </summary>
        public bool IsTaxInclusive { get; set; }

        // ── Place of supply ────────────────────────────────────────────────────────────

        /// <summary>Our registered state's two-digit code.</summary>
        public string SupplierStateCode { get; set; } = string.Empty;

        /// <summary>The place of supply's two-digit code. Equal codes mean CGST+SGST, different
        /// codes mean IGST — never a choice on a form.</summary>
        public string PlaceOfSupplyStateCode { get; set; } = string.Empty;

        // ── TDS inputs ─────────────────────────────────────────────────────────────────

        /// <summary>P2P or AGENT. Empty means unknown, and the engine then deducts nothing and
        /// says so: Bharti Cellular makes the relationship the whole question, so guessing it
        /// either over-deducts from a distributor or under-deducts from an agent.</summary>
        public string PartyRelationship { get; set; } = string.Empty;

        /// <summary>Commission already paid to this party this financial year, BEFORE this
        /// transaction. Section 194H's threshold is annual per party, so the engine cannot judge
        /// it from one payment; the caller brings the running total.</summary>
        public decimal PartyYtdCommission { get; set; }

        public bool PartyHasValidPAN { get; set; }

        public bool PartyIs206ABFlagged { get; set; }

        /// <summary>Section 197 certificate rate, when one is in force for this date and still
        /// inside its amount cap. 0 = none.</summary>
        public decimal PartyLDCRate { get; set; }
    }
}
