using Project.Processing;

namespace Project.Response.User
{
    public class AgentDetailResponse
    {
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private bool _IsActive;
        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _BusinessName;

        public string BusinessName
        {
            get => _BusinessName.TrimIfNotNull();
            set => _BusinessName = value.TrimIfNotNull();
        }

        private string _DBAName;

        public string DBAName
        {
            get => _DBAName.TrimIfNotNull();
            set => _DBAName = value.TrimIfNotNull();
        }

        private string _BusinessFilingType;

        public string BusinessFilingType
        {
            get => _BusinessFilingType.TrimIfNotNull();
            set => _BusinessFilingType = value.TrimIfNotNull();
        }

        private string _BusinessPAN;

        public string BusinessPAN
        {
            get => _BusinessPAN.TrimIfNotNull();
            set => _BusinessPAN = value.TrimIfNotNull();
        }

        private string _GSTIN;

        public string GSTIN
        {
            get => _GSTIN.TrimIfNotNull();
            set => _GSTIN = value.TrimIfNotNull();
        }

        private string _BusinessAddress;

        public string BusinessAddress
        {
            get => _BusinessAddress.TrimIfNotNull();
            set => _BusinessAddress = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _ZipCode;

        public string ZipCode
        {
            get => _ZipCode.TrimIfNotNull();
            set => _ZipCode = value.TrimIfNotNull();
        }

        private string _Categorycode;

        public string Categorycode
        {
            get => _Categorycode.TrimIfNotNull();
            set => _Categorycode = value.TrimIfNotNull();
        }

        private string _Subcategorycode;

        public string Subcategorycode
        {
            get => _Subcategorycode.TrimIfNotNull();
            set => _Subcategorycode = value.TrimIfNotNull();
        }

        private string _OwnerSignatoryName;

        public string OwnerSignatoryName
        {
            get => _OwnerSignatoryName.TrimIfNotNull();
            set => _OwnerSignatoryName = value.TrimIfNotNull();
        }

        private string _OwnerPAN;

        public string OwnerPAN
        {
            get => _OwnerPAN.TrimIfNotNull();
            set => _OwnerPAN = value.TrimIfNotNull();
        }

        private string _OwnerDOB;
        public string OwnerDOB
        {
            get => _OwnerDOB.TrimIfNotNull();
            set => _OwnerDOB = value.TrimIfNotNull();
        }

        private string _AuthorisedSignatoryAddress;
        public string AuthorisedSignatoryAddress
        {
            get => _AuthorisedSignatoryAddress.TrimIfNotNull();
            set => _AuthorisedSignatoryAddress = value.TrimIfNotNull();
        }

        private string _OwnerState;
        public string OwnerState
        {
            get => _OwnerState.TrimIfNotNull();
            set => _OwnerState = value.TrimIfNotNull();
        }

        private string _OwnerCity;
        public string OwnerCity
        {
            get => _OwnerCity.TrimIfNotNull();
            set => _OwnerCity = value.TrimIfNotNull();
        }

        private string _OwnerZipCode;
        public string OwnerZipCode
        {
            get => _OwnerZipCode.TrimIfNotNull();
            set => _OwnerZipCode = value.TrimIfNotNull();
        }

        private string _DeviceNo;
        public string DeviceNo
        {
            get => _DeviceNo.TrimIfNotNull();
            set => _DeviceNo = value.TrimIfNotNull();
        }

        private string _AgentCode;
        public string AgentCode
        {
            get => _AgentCode.TrimIfNotNull();
            set => _AgentCode = value.TrimIfNotNull();
        }

    }
}