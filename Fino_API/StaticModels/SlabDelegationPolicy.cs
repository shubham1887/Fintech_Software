namespace FintechApp.StaticModels
{
    /// <summary>
    /// Who may cut a commission card, who they may hand it to, and which tier on it they are
    /// allowed to raise for themselves.
    ///
    /// The delegation model in one paragraph: admin writes a card that puts a reseller's whole net
    /// commission on that reseller's own line — ₹12 on the distributor line, not ₹10 retailer plus
    /// ₹2 distributor — and assigns it to them. The reseller then cuts child cards for the tiers
    /// below and decides what to pass down; whatever they do not pass down they keep, because the
    /// platform credits them the ₹12 and their own wallet is debited for what they gave away.
    /// Nobody can hand out more than they hold, at any band, on any service — which is the one
    /// invariant the whole feature exists to keep, and the reason every rule below is enforced on
    /// the server and merely mirrored in the UI.
    /// </summary>
    public static class SlabDelegationPolicy
    {
        /// <summary>
        /// Which roles a given tier may ASSIGN a slab to. A tier that is absent cuts no cards at
        /// all: a retailer and a client read their rate and nothing more.
        ///
        /// Admin is listed with every tier, Client included — the operator still assigns the
        /// top-level card the way they always have.
        /// </summary>
        public static readonly Dictionary<int, int[]> AssignableTo = new()
        {
            [UserTypeCode.Admin] = new[]
            {
                UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
                UserTypeCode.Retailer, UserTypeCode.Client, UserTypeCode.ApiUser,
            },
            [UserTypeCode.SupportUser] = new[]
            {
                UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
                UserTypeCode.Retailer, UserTypeCode.Client, UserTypeCode.ApiUser,
            },
            // A super-distributor's own network: its distributors, and the retailers it holds
            // directly (a "Direct Retailer" in the hierarchy diagram).
            [UserTypeCode.SuperDistributor] = new[] { UserTypeCode.Distributor, UserTypeCode.Retailer },
            [UserTypeCode.Distributor] = new[] { UserTypeCode.Retailer },
        };

        /// <summary>Whether this tier may create slabs at all.</summary>
        public static bool CanCreateSlab(int userTypeID) => AssignableTo.ContainsKey(userTypeID);

        /// <summary>Whether <paramref name="ownerTypeID"/> may assign a slab to a user of that role.</summary>
        public static bool CanAssignTo(int ownerTypeID, int targetTypeID) =>
            AssignableTo.TryGetValue(ownerTypeID, out var roles) && Array.IndexOf(roles, targetTypeID) >= 0;

        /// <summary>
        /// The tier column a role is PAID from. On the card above them it is their income — the
        /// whole net amount they have to spend. On a card they wrote themselves it is left at
        /// zero, because nobody funds an owner out of their own card.
        ///
        /// This is the mechanics of the business ask: "give me the full net amount and I will
        /// decide what the retailer gets". A distributor whose own card says ₹12 and who writes
        /// ₹10 onto a retailer card has kept ₹2 — and that ₹2 is never written down anywhere. It
        /// is the gap between what the platform credits them and what their wallet is debited for
        /// the retailer, which is why the books cannot be made to disagree with the rate card.
        /// </summary>
        public static string? OwnTierOf(int ownerTypeID) => ownerTypeID switch
        {
            UserTypeCode.SuperDistributor => Entities.SlabAuditTier.SuperDistributor,
            UserTypeCode.Distributor => Entities.SlabAuditTier.Distributor,
            _ => null, // admin owns the ceiling itself; nobody else cuts cards
        };

        /// <summary>
        /// The tier column that pays a user of this role, for every role — including the ones that
        /// cut no cards. <see cref="OwnTierOf"/> answers the same question but only for owners, and
        /// returns null for everyone else; the commission engine needs an answer for a retailer and
        /// a client too, because it now reads each level's rate off that level's own card.
        /// </summary>
        public static string TierForRole(int userTypeID) => userTypeID switch
        {
            UserTypeCode.SuperDistributor => Entities.SlabAuditTier.SuperDistributor,
            UserTypeCode.Distributor => Entities.SlabAuditTier.Distributor,
            UserTypeCode.Client => Entities.SlabAuditTier.Client,
            _ => Entities.SlabAuditTier.Retailer,
        };

        /// <summary>
        /// The tier an owner passes a new card's money down to by default — the role directly
        /// below them. A card is created at pass-through (the owner keeps nothing) so that making
        /// one changes no economics until its owner decides something.
        /// </summary>
        public static string DefaultPassThroughTier(int ownerTypeID) => ownerTypeID switch
        {
            UserTypeCode.SuperDistributor => Entities.SlabAuditTier.Distributor,
            _ => Entities.SlabAuditTier.Retailer,
        };

        /// <summary>
        /// Which tier columns a given role may SET on a commission band — the tiers strictly BELOW
        /// it, and nothing else.
        ///
        /// A distributor sells to retailers, so a retailer rate is the only rate it has any
        /// business deciding. A super-distributor sells to distributors and to retailers it holds
        /// directly, so it decides those two. Its own column is not in the list because it is
        /// computed, and the columns above it are not in the list because they are not its money.
        ///
        /// Admin keeps all four, Client included: the operator still writes the card that caps the
        /// whole ecosystem, and a Client account is a tier it sells to directly.
        /// </summary>
        public static string[] TiersSettableBy(int userTypeID) => userTypeID switch
        {
            UserTypeCode.Admin or UserTypeCode.SupportUser => new[]
            {
                Entities.SlabAuditTier.Retailer, Entities.SlabAuditTier.Distributor,
                Entities.SlabAuditTier.SuperDistributor, Entities.SlabAuditTier.Client,
            },
            UserTypeCode.SuperDistributor => new[]
            {
                Entities.SlabAuditTier.Distributor, Entities.SlabAuditTier.Retailer,
            },
            UserTypeCode.Distributor => new[] { Entities.SlabAuditTier.Retailer },
            _ => Array.Empty<string>(),
        };

        /// <summary>The wording a refusal uses. Stated once so the API and the UI agree.</summary>
        public static class Message
        {
            public const string ExceedsParent =
                "Commission cannot exceed the commission assigned by your parent.";
            public const string TypeMismatch =
                "Commission type must match the parent slab.";
            public const string Unauthorized =
                "You are not authorized to create slabs for this user type.";
            public const string ParentUpdated =
                "Parent commission has changed. Please update your slab before continuing.";
            public const string NotYourSlab =
                "This commission slab is not managed by your account.";
            public const string NoParentSlab =
                "No commission slab is assigned to your account yet. Ask your parent to assign one before creating child slabs.";
            public const string GstLocked =
                "GST settings are inherited from the parent slab and cannot be changed.";
            /// <summary>
            /// Kept for the rule that no longer exists. The owner's margin used to be written into
            /// their own column, which could not hold a figure that was part flat and part
            /// percentage. Nothing is written there now — the margin is the gap between what the
            /// platform credits the owner and what their wallet is debited — so a reseller is free
            /// to price in whichever unit it likes against either kind of parent rate.
            /// </summary>
            public const string MarginNotExpressible =
                "Use the same commission type (percent or flat) as your parent's band for this range.";
            public const string NothingSettable =
                "Your account type does not set commission rates.";
            public const string TotalExceedsParent =
                "This band hands out more than your own commission for the same amount. Reduce what you are giving away.";
            public const string OutsideNetwork =
                "That user is not in your network.";
            /// <summary>
            /// Deleting a card somebody is earning on would silently move them onto their upline's
            /// rates, so the assignments have to be taken back first — by the person who made them,
            /// who is the only one who knows what those users should earn instead.
            /// </summary>
            public const string SlabInUse =
                "This slab is still assigned to {0} {1}. Remove those assignments first, then delete it.";
            public const string SlabHasChildren =
                "{0} {1} been cut from this slab. Delete those first, then this one.";
            public const string NotADelegatedSlab =
                "This is the platform's own slab, not one cut by a reseller — it is managed from Commission Slabs.";
        }
    }
}
