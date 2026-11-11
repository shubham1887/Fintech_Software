using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One push-capable device belonging to a user.
    ///
    /// Replaces the single AppFCMToken / WebFCMToken columns on UserMaster: a retailer signs in
    /// on a phone AND the browser, reinstalls the app (new token), or changes handset — a single
    /// column silently loses every device but the last one, which is why alerts "sent" from the
    /// admin panel arrived nowhere. Tokens are rows now, and a token Firebase reports as dead is
    /// deactivated rather than retried forever.
    /// </summary>
    public class UserDevice : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserDeviceID { get; set; }

        public int UserID { get; set; }

        /// <summary>android | web | ios — decides which FCM options the message carries.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Platform { get; set; } = string.Empty;

        /// <summary>The FCM registration token. Unique: one row per physical device.</summary>
        [Required]
        [Column(TypeName = "varchar(500)")]
        public string FcmToken { get; set; } = string.Empty;

        /// <summary>Manufacturer + model (android) or the browser name (web) — for the user's device list.</summary>
        [Column(TypeName = "nvarchar(200)")]
        public string DeviceName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string AppVersion { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        /// <summary>Last time the app refreshed this token — an idle device is not a dead one.</summary>
        public DateTime LastSeenDateTime { get; set; }

        /// <summary>Why the token was deactivated (Firebase UNREGISTERED, signed out, replaced).</summary>
        [Column(TypeName = "varchar(200)")]
        public string DeactivatedReason { get; set; } = string.Empty;
    }
}
