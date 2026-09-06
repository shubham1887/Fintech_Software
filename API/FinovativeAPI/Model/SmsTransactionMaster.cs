using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class SmsTransactionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SmsTransactionMasterID { get; set; }

        public int TransactionMasterID { get; set; }

        [ForeignKey("TransactionMasterID")]
        public TransactionMaster TransactionMaster { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string BillNumber { get; set; }

        public bool IsProcess { get; set; }
        public bool IsSuccess { get; set; }
    }
}