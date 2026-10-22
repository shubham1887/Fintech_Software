using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// The posting outbox: a business event waiting to become a voucher.
    ///
    /// Why an outbox and not a direct post. A ledger posting must not fail silently and must not be
    /// made twice. A row here is written when the event happens; the engine turns it into a voucher
    /// in its own transaction and marks it Posted. If the posting fails — a locked period, a missing
    /// rule, a bad amount — the row is marked Failed with the reason and retried, so a posting that
    /// could not be made is a visible row with an error, never a number quietly left wrong.
    ///
    /// EventKey is the idempotency key: a unique index on it means the same event enqueued twice is
    /// one row, so a retry of the producer cannot double-post.
    /// </summary>
    public class AcctPostingOutbox : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctPostingOutboxID { get; set; }

        [Column(TypeName = "varchar(40)")]
        public string EventType { get; set; } = string.Empty;

        /// <summary>Unique per real-world event, e.g. "WALLET_TOPUP:FundTxn:48213". The control.</summary>
        [Column(TypeName = "varchar(100)")]
        public string EventKey { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        /// <summary>The event's amounts and references, as JSON. The posting rule's AmountField names
        /// a property in here.</summary>
        [Column(TypeName = "varchar(max)")]
        public string PayloadJson { get; set; } = string.Empty;

        [Column(TypeName = "varchar(300)")]
        public string Narration { get; set; } = string.Empty;

        /// <summary>Pending / Posted / Failed / Skipped.</summary>
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        public int AttemptCount { get; set; }

        public DateTime? LastAttemptOn { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string LastError { get; set; } = string.Empty;

        /// <summary>The voucher produced, once it posts.</summary>
        public int PostedVoucherID { get; set; }
    }

    /// <summary>
    /// A closed accounting period. While a period is locked no voucher may post into it — a late
    /// correction has to go into an open period as an adjustment, so a filed month cannot change
    /// after the fact.
    /// </summary>
    public class AcctPeriodLock : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcctPeriodLockID { get; set; }

        [Column(TypeName = "varchar(8)")]
        public string PeriodKey { get; set; } = string.Empty;

        public DateTime LockedOn { get; set; }

        public int LockedByUserID { get; set; }

        [Column(TypeName = "varchar(300)")]
        public string Note { get; set; } = string.Empty;
    }
}
