using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class TransactionDetails : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionDetailID { get; set; }

        public int UserID { get; set; }
        public int ReferenceID { get; set; }
        public int AssignedUserID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public int WalletID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string DOCType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal TransactionAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal GST { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal TDS { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AmountCreditDebit { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string CommissionCreditDebit { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string TransactionDescription { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string ConsumerNumber { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string PayMode { get; set; }
    }
}