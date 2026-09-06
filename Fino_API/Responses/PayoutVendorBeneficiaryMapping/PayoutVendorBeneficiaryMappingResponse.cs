namespace FintechApp.Responses.PayoutVendorBeneficiaryMapping
{
    public class PayoutVendorBeneficiaryMappingResponse
    {
    public int PayoutVendorBeneficiaryMappingID { get; set; }
    public int ProviderID { get; set; }
    public string PayoutVendorBeneficiaryID { get; set; } = string.Empty;
    public int PayoutBeneficaryID { get; set; }
    }
}
