namespace FintechApp.Responses.ProviderMaster
{
    public class ProviderMasterResponse
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderUrl { get; set; } = string.Empty;
        public string ProviderUniqueID { get; set; } = string.Empty;

        /// <summary>Public id for this provider's inbound callback address:
        /// <c>&lt;api-base&gt;/Callback/p/{CallbackKey}</c>. Safe to display and to hand out.</summary>
        public string CallbackKey { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsHLR { get; set; }
        public decimal RechargeBalance { get; set; }
        public decimal DMTBalance { get; set; }
        public int ResponseGroupID { get; set; }
        public bool IsRealAPI { get; set; }
        public string LoginID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string SupportNumber { get; set; } = string.Empty;
        public int LowBalanceAmount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
