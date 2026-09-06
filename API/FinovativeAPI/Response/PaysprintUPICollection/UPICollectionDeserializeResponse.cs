using Project.Processing;

namespace Project.Response.PaysprintUPICollection
{
    public class UPICollectionDeserializeResponse
    {
        private string _IfscCode;

        public string IfscCode
        {
            get => _IfscCode.TrimIfNotNull();
            set => _IfscCode = value.TrimIfNotNull();
        }

        private string _PartnerAddress;

        public string PartnerAddress
        {
            get => _PartnerAddress.TrimIfNotNull();
            set => _PartnerAddress = value.TrimIfNotNull();
        }

        private string _PartnerCity;

        public string PartnerCity
        {
            get => _PartnerCity.TrimIfNotNull();
            set => _PartnerCity = value.TrimIfNotNull();
        }

        private string _PartnerState;

        public string PartnerState
        {
            get => _PartnerState.TrimIfNotNull();
            set => _PartnerState = value.TrimIfNotNull();
        }

        private string _PartnerPincode;

        public string PartnerPincode
        {
            get => _PartnerPincode.TrimIfNotNull();
            set => _PartnerPincode = value.TrimIfNotNull();
        }

        private string _PartnerMobile;

        public string PartnerMobile
        {
            get => _PartnerMobile.TrimIfNotNull();
            set => _PartnerMobile = value.TrimIfNotNull();
        }

        private string _PartnerCurrentAccount;

        public string PartnerCurrentAccount
        {
            get => _PartnerCurrentAccount.TrimIfNotNull();
            set => _PartnerCurrentAccount = value.TrimIfNotNull();
        }
    }
}