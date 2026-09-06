namespace FintechApp.Responses.ExternalMedia
{
    public class ExternalMediaResponse
    {
    public int ExternalMediaID { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
