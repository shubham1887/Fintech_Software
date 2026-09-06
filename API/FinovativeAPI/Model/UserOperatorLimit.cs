using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserOperatorLimit : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserOperatorLimitID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [DefaultValue(-1)]
        [Column(TypeName = "decimal(18,5)")]
        public decimal LimitAmount { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}