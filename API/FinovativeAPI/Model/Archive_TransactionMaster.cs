using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Model
{
    public class Archive_TransactionMaster : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArchiveTransactionMasterID { get; set; }

        //[Index(IsUnique = true)]
        public int TransactionMasterID { get; set; }

        public int UserID { get; set; }

        public int ServiceID { get; set; }

        public int OperatorID { get; set; }

        public int InboxID { get; set; }

        public int WalletID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ConsumerNumber { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Description { get; set; }

        public int ProviderID { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderOpeningBalance { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ProviderCommission { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProviderClosingBalance { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string ProviderRequestID { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string ReferenceNo { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string APIUserRequestID { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string ProviderDOCNo { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string ProviderRefNo { get; set; } //this is used for Status check

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerTDS { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerGST { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerGST { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string Bill_CustomerMessageMobileNo { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal CashBack { get; set; }

        public int HitBackCount { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string LapuNo { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional1 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional2 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional3 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional4 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional5 { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Bill_Optional6 { get; set; }

        // *** dmt *** start ****

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderID { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_SenderNumber { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_BeneficiaryID { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string DMT_BeneficiaryName { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_Bank { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_IFSC { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string DMT_BeneficaryNameByBank { get; set; }

        // neft and imps
        [Column(TypeName = "varchar(100)")]
        public string DMT_PaymentType { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_BillNumber { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DMT_OriginalAmount { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string DMT_StatusCode { get; set; }

        // *** dmt *** end ****

        public bool IsDispute { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Location { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string SimCardDetails { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Through { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string Response { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Circle { get; set; }

        public int DistributerUserID { get; set; }

        public int SuperDistributerUserID { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ComplaintStatus { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminCommission { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string CardType { get; set; }

        public int StateID { get; set; }

        [DefaultValue(false)]
        public bool IsROffer { get; set; }

        [DefaultValue(true)]
        public bool IsOnlineDMT { get; set; }

        [DefaultValue(false)]
        public bool IsProviderLowBalance { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string ChangeStatus { get; set; }

        public DateTime ChangeStatusDOC { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AEPSResponseJSON { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PendingProviderStatus { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal AdminProfit { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string ResponseType { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal ROfferCommission { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal RetailerCommissionHold { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal DistributerCommissionHold { get; set; }

        [Column(TypeName = "decimal(18,5)")]
        public decimal SuperDistributerCommissionHold { get; set; }

        [DefaultValue(false)]
        public bool SettlementAtProviderEnd { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string BBPS_CustomerName { get; set; }
    }
}