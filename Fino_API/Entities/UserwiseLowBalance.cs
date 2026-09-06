using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserwiseLowBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserwiseLowBalanceID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public virtual WalletMaster? WalletMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }
    }
}
