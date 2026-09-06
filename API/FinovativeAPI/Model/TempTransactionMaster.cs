using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TempTransactionMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionMasterID { get; set; }

        public int UserID { get; set; }

        public int ServiceID { get; set; }

        public int OperatorID { get; set; }
    }
}