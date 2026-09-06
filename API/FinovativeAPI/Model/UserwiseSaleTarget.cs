using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserwiseSaleTarget : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserwiseSaleTargetID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int WalletID { get; set; }

        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        public int ParentID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Commission { get; set; }

        [Column(TypeName = "varchar(100)")]
        [DefaultValue("Fix")]
        public string CommissionType { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }
    }
}