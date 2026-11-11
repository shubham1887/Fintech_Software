using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One admin decision about whether a menu entry is shown.
    ///
    /// The menu definition ships with the product and lists every screen the product has. Any
    /// given install uses a fraction of them: a network that does not sell AEPS has no use for
    /// four AEPS reports, and the operator reading a thirty-row Reports group to find the two
    /// they use is the whole complaint. Deleting rows from the definition is not the answer —
    /// the next install needs them, and so does this one the day it switches that service on.
    ///
    /// So visibility becomes data. The table holds EXCEPTIONS only: no row means the entry
    /// behaves exactly as the shipped definition says, which is what makes this safe to add to a
    /// running install — a fresh database changes nothing until an admin hides something.
    ///
    /// This is presentation, not permission. Hiding an entry takes it off the menu; it does not
    /// revoke access to the endpoint behind it, which is <c>ApiAccessPolicy</c>'s job and stays
    /// its job. A screen reachable by a bookmark is still reachable, and that is deliberate: a
    /// menu toggle that silently became a security control would be a security control nobody
    /// could audit.
    /// </summary>
    public class MenuVisibilitySetting : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuVisibilitySettingID { get; set; }

        /// <summary>
        /// What this row is about: a leaf's URL (lower-cased, e.g. <c>/aepsreport</c>), or
        /// <c>group:reports</c> for a whole group, which has no URL of its own.
        ///
        /// The URL and not the display name, because an admin may rename a menu entry and the
        /// decision "we do not use this screen" should survive that.
        /// </summary>
        [Column(TypeName = "varchar(200)")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>The label as it stood when the decision was made — for the screen and the audit
        /// trail, never for matching.</summary>
        [Column(TypeName = "varchar(150)")]
        public string MenuName { get; set; } = string.Empty;

        /// <summary>
        /// Which role this applies to; <c>0</c> means every role.
        ///
        /// Both are needed. "We do not use subscriptions" is an install-wide fact and should be
        /// said once; "retailers should not see the onboarding report" is about one role. A
        /// role-specific row wins over the all-roles row, so an entry can be hidden everywhere and
        /// kept for admin.
        /// </summary>
        public int UserTypeID { get; set; }

        /// <summary>
        /// False hides the entry. A row with True is an explicit "show this", which exists so a
        /// role can be carved out of an all-roles hide without deleting the broader decision.
        /// </summary>
        public bool IsVisible { get; set; }

        public int UpdatedByUserID { get; set; }
    }
}
