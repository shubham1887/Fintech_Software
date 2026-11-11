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
        /// <summary>
        /// A reversal row's reading in words: "Request" (raised, awaiting a decision) or
        /// "Reversal" (money actually pulled back). The Fund Reverse Report carries both kinds,
        /// and Status alone does not separate them — an approved request and a direct reverse are
        /// both SUCCESS — so the report needs the distinction named rather than inferred from the
        /// raw Type / Through tokens. Empty for rows that are not reversal-shaped.
        /// </summary>
        public string ReverseKind { get; set; } = string.Empty;
        public decimal CashCollectionAmount { get; set; }
        public string CashCollectionRemark { get; set; } = string.Empty;
        public int RequestToID { get; set; }
        /// <summary>
        /// Who has to decide this request, by name. Resolved rather than stored: a row whose
        /// RequestToID was never set is attributed to the raiser's parent (else the admin), the
        /// same way the request flow would have routed it. Lets the queue show whose decision each
        /// row is instead of leaving "why am I seeing this?" unanswered.
        /// </summary>
        public string RequestToName { get; set; } = string.Empty;
        public int RequestAcceptedUserID { get; set; }
        public DateTime DOC { get; set; }
    }
}
