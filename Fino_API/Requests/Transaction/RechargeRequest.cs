namespace FintechApp.Requests.Transaction
{
    public class RechargeRequest
    {
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string MobileNo { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string Param1 { get; set; } = string.Empty;
        public string Param2 { get; set; } = string.Empty;
        public string Param3 { get; set; } = string.Empty;
        public string BBPS_CustomerName { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
    }
}
