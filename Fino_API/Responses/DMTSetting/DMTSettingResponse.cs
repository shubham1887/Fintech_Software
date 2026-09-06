namespace FintechApp.Responses.DMTSetting
{
    public class ActiveDMTListResponse
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = "DMT";
        public bool IsPayoutDMT { get; set; }
        public string ProviderCode { get; set; } = "DMT";
    }

    public class TransferModeResponse
    {
        public string TransferMode { get; set; } = string.Empty;
    }

    public class DMTSettingResponse
    {
        public int DMTSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActiveForWebsite { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
        public string WalletID { get; set; } = string.Empty;
        public string CallBackURL { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal SplitAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal SenderLimit { get; set; }
        public bool IsCCF { get; set; }
        public bool IsNeft { get; set; }
        public int Sequence { get; set; }
        public string Mode { get; set; } = string.Empty;
        public bool IsPayoutDMT { get; set; }
        public string ProviderCode { get; set; } = string.Empty;
        public bool IsExpressDMT { get; set; }
    }

    public class DMTSettingDropdownResponse
    {
        public int DMTSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
