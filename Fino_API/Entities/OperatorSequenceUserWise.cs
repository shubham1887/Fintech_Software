using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorSequenceUserWise : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorSequenceUserWiseID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual UserMaster? UserMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public virtual ServiceMaster? ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public virtual OperatorMaster? OperatorMaster { get; set; }

        public int ProviderID { get; set; }

        [ForeignKey("ProviderID")]
        public virtual ProviderMaster? ProviderMaster { get; set; }
    }
}
