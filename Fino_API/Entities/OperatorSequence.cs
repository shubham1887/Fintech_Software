using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintechApp.Entities
{
    public class OperatorSequence : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OperatorSequenceID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int Count { get; set; }
        public virtual ServiceMaster? ServiceMaster { get; set; }
        public virtual OperatorMaster? OperatorMaster { get; set; }
        public virtual ProviderMaster? ProviderMaster { get; set; }
    }
}
