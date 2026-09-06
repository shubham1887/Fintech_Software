using Project.Processing;

namespace Project.Response.PayoutVendorBeneficiaryMapping
{
    public class ViewModel
    {
        private int _PayoutVendorBeneficiaryMappingID;

        public int PayoutVendorBeneficiaryMappingID
        {
            get => _PayoutVendorBeneficiaryMappingID;
            set => _PayoutVendorBeneficiaryMappingID = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _PayoutVendorBeneficiaryID;

        public string PayoutVendorBeneficiaryID
        {
            get => _PayoutVendorBeneficiaryID.TrimIfNotNull();
            set => _PayoutVendorBeneficiaryID = value.TrimIfNotNull();
        }

        private int _PayoutBeneficiaryID;

        public int PayoutBeneficiaryID
        {
            get => _PayoutBeneficiaryID;
            set => _PayoutBeneficiaryID = value;
        }
    }
}
