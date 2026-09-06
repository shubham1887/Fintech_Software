using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class OfflineProductUIControlMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OfflineProductUIControlMappingID { get; set; }

        public int OfflineServiceMasterID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int OfflineProductMasterID { get; set; }

        [ForeignKey("OfflineProductMasterID")]
        public OfflineProductMaster OfflineProductMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Label { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ControlType { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required]
        public int MinLength { get; set; }

        [Required]
        public int MaxLength { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ErrorMessage { get; set; }

        [Required]
        public bool IsRequired { get; set; }
    }
}