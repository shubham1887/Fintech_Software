namespace FintechApp.Responses.EmailSetting
{
    public class EmailSettingResponse
    {
        public int EmailSettingMasterID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool IsSSL { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }
}
