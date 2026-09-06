namespace FintechApp.Responses.DMTSender
{
    public class SenderDetailsResponse
    {
        public string SenderGUID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Consumed { get; set; }
        public decimal RemainingLimit { get; set; }
        // Admin-configured display/per-transfer cap (SettingMaster key 'ShowLimitInDMT'). 0 ⇒ not set.
        public decimal DisplayLimit { get; set; }
    }

    public class SenderByAccountResponse
    {
        public string Name { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
    }
}
