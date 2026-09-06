using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ProviderOperatorMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderOperatorID { get; set; }

        public int ProviderID { get; set; }

        public int OperatorID { get; set; }

        public int ServiceID { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string? Operatorcode { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? MarginType { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal CommissionAdmin { get; set; }

        public int PendingCount { get; set; }

        public int FailureCount { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}
