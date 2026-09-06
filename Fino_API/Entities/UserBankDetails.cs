using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class UserBankDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserBankDetailsID { get; set; }
        public int UserID { get; set; }
        [Column(TypeName = "varchar(500)")] public string BankName { get; set; } = string.Empty;
        [Column(TypeName = "varchar(50)")] public string AccountNumber { get; set; } = string.Empty;
        [Column(TypeName = "varchar(50)")] public string IFSC { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        [Column(TypeName = "varchar(200)")] public string HolderName { get; set; } = string.Empty;
        [Column(TypeName = "varchar(100)")] public string Branch { get; set; } = string.Empty;
        public string? QRCodeImageURL { get; set; }
        [Column(TypeName = "varchar(100)")] public string? UPINumber { get; set; }
        public virtual UserMaster? UserMaster { get; set; }
    }
}
