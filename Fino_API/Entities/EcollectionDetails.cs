using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class EcollectionDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EcollectionDetailsID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int WalletID { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string VirtualACCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string UTR { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string PayeeAccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Reason { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }
    }
}
