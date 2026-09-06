using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ROfferCommission : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ROfferCommissionID { get; set; }

        public int SlabMasterID { get; set; }

        [ForeignKey("SlabMasterID")]
        public virtual SlabMaster? SlabMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string RetailerCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string RetailerCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributorComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string DistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string DistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string SuperDistributorCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string SuperDistributorCommissionCategory { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClientComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ClientCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(500)")]
        public string ClientCommissionCategory { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}
