namespace FintechApp.Responses.ComplaintMaster
{
    public class ComplaintMasterResponse
    {
        public int ComplaintMasterID { get; set; }
        public int UserID { get; set; }
        public int TransactionMasterID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CustomerRemark { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string ComplaintStatus { get; set; } = string.Empty;
        public int TransactionMasterDOCInt { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ComplaintType { get; set; }
        public string? NewConsumerNumber { get; set; }
        public int ProviderID { get; set; }
        public string? ProviderRequestID { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime DateTime { get; set; }

        // Display names so the complaint screen never has to show a raw id.
        public string UserName { get; set; } = string.Empty;
        public string UserMobile { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
    }
}
