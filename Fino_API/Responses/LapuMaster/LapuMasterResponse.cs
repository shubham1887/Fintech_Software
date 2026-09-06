namespace FintechApp.Responses.LapuMaster
{
    public class LapuMasterResponse
    {
    public int LapuMasterID { get; set; }
    public int LapuPartyMasterID { get; set; }
    public int OperatorID { get; set; }
    public string LapuNo { get; set; } = string.Empty;
    public string ProivderLapuID { get; set; } = string.Empty;
    public decimal ClosingBalance { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
