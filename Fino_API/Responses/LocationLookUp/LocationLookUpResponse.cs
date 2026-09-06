namespace FintechApp.Responses.LocationLookUp
{
    public class LocationLookUpResponse
    {
    public int LocationLookUpID { get; set; }
    public int TableID { get; set; }
    public string SimCardDetails { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    }
}
