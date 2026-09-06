namespace FintechApp.Requests.Fund
{
    public class FundUpdateRequest
    {
        public int FundTransactionID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SenderRemark { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal CashCollectionAmount { get; set; }
        public string CashCollectionRemark { get; set; } = string.Empty;
        // Optional: the approver may correct a Mode / Sub-Mode the requester picked by mistake.
        // 0 / null = leave unchanged.
        public int? ModeID { get; set; }
        public int? SubModeID { get; set; }
    }
}
