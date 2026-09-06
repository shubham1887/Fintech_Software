using Project.Processing;
using Project.Requests.User;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class MemberDetailResponse
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _ParentAliasName;

        public string ParentAliasName
        {
            get => _ParentAliasName.TrimIfNotNull();
            set => _ParentAliasName = value.TrimIfNotNull();
        }

        private string _ParentName;
        public string ParentName
        {
            get => _ParentName.ToTitleCase();
            set => _ParentName = value.ToTitleCase();
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


        private string _SuperParentAliasName;

        public string SuperParentAliasName
        {
            get => _SuperParentAliasName.TrimIfNotNull();
            set => _SuperParentAliasName = value.TrimIfNotNull();
        }

        private string _SuperParentName;

        public string SuperParentName
        {
            get => _SuperParentName.TrimIfNotNull();
            set => _SuperParentName = value.TrimIfNotNull();
        }

        private string _SuperParentFirmName;

        public string SuperParentFirmName
        {
            get => _SuperParentFirmName.TrimIfNotNull();
            set => _SuperParentFirmName = value.TrimIfNotNull();
        }

        private string _SuperParentMobileNumber;

        public string SuperParentMobileNumber
        {
            get => _SuperParentMobileNumber.TrimIfNotNull();
            set => _SuperParentMobileNumber = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _PersonName;

        public string PersonName
        {
            get => _PersonName.ToTitleCase();
            set => _PersonName = value.ToTitleCase();
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }     

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
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

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
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

        private bool _IsKYCApproved;

        public bool IsKYCApproved
        {
            get => _IsKYCApproved;
            set => _IsKYCApproved = value;
        }
        private string _Logo;
        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }
        private string _YesBankLogo;
        public string YesBankLogo
        {
            get => _YesBankLogo.TrimIfNotNull();
            set => _YesBankLogo = value.TrimIfNotNull();
        }
        

        private List<UserServiceMappingDetailList> _list;

        public List<UserServiceMappingDetailList> UserServiceList
        {
            get => _list;
            set => _list = value;
        }

        public int StateID { get; internal set; }


        private bool _IsLoginOTP;

        public bool IsLoginOTP
        {
            get => _IsLoginOTP;
            set => _IsLoginOTP = value;
        }

        private float _RechargeBalance;

        public float RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private float _DMTBalance;

        public float DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private float _AEPSBalance;

        public float AEPSBalance
        {
            get => _AEPSBalance;
            set => _AEPSBalance = value;
        }

        private float _Outstanding;

        public float Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private bool _IsWeb;

        public bool IsWeb
        {
            get => _IsWeb;
            set => _IsWeb = value;
        }

        private bool _IsApp;

        public bool IsApp
        {
            get => _IsApp;
            set => _IsApp = value;
        }

        private bool _IsInvoiceApplicable;

        public bool IsInvoiceApplicable
        {
            get => _IsInvoiceApplicable;
            set => _IsInvoiceApplicable = value;
        }

        private decimal targetAmount;

        public decimal TargetAmount
        {
            get => targetAmount;
            set => targetAmount = value;
        }

        private bool _AllowRealAPI;

        public bool AllowRealAPI
        {
            get => _AllowRealAPI;
            set => _AllowRealAPI = value;
        }

        private bool _IsChannel;

        public bool IsChannel
        {
            get => _IsChannel;
            set => _IsChannel = value;
        }

        private string _SlabName;

        public string SlabName
        {
            get => _SlabName.TrimIfNotNull();
            set => _SlabName = value.TrimIfNotNull();
        }

        private string _Balances;

        public string Balances
        {
            get => _Balances.TrimIfNotNull();
            set => _Balances = value.TrimIfNotNull();
        }

        private string _BalanceCappings;

        public string BalanceCappings
        {
            get => _BalanceCappings.TrimIfNotNull();
            set => _BalanceCappings = value.TrimIfNotNull();
        }

        private string _ProfileImagePath;

        public string ProfileImagePath
        {
            get => _ProfileImagePath.TrimIfNotNull();
            set => _ProfileImagePath = value.TrimIfNotNull();
        }

        private float _TotalRechargeBalance;

        public float TotalRechargeBalance
        {
            get => _TotalRechargeBalance;
            set => _TotalRechargeBalance = value;
        }

        private float _TotalDMTBalance;

        public float TotalDMTBalance
        {
            get => _TotalDMTBalance;
            set => _TotalDMTBalance = value;
        }

        private float _TotalAEPSBalance;

        public float TotalAEPSBalance
        {
            get => _TotalAEPSBalance;
            set => _TotalAEPSBalance = value;
        }

        private string _Details;

        public string Details
        {
            get => _Details;
            set => _Details = value;
        }

        private string _IsKYCApprovedStr;

        public string IsKYCApprovedStr
        {
            get => _IsKYCApprovedStr.TrimIfNotNull();
            set => _IsKYCApprovedStr = value.TrimIfNotNull();
        }

        private string _VirtualAccountNumber;

        public string VirtualAccountNumber
        {
            get => _VirtualAccountNumber;
            set => _VirtualAccountNumber = value;
        }

        private string _JoiningByUser;

        public string JoiningByUser
        {
            get => _JoiningByUser.TrimIfNotNull();
            set => _JoiningByUser = value.TrimIfNotNull();
        }

        private string _DOC;

        public string DOC
        {
            get => _DOC;
            set => _DOC = value;
        }

        private bool _IsTwoFA;

        public bool IsTwoFA
        {
            get => _IsTwoFA;
            set => _IsTwoFA = value;
        }

        private string _CityName;

        public string CityName
        {
            get => _CityName.TrimIfNotNull();
            set => _CityName = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
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

        private decimal _MaxOutstanding;

        public decimal MaxOutstanding
        {
            get => _MaxOutstanding;
            set => _MaxOutstanding = value;
        }

        private string _ResponseType;

        public string ResponseType
        {
            get => _ResponseType.TrimIfNotNull();
            set => _ResponseType = value.TrimIfNotNull();
        }

        private int _PayoutAccountCount;

        public int PayoutAccountCount
        {
            get => _PayoutAccountCount;
            set => _PayoutAccountCount = value;
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private float _CappRechargeBalance;

        public float CappRechargeBalance
        {
            get => _CappRechargeBalance;
            set => _CappRechargeBalance = value;
        }

        private float _CappDMTBalance;

        public float CappDMTBalance
        {
            get => _CappDMTBalance;
            set => _CappDMTBalance = value;
        }

        private float _CappAEPSBalance;

        public float CappAEPSBalance
        {
            get => _CappAEPSBalance;
            set => _CappAEPSBalance = value;
        }


        private int _DOCInt;

        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        private bool _IseKYC;

        public bool IseKYC
        {
            get => _IseKYC;
            set => _IseKYC = value;
        }

        private bool _IsCommissionHold;

        public bool IsCommissionHold
        {
            get => _IsCommissionHold;
            set => _IsCommissionHold = value;
        }
        
        private string _GSTPrinciple;

        public string GSTPrinciple
        {
            get => _GSTPrinciple.TrimIfNotNull();
            set => _GSTPrinciple = value.TrimIfNotNull();
        }
        
        private string _NPCIAgentID;

        public string NPCIAgentID
        {
            get => _NPCIAgentID.TrimIfNotNull();
            set => _NPCIAgentID = value.TrimIfNotNull();
        }

    }
}