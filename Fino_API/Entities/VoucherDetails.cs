using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class VoucherDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherDetailsID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        [Column(TypeName = "varchar(80)")]
        public string? Mode { get; set; }

        public decimal OutstandingOpeningBal { get; set; }

        public decimal Amount { get; set; }

        public decimal OutstandingClossingBal { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? Remark { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? Credit { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? Debit { get; set; }
    }
}
