using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class VoucherMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoucherMasterID { get; set; }

        public int VoucherTypeID { get; set; }

        [ForeignKey("VoucherTypeID")]
        public virtual VoucherType? VoucherType { get; set; }

        public DateTime SelectDate { get; set; }

        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? CrGroup { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? CrName { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? DrGroup { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? DrName { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? Remark { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? Status { get; set; }

        public int SubmitedBy { get; set; }

        public int ApproveBy { get; set; }

        public int UserID { get; set; }

        public bool Is_Delete { get; set; }
    }
}
