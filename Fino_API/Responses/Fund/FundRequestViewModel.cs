namespace FintechApp.Responses.Fund
{
    public class FundRequestViewModel
    {
        public int FundTransactionID { get; set; }
        public int SenderID { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public int ReceiverID { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverMobile { get; set; } = string.Empty;
        public int WalletID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal SenderOpeningBalance { get; set; }
        public decimal SenderClosingBalance { get; set; }
        public decimal ReceiverOpeningBalance { get; set; }
        public decimal ReceiverClosingBalance { get; set; }
        public string Remark { get; set; } = string.Empty;
        public string UTRNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string PayMode { get; set; } = string.Empty;
        public int? ModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public int? SubModeID { get; set; }
        public string SubModeName { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string ReceiptPath { get; set; } = string.Empty;
        public DateTime TransactionDOC { get; set; }
        public int TransactionDOCInt { get; set; }
        public string DOCType { get; set; } = string.Empty;
        public decimal CashCollectionAmount { get; set; }
        public string CashCollectionRemark { get; set; } = string.Empty;
        public int RequestToID { get; set; }
        public int RequestAcceptedUserID { get; set; }
        public DateTime DOC { get; set; }
    }
}
