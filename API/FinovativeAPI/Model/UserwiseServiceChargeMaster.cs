using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class UserwiseServiceChargeMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserwiseServiceChargeMasterID { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public UserMaster UserMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OneTime { get; set; }

        [DefaultValue(false)]
        public bool? IsDelete { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
    }
}