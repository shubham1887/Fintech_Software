using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class PaymentGateway : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentGatewayID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string OrderID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal GatewayCharge { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Commission { get; set; }

        [Required]
        public int WalletID { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string TransactionID { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string PayMode { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        public int PaymentProviderID { get; set; }

        [Required]
        public int IsUpdate { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Remark { get; set; }

        public int DistributerUserID { get; set; }
        public int SuperDistributerUserID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminGST { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ReferenceID { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string Sender { get; set; }

        public int ServiceID { get; set; }

        [ForeignKey("ServiceID")]
        public ServiceMaster ServiceMaster { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string PaymentStatus { get; set; }
    }
}