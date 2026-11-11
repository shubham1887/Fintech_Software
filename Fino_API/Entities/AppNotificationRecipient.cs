using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One user's copy of an alert: their unread flag, and whether a push left the building.
    /// This is what the bell reads, so an alert survives a phone that was switched off.
    /// </summary>
    public class AppNotificationRecipient : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppNotificationRecipientID { get; set; }

        public int AppNotificationID { get; set; }

        public int UserID { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDateTime { get; set; }

        /// <summary>True once FCM accepted a message for at least one of this user's devices.</summary>
        public bool PushSent { get; set; }

        public DateTime? PushDateTime { get; set; }

        [ForeignKey(nameof(AppNotificationID))]
        public AppNotification? AppNotification { get; set; }
    }
}
