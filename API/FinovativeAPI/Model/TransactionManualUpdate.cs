using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TransactionManualUpdate : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionManualUpdateID { get; set; }

        public int TransactionMasterID { get; set; }

        [ForeignKey("TransactionMasterID")]
        public TransactionMaster TransactionMaster { get; set; }

        public int ManualUpdatedBy { get; set; }

        [ForeignKey("ManualUpdatedBy")]
        public UserMaster UserMaster { get; set; }

        public string ProviderrefNo { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string OldStatus { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string NewStatus { get; set; }
    }
}