using FintechApp.Requests.SubscriptionPlan;
using FintechApp.Responses.SubscriptionPlan;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Aggregate service for the subscription feature: plan management (a plan header
    /// (ServiceSubscriptionCreatePlanMaster) with the services it bundles
    /// (ServiceSubscriptionPlanDetails)) and the per-user subscription lifecycle
    /// (UserSubscription rows with a validity window, plus permanent-free exemption that
    /// cascades down the user hierarchy).
    /// </summary>
    public interface ISubscriptionPlanService
    {
        // ── Plan management ──
        Task<SavePlanResult> SavePlan(SavePlanRequest request);
        Task<List<SubscriptionPlanDetailResponse>> GetPlansWithServices();
        Task<SubscriptionPlanDetailResponse?> GetPlanWithServices(int planId);
        Task<string> DeletePlan(int planId);
        Task<List<int>> GetServiceIdsForPlan(int planId);
        Task<decimal> GetPlanAmount(int planId);
        /// <summary>Union of every service bundled in any plan — the "paid" services subject to locking.</summary>
        Task<List<int>> GetAllPlanServiceIds();

        // ── Subscription lifecycle ──
        /// <summary>Record a fresh subscription / renewal for a user against a plan (sets the expiry window).</summary>
        Task RecordSubscription(int userId, int planId);
        /// <summary>The user's currently-in-force (non-expired, active) subscription, or null.</summary>
        Task<Entities.UserSubscription?> GetActiveSubscription(int userId);
        /// <summary>The user, or any ancestor in the ParentID chain, is permanently free.</summary>
        Task<bool> IsEffectivelyExempt(int userId);
        /// <summary>The caller's subscription standing (drives the header counter, banner and gating).</summary>
        Task<MySubscriptionStatusResponse> GetMyStatus(int userId, bool enabled, int userTypeId);
        /// <summary>Admin: subscription standing for every user that can be required to subscribe.</summary>
        Task<List<UserSubscriptionStatusResponse>> GetUsersStatus();
        /// <summary>Admin: mark a user permanently free (or back into the requirement).</summary>
        Task<string> SetExempt(int userId, bool isExempt);

        /// <summary>The company/admin user id that subscription revenue is credited to (0 if none).</summary>
        Task<int> GetAdminUserId();
    }
}
