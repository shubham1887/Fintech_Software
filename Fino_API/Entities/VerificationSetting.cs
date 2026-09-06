namespace FintechApp.Entities
{
    public class VerificationSetting
    {
        public int VerificationSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string CallBackURL { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string AuthCode { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
