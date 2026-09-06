using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ServiceSubscriptionCreatePlanMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceSubscriptionCreatePlanMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string PlanName { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(800)")]
        public string ImageURL { get; set; }

        [DefaultValue(false)]
        public bool IsDefault { get; set; }
    }
}