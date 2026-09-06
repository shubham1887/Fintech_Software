namespace FintechApp.Responses.ProviderOperatorDenominationMapping
{
    public class ProviderOperatorDenominationMappingResponse
    {
    public int ProviderOperatorDenominationMappingID { get; set; }
    public int ProviderID { get; set; }
    public int OperatorID { get; set; }
    public int DenominationMasterID { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string DenominationRange { get; set; } = string.Empty;
    }
}
