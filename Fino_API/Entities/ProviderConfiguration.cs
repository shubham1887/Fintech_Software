using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderConfiguration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigurationID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; } = null!;

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; } = null!;

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string OpCode { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string Lapu { get; set; } = string.Empty;

        [Column(TypeName = "varchar(3000)")]
        public string Quantity { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminComm { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string AdminCommType { get; set; } = string.Empty;

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "varchar(20)")]
        public string CommissionCategory { get; set; } = string.Empty;

        public int PendingCount { get; set; }

        public int PendingInMinute { get; set; }

        public int FailCount { get; set; }

        public int FailInMinute { get; set; }
    }
}
