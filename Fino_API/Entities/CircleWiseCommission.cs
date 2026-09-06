using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class CircleWiseCommission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircleWiseCommissionID { get; set; }

        public int SlabMasterID { get; set; }
        public int OperatorID { get; set; }
        public int StateID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? RetailerCommType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? RetailerCommissionCategory { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributorComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? DistributorCommType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? DistributorCommissionCategory { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributorComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? SuperDistributorCommType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? SuperDistributorCommissionCategory { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClientComm { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? ClientCommType { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? ClientCommissionCategory { get; set; }

        public virtual SlabMaster? SlabMaster { get; set; }
        public virtual OperatorMaster? OperatorMaster { get; set; }
    }
}
