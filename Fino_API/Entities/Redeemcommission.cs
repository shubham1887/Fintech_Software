using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class Redeemcommission : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RedeemcommissionID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int WalletID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string AdminRemark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string LastYearMonth { get; set; } = string.Empty;
    }
}
