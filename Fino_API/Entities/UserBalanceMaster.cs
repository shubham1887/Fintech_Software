using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserBalanceMaster :BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBalanceMasterID { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster? WalletMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster? UserMaster { get; set; }


    }
}
