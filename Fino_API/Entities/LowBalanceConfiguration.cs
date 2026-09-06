using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class LowBalanceConfiguration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LowBalanceConfigurationID { get; set; }

        public int UserID { get; set; }
        public int WalletID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string? MobileNumber { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? EmailID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string? WebHookURL { get; set; }

        public bool IsActive { get; set; }

        public virtual UserMaster? UserMaster { get; set; }
        public virtual WalletMaster? WalletMaster { get; set; }
    }
}
