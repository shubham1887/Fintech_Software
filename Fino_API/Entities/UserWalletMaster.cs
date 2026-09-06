using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserWalletMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserWalletMasterID { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public virtual WalletMaster? WalletMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }

        /// <summary>Portion of the wallet balance that is on hold / frozen (not spendable).</summary>
        [Column(TypeName = "decimal(18,5)")]
        public decimal HoldBalance { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? BalanceHash { get; set; }
    }
}
