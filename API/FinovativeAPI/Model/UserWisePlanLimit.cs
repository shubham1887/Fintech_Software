using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserWisePlanLimit : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserWisePlanLimitID { get; set; }

        public int UserID { get; set; }

        public int UserTypeID { get; set; }

        public int CircleHitCount { get; set; }

        public int PlanHitCount { get; set; }

        public int BillFetchHitCount { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }

    }
}
