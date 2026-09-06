using Project.Processing;

namespace Project.StaticModel
{
    public class Company
    {
        private static string _AdminMobileNumber;

        public static string AdminMobileNumber
        {
            get => _AdminMobileNumber.TrimIfNotNull();
            set => _AdminMobileNumber = value.TrimIfNotNull();
        }

        private static string _ClientName;

        public static string ClientName
        {
            get => _ClientName.TrimIfNotNull();
            set => _ClientName = value.TrimIfNotNull();
        }

        private static string _WebAPIURL;

        public static string WebAPIURL
        {
            get => _WebAPIURL.TrimIfNotNull();
            set => _WebAPIURL = value.TrimIfNotNull();
        }

        private static int _DefaultAdminUserID;

        public static int AdminUserID
        {
            get => _DefaultAdminUserID;
            set => _DefaultAdminUserID = value;
        }

        private static int _DefaultSlabID;

        public static int DefaultSlabID
        {
            get => _DefaultSlabID;
            set => _DefaultSlabID = value;
        }

        private static string _DefaultSlabName;

        public static string DefaultSlabName
        {
            get => _DefaultSlabName.TrimIfNotNull();
            set => _DefaultSlabName = value.TrimIfNotNull();
        }

        private static string _XMPPDomain;

        public static string XMPPDomain
        {
            get => _XMPPDomain.TrimIfNotNull();
            set => _XMPPDomain = value.TrimIfNotNull();
        }

        private static string _XMPPWebSiteUserName;

        public static string XMPPWebSiteUserName
        {
            get => _XMPPWebSiteUserName.TrimIfNotNull();
            set => _XMPPWebSiteUserName = value.TrimIfNotNull();
        }

        private static string _XMPPWebSiteUserPassword;

        public static string XMPPWebSiteUserPassword
        {
            get => _XMPPWebSiteUserPassword.TrimIfNotNull();
            set => _XMPPWebSiteUserPassword = value.TrimIfNotNull();
        }

        private static string _XMPPOtomaxUser;

        public static string XMPPOtomaxUser
        {
            get => _XMPPOtomaxUser.TrimIfNotNull();
            set => _XMPPOtomaxUser = value.TrimIfNotNull();
        }

        private static int _XMPPProviderID;

        public static int XMPPProviderID
        {
            get => _XMPPProviderID;
            set => _XMPPProviderID = value;
        }

        private static int _AlleProviderID;

        public static int AlleProviderID
        {
            get => _AlleProviderID;
            set => _AlleProviderID = value;
        }

        private static int _PayUProviderID;

        public static int PayUProviderID
        {
            get => _PayUProviderID;
            set => _PayUProviderID = value;
        }

        private static int _AmbikaProviderID;

        public static int AmbikaProviderID
        {
            get => _AmbikaProviderID;
            set => _AmbikaProviderID = value;
        }

        private static int _ApiClubProviderID;

        public static int ApiClubProviderID
        {
            get => _ApiClubProviderID;
            set => _ApiClubProviderID = value;
        }

        private static int _EZYPayV1ProviderID;

        public static int EZYPayV1ProviderID
        {
            get => _EZYPayV1ProviderID;
            set => _EZYPayV1ProviderID = value;
        }

        private static int _EkoProviderID;

        public static int EkoProviderID
        {
            get => _EkoProviderID;
            set => _EkoProviderID = value;
        }

        private static int _EkoPayoutProviderID;

        public static int EkoPayoutProviderID
        {
            get => _EkoPayoutProviderID;
            set => _EkoPayoutProviderID = value;
        }

        private static int _IMoneyPayProviderID;

        public static int IMoneyPayProviderID
        {
            get => _IMoneyPayProviderID;
            set => _IMoneyPayProviderID = value;
        }

        private static int _RazorPayProviderID;

        public static int RazorPayProviderID
        {
            get => _RazorPayProviderID;
            set => _RazorPayProviderID = value;
        }

        private static int _WorldLineProviderID;

        public static int WorldLineProviderID
        {
            get => _WorldLineProviderID;
            set => _WorldLineProviderID = value;
        }

        private static int _PaysprintProviderID;

        public static int PaysprintProviderID
        {
            get => _PaysprintProviderID;
            set => _PaysprintProviderID = value;
        }
        

        private static int _PaysprintPaytmAEPSProviderID;

        public static int PaysprintPaytmAEPSProviderID
        {
            get => _PaysprintPaytmAEPSProviderID;
            set => _PaysprintPaytmAEPSProviderID = value;
        }

        private static int _FingpayProviderID;

        public static int FingpayProviderID
        {
            get => _FingpayProviderID;
            set => _FingpayProviderID = value;
        }

        private static int _MahagramProviderID;

        public static int MahagramProviderID
        {
            get => _MahagramProviderID;
            set => _MahagramProviderID = value;
        }

        private static int _PaytmProviderID;

        public static int PaytmProviderID
        {
            get => _PaytmProviderID;
            set => _PaytmProviderID = value;
        }

        private static int _CyberPlateProviderID;

        public static int CyberPlateProviderID
        {
            get => _CyberPlateProviderID;
            set => _CyberPlateProviderID = value;
        }

        private static int _MROAProviderID;

        public static int MROAProviderID
        {
            get => _MROAProviderID;
            set => _MROAProviderID = value;
        }

        private static int _ICICIProviderID;

        public static int ICICIProviderID
        {
            get => _ICICIProviderID;
            set => _ICICIProviderID = value;
        }

        private static int _PayInProviderID;

        public static int PayInProviderID
        {
            get => _PayInProviderID;
            set => _PayInProviderID = value;
        }

        private static int _OpenBankProviderID;

        public static int OpenBankProviderID
        {
            get => _OpenBankProviderID;
            set => _OpenBankProviderID = value;
        }

        private static int _PaytmPaymentGatwayProviderID;

        public static int PaytmPaymentGatwayProviderID
        {
            get => _PaytmPaymentGatwayProviderID;
            set => _PaytmPaymentGatwayProviderID = value;
        }

        private static int _PaytmCCFProviderID;

        public static int PaytmCCFProviderID
        {
            get => _PaytmCCFProviderID;
            set => _PaytmCCFProviderID = value;
        }

        private static int _YesBankProviderID;

        public static int YesBankProviderID
        {
            get => _YesBankProviderID;
            set => _YesBankProviderID = value;
        }

        private static int _VastWebProviderID;

        public static int VastWebProviderID
        {
            get => _VastWebProviderID;
            set => _VastWebProviderID = value;
        }

        private static int _BranchxProviderID;

        public static int BranchxProviderID
        {
            get => _BranchxProviderID;
            set => _BranchxProviderID = value;
        }

        private static int _VSmartProviderID;

        public static int VSmartProviderID
        {
            get => _VSmartProviderID;
            set => _VSmartProviderID = value;
        }

        private static int _ToshaniBankProviderID;

        public static int ToshaniBankProviderID
        {
            get => _ToshaniBankProviderID;
            set => _ToshaniBankProviderID = value;
        }

        private static int _RoundPayProviderID;

        public static int RoundPayProviderID
        {
            get => _RoundPayProviderID;
            set => _RoundPayProviderID = value;
        }

        private static int _ZwitchProviderID;

        public static int ZwitchProviderID
        {
            get => _ZwitchProviderID;
            set => _ZwitchProviderID = value;
        }

        private static int _JaiJuiProviderID;

        public static int JaiJuiProviderID
        {
            get => _JaiJuiProviderID;
            set => _JaiJuiProviderID = value;
        }

        private static string _UIURL;

        public static string UIURL
        {
            get => _UIURL.TrimIfNotNull();
            set => _UIURL = value.TrimIfNotNull();
        }

        private static string _ConsistyLoginToken;

        public static string ConsistyLoginToken
        {
            get => _ConsistyLoginToken.TrimIfNotNull();
            set => _ConsistyLoginToken = value.TrimIfNotNull();
        }

        private static int _InstantPayUPIGatewayID;

        public static int InstantPayUPIGatewayID
        {
            get => _InstantPayUPIGatewayID;
            set => _InstantPayUPIGatewayID = value;
        }

        private static int _MasterMoneyProviderID;

        public static int MasterMoneyProviderID
        {
            get => _MasterMoneyProviderID;
            set => _MasterMoneyProviderID = value;
        }

        private static int _MasterMoneyPureDMTProviderID;

        public static int MasterMoneyPureDMTProviderID
        {
            get => _MasterMoneyPureDMTProviderID;
            set => _MasterMoneyPureDMTProviderID = value;
        }

        private static int _EpocketProviderID;

        public static int EpocketProviderID
        {
            get => _EpocketProviderID;
            set => _EpocketProviderID = value;
        }

        private static int _MinksProviderID;

        public static int MinksProviderID
        {
            get => _MinksProviderID;
            set => _MinksProviderID = value;
        }

        private static int _ICashProviderID;

        public static int ICashProviderID
        {
            get => _ICashProviderID;
            set => _ICashProviderID = value;
        }

        private static int _NEFTProviderID;

        public static int NEFTProviderID
        {
            get => _NEFTProviderID;
            set => _NEFTProviderID = value;
        }

        private static string _CommissionOnTransfer;

        public static string CommissionOnTransfer
        {
            get => _CommissionOnTransfer.TrimIfNotNullWithDefault("No");
            set => _CommissionOnTransfer = value.TrimIfNotNullWithDefault("No");
        }

        private static string _FireAndForgetRechargeEarly;

        public static string FireAndForgetRechargeEarly
        {
            get => _FireAndForgetRechargeEarly.TrimIfNotNullWithDefault("No");
            set => _FireAndForgetRechargeEarly = value.TrimIfNotNullWithDefault("No");
        }

        private static string _PaymentGatewayKYC;

        public static string PaymentGatewayKYC
        {
            get => _PaymentGatewayKYC.TrimIfNotNullWithDefault("Yes");
            set => _PaymentGatewayKYC = value.TrimIfNotNullWithDefault("Yes");
        }

        private static string _OutsideRoleKYCForMobileDTH;

        public static string OutsideRoleKYCForMobileDTH
        {
            get => _OutsideRoleKYCForMobileDTH.TrimIfNotNullWithDefault("Yes");
            set => _OutsideRoleKYCForMobileDTH = value.TrimIfNotNullWithDefault("Yes");
        }

        private static string _PGCommissionWithDownline;

        public static string PGCommissionWithDownline
        {
            get => _PGCommissionWithDownline.TrimIfNotNullWithDefault("Yes");
            set => _PGCommissionWithDownline = value.TrimIfNotNullWithDefault("Yes");
        }

        private static string _IsUplineActive;

        public static string IsUplineActive
        {
            get => _IsUplineActive.TrimIfNotNullWithDefault("Yes");
            set => _IsUplineActive = value.TrimIfNotNullWithDefault("Yes");
        }

        private static string _XMPPResend;

        public static string XMPPResend
        {
            get => _XMPPResend.TrimIfNotNullWithDefault("No");
            set => _XMPPResend = value.TrimIfNotNullWithDefault("No");
        }

        private static int _MosambeeProviderID;

        public static int MosambeeProviderID
        {
            get => _MosambeeProviderID;
            set => _MosambeeProviderID = value;
        }

        private static int _BankITProviderID;

        public static int BankITProviderID
        {
            get => _BankITProviderID;
            set => _BankITProviderID = value;
        }

        private static int _BsnlOperatorID;

        public static int BsnlOperatorID
        {
            get => _BsnlOperatorID;
            set => _BsnlOperatorID = value;
        }

        private static int _BsnlSpecialOperatorID;

        public static int BsnlSpecialOperatorID
        {
            get => _BsnlSpecialOperatorID;
            set => _BsnlSpecialOperatorID = value;
        }

        private static string _OverDraft;

        public static string OverDraft
        {
            get => _OverDraft.TrimIfNotNull();
            set => _OverDraft = value.TrimIfNotNull();
        }

        private static string _IsW2OFundTransfer;

        public static string IsW2OFundTransfer
        {
            get => _IsW2OFundTransfer.TrimIfNotNull();
            set => _IsW2OFundTransfer = value.TrimIfNotNull();
        }

        private static string _IsWhiteLabel;

        public static string IsWhiteLabel
        {
            get => _IsWhiteLabel.TrimIfNotNull();
            set => _IsWhiteLabel = value.TrimIfNotNull();
        }

        private static string _NameDisplayFormat;

        public static string NameDisplayFormat
        {
            get => _NameDisplayFormat.TrimIfNotNull();
            set => _NameDisplayFormat = value.TrimIfNotNull();
        }

        private static string _GSTHold;

        public static string GSTHold
        {
            get => _GSTHold.TrimIfNotNull();
            set => _GSTHold = value.TrimIfNotNull();
        }

        private static string _IsPendingProvider;

        public static string IsPendingProvider
        {
            get => _IsPendingProvider.TrimIfNotNull();
            set => _IsPendingProvider = value.TrimIfNotNull();
        }

        private static int _MyNetLinkProviderID;

        public static int MyNetLinkProviderID

        {
            get => _MyNetLinkProviderID;

            set => _MyNetLinkProviderID = value;
        }

        private static int _QuadrantIndiaProviderID;

        public static int QuadrantIndiaProviderID

        {
            get => _QuadrantIndiaProviderID;

            set => _QuadrantIndiaProviderID = value;
        }     

        private static int _KinsenProviderID;

        public static int KinsenProviderID

        {
            get => _KinsenProviderID;

            set => _KinsenProviderID = value;
        }

        private static int _GoindiapayProviderID;

        public static int GoindiapayProviderID

        {
            get => _GoindiapayProviderID;

            set => _GoindiapayProviderID = value;
        }

        private static int _ICICICIBProviderID;

        public static int ICICICIBProviderID

        {
            get => _ICICICIBProviderID;

            set => _ICICICIBProviderID = value;
        }

        private static int _PayPaisaProviderID;

        public static int PayPaisaProviderID

        {
            get => _PayPaisaProviderID;

            set => _PayPaisaProviderID = value;
        }

        private static int _MottoPeProviderID;

        public static int MottoPeProviderID

        {
            get => _MottoPeProviderID;

            set => _MottoPeProviderID = value;
        }

        private static int _OnlyAPIProviderID;

        public static int OnlyAPIProviderID

        {
            get => _OnlyAPIProviderID;

            set => _OnlyAPIProviderID = value;
        }

        private static int _OnePaisaProviderID;

        public static int OnePaisaProviderID

        {
            get => _OnePaisaProviderID;

            set => _OnePaisaProviderID = value;
        }
        
        private static int _IMPSGuruProviderID;

        public static int IMPSGuruProviderID

        {
            get => _IMPSGuruProviderID;

            set => _IMPSGuruProviderID = value;
        }

        private static int _WaayuPayProviderID;

        public static int WaayuPayProviderID

        {
            get => _WaayuPayProviderID;

            set => _WaayuPayProviderID = value;
        }


        private static int _JusPayProviderID;

        public static int JusPayProviderID

        {
            get => _JusPayProviderID;

            set => _JusPayProviderID = value;
        }

        private static int _LevinProviderID;

        public static int LevinProviderID

        {
            get => _LevinProviderID;

            set => _LevinProviderID = value;
        }

        private static int _CashFreeProviderID;

        public static int CashFreeProviderID

        {
            get => _CashFreeProviderID;

            set => _CashFreeProviderID = value;
        }

        private static int _MobikwikProviderID;

        public static int MobikwikProviderID

        {
            get => _MobikwikProviderID;

            set => _MobikwikProviderID = value;
        }

        private static int _ClickPayProviderID;

        public static int ClickPayProviderID

        {
            get => _ClickPayProviderID;

            set => _ClickPayProviderID = value;
        }

        private static int _ZplusProviderID;

        public static int ZplusProviderID

        {
            get => _ZplusProviderID;

            set => _ZplusProviderID = value;
        }

        private static int _EzyPayProviderID;

        public static int EzyPayProviderID

        {
            get => _EzyPayProviderID;

            set => _EzyPayProviderID = value;
        }

        private static int _EzyPayPaytmProviderID;

        public static int EzyPayPaytmProviderID

        {
            get => _EzyPayPaytmProviderID;

            set => _EzyPayPaytmProviderID = value;
        }

        private static int _PrimePayProviderID;

        public static int PrimePayProviderID

        {
            get => _PrimePayProviderID;

            set => _PrimePayProviderID = value;
        }

        private static int _PhonePayProviderID;

        public static int PhonePayProviderID

        {
            get => _PhonePayProviderID;

            set => _PhonePayProviderID = value;
        }

        private static int _HyptoProviderID;

        public static int HyptoProviderID

        {
            get => _HyptoProviderID;

            set => _HyptoProviderID = value;
        }

        private static int _CredoPayProviderID;

        public static int CredoPayProviderID

        {
            get => _CredoPayProviderID;

            set => _CredoPayProviderID = value;
        }

        private static int _ZplusPayoutProviderID;

        public static int ZplusPayoutProviderID

        {
            get => _ZplusPayoutProviderID;

            set => _ZplusPayoutProviderID = value;
        }

        private static int _PayOneMoneyProviderID;

        public static int PayOneMoneyProviderID

        {
            get => _PayOneMoneyProviderID;

            set => _PayOneMoneyProviderID = value;
        }

        private static int _RMGPayProviderIDD;

        public static int RMGPayProviderID

        {
            get => _RMGPayProviderIDD;

            set => _RMGPayProviderIDD = value;
        }

        private static int _RMGPay5LProviderID;

        public static int RMGPay5LProviderID

        {
            get => _RMGPay5LProviderID;

            set => _RMGPay5LProviderID = value;
        }


        private static int _ClareInfoTechProviderID;

        public static int ClareInfoTechProviderID

        {
            get => _ClareInfoTechProviderID;

            set => _ClareInfoTechProviderID = value;
        }

        private static int _RoyalPayProviderID;

        public static int RoyalPayProviderID

        {
            get => _RoyalPayProviderID;

            set => _RoyalPayProviderID = value;
        }

        private static string _CommissionHold;

        public static string CommissionHold

        {
            get => _CommissionHold.TrimIfNotNull();

            set => _CommissionHold = value.TrimIfNotNull();
        }

        private static string _AutoBalanceTransfer;

        public static string AutoBalanceTransfer

        {
            get => _AutoBalanceTransfer.TrimIfNotNull();

            set => _AutoBalanceTransfer = value.TrimIfNotNull();
        }

        private static string _CallbackStatusChangeAllowed;

        public static string CallbackStatusChangeAllowed

        {
            get => _CallbackStatusChangeAllowed.TrimIfNotNull();

            set => _CallbackStatusChangeAllowed = value.TrimIfNotNull();
        }

        private static string _FreeUPIAutoSuccess;

        public static string FreeUPIAutoSuccess

        {
            get => _FreeUPIAutoSuccess.TrimIfNotNull();

            set => _FreeUPIAutoSuccess = value.TrimIfNotNull();
        }

        private static string _PayoutAccountApprovalRequired;

        public static string PayoutAccountApprovalRequired

        {
            get => _PayoutAccountApprovalRequired.TrimIfNotNull();

            set => _PayoutAccountApprovalRequired = value.TrimIfNotNull();
        }

        private static string _BlankTransactionIDAllowed;

        public static string BlankTransactionIDAllowed

        {
            get => _BlankTransactionIDAllowed.TrimIfNotNull();

            set => _BlankTransactionIDAllowed = value.TrimIfNotNull();
        }

        private static string _BackDateFundTransfer;

        public static string BackDateFundTransfer

        {
            get => _BackDateFundTransfer.TrimIfNotNull();

            set => _BackDateFundTransfer = value.TrimIfNotNull();
        }

        private static string _AutoEnableUser;

        public static string AutoEnableUser
        {
            get => _AutoEnableUser.TrimIfNotNull();
            set => _AutoEnableUser = value.TrimIfNotNull();
        }

        public static int RapiPayProviderID { get; set; }

        private static int _MorePayProviderID;

        public static int MorePayProviderID
        {
            get => _MorePayProviderID;
            set => _MorePayProviderID = value;
        }

        private static int _InstantPayPayoutProviderID;

        public static int InstantPayPayoutProviderID
        {
            get => _InstantPayPayoutProviderID;
            set => _InstantPayPayoutProviderID = value;
        }

        private static int _EaseBuzzProviderID;

        public static int EaseBuzzProviderID
        {
            get => _EaseBuzzProviderID;
            set => _EaseBuzzProviderID = value;
        }

        private static int _PaisarewardsProviderID;

        public static int PaisarewardsProviderID

        {
            get => _PaisarewardsProviderID;
            set => _PaisarewardsProviderID = value;
        }

        private static int _DikshupayProviderID;

        public static int DikshupayProviderID

        {
            get => _DikshupayProviderID;
            set => _DikshupayProviderID = value;
        }

        private static int _IServeUProviderID;

        public static int IServeUProviderID

        {
            get => _IServeUProviderID;
            set => _IServeUProviderID = value;
        }

        private static int _IServeULiteProviderID;

        public static int IServeULiteProviderID

        {
            get => _IServeULiteProviderID;
            set => _IServeULiteProviderID = value;
        }

        private static int _SafexPayProviderID;

        public static int SafexPayProviderID

        {
            get => _SafexPayProviderID;
            set => _SafexPayProviderID = value;
        }

        private static int _ShantilexProviderID;

        public static int ShantilexProviderID

        {
            get => _ShantilexProviderID;
            set => _ShantilexProviderID = value;
        }

        private static int _FidyPayProviderID;

        public static int FidyPayProviderID

        {
            get => _FidyPayProviderID;
            set => _FidyPayProviderID = value;
        }
        private static int _CanaraBankProviderID;

        public static int CanaraBankProviderID

        {
            get => _CanaraBankProviderID;
            set => _CanaraBankProviderID = value;
        }

        private static int _PDRSProviderID;

        public static int PDRSProviderID
        {
            get => _PDRSProviderID;
            set => _PDRSProviderID = value;
        }

        private static int _ArchiveDataInDays;

        public static int ArchiveDataInDays
        {
            get => _ArchiveDataInDays;
            set => _ArchiveDataInDays = value;
        }

        private static string _IsLastBalanceChecking;

        public static string IsLastBalanceChecking
        {
            get => _IsLastBalanceChecking.TrimIfNotNull();
            set => _IsLastBalanceChecking = value.TrimIfNotNull();
        }
        private static int _PanMitraProviderID;

        public static int PanMitraProviderID
        {
            get => _PanMitraProviderID;
            set => _PanMitraProviderID = value;
        }
        private static int _PaySwiffProviderID;

        public static int PaySwiffProviderID
        {
            get => _PaySwiffProviderID;
            set => _PaySwiffProviderID = value;
        }

        private static string _ServicewiseSubscription;

        public static string ServicewiseSubscription
        {
            get => _ServicewiseSubscription.TrimIfNotNull();
            set => _ServicewiseSubscription = value.TrimIfNotNull();
        }

        private static string _PlanApiForAPIUser;

        public static string PlanApiForAPIUser
        {
            get => _PlanApiForAPIUser.TrimIfNotNull();
            set => _PlanApiForAPIUser = value.TrimIfNotNull();
        }

        private static string _IsCreditInstitution;

        public static string IsCreditInstitution
        {
            get => _IsCreditInstitution.TrimIfNotNull();
            set => _IsCreditInstitution = value.TrimIfNotNull();
        }
        
        private static string _IsFintechLeadGeneration;

        public static string IsFintechLeadGeneration
        {
            get => _IsFintechLeadGeneration.TrimIfNotNull();
            set => _IsFintechLeadGeneration = value.TrimIfNotNull();
        }

        private static string _BillFetchValidation;

        public static string BillFetchValidation
        {
            get => _BillFetchValidation.TrimIfNotNull();
            set => _BillFetchValidation = value.TrimIfNotNull();
        }
        private static string _IsPGSettlementHold;
        public static string IsPGSettlementHold
        {
            get => _IsPGSettlementHold.TrimIfNotNull();
            set => _IsPGSettlementHold = value.TrimIfNotNull();
        }

        private static string _CallbackStatusSuccesstoFailOnly;
        public static string CallbackStatusSuccessToFailOnly
        {
            get => _CallbackStatusSuccesstoFailOnly.TrimIfNotNull();
            set => _CallbackStatusSuccesstoFailOnly = value.TrimIfNotNull();
        }

        private static decimal _CCFMinimumAmount;
        public static decimal CCFMinimumAmount
        {
            get => _CCFMinimumAmount;
            set => _CCFMinimumAmount = value;
        }
        private static decimal _CCFInPercentage;
        public static decimal CCFInPercentage
        {
            get => _CCFInPercentage;
            set => _CCFInPercentage = value;
        }
    }
}