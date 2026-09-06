using Project.Processing;

namespace Project.Response.User
{
    public class ProfileResponse
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private string _PinCode;

        public string PinCode
        {
            get => _PinCode.TrimIfNotNull();
            set => _PinCode = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.ToTitleCase();
            set => _PersonName = value.ToTitleCase();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _PANNo;

        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.ToTitleCase();
            set => _Address = value.ToTitleCase();
        }
        
        private string _ShopAddress;

        public string ShopAddress
        {
            get => _ShopAddress.TrimIfNotNull();
            set => _ShopAddress = value.TrimIfNotNull();
        }

        private string _ParentRole;

        public string ParentRole
        {
            get => _ParentRole.ToTitleCase();
            set => _ParentRole = value.ToTitleCase();
        }

        private string _ParentFirmName;

        public string ParentFirmName
        {
            get => _ParentFirmName.TrimIfNotNull();
            set => _ParentFirmName = value.TrimIfNotNull();
        }

        private string _ParentMobileNumber;

        public string ParentMobileNumber
        {
            get => _ParentMobileNumber.TrimIfNotNull();
            set => _ParentMobileNumber = value.TrimIfNotNull();
        }

        private string _ProfileImagePath;

        public string ProfileImagePath
        {
            get => _ProfileImagePath.TrimIfNotNull();
            set => _ProfileImagePath = value.TrimIfNotNull();
        }

        private bool _IsKYCApproved;

        public bool IsKYCApproved
        {
            get => _IsKYCApproved;
            set => _IsKYCApproved = value;
        }

        private string _KYCStatus;

        public string KYCStatus
        {
            get => _KYCStatus.TrimIfNotNull();
            set => _KYCStatus = value.TrimIfNotNull();
        }

        private int _Pin;
        public int Pin { get => _Pin; set => _Pin = value; }
        private string _VirtualAccountNumber;

        public string VirtualAccountNumber
        {
            get => _VirtualAccountNumber.TrimIfNotNull();
            set => _VirtualAccountNumber = value.TrimIfNotNull();
        }

        private bool _IsInvoiceApplicable;
        public bool IsInvoiceApplicable { get => _IsInvoiceApplicable; set => _IsInvoiceApplicable = value; }

        public string IsActiveWebStr
        {
            get => (IsInvoiceApplicable) ? "Yes" : "No";
        }

        private bool isChannel;
        public bool IsChannel { get => isChannel; set => isChannel = value; }

        public string IsChannelStr
        {
            get => (IsChannel) ? "Yes" : "No";
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName;
            set => _SlabName = value;
        }

        private bool _IsTwoFA;

        public bool IsTwoFA
        {
            get => _IsTwoFA;
            set => _IsTwoFA = value;
        }

        private bool _IsReferralPopup;

        public bool IsReferralPopup
        {
            get => _IsReferralPopup;
            set => _IsReferralPopup = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private bool _AllowRealAPI;

        public bool AllowRealAPI
        {
            get => _AllowRealAPI;
            set => _AllowRealAPI = value;
        }

        private int _WalletIDforEcollection;

        public int WalletIDforEcollection
        {
            get => _WalletIDforEcollection;
            set => _WalletIDforEcollection = value;
        }

        private int _StaticQRWalletID;

        public int StaticQRWalletID
        {
            get => _StaticQRWalletID;
            set => _StaticQRWalletID = value;
        }

        private decimal _CommissionOnTransfer;

        public decimal CommissionOnTransfer
        {
            get => _CommissionOnTransfer;
            set => _CommissionOnTransfer = value;
        }

        private bool _IsPin;

        public bool IsPin
        {
            get => _IsPin;
            set => _IsPin = value;
        }

        private bool _NumberFirst;

        public bool NumberFirst
        {
            get => _NumberFirst;
            set => _NumberFirst = value;
        }

        private string _ReferralURL;

        public string ReferralURL
        {
            get => _ReferralURL.TrimIfNotNull();
            set => _ReferralURL = value.TrimIfNotNull();
        }

        private decimal _ReferralAmount;

        public decimal ReferralAmount
        {
            get => _ReferralAmount;
            set => _ReferralAmount = value;
        }

        private decimal _ReferralBenefitMinAmount;

        public decimal ReferralBenefitMinAmount
        {
            get => _ReferralBenefitMinAmount;
            set => _ReferralBenefitMinAmount = value;
        }

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType.TrimIfNotNull();
            set => _ResponseType = value.TrimIfNotNull();
        }

        private bool _IsCommissionHold;

        public bool IsCommissionHold
        {
            get => _IsCommissionHold;
            set => _IsCommissionHold = value;
        }
    }
}