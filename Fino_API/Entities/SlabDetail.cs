using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class SlabDetail : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SlabDetailID { get; set; }

        public int SlabMasterID { get; set; }

        [ForeignKey("SlabMasterID")]
        public SlabMaster SlabMaster { get; set; } = null!;

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } = null!;

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RetailerCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RetailerCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string DistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string SuperDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string SuperDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClientComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ClientCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string ClientCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealRetailerComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealRetailerCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealRetailerCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealSuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealSuperDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealSuperDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal RealClientComm { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string RealClientCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string RealClientCommissionCategory { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
