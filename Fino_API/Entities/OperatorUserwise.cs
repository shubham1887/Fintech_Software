using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorUserwise : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorUserwiseID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public int UsageLimit { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string? BlockedAmount { get; set; }
    }
}
