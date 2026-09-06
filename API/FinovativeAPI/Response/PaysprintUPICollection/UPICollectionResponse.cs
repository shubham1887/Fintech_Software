using Project.Processing;

namespace Project.Response.PaysprintUPICollection
{
    public class UPICollectionResponse
    {
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _IFSCCode;

        public string IFSCCode
        {
            get => _IFSCCode.TrimIfNotNull();
            set => _IFSCCode = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _PANNumber;

        public string PANNumber
        {
            get => _PANNumber.TrimIfNotNull();
            set => _PANNumber = value.TrimIfNotNull();
        }

        private string _StaticQRLink;

        public string StaticQRLink
        {
            get => _StaticQRLink.TrimIfNotNull();
            set => _StaticQRLink = value.TrimIfNotNull();
        }

        private string _RefID;

        public string RefID
        {
            get => _RefID.TrimIfNotNull();
            set => _RefID = value.TrimIfNotNull();
        }
    }
}