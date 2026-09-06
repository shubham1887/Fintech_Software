namespace FintechApp.Responses.FingerPrintDeviceType
{
    public class FingerPrintDeviceTypeResponse
    {
    public int FingerPrintDeviceTypeID { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
