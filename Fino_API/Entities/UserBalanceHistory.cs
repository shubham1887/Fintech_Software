using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserBalanceHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBalanceHistoryID { get; set; }

        public int UserID { get; set; }

        public int WalletID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Balance { get; set; }
    }
}
