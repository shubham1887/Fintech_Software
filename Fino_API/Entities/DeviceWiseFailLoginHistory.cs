namespace FintechApp.Entities
{
    public class DeviceWiseFailLoginHistory
    {
        public int DeviceWiseFailLoginHistoryID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DeviceID { get; set; } = string.Empty;
    }
}
