namespace FintechApp.StaticModels
{
    /// <summary>
    /// How specifically a TDS rule is addressed. Matched MOST SPECIFIC FIRST, so an install can
    /// say one thing for the whole platform and then carve out a single retailer on a single
    /// service without touching anything else.
    ///
    /// The six scopes are what the brief actually asks for: "konse service ko dena hai, all user
    /// type ko, ya fir particular user ko dena hai, ya sirf uske sirf service ko".
    /// </summary>
    public static class TdsScope
    {
        /// <summary>One user, on one service. The narrowest thing anyone can say.</summary>
        public const string UserService = "USER_SERVICE";

        /// <summary>One user, on every service they transact.</summary>
        public const string User = "USER";

        /// <summary>A whole role (retailer, distributor, …) on one service.</summary>
        public const string UserTypeService = "USERTYPE_SERVICE";

        /// <summary>A whole role, on every service.</summary>
        public const string UserType = "USERTYPE";

        /// <summary>One service, whoever earns the commission on it.</summary>
        public const string Service = "SERVICE";

        /// <summary>The house default. One row is enough to start.</summary>
        public const string Global = "GLOBAL";

        public static readonly string[] All = { UserService, User, UserTypeService, UserType, Service, Global };

        /// <summary>
        /// How specific a scope is. Higher wins when two rules both match, and the numbers are
        /// spaced so a deployment can slot something in between with an explicit Priority.
        /// </summary>
        public static int Rank(string? scope) => (scope ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            UserService => 60,
            User => 50,
            UserTypeService => 40,
            UserType => 30,
            Service => 20,
            Global => 10,
            _ => 0,
        };

        /// <summary>What a scope needs filled in to mean anything. Checked before a rule is saved.</summary>
        public static bool NeedsUser(string? scope) => Rank(scope) is 60 or 50;
        public static bool NeedsUserType(string? scope) => Rank(scope) is 40 or 30;
        public static bool NeedsService(string? scope) => Rank(scope) is 60 or 40 or 20;
    }

    /// <summary>Whether a TDS figure is a percentage of the commission or a flat rupee amount.</summary>
    public static class TdsChargeType
    {
        /// <summary>A percentage — the ordinary case. 0.01 means 0.01%, not 1%.</summary>
        public const string Percent = "PERCENT";

        /// <summary>A flat rupee amount per transaction, whatever the commission is.</summary>
        public const string Fix = "FIX";

        public static readonly string[] All = { Percent, Fix };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            if (v.Equals(Fix, StringComparison.OrdinalIgnoreCase) ||
                v.Equals("FLAT", StringComparison.OrdinalIgnoreCase) ||
                v.Equals("FIXED", StringComparison.OrdinalIgnoreCase)) return Fix;
            return Percent;
        }
    }

    /// <summary>
    /// What the TDS is computed ON. This is the one setting that quietly changes the figure, so it
    /// is written down rather than assumed.
    /// </summary>
    public static class TdsApplyOn
    {
        /// <summary>
        /// The commission NET of GST — the default, and the correct one. TDS is a tax on income,
        /// and the GST collected along with a commission is not the earner's income: deducting on
        /// the gross figure taxes the tax.
        /// </summary>
        public const string CommissionNet = "COMMISSION_NET";

        /// <summary>The commission exactly as quoted, GST included. Only where a contract says so.</summary>
        public const string CommissionGross = "COMMISSION_GROSS";

        public static readonly string[] All = { CommissionNet, CommissionGross };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            return v.Equals(CommissionGross, StringComparison.OrdinalIgnoreCase) ? CommissionGross : CommissionNet;
        }
    }

    /// <summary>What was done with a period's withheld TDS for one party.</summary>
    public static class TdsSettlementMode
    {
        /// <summary>Paid to the government. The reference is the challan / BSR detail.</summary>
        public const string Deposited = "DEPOSITED";

        /// <summary>
        /// Given back to the party as wallet balance instead of being deposited — the party
        /// produced a PAN, or a rule was wrong. It moves real money, so it is Admin's alone.
        /// </summary>
        public const string WalletSettled = "WALLET_SETTLED";

        public static readonly string[] All = { Deposited, WalletSettled };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim();
            return v.Equals(WalletSettled, StringComparison.OrdinalIgnoreCase) ? WalletSettled : Deposited;
        }
    }

    /// <summary>
    /// Key Setting rows this subsystem reads. A curated feature toggle, same shape as
    /// IsKYCEnforced and IsSubscriptionEnabled, so it appears on the Key Setting screen.
    /// </summary>
    public static class SettingKeys
    {
        /// <summary>
        /// The TDS master switch. OFF until somebody turns it on deliberately, because turning it
        /// on starts withholding real money from real people's commissions.
        /// </summary>
        public const string TDSEnabled = "IsTDSEnabled";
    }
}
