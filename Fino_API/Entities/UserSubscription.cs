using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One subscription purchase: a user subscribing to a plan for a validity window.
    /// Renewing writes a fresh row (the latest active, non-expired row wins). The audit
    /// trail is kept; expiry is driven by <see cref="ExpiryDate"/>.
    ///
    /// A row is written both when the user subscribes themselves (SubscriptionPlan/Subscribe,
    /// wallet charged) and when an admin grants a plan (SubscriptionPlan/AssignPlan) — the
    /// admin grant records WHO granted it, whether it was complimentary (no wallet movement)
    /// and why, so free plans are auditable.
    /// </summary>
    public class UserSubscription : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSubscriptionID { get; set; }

        public int UserID { get; set; }

        /// <summary>FK to ServiceSubscriptionCreatePlanMaster.</summary>
        public int PlanID { get; set; }

        /// <summary>What was actually charged for this window. 0 for a complimentary grant.</summary>
        public decimal Amount { get; set; }

        public DateTime StartDate { get; set; }

        /// <summary>End of the validity window. Far-future for lifetime (ValidityDays = 0) plans.</summary>
        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Admin who granted this window; 0 when the user subscribed themselves.</summary>
        public int AssignedByUserID { get; set; }

        /// <summary>Granted without charging the wallet (goodwill / migration / trial).</summary>
        public bool IsComplimentary { get; set; }

        /// <summary>Why the window was granted or revoked. Free text, shown in the audit trail.</summary>
        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;
    }
}
