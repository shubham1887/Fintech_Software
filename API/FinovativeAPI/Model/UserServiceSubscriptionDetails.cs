using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserServiceSubscriptionDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserServiceSubscriptionDetailsID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }


        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int ServiceAndUserwiseSubscriptionID { get; set; }

        [ForeignKey("ServiceAndUserwiseSubscriptionID")]
        public ServiceAndUserwiseSubscription ServiceAndUserwiseSubscription { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

    }

}
