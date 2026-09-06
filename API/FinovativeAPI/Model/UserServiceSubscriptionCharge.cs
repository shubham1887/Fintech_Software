using Project.Processing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Model
{
    public class UserServiceSubscriptionCharge : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserServiceSubscriptionChargeID { get; set; }

        [Required]
        public int UserID { get; set; }


        public int ServiceID { get; set; }


        public int ServiceAndUserwiseSubscriptionID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Charge { get; set; }

        private string _Remark;

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        public int Sequence { get; set; }


        [DefaultValue(false)]
        public bool IsAdminApproved { get; set; }
    }
}
