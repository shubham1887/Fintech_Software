using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserPlanUtilization : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserPlanUtilizationID { get; set; }

        public int UserID { get; set; }

        public int CircleFetchCount { get; set; }

        public int PlanFetchCount { get; set; }

        public int BillFetchCount { get; set; }

        public int TotalPlanCount { get; set; }

        public int TotalCircleCount { get; set; }

        public int TotalBillFetchCount { get; set; }
    }
}
