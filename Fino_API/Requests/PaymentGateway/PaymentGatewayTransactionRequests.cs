namespace FintechApp.Requests.PaymentGateway
{
    public class SelectPGByOrderRequest
    {
        public string OrderID { get; set; } = string.Empty;
    }

    public class SelectPGReportRequest
    {
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class UpdatePGStatusManuallyRequest
    {
        public int PaymentGatewayID { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
