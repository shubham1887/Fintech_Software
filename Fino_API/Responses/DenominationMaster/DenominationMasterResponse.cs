namespace FintechApp.Responses.DenominationMaster
{
    public class DenominationMasterResponse
    {
        public int DenominationMasterID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DenominationRange { get; set; } = string.Empty;
        public bool IsStop { get; set; }
        public DateTime DateTime { get; set; }
    }
}
