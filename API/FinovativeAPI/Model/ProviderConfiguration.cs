using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ProviderConfiguration : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigurationID { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public ProviderMaster ProviderMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string OpCode { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Lapu { get; set; }

        [Column(TypeName = "varchar(3000)")]
        public string Quantity { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminComm { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string AdminCommType { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CommissionCategory { get; set; }

        public int PendingCount { get; set; }

        public int PendingInMinute { get; set; }

        public int FailCount { get; set; }

        public int FailInMinute { get; set; }
    }
}