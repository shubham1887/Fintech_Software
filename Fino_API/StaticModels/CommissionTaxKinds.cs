namespace FintechApp.StaticModels
{
    /// <summary>
    /// Which level a commission tax treatment was set at. Resolved most specific first, the same
    /// way a tax rule is — so an install can set one house default and override only the providers
    /// and slabs that differ.
    /// </summary>
    public static class CommissionTaxScope
    {
        /// <summary>The house default. Exactly one, so a fresh install behaves predictably.</summary>
        public const string Global = "GLOBAL";

        /// <summary>Everything on one service. ScopeID is the ServiceID.</summary>
        public const string Service = "SERVICE";

        /// <summary>Money we receive FROM a provider. ScopeID is the ProviderID.</summary>
        public const string Provider = "PROVIDER";

        /// <summary>Money we pay OUT on a slab. ScopeID is the SlabMasterID.</summary>
        public const string Slab = "SLAB";

        public static readonly string[] All = { Global, Service, Provider, Slab };

        /// <summary>How specific a scope is. Higher wins when two rows both match.</summary>
        public static int Rank(string? scope) => (scope ?? string.Empty).ToUpperInvariant() switch
        {
            Slab => 40,
            Provider => 30,
            Service => 20,
            Global => 10,
            _ => 0,
        };
    }

    /// <summary>Whether a commission figure already contains GST.</summary>
    public static class CommissionTaxMode
    {
        /// <summary>The figure contains the tax, so it is backed OUT of it. A retailer quoted a
        /// Rs 15 charge pays Rs 15, of which Rs 2.29 is GST at 18%.</summary>
        public const string Inclusive = "INCLUSIVE";

        /// <summary>The tax is added ON TOP of the figure. A provider paying Rs 100 commission
        /// exclusive actually owes Rs 118.</summary>
        public const string Exclusive = "EXCLUSIVE";

        /// <summary>No GST on this commission at all.</summary>
        public const string None = "NONE";

        public static readonly string[] All = { Inclusive, Exclusive, None };
    }

    /// <summary>What the GST is charged ON. The single most consequential setting here.</summary>
    public static class CommissionTaxBase
    {
        /// <summary>The commission earned. Agency work — DMT, AEPS, payout.</summary>
        public const string Commission = "COMMISSION";

        /// <summary>The whole transaction amount. Trading — talktime bought and resold.</summary>
        public const string Gross = "GROSS";

        public static readonly string[] All = { Commission, Gross };
    }

    /// <summary>Which tier's commission a TDS deduction was taken from.</summary>
    public static class CommissionTier
    {
        public const string Retailer = "Retailer";
        public const string Distributor = "Distributor";
        public const string SuperDistributor = "SuperDistributor";

        public static readonly string[] All = { Retailer, Distributor, SuperDistributor };
    }

    /// <summary>
    /// The GST decision carried by ONE ROW of a rate card — one provider commission band, one slab
    /// band. Same vocabulary as <see cref="CommissionTaxMode"/> plus DEFAULT, which is what every
    /// row that existed before this column holds: "this band does not decide, fall through to the
    /// commission tax profile". That is what makes a live install's figures identical on the day
    /// this ships, and it is why a row is never READ as NONE by accident.
    /// </summary>
    public static class ChargeGstType
    {
        /// <summary>Not decided on the row. Falls through to CommissionTaxProfiles
        /// (SLAB → PROVIDER → SERVICE → GLOBAL), exactly as before this column existed. An
        /// untouched row holds an empty string, which reads the same way.</summary>
        public const string Default = "DEFAULT";

        /// <summary>The charge on the row already contains the GST: 5.90 is 5.00 + 0.90 at 18%.</summary>
        public const string Inclusive = CommissionTaxMode.Inclusive;

        /// <summary>The GST goes on top of the row: 5.00 costs 5.90 at 18%.</summary>
        public const string Exclusive = CommissionTaxMode.Exclusive;

        /// <summary>No GST on this band at all. Set deliberately — never inferred.</summary>
        public const string None = CommissionTaxMode.None;

        public static readonly string[] All = { Default, Inclusive, Exclusive, None };

        /// <summary>True when the row leaves the decision to the profile.</summary>
        public static bool IsDefault(string? value) => Normalize(value) == Default;

        /// <summary>
        /// Canonical spelling. Blank, and anything we do not recognise, returns DEFAULT: a typo in
        /// a free-text column must fall through to the configured profile, never quietly become
        /// "no GST" and hand somebody back the tax they owe.
        /// </summary>
        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            if (v.Length == 0) return Default;

            if (v.Equals(Inclusive, StringComparison.OrdinalIgnoreCase) ||
                v.Equals("INC", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("INCLUDING GST", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("INCLUSIVE GST", StringComparison.OrdinalIgnoreCase)) return Inclusive;

            if (v.Equals(Exclusive, StringComparison.OrdinalIgnoreCase) ||
                v.Equals("EXC", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("EXCL", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("EXCLUSIVE GST", StringComparison.OrdinalIgnoreCase)) return Exclusive;

            if (v.Equals(None, StringComparison.OrdinalIgnoreCase) ||
                v.Equals("NO GST", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("NOGST", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("NA", StringComparison.OrdinalIgnoreCase)) return None;

            return Default;
        }
    }

    /// <summary>
    /// What a configured charge figure MEANS — a flat rupee amount, or a percentage of the
    /// transaction. The columns that hold it are free text and three screens have spelled the same
    /// idea differently (Flexi/Fix, Percentage/Flat, %), so the figure stamped on a transaction
    /// carries one canonical word instead of whichever screen wrote the row.
    /// </summary>
    public static class ChargeBasis
    {
        public const string Flat = "FLAT";
        public const string Percentage = "PERCENTAGE";
    }
}
