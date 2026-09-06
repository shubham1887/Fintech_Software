using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OfflineProductMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OfflineProductMasterID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal PackageMRP { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal BookingAmount { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Validity { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string OpType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }

        public bool IsActive { get; set; }
    }
}