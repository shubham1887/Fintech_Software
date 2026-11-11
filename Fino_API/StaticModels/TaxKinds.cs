namespace FintechApp.StaticModels
{
    /// <summary>
    /// What a tax rate row IS. Stored as a short string rather than an enum so a new head can be
    /// added from the admin screen without a release — the whole point of the dynamic engine.
    /// </summary>
    public static class TaxType
    {
        public const string GST = "GST";
        public const string Cess = "CESS";
        public const string TDS = "TDS";
        public const string TCS = "TCS";
    }

    /// <summary>
    /// Which value a tax is charged ON. This single field is what the 18cr-sales against
    /// 19cr-purchase mismatch came down to: the suppliers bill recharge on the gross top-up
    /// (Ezytm, Erech — SAC 998413) and DMT on the charge alone (JaiJui — SAC 997112/997159), so
    /// a register that adds both together is comparing two different quantities.
    ///
    /// Every invoice line and every stamped transaction records both the gross value and the
    /// taxable value, plus the basis that produced the second from the first.
    /// </summary>
    public static class BillingBasis
    {
        /// <summary>Taxable value = the whole throughput. Trading: talktime bought and resold.</summary>
        public const string Gross = "GROSS";

        /// <summary>Taxable value = the commission earned. Agency: DMT, AEPS.</summary>
        public const string Commission = "COMMISSION";

        /// <summary>Taxable value = the fee charged. Payout, verification, subscription.</summary>
        public const string Charge = "CHARGE";

        /// <summary>Taxable value = 0, but the turnover still has to be reported.</summary>
        public const string Exempt = "EXEMPT";

        /// <summary>Outside the scope of GST entirely.</summary>
        public const string NonGst = "NONGST";

        public static readonly string[] All = { Gross, Commission, Charge, Exempt, NonGst };

        public static bool IsValid(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            Array.Exists(All, x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Intra-state or inter-state — which decides CGST+SGST against IGST. Never a choice someone
    /// makes on a form: it falls out of the supplier's registered state and the place of supply.
    /// Ezytm billed Rajasthan (08) to Maharashtra (27) and so charged IGST; JaiJui billed Pune
    /// (27) to Panvel (27) and so charged CGST+SGST. Same rate, different heads, and the wrong
    /// head is only fixed by a refund.
    /// </summary>
    public static class SupplyNature
    {
        public const string Intrastate = "INTRASTATE";
        public const string Interstate = "INTERSTATE";
        public const string Export = "EXPORT";
        public const string SEZ = "SEZ";

        public static readonly string[] All = { Intrastate, Interstate, Export, SEZ };
    }

    /// <summary>Which side of the books a rule applies to. A service is often billed to us on one
    /// basis and by us on another, so direction is part of the rule key, not a property of the
    /// service.</summary>
    public static class TaxDirection
    {
        public const string Sale = "SALE";
        public const string Purchase = "PURCHASE";
    }

    /// <summary>
    /// Whether a party acts for us or trades with us. The Supreme Court held in Bharti Cellular
    /// (2024) that a discount given to a prepaid distributor is not "commission", because the
    /// relationship is principal-to-principal rather than principal-agent, so Section 194H does
    /// not bite. The same platform pays both kinds, so the distinction is per party per service
    /// and has to be backed by the actual agreement — never inferred.
    /// </summary>
    public static class PartyRelationship
    {
        /// <summary>We sell to them; what they keep is a discount. No 194H.</summary>
        public const string PrincipalToPrincipal = "P2P";

        /// <summary>They act for us; what they keep is commission. 194H applies.</summary>
        public const string Agent = "AGENT";

        public static readonly string[] All = { PrincipalToPrincipal, Agent };
    }

    /// <summary>
    /// Which way a leg of a reciprocal wallet swap ran.
    ///
    /// The asymmetry matters: an OUTWARD leg is our own supply, so we can and must raise the
    /// invoice ourselves. An INWARD leg is the partner's supply, so the document has to be
    /// obtained from them — it cannot be generated, only chased, which is why those legs sit in a
    /// queue instead of quietly resolving themselves.
    /// </summary>
    public static class ExchangeLegDirection
    {
        /// <summary>We credited them. Our sale.</summary>
        public const string Outward = "OUTWARD";

        /// <summary>They credited us. Their sale, our purchase.</summary>
        public const string Inward = "INWARD";
    }

    /// <summary>Whether a leg has the document it needs.</summary>
    public static class ExchangeDocumentStatus
    {
        public const string Invoiced = "Invoiced";

        /// <summary>The queue. On an inward leg this means the partner's invoice has not arrived.</summary>
        public const string AwaitingDocument = "AwaitingDocument";

        /// <summary>Given up on, with a reason. Deliberately explicit: a leg that is simply left
        /// awaiting for ever is indistinguishable from one nobody looked at.</summary>
        public const string WrittenOff = "WrittenOff";
    }

    /// <summary>GST registration status of the other party, which picks the document route:
    /// a tax invoice to a registered party, a consolidated B2C invoice to an unregistered one,
    /// and a self-invoice where we have to pay the tax ourselves.</summary>
    public static class PartyRegistrationStatus
    {
        public const string Registered = "REGISTERED";
        public const string Unregistered = "UNREGISTERED";
        public const string Composition = "COMPOSITION";
        public const string SEZ = "SEZ";
        public const string Overseas = "OVERSEAS";

        public static readonly string[] All = { Registered, Unregistered, Composition, SEZ, Overseas };
    }
}
