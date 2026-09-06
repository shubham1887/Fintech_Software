using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FintechApp.Helpers;

namespace FintechApp.Entities
{
    public class FundTransaction : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FundTransactionID { get; set; }

        [Required]
        public int SenderID { get; set; }
        [ForeignKey("SenderID")]
        public UserMaster Sender { get; set; }

        [Required]
        public int ReceiverID { get; set; }
        [ForeignKey("ReceiverID")]
        public UserMaster Receiver { get; set; }

        public int WalletID { get; set; }
        [ForeignKey("WalletID")]
        public WalletMaster WalletMaster { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal OriginalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal CashCollectionAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal SenderOpeningBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal SenderClosingBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ReceiverOpeningBalance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,5)")]
        public decimal ReceiverClosingBalance { get; set; }

        private string _Remark;
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Remark
        {
            get => _Remark.ToTitleCase();
            set => _Remark = value.ToTitleCase();
        }

        [Column(TypeName = "varchar(100)")]
        public string UTRNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Type { get; set; }

        private string _AdminRemark;
        [Column(TypeName = "varchar(100)")]
        public string AdminRemark
        {
            get => _AdminRemark.ToTitleCase();
            set => _AdminRemark = value.ToTitleCase();
        }

        [Column(TypeName = "varchar(100)")]
        public string PayMode { get; set; }

        // Fund-request Mode / SubMode (new master-driven method + source/destination). Nullable so
        // legacy rows and the free-text PayMode/BankName path keep working unchanged.
        public int? ModeID { get; set; }
        [ForeignKey("ModeID")]
        public Mode Mode { get; set; }

        public int? SubModeID { get; set; }
        [ForeignKey("SubModeID")]
        public SubMode SubMode { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Through { get; set; }

        public int RequestToID { get; set; }
        [ForeignKey("RequestToID")]
        public UserMaster RequestTo { get; set; }

        private string _CashCollectionRemark;
        [Column(TypeName = "varchar(100)")]
        public string CashCollectionRemark
        {
            get => _CashCollectionRemark.ToTitleCase();
            set => _CashCollectionRemark = value.ToTitleCase();
        }

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string BankName { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string ReceiptPath { get; set; }

        public DateTime TransactionDOC { get; set; }

        public int TransactionDOCInt { get; set; }

        [DefaultValue(false)]
        public bool IsDelete { get; set; }

        public int RequestAcceptedUserID { get; set; }
    }
}