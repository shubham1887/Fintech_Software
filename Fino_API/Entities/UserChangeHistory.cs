using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One recorded change to a user account: what changed, from what to what, who did it, why,
    /// and from where.
    ///
    /// The system already had three half-answers to this question — UserActionHistory (no "who
    /// did it"), LastChangeUserDetails (onboarding drafts only, no actor, no time) and the
    /// per-feature logs (WalletHoldHistory, PasswordChangeHistory) — so "who deactivated this
    /// retailer, and on whose instruction" was not answerable for any change made from the Users
    /// screen. This table is the single append-only answer for every change the panel makes to a
    /// user record: edit, activate, deactivate, delete, restore, re-parent, commission hold,
    /// channel toggles, slab and service changes.
    ///
    /// Rows are never updated or deleted — a correction is its own row, so the history reads as a
    /// ledger. One change to several fields writes one row per field, which is what makes the
    /// report answer "what exactly changed" rather than "something changed".
    /// </summary>
    public class UserChangeHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserChangeHistoryID { get; set; }

        /// <summary>The user the change was made TO.</summary>
        public int UserID { get; set; }

        /// <summary>The user who MADE the change (admin, support, or the upline who owns them).</summary>
        public int ChangedByUserID { get; set; }

        /// <summary>
        /// What kind of change: CREATE, UPDATE, ACTIVATE, DEACTIVATE, DELETE, RESTORE, TRANSFER,
        /// COMMISSION_HOLD, COMMISSION_RELEASE, APP_ACCESS, WEB_ACCESS, INVOICE, SLAB, SERVICE,
        /// PASSWORD_RESET, BULK_* . Kept as text (not an enum) so a new change site does not need
        /// a schema patch.
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(40)")]
        public string Action { get; set; } = string.Empty;

        /// <summary>The field or aspect that changed — "Mobile Number", "Parent", "Status".</summary>
        [Column(TypeName = "varchar(100)")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>Value before the change, rendered for reading (never a raw id on its own).</summary>
        [Column(TypeName = "varchar(1000)")]
        public string OldValue { get; set; } = string.Empty;

        /// <summary>Value after the change.</summary>
        [Column(TypeName = "varchar(1000)")]
        public string NewValue { get; set; } = string.Empty;

        /// <summary>Why — the operator's remark. The column the report exists for.</summary>
        [Column(TypeName = "varchar(500)")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Where the change was made from, as the session reported it.</summary>
        [Column(TypeName = "varchar(64)")]
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>Web / Android / API — the channel the change came through.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Platform { get; set; } = string.Empty;
    }
}
