using Project.Processing;

namespace Project.Response.UPIVendorBeneficiaryMapping
{
    public class ViewModel
    {
        private int _UPIVendorBeneficiaryMappingID;

        public int UPIVendorBeneficiaryMappingID
        {
            get => _UPIVendorBeneficiaryMappingID;
            set => _UPIVendorBeneficiaryMappingID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _UPIVendorBeneficiaryID;

        public string UPIVendorBeneficiaryID
        {
            get => _UPIVendorBeneficiaryID.TrimIfNotNull();
            set => _UPIVendorBeneficiaryID = value.TrimIfNotNull();
        }

        private int _UPIBeneficiaryID;

        public int UPIBeneficiaryID
        {
            get => _UPIBeneficiaryID;
            set => _UPIBeneficiaryID = value;
        }
    }
}