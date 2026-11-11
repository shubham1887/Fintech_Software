namespace FintechApp.StaticModels
{
    /// <summary>
    /// One mobile number, two roles.
    ///
    /// A distributor who also runs a retail counter, a super-distributor who sells directly, an
    /// API client who needs a retailer login to test against — all of them were forced onto a
    /// second mobile number, because sign-in is keyed on the number and the number was unique
    /// across the whole user table. They end up with two phones, two passwords and no way to move
    /// between the two without signing out.
    ///
    /// A linked profile is a SECOND REAL ACCOUNT, not a disguise: its own wallet, slab, services,
    /// user code and ledger. What it shares with the primary is the mobile number and the
    /// credentials. Signing in authenticates the primary and then offers the choice; the switcher
    /// re-issues a token for whichever one the person picks.
    ///
    /// Who may set one up, and as what, is deliberately narrow:
    ///
    ///   Distributor        → Retailer
    ///   Super Distributor  → Retailer or Distributor (one of them)
    ///   API User           → Retailer
    ///
    /// A retailer has nothing below them to also be, so they are not offered it; nor is the
    /// linked account itself — a second role is a property of the primary, and allowing the
    /// secondary to add a third would turn a pair into a tree of identities behind one password.
    /// Exactly one linked account per primary, for the same reason.
    /// </summary>
    public static class LinkedProfilePolicy
    {
        /// <summary>Roles a primary of each type may run as a second login. Empty / absent = none.</summary>
        private static readonly Dictionary<int, int[]> SecondaryRoles = new()
        {
            [UserTypeCode.Distributor] = new[] { UserTypeCode.Retailer },
            [UserTypeCode.SuperDistributor] = new[] { UserTypeCode.Retailer, UserTypeCode.Distributor },
            [UserTypeCode.ApiUser] = new[] { UserTypeCode.Retailer },
        };

        /// <summary>The roles this user type may add as a second login (empty when it may not).</summary>
        public static int[] AllowedFor(int userTypeID) =>
            SecondaryRoles.TryGetValue(userTypeID, out var roles) ? roles : Array.Empty<int>();

        /// <summary>Can an account of this type set up a second login at all?</summary>
        public static bool CanSetUp(int userTypeID) => AllowedFor(userTypeID).Length > 0;

        /// <summary>Is <paramref name="secondaryRole"/> a legal second role for this primary?</summary>
        public static bool IsAllowed(int primaryUserTypeID, int secondaryRole) =>
            Array.IndexOf(AllowedFor(primaryUserTypeID), secondaryRole) >= 0;

        /// <summary>Suffix appended to the primary's firm name when the operator supplies none.</summary>
        public static string DefaultFirmSuffix(int secondaryRole) => secondaryRole switch
        {
            UserTypeCode.Retailer => "Retail Counter",
            UserTypeCode.Distributor => "Distribution",
            _ => "Linked",
        };
    }
}
