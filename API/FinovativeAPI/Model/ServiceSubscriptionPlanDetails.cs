using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ServiceSubscriptionPlanDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceSubscriptionPlanDetailsID { get; set; }

        public int ServiceSubscriptionCreatePlanMasterID { get; set; }

        [ForeignKey("ServiceSubscriptionCreatePlanMasterID")]
        public ServiceSubscriptionCreatePlanMaster ServiceSubscriptionCreatePlanMaster { get; set; }

        public int ServiceID { get; set; }
    }
}