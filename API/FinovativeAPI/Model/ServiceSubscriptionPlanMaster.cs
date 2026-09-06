using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ServiceSubscriptionPlanMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceSubscriptionPlanMasterID { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string PlanName { get; set; }
    }
}
