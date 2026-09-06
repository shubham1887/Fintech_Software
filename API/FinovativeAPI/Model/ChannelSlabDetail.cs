using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ChannelSlabDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChannelSlabDetailID { get; set; }

        public int ChannelSlabMasterID { get; set; }

        [ForeignKey("ChannelSlabMasterID")]
        public ChannelSlabMaster ChannelSlabMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Commission { get; set; }

        [Column(TypeName = "varchar(100)")]
        [DefaultValue("Fix")]
        public string CommissionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        public int UplineUserID { get; set; }
        public bool IsDelete { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal RealCommission { get; set; }

        [Column(TypeName = "varchar(100)")]
        [DefaultValue("Fix")]
        public string RealCommissionType { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string GeneralCommissionCategory { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string RealCommissionCategory { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal UplineCommission { get; set; }

        public bool IsActive { get; set; }
    }
}