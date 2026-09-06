using Project.Processing;

namespace Project.Response.VendorBeneficiaryMapping
{
    public class ViewModel
    {
        private int _VendorBeneficiaryMappingID;

        public int VendorBeneficiaryMappingID
        {
            get => _VendorBeneficiaryMappingID;
            set => _VendorBeneficiaryMappingID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _VendorBeneficiaryID;

        public string VendorBeneficiaryID
        {
            get => _VendorBeneficiaryID.TrimIfNotNull();
            set => _VendorBeneficiaryID = value.TrimIfNotNull();
        }


        private int _BeneficiaryID;

        public int BeneficiaryID
        {
            get => _BeneficiaryID;
            set => _BeneficiaryID = value;
        }
    }
}
