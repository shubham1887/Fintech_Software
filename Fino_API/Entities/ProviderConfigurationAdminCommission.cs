using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderConfigurationAdminCommission : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderConfigurationAdminCommissionID { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public virtual ProviderMaster? ProviderMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminComm { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string AdminCommType { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string AdminCommissionCategory { get; set; } = string.Empty;
    }
}
