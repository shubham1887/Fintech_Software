namespace FintechApp.Responses.ServiceWiseSliderSetting
{
    public class ServiceWiseSliderSettingResponse
    {
    public int ServiceWiseSliderSettingID { get; set; }
    public int ServiceID { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReferenceLink { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public bool IsDelete { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
