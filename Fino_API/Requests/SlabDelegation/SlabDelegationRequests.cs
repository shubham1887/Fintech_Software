namespace FintechApp.Requests.SlabDelegation
{
    /// <summary>
    /// Cut a new child card out of the one assigned to me. Nothing about the SHAPE of the card is
    /// accepted here — the bands, the services, the operators, the amount ranges, the commission
    /// types and the GST decision are all cloned from the parent, because the child is only ever
    /// allowed to move the figures (Rules 3, 4 and 5). All the caller chooses is a name and who
    /// the card is for.
    /// </summary>
    public class CreateChildSlabRequest
    {
        public string SlabName { get; set; } = string.Empty;

        /// <summary>
        /// The role this card is meant for (UserTypeCode). Checked against
        /// SlabDelegationPolicy.AssignableTo, so a distributor can only ever make retailer cards.
        /// It is a label on the card, not a restriction on assignment — assignment re-checks the
        /// actual target user's role at the time it happens.
        /// </summary>
        public int ForUserTypeID { get; set; }
    }

    /// <summary>One tier's figure on a band, as the owner typed it.</summary>
    public class DelegatedTierInput
    {
        public decimal Comm { get; set; }
        /// <summary>Flexi / Fix (percent / flat). Blank keeps whatever the row already has.</summary>
        public string CommType { get; set; } = string.Empty;
        /// <summary>Commission / Surcharge. Blank keeps whatever the row already has.</summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Change ONE band of a card I own.
    ///
    /// <see cref="Tiers"/> is keyed by tier name (Retailer / Distributor / SuperDistributor /
    /// Client) and only the tiers this owner may set are read from it — a distributor's Retailer,
    /// a super-distributor's Distributor and Retailer. Anything else in the map is ignored rather
    /// than refused, so an older client sending all four cannot write a column it has no business
    /// in, and the OWNER'S OWN column is never read from here at all: it is computed as the
    /// parent's allowance minus what they just gave away.
    ///
    /// The amount range IS accepted now — a reseller may split one of its parent's bands into
    /// several of its own. What bounds it is the ceiling check, which compares the two cards as
    /// money across the amount range rather than row against row. GST stays absent: the tax
    /// treatment of a charge follows the parent band and is not the reseller's to re-decide.
    /// </summary>
    public class UpdateDelegatedRowRequest
    {
        public int SlabDetailID { get; set; }

        /// <summary>Null = leave the band where it is.</summary>
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }

        public Dictionary<string, DelegatedTierInput> Tiers { get; set; } = new();
    }

    /// <summary>Add a new band to a card I own.</summary>
    public class CreateDelegatedRowRequest
    {
        public int SlabMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public Dictionary<string, DelegatedTierInput> Tiers { get; set; } = new();
    }

    /// <summary>Remove a band from a card I own.</summary>
    public class DeleteDelegatedRowRequest
    {
        public int SlabDetailID { get; set; }
    }

    /// <summary>Hand one of my cards to one of my downline users.</summary>
    public class AssignDelegatedSlabRequest
    {
        public int SlabMasterID { get; set; }
        public int UserID { get; set; }
    }

    /// <summary>Pull every out-of-limit band on one of my cards back inside the parent's limits.</summary>
    /// <summary>Remove one of my own cards altogether.</summary>
    public class DeleteDelegatedSlabRequest
    {
        public int SlabMasterID { get; set; }
    }

    public class FixToParentRequest
    {
        public int SlabMasterID { get; set; }
    }

    public class SlabDelegationViewRequest
    {
        public int SlabMasterID { get; set; }
    }

    public class SlabAuditRequest
    {
        public int SlabMasterID { get; set; }
        public int ServiceID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
