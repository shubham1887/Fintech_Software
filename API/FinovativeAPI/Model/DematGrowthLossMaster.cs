using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Project.Model
{
    public class DematGrowthLossMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DematGrowthLossMasterID { get; set; }
        public int UserID { get; set; }        
  
        public int ServiceSubscriptionPlanMasterID { get; set; }

        public int ServiceID { get; set; }
        [ForeignKey("ServiceID")]
        public ServiceMaster? ServiceMaster { get; set; }
         
        [Column(TypeName = "varchar(100)")]
        public string? GrowthLoss { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        public int APIProviderId { get; set; }
    
        public bool IsDelete { get; set; }
        public DateTime ManualDate { get; set; }
        public int ManualDOCInt { get; set; }

    }
}