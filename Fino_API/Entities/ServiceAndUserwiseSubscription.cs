using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ServiceAndUserwiseSubscription : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceAndUserwiseSubscriptionID { get; set; }

        public int UserTypeID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string SubscriptionType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,5)")]
        public decimal OriginalCharge { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        public DateTime EndDate { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Remark { get; set; } = string.Empty;

        [Column(TypeName = "varchar(max)")]
        public string LoginID { get; set; } = string.Empty;

        public bool IsAutoDebit { get; set; }
        public int ServiceSubscriptionPlanMasterID { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Status { get; set; } = string.Empty;

        public int DaysCount { get; set; }
    }
}
