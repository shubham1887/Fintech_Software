namespace FintechApp.Responses.SubscriptionPlan
{
    // A subscription plan enriched with the services it bundles. Used by both the admin
    // management screen (edit) and the user-facing subscribe screen (IsSubscribed set there).
    public class SubscriptionPlanDetailResponse
    {
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int ValidityDays { get; set; }
        public List<int> ServiceIDs { get; set; } = new();
        public int ServiceCount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }

        // Populated only for the user-facing "available plans" view.
        public bool IsSubscribed { get; set; }
    }

    // Result of a SavePlan call.
    public class SavePlanResult
    {
        public int PlanID { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// The caller's subscription standing — drives the header day-counter, the renew banner
    /// and all client-side gating.
    /// </summary>
    public class MySubscriptionStatusResponse
    {
        /// <summary>Master Key Setting (IsSubscriptionEnabled). Off → the whole feature is hidden.</summary>
        public bool Enabled { get; set; }
        /// <summary>The caller (or an ancestor) is permanently free.</summary>
        public bool Exempt { get; set; }
        /// <summary>Enabled AND not exempt AND a role that must subscribe.</summary>
        public bool Required { get; set; }
        /// <summary>A non-expired subscription is in force.</summary>
        public bool Active { get; set; }
        public int PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        /// <summary>Whole days left until expiry (0 when expired / none).</summary>
        public int DaysRemaining { get; set; }
    }

    /// <summary>
    /// One row of the admin "who is subscribed / who is out" list, and the single-user
    /// standing the Assign Plan desk shows before an admin grants or revokes a plan.
    /// </summary>
    public class UserSubscriptionStatusResponse
    {
        public int UserID { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        /// <summary>The auto-generated user code (UserMaster.AliasName), e.g. RT0007.</summary>
        public string UserCode { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        /// <summary>The account itself is active. A deactivated account can still hold a plan.</summary>
        public bool IsUserActive { get; set; }
        public bool Exempt { get; set; }
        public bool Active { get; set; }
        public int PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
        /// <summary>The in-force window never expires (a 0-validity "lifetime" plan).</summary>
        public bool Lifetime { get; set; }
        /// <summary>The in-force window was granted free of charge by an admin.</summary>
        public bool IsComplimentary { get; set; }
    }

    /// <summary>
    /// One line of a user's subscription audit trail — every window ever granted, whether
    /// bought by the user or assigned by an admin, newest first.
    /// </summary>
    public class UserSubscriptionHistoryResponse
    {
        public int UserSubscriptionID { get; set; }
        public int PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        /// <summary>True only for the window currently in force (active and not expired).</summary>
        public bool InForce { get; set; }
        public bool IsComplimentary { get; set; }
        public bool Lifetime { get; set; }
        /// <summary>Admin who granted it; empty when the user subscribed themselves.</summary>
        public string AssignedBy { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }

    /// <summary>What an admin grant actually did — echoed back so the UI can confirm precisely.</summary>
    public class AssignPlanResult
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal AmountCharged { get; set; }
        public bool WalletCharged { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Lifetime { get; set; }
        public int ServicesActivated { get; set; }
    }
}
