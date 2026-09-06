using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserCreationMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserCreationMasterID { get; set; }

        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public int WalletID { get; set; }
        public int OpeningIdCount { get; set; }
        public int PurchaseCount { get; set; }
        public int ClosingIdCount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Type { get; set; }

        public int CreatedUserID { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
        public virtual ServiceMaster? ServiceMaster { get; set; }
        public virtual WalletMaster? WalletMaster { get; set; }
    }
}
