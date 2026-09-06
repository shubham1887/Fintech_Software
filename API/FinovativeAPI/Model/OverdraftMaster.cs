using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OverdraftMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OverdraftMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Overdraft { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal MaxOverdraft { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster Wallet { get; set; }

        [DefaultValue(false)]
        public bool IsActive { get; set; }
    }
}