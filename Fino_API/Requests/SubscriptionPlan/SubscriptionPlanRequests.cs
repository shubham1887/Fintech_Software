namespace FintechApp.Requests.SubscriptionPlan
{
    public class SelectByServiceIDRequest
    {
        public int ServiceID { get; set; }
    }

    // Create or update a subscription plan together with the list of services it bundles,
    // in a single call. PlanID = 0 creates a new plan; > 0 updates the existing one and
    // reconciles its service list (adds new links, soft-deletes removed ones).
    public class SavePlanRequest
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int ValidityDays { get; set; }
        public List<int> ServiceIDs { get; set; } = new();
    }

    // Admin: mark a user permanently free from (or back into) the subscription requirement.
    public class SetExemptRequest
    {
        public int UserID { get; set; }
        public bool IsExempt { get; set; }
    }

    // Look up a single plan (with its services) by id.
    public class GetPlanRequest
    {
        public int PlanID { get; set; }
    }

    // Subscribe a user to a plan. The plan amount is resolved server-side from the plan
    // (the client-supplied value is ignored) and charged from the user's Recharge wallet;
    // every service in the plan is then activated for the user.
    public class SubscribeRequest
    {
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
        public int UserID { get; set; } // admin may subscribe on behalf of a user; 0 = self
    }

    public class IsSubscribedRequest
    {
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }
    }

    // Admin: grant (or renew / extend) a plan for a user from the Assign Plan desk.
    // The plan price is resolved server-side; the client only chooses HOW it is settled:
    //   ChargeWallet = true  → the user's Recharge wallet is debited and the company wallet
    //                          credited, exactly as a self-subscribe would.
    //   ChargeWallet = false → complimentary grant, no wallet movement (audited as such).
    // ValidityDays = 0 keeps the plan's own validity; > 0 overrides it for this window only.
    public class AssignPlanRequest
    {
        public int UserID { get; set; }
        public int PlanID { get; set; }
        public bool ChargeWallet { get; set; }
        public int ValidityDays { get; set; }
        /// <summary>Renewing the SAME plan stacks onto the remaining days instead of discarding them.</summary>
        public bool ExtendCurrent { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    // Admin: end a user's current subscription window immediately.
    public class RevokeSubscriptionRequest
    {
        public int UserID { get; set; }
        public string Remark { get; set; } = string.Empty;
    }

    // Admin: one user's subscription standing + the full grant/renewal audit trail.
    public class UserSubscriptionRequest
    {
        public int UserID { get; set; }
    }

    // How a subscription window is written. Internal to the service layer: the self-subscribe
    // path passes nothing (plan defaults, charged, no grantor); the admin grant fills it in.
    public class RecordSubscriptionOptions
    {
        /// <summary>Override the plan's validity for this window only. 0 or less = use the plan's.</summary>
        public int ValidityDaysOverride { get; set; }
        /// <summary>What was actually charged. Null = the plan's own price.</summary>
        public decimal? Amount { get; set; }
        public bool IsComplimentary { get; set; }
        public int AssignedByUserID { get; set; }
        public string Remark { get; set; } = string.Empty;
        /// <summary>Stack onto the remaining days of the in-force window when it is the same plan.</summary>
        public bool ExtendFromCurrentExpiry { get; set; }
    }
}
