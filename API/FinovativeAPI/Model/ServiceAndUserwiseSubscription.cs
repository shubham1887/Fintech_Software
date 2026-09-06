using Newtonsoft.Json;
using Project.Processing;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
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
        public string SubscriptionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OriginalCharge { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        public DateTime EndDate { get; set; }


        private string _Remark;

        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }


        private string _LoginID;
 
        [Column(TypeName = "varchar(max)")]
        public string LoginID
        {
            get => _LoginID.TrimIfNotNull();
            set => _LoginID = value.TrimIfNotNull();
        }

        [DefaultValue(true)]
        public bool IsAutoDebit { get; set; }

        public int ServiceSubscriptionPlanMasterID { get; set; }

      
        [Column(TypeName = "varchar(max)")]
        public string Status { get; set; }

        public int DaysCount { get; set; }

    }
}
