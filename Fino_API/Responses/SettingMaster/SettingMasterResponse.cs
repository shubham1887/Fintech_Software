namespace FintechApp.Responses.SettingMaster
{
    public class SettingMasterResponse
    {
        public int SettingMasterID { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsOn { get; set; }
        public bool IsSecure { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPassword { get; set; }
        public DateTime DateTime { get; set; }
    }
}
