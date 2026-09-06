using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class ThreewayReconCompletion : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ThreewayReconCompletionID { get; set; }

        public int TransactionMasterID { get; set; }
        [ForeignKey("TransactionMasterID")]
        public TransactionMaster TransactionMaster { get; set; }
        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        public int OperatorID { get; set; }

        [ForeignKey("OperatorID")]
        public OperatorMaster OperatorMaster { get; set; }
    }
}
