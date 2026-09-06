using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserBalanceCapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBalanceCappingID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Required]
        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }
    }
}