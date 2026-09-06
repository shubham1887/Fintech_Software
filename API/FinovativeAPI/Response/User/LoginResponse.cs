using Newtonsoft.Json;
using Project.Processing;
using System;
using System.Collections.Generic;

namespace Project.Response.User
{
    public class LoginResponse
    {
        private bool _IsPGSettlementHold;
        public bool IsPGSettlementHold
        {
            get => _IsPGSettlementHold;
            set => _IsPGSettlementHold = value;
        }
        public int UserID;
        public int UserTypeID;

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }
        private string _PANNo;

        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }

        private string _SecureToken;

        public string SecureToken
        {
            get => _SecureToken.TrimIfNotNull();
            set => _SecureToken = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private bool _IsKYCApproved;

        public bool IsKYCApproved
        {
            get => _IsKYCApproved;
            set => _IsKYCApproved = value;
        }

        private bool _IseKYC;

        public bool IseKYC
        {
            get => _IseKYC;
            set => _IseKYC = value;
        }
        private bool _IsCashFreeAiUser;

        public bool IsCashFreeAiUser
        {
            get => _IsCashFreeAiUser;
            set => _IsCashFreeAiUser = value;
        }
        

        private string _RedirectTo;

        public string RedirectTo
        {
            get => _RedirectTo.TrimIfNotNull();
            set => _RedirectTo = value.TrimIfNotNull();
        }
        private string _AliasName;

        public string AliasName
        {
            get => _AliasName.TrimIfNotNull();
            set => _AliasName = value.TrimIfNotNull();
        }
        
        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private bool _IsNewDevice;

        public bool IsNewDevice
        {
            get => _IsNewDevice;
            set => _IsNewDevice = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private bool _IsLoginOTP;

        public bool IsLoginOTP
        {
            get => _IsLoginOTP;
            set => _IsLoginOTP = value;
        }

        private bool _IsOTPSent;

        public bool IsOTPSent
        {
            get => _IsOTPSent;
            set => _IsOTPSent = value;
        }

        private bool _IsTwoFA;

        public bool IsTwoFA
        {
            get => _IsTwoFA;
            set => _IsTwoFA = value;
        }

        private bool _NumberFirst;

        public bool NumberFirst
        {
            get => _NumberFirst;
            set => _NumberFirst = value;
        }

        private bool _IsPasswordExpired;

        public bool IsPasswordExpired
        {
            get => _IsPasswordExpired;
            set => _IsPasswordExpired = value;
        }
        private bool _IsFundRequestAmountEdit;
        public bool IsFundRequestAmountEdit
        {
            get => _IsFundRequestAmountEdit;
            set => _IsFundRequestAmountEdit = value;
        }
        private string _LoginIP;

        public string LoginIP
        {
            get => _LoginIP.TrimIfNotNull();
            set => _LoginIP = value.TrimIfNotNull();
        }

        private string _ProfileImagePath;

        public string ProfileImagePath
        {
            get => _ProfileImagePath.TrimIfNotNull();
            set => _ProfileImagePath = value.TrimIfNotNull();
        }

        private string _SubscriptionDays;

        public string SubscriptionDays
        {
            get => _SubscriptionDays.TrimIfNotNull();
            set => _SubscriptionDays = value.TrimIfNotNull();
        }

        private string _WhatsappURL;

        public string WhatsappURL
        {
            get => _WhatsappURL.TrimIfNotNull();
            set => _WhatsappURL = value.TrimIfNotNull();
        }

        private string _ActiveDMTNeft;

        public string ActiveDMTNeft
        {
            get => _ActiveDMTNeft.TrimIfNotNull();
            set => _ActiveDMTNeft = value.TrimIfNotNull();
        }

        private bool _IsInitialPassword;

        public bool IsInitialPassword
        {
            get => _IsInitialPassword;
            set => _IsInitialPassword = value;
        }

        private bool _ServicewiseSubscription;

        public bool ServicewiseSubscription
        {
            get => _ServicewiseSubscription;
            set => _ServicewiseSubscription = value;
        }

        private string _EmailID;

        private string _AppDashboardType;

        public string AppDashboardType
        {
            get => _AppDashboardType.TrimIfNotNull();
            set => _AppDashboardType = value.TrimIfNotNull();
        }

        public List<UserWallet> Wallets = new List<UserWallet>();

        public string WalletsInString { get => JsonConvert.SerializeObject(Wallets); }

        public List<UISettingsViewModel> UISettings = new List<UISettingsViewModel>();

        public string UISettingsInString { get => JsonConvert.SerializeObject(UISettings); }

        public int NewsCount
        {
            get; set;
        }

        public List<ActiveDMTList> ActiveDMTList = new List<ActiveDMTList>();

        private string _ActiveDMT;

        public string ActiveDMT
        {
            get => _ActiveDMT.TrimIfNotNull();
            set => _ActiveDMT = value.TrimIfNotNull();
        }

        public List<ActiveAEPSList> ActiveAEPSList = new List<ActiveAEPSList>();
        private string _ActiveAEPS;

        public string ActiveAEPS
        {
            get => _ActiveAEPS.TrimIfNotNull();
            set => _ActiveAEPS = value.TrimIfNotNull();
        }

        private bool _IsChannel;

        public bool IsChannel
        {
            get => _IsChannel;
            set => _IsChannel = value;
        }

        private string _IsCashCollection;

        public string IsCashCollection
        {
            get => _IsCashCollection;
            set => _IsCashCollection = value;
        }
        
        private string _IsEkycUserProfileUpdate;

        public string IsEkycUserProfileUpdate
        {
            get => _IsEkycUserProfileUpdate;
            set => _IsEkycUserProfileUpdate = value;
        }

        private string _IsTargetOnSale;

        public string IsTargetOnSale
        {
            get => _IsTargetOnSale;
            set => _IsTargetOnSale = value;
        }

        private string _IsOverDraft;

        public string IsOverDraft
        {
            get => _IsOverDraft;
            set => _IsOverDraft = value;
        }
        
        private string _PlanApiForAPIUser;

        public string PlanApiForAPIUser
        {
            get => _PlanApiForAPIUser;
            set => _PlanApiForAPIUser = value;
        }
        
        private string _IsCreditInstitution;

        public string IsCreditInstitution
        {
            get => _IsCreditInstitution;
            set => _IsCreditInstitution = value;
        }
        
        private string _IsFintechLeadGeneration;

        public string IsFintechLeadGeneration
        {
            get => _IsFintechLeadGeneration;
            set => _IsFintechLeadGeneration = value;
        }

        private bool _IsOverDraftActive;

        public bool IsOverDraftActive
        {
            get => _IsOverDraftActive;
            set => _IsOverDraftActive = value;
        }

        private string _IsW2OFundTransfer;

        public string IsW2OFundTransfer
        {
            get => _IsW2OFundTransfer;
            set => _IsW2OFundTransfer = value;
        }

        private string _IsWhiteLabel;

        public string IsWhiteLabel
        {
            get => _IsWhiteLabel;
            set => _IsWhiteLabel = value;
        }

        private string _IsPendingProvider;

        public string IsPendingProvider
        {
            get => _IsPendingProvider;
            set => _IsPendingProvider = value;
        }

        private string _CommissionOnTransfer;

        public string CommissionOnTransfer
        {
            get => _CommissionOnTransfer.TrimIfNotNullWithDefault("No");
            set => _CommissionOnTransfer = value.TrimIfNotNullWithDefault("No");
        }

        public string IsPin { get; internal set; }

        private bool _IsUPIVerify;

        public bool IsUPIVerify
        {
            get => _IsUPIVerify;
            set => _IsUPIVerify = value;
        }

        private string _NewsMessage;

        public string NewsMessage
        {
            get => _NewsMessage.TrimIfNotNull();
            set => _NewsMessage = value.TrimIfNotNull();
        }

        private string _HyperLink;

        public string HyperLink
        {
            get => _HyperLink.TrimIfNotNull();
            set => _HyperLink = value.TrimIfNotNull();
        }

        private bool _IsGSTHold;

        public bool IsGSTHold
        {
            get => _IsGSTHold;
            set => _IsGSTHold = value;
        }

        private bool _CommissionHold;

        public bool CommissionHold
        {
            get => _CommissionHold;
            set => _CommissionHold = value;
        }

        private bool _AutoBalanceTransfer;

        public bool AutoBalanceTransfer
        {
            get => _AutoBalanceTransfer;
            set => _AutoBalanceTransfer = value;
        }

        private bool _BackDateFundTransfer;

        public bool BackDateFundTransfer
        {
            get => _BackDateFundTransfer;
            set => _BackDateFundTransfer = value;
        }

        private bool _IsSignupReferral;

        public bool IsSignupReferral
        {
            get => _IsSignupReferral;
            set => _IsSignupReferral = value;
        }

        private bool _IsMobikwikBBPS;

        public bool IsMobikwikBBPS
        {
            get => _IsMobikwikBBPS;
            set => _IsMobikwikBBPS = value;
        }
        private bool _IsW2OWithSenderWallet;

        public bool IsW2OWithSenderWallet
        {
            get => _IsW2OWithSenderWallet;
            set => _IsW2OWithSenderWallet = value;
        }
        

        private bool _IsServiceCharge;

        public bool IsServiceCharge
        {
            get => _IsServiceCharge;
            set => _IsServiceCharge = value;
        }
        private bool _Voucher;

        public bool Voucher
        {
            get => _Voucher;
            set => _Voucher = value;
        }

        private bool _IsProviderPerformanceGraph;

        public bool IsProviderPerformanceGraph
        {
            get => _IsProviderPerformanceGraph;
            set => _IsProviderPerformanceGraph = value;
        }
        private string _IsUserListOnDashboard;

        public string IsUserListOnDashboard
        {
            get => _IsUserListOnDashboard.TrimIfNotNull();
            set => _IsUserListOnDashboard = value.TrimIfNotNull();
        }
        private string _FundReversalUpline;

        public string FundReversalUpline
        {
            get => _FundReversalUpline.TrimIfNotNull();
            set => _FundReversalUpline = value.TrimIfNotNull();
        }

        private string _ReportFontSize;

        public string ReportFontSize
        {
            get => _ReportFontSize.TrimIfNotNull();
            set => _ReportFontSize = value.TrimIfNotNull();
        }

        private bool _IsReferralPopup;

        public bool IsReferralPopup
        {
            get => _IsReferralPopup;
            set => _IsReferralPopup = value;
        }

        private int _QuickSearchBackDays;

        public int QuickSearchBackDays
        {
            get => _QuickSearchBackDays;
            set => _QuickSearchBackDays = value;
        }
        
        private int _DMTSenderNumberLength;

        public int DMTSenderNumberLength
        {
            get => _DMTSenderNumberLength;
            set => _DMTSenderNumberLength = value;
        }

        public MenuResponseWraper MenuResponse = new MenuResponseWraper();
    }

    public class MenuResponseWraper
    {
        public List<Response.SupportUserAccessDetail.Parent> data = new List<Response.SupportUserAccessDetail.Parent>();
    }

    public class UserWallet
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private decimal _Balance;

        public decimal Balance
        {
            get => _Balance;
            set => _Balance = value;
        }

        private string _ShortName;

        public string ShortName
        {
            get => _ShortName.TrimIfNotNull();
            set => _ShortName = value.TrimIfNotNull();
        }

        public int WalletID;
    }

    public class ActiveDMTList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsPayoutDMT;

        public bool IsPayoutDMT
        {
            get => _IsPayoutDMT;
            set => _IsPayoutDMT = value;
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNullWithDefault("DMT");
            set => _DisplayName = value.TrimIfNotNullWithDefault("DMT");
        }

        private string _ProviderCode;

        public string ProviderCode
        {
            get => _ProviderCode.TrimIfNotNullWithDefault("DMT");
            set => _ProviderCode = value.TrimIfNotNullWithDefault("DMT");
        }
    }

    public class ActiveAEPSList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNullWithDefault("AEPS");
            set => _DisplayName = value.TrimIfNotNullWithDefault("AEPS");
        }
       
       
    }
}