namespace FintechApp.Entities
{
    public class LeadGenerationMaster
    {
        public int LeadGenerationMasterID { get; set; }
        public int UserID { get; set; }
        public string FormID { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string PanNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public DateTime DOB { get; set; } = DateTime.MinValue;
    }
}
