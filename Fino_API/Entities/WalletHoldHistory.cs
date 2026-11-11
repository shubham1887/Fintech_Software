using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    /// <summary>
    /// One change to a wallet's held (frozen) balance.
    ///
    /// A hold takes money a user can see but cannot spend, so "who froze it, when, on which wallet
    /// and why" has to be answerable afterwards. Until this table existed the hold was applied as a
    /// bare number on UserWalletMaster and the remark the admin typed was thrown away — leaving
    /// nobody able to explain a frozen balance, or release it with any confidence.
    ///
    /// Rows are append-only: a release is its own row with a negative <see cref="Amount"/>, so the
    /// history reads as a ledger rather than being overwritten.
    /// </summary>
    public class WalletHoldHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WalletHoldHistoryID { get; set; }

        /// <summary>The user whose wallet was held.</summary>
        public int UserID { get; set; }

        /// <summary>Which wallet — a hold is always against one wallet, never the account as a whole.</summary>
        public int WalletID { get; set; }

        /// <summary>Positive = placed, negative = released.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        /// <summary>The wallet's total held balance after this change.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal HoldBalanceAfter { get; set; }

        /// <summary>The wallet's spendable balance at the time, for context when reading it back.</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal WalletBalance { get; set; }

        /// <summary>Who placed or released it.</summary>
        public int ActionByUserID { get; set; }

        /// <summary>HOLD or RELEASE.</summary>
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Action { get; set; } = string.Empty;

        /// <summary>Why — the question this table exists to answer.</summary>
        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; } = string.Empty;
    }
}
