using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TargetSaleCommission : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TargetSaleCommissionID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        public int ParentID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SaleAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal CommissionAmount { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string SaleMonthDOC { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }
    }
}