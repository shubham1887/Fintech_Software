namespace FintechApp.Responses.DematSetting {
    public class DematSettingResponse {
        public int DematSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string UTMSource { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Sequence { get; set; }
        public bool IsActiveWeb { get; set; }
        public bool IsActive { get; set; }
    }
}
