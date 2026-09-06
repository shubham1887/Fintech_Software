using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserCreationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserCreationMasterID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Required]
        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        public int OpeningIdCount { get; set; }
        public int PurchaseCount { get; set; }
        public int ClosingIdCount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; }

        public int CreatedUserID { get; set; }
    }
}