namespace FintechApp.Entities
{
    public class UPIVendorBeneficiaryMapping
    {
        public int UPIVendorBeneficiaryMappingID { get; set; }
        public int ProviderID { get; set; }
        public string UPIVendorBeneficiaryID { get; set; } = string.Empty;
        public int UpiBeneficiaryID { get; set; }
        public UpiBeneficiary UpiBeneficiary { get; set; } = null!;
    }
}
