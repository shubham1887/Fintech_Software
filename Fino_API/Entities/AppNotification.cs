using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One alert, stored once. Who it reached is <see cref="AppNotificationRecipient"/>.
    ///
    /// Before this existed an "App Alert" was fire-and-forget FCM: nothing was written down, so
    /// there was no bell feed to read it in, no way to tell whether it was delivered, and no
    /// record of who it went to. The push is now the *secondary* channel — the row is the alert.
    /// </summary>
    public class AppNotification : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppNotificationID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(200)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(1000)")]
        public string Body { get; set; } = string.Empty;

        /// <summary>Optional banner shown in the feed and in the expanded push.</summary>
        [Column(TypeName = "varchar(500)")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>Where tapping it goes — an in-app path such as "/s/TicketReport".</summary>
        [Column(TypeName = "varchar(300)")]
        public string ActionUrl { get; set; } = string.Empty;

        /// <summary>NotificationKind: Info | Alert | Promo | Ticket | Fund | Txn | System.</summary>
        [Column(TypeName = "varchar(30)")]
        public string Kind { get; set; } = "Info";

        /// <summary>NotificationAudience: All | Role | Users | Downline.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Audience { get; set; } = "All";

        /// <summary>Comma-separated user-type ids when Audience = Role (e.g. "3,4").</summary>
        [Column(TypeName = "varchar(100)")]
        public string AudienceRoles { get; set; } = string.Empty;

        /// <summary>Who sent it. 0 for alerts the platform raises itself.</summary>
        public int CreatedByUserID { get; set; }

        /// <summary>How many recipient rows were written — the honest "sent to N users".</summary>
        public int RecipientCount { get; set; }

        /// <summary>How many of those had at least one live device the push actually went to.</summary>
        public int PushedCount { get; set; }

        /// <summary>
        /// When this alert stops appearing in the bell. Null = it stays until it is deleted.
        ///
        /// Alerts are overwhelmingly time-bound — "server maintenance tonight", "cashback till
        /// Sunday" — and nothing retired them, so a user's feed kept filling with notices that
        /// had stopped being true. The row is kept (the sent-alert ledger still reports what went
        /// out, to how many, and how much of it was read); only the reading of it stops.
        /// </summary>
        public DateTime? ExpiresOn { get; set; }
    }
}
