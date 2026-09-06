using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class ServiceSubscriptionPlanMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceSubscriptionPlanMasterID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string PlanName { get; set; } = string.Empty;
    }
}
