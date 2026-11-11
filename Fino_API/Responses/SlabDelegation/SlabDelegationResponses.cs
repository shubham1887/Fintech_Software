namespace FintechApp.Responses.SlabDelegation
{
    /// <summary>One card in the owner's list.</summary>
    public class DelegatedSlabSummary
    {
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public int? ParentSlabID { get; set; }
        public string ParentSlabName { get; set; } = string.Empty;
        public int? OwnerUserID { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int OwnerUserType { get; set; }
        public bool IsDelegatedSlab { get; set; }
        public bool IsActive { get; set; }

        /// <summary>ACTIVE, or INVALID when at least one band is outside the parent's limits.</summary>
        public string Status { get; set; } = string.Empty;

        public int BandCount { get; set; }

        /// <summary>How many bands are out of limit — the number the "needs attention" chip shows.</summary>
        public int InvalidBandCount { get; set; }

        /// <summary>How many users this card is currently assigned to.</summary>
        public int AssignedUserCount { get; set; }

        public DateTime DateTime { get; set; }
    }

    /// <summary>
    /// One band, with the owner's figures beside the ceiling each was measured against. The pair is
    /// returned together on purpose: an editor that shows "₹9" without showing "out of ₹11" makes
    /// the owner guess at their own headroom, and guessing is what the refusal messages are for.
    /// </summary>
    public class DelegatedSlabBand
    {
        public int SlabDetailID { get; set; }
        public int? ParentSlabDetailID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }

        public decimal RetailerComm { get; set; }
        public decimal DistributorComm { get; set; }
        public decimal SuperDistributorComm { get; set; }
        public decimal ClientComm { get; set; }

        /// <summary>Flexi / Fix, inherited from the parent and not editable here (Rule 3).</summary>
        public string RetailerCommType { get; set; } = string.Empty;
        public string DistributorCommType { get; set; } = string.Empty;
        public string SuperDistributorCommType { get; set; } = string.Empty;
        public string ClientCommType { get; set; } = string.Empty;

        /// <summary>Commission / Surcharge, inherited from the parent.</summary>
        public string RetailerCommissionCategory { get; set; } = string.Empty;
        public string DistributorCommissionCategory { get; set; } = string.Empty;
        public string SuperDistributorCommissionCategory { get; set; } = string.Empty;
        public string ClientCommissionCategory { get; set; } = string.Empty;

        /// <summary>The live parent figures — re-read, not the stored snapshot, so a parent edit
        /// shows up the next time the screen is opened.</summary>
        public decimal ParentRetailerComm { get; set; }
        public decimal ParentDistributorComm { get; set; }
        public decimal ParentSuperDistributorComm { get; set; }
        public decimal ParentClientComm { get; set; }

        /// <summary>
        /// What the owner themselves is paid at the two ends of this band — their own line on the
        /// card above, which is the whole net amount they have to divide up. NOT the parent band's
        /// total: the parent's other columns are the parent's own costs.
        ///
        /// Two figures rather than one because a percentage band pays differently at ₹100 and at
        /// ₹9,999, and a single number would be a half-truth at one end.
        ///
        /// Sent so the editor can show the owner what each rate they type leaves them, without the
        /// browser having to re-implement the payout arithmetic the server validates with. The
        /// server stays the authority; this is the preview.
        /// </summary>
        public decimal AllowanceAtFrom { get; set; }
        public decimal AllowanceAtTo { get; set; }

        /// <summary>
        /// What this band hands out at each end — every tier on it, since the owner's own column
        /// is held at zero — and what that leaves the owner. Derived, never stored: the margin is
        /// the gap between the credit the platform posts the owner and the debit for what they
        /// passed down, and the ledger is where it actually exists.
        /// </summary>
        public decimal GivenAwayAtFrom { get; set; }
        public decimal GivenAwayAtTo { get; set; }
        public decimal MarginAtFrom { get; set; }
        public decimal MarginAtTo { get; set; }

        public string GSTType { get; set; } = string.Empty;
        public decimal GSTRatePercent { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>Why this band is invalid, in the words the owner is shown. Empty when ACTIVE.</summary>
        public string StatusReason { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    /// <summary>A card with its bands — the payload behind both the read-only parent view and the editor.</summary>
    public class DelegatedSlabView
    {
        public DelegatedSlabSummary Slab { get; set; } = new();
        public List<DelegatedSlabBand> Bands { get; set; } = new();

        /// <summary>
        /// The tier column holding the signed-in owner's own margin — "Distributor" for a
        /// distributor, "SuperDistributor" for a super-distributor. Empty on the read-only parent
        /// view and for admin. They never type into it; the editor shows it as a computed figure
        /// so they can see what each rate they set leaves them.
        /// </summary>
        public string OwnTier { get; set; } = string.Empty;

        /// <summary>
        /// The tier columns this caller may actually SET, in display order — the tiers strictly
        /// below them. A distributor gets ["Retailer"], a super-distributor
        /// ["Distributor","Retailer"], admin all four.
        ///
        /// Served from the API rather than worked out in the browser so there is ONE answer to
        /// "which rates are mine to decide": the same list is what the save endpoint reads from,
        /// so a column the UI should not draw is also a column the server will not write.
        /// </summary>
        public List<string> SettableTiers { get; set; } = new();

        /// <summary>False on the parent card, which the owner may look at and nothing else.</summary>
        public bool IsEditable { get; set; }
    }

    /// <summary>Which of my downline users hold which of my cards.</summary>
    public class DelegatedAssignment
    {
        public int UserSlabMappingID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserType { get; set; } = string.Empty;
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }

    public class SlabAuditRow
    {
        public long SlabCommissionAuditID { get; set; }
        public int SlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public int SlabDetailID { get; set; }
        public int? OwnerUserID { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int? ParentUserID { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public int UpdatedByUserID { get; set; }
        public string UpdatedByName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public string Tier { get; set; } = string.Empty;
        public decimal OldCommission { get; set; }
        public decimal NewCommission { get; set; }
        public string OldCommType { get; set; } = string.Empty;
        public string NewCommType { get; set; } = string.Empty;
        public decimal ParentCommission { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }

    public class SlabAuditWrapper
    {
        public int TotalCount { get; set; }
        public List<SlabAuditRow> Rows { get; set; } = new();
    }
}
