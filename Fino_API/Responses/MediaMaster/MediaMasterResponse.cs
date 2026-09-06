namespace FintechApp.Responses.MediaMaster
{
    public class MediaMasterResponse
    {
    public int MediaMasterID { get; set; }
    public int UserID { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string MediaPath { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
