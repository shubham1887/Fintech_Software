namespace FintechApp.Responses.LapuPartyMaster
{
    public class LapuPartyMasterResponse
    {
    public int LapuPartyMasterID { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EmailID { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
