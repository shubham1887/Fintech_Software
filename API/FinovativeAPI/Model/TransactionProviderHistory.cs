using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TransactionProviderHistory : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RechargeProviderHistoryID { get; set; }

        public int ProviderID { get; set; }

        public int OperatorID { get; set; }
        public int TransactionMasterID { get; set; }
        public string ProviderRequestID { get; set; }
    }
}