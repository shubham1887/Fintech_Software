using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TransactionStatusChange: BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionStatusChangeID { get; set; }

        public int UserID { get; set; }

        public int TransactionMasterID { get; set; }
        [ForeignKey("TransactionMasterID")]
        public TransactionMaster TransactionMaster { get; set; }

        public int TransactionDOCInt { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string OldStatus { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string NewStatus { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Remark { get; set; }

    }
}
