using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserSubscriptionPlanMapping : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserSubscriptionPlanMappingID { get; set; }

        public int UserID { get; set; }

        public int ServiceSubscriptionCreatePlanMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string PlanName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }
  
        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }
    }
}