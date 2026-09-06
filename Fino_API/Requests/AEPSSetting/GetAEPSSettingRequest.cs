namespace FintechApp.Requests.AEPSSetting
{
    public class GetAEPSSettingRequest
    {
        public int AEPSSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
