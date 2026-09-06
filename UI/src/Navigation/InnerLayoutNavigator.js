import React, { lazy } from 'react';

import AccessDenied from 'Pages/InnerPages/AccessDenied';
import Company from 'Pages/InnerPages/Company/Company';
import MenuAdd from 'Pages/InnerPages/MenuMaster/Index';


import { Index as DMTSettings } from 'Pages/InnerPages/DMTSettings/Index';
import { Index as VerificationSettings } from 'Pages/InnerPages/VerificationSetting/index';




import { AdminAccounting } from 'Pages/InnerPages/AdminAccounting/Index';
import { Report as AEPSReport } from 'Pages/InnerPages/AEPS/Report';
import { Report as MicroATMReport } from 'Pages/InnerPages/MicroATM/Report';
import { POSReport } from 'Pages/InnerPages/POS/Report';

import { BalanceCappingUserwise } from 'Pages/InnerPages/BalanceCapping/Index';
import { Add as BBPSConfigurationAdd } from 'Pages/InnerPages/BBPSConfiguration/Add';
import { Edit as BBPSConfigurationEdit } from 'Pages/InnerPages/BBPSConfiguration/Edit';
import { BBPSConfiguration } from 'Pages/InnerPages/BBPSConfiguration/Index';
import { Index as BulkMessageIndex } from 'Pages/InnerPages/BulkMessage/Index';
import { CallbackAdd } from 'Pages/InnerPages/CallbackMaster/CallbackAdd';
import { CallbackEdit } from 'Pages/InnerPages/CallbackMaster/CallbackEdit';


import { CircleAPIAdd } from 'Pages/InnerPages/CircleAPI/Add';

import { Complaints } from 'Pages/InnerPages/Complaints/Index';

import { DistributorDashboardV2 } from 'Pages/InnerPages/DashBoard/DistributorDashboardV2';
import { RetailerDashboard } from 'Pages/InnerPages/DashBoard/RetailerDashboard';
import { ProviderDayBookReport } from 'Pages/InnerPages/DayBookReport/ProviderDayBook';
import { DaybookReport } from 'Pages/InnerPages/DayBookReport/Select';
import { DenominationAdd } from 'Pages/InnerPages/Denomination/Add';
import { DenominationMapping } from 'Pages/InnerPages/DenominationMapping/Index';
import { DenominationDetails } from 'Pages/InnerPages/DenominationMapping/DenominationDetails';

import { EcollectionSetting } from 'Pages/InnerPages/EcollectionSetting/Index';
import { EmailAdd } from 'Pages/InnerPages/EmailSetting/EmailAdd';
import { EmailEdit } from 'Pages/InnerPages/EmailSetting/EmailEdit';
import { EmailSetting } from 'Pages/InnerPages/EmailSetting/Index';
import { EcollectionReport } from 'Pages/InnerPages/Fund/EcollectionReport';
import { FundRequestIndex } from 'Pages/InnerPages/Fund/Index';
import { Reverse } from 'Pages/InnerPages/Fund/Reverse';
import { ReverseRequest } from 'Pages/InnerPages/Fund/ReverseRequest';
import { ReverseRequestReport } from 'Pages/InnerPages/Fund/ReverseRequestReport';
import { ReverseTransferReport } from 'Pages/InnerPages/Fund/ReverseTransferReport';
import { Transfer } from 'Pages/InnerPages/Fund/Transfer';
import { FundTransferReport } from 'Pages/InnerPages/Fund/TransferReport';
import { MoveFundsReport } from 'Pages/InnerPages/Report/MoveFundsReport';


import { Index as Invoice } from 'Pages/InnerPages/Invoice/Index';
import { Index as InvoiceSettings } from 'Pages/InnerPages/InvoiceSetting/Index';
import { IPAdd } from 'Pages/InnerPages/IPMaster/Add';
import { PendingIP } from 'Pages/InnerPages/IPMaster/SelectPendingIP';
import { KYCApprove } from 'Pages/InnerPages/KYC/index';
import { KYCSetting } from 'Pages/InnerPages/KYC/KycSetting';


import { MessageSetting } from 'Pages/InnerPages/MessageSetting/Index';

import { WebHookSetting } from 'Pages/InnerPages/WebHookSetting/Index';

import { News } from 'Pages/InnerPages/News/Index';
import { NotificationAdd } from 'Pages/InnerPages/Notification/Notification';

import { OperatorUserwise } from 'Pages/InnerPages/OperatorUserwise/Index';
import { OutsideRoleSetting } from 'Pages/InnerPages/OutsideRoleSetting/Index';

import { PGFail } from 'Pages/InnerPages/PaymentGateway/PGFail';
import { PGSuccess } from 'Pages/InnerPages/PaymentGateway/PGSuccess';
import { PTransaction } from 'Pages/InnerPages/PaymentGateway/PTransaction';

import { GatewayTransaction } from 'Pages/InnerPages/PaymentGateway/Transaction';


import { PaymentGateWaySetting } from 'Pages/InnerPages/PaymentGatewaySetting/Index';

import { UserwisePaymentGateway } from 'Pages/InnerPages/PaymentGatewaySetting/UserwisePaymentGateway';
import { AEPSSetting } from 'Pages/InnerPages/AEPSSetting/Index';
import { MicroATMSetting } from 'Pages/InnerPages/MicroATMSetting/Index';
import { POSSetting } from 'Pages/InnerPages/POSSettingMaster/Index';

import { PayoutTransaction } from 'Pages/InnerPages/Payout/PayoutTransaction';
import { PayoutTransactionV1 } from 'Pages/InnerPages/PayoutV1/PayoutTransaction';

import { SmartPayTransaction } from 'Pages/InnerPages/SmartPay/SmartPayTransaction';


import { BeneficiaryReport } from 'Pages/InnerPages/PayoutV1/BeneficiaryReport';



import CircleSwitching from 'Pages/InnerPages/ProviderConfiguration/CircleSwitching';
import { CircleIndex } from 'Pages/InnerPages/ProviderConfiguration/CircleIndex';
import { AddPurchase } from 'Pages/InnerPages/ProviderPurchase/Add';
import { ProviderResponseAdd } from 'Pages/InnerPages/ProviderResponse/Add';
import { UserAlias } from 'Pages/InnerPages/Users/UserAlias';

import { Index as PushNotificationsIndex } from 'Pages/InnerPages/PushNotifications/Index';
import { QRCodeGenerate } from 'Pages/InnerPages/QR/QRCodeGenerate';
import { ReconciliationUpload } from 'Pages/InnerPages/Reconciliation/Upload';
import { ReconciliationSettingsAdd } from 'Pages/InnerPages/ReconciliationSettings/Add';
import { ReconciliationSettingsEdit } from 'Pages/InnerPages/ReconciliationSettings/Edit';
import { ReconciliationSettings } from 'Pages/InnerPages/ReconciliationSettings/Index';

import { SmsReport } from 'Pages/InnerPages/SentSms/Index';
import { SequenceUserWiseAdd } from 'Pages/InnerPages/SequenceUserWise/SequenceUserWiseAdd';
import { UserWiseDMTSequenceAdd } from 'Pages/InnerPages/SequenceUserWise/UserwiseDMTSequenceAdd';

import { UserwiseSequenceIndex } from 'Pages/InnerPages/SequenceUserWise/Index';
import { UserWiseCircleAdd } from 'Pages/InnerPages/CircleWiseUserWiseOperatorOff/UserwiseCircleAdd';
import { UserwiseCircleIndex } from 'Pages/InnerPages/CircleWiseUserWiseOperatorOff/Index';

import { ServiceChargeDeductionReport } from 'Pages/InnerPages/ServiceChargeDeduction/Report';
import { Services } from 'Pages/InnerPages/Services/Index';
import { UserwiseServiceCharge } from 'Pages/InnerPages/Services/UserwiseServiceCharge';
import { ServiceSubscriptionCharge } from 'Pages/InnerPages/ServiceSubscriptionCharge';
import { CreatePlan } from 'Pages/InnerPages/ServiceSubscriptionCharge/CreatePlan';
import { CreatePlanIndex } from 'Pages/InnerPages/ServiceSubscriptionCharge/CreatePlanIndex';
import { CreatePlanEdit } from 'Pages/InnerPages/ServiceSubscriptionCharge/CreatePlanEdit';
import { AssignPlan } from 'Pages/InnerPages/ServiceSubscriptionCharge/AssignPlan';
import { Subscription } from 'Pages/InnerPages/ServiceSubscriptionCharge/Subscription';

import { PendingSubcriptionCharge } from 'Pages/InnerPages/Report/PendingSubscriptionCharge';
import { SubscriptionPendingPayment } from 'Pages/InnerPages/ServiceSubscriptionCharge/SubscriptionPendingPayment';
import { SelectSettingMaster } from 'Pages/InnerPages/SettingMaster/Index';
import { SettingEdit } from 'Pages/InnerPages/SettingMaster/SettingEdit';
import { AdjustCommission } from 'Pages/InnerPages/Slabs/AdjustCommissions';
import { RangeWiseSequence } from 'Pages/InnerPages/Sequence/RangeWiseSequence';

import { Index as SliderIndex } from 'Pages/InnerPages/Slider/Index';
import { SMSAdd } from 'Pages/InnerPages/SMS/SMSAdd';
import { SupportAccess } from 'Pages/InnerPages/SupportUser/Index';
import { Index as ThirdPartyServiceProvider } from 'Pages/InnerPages/ThirdPartyServiceProvider/Index';
import { InstaProvider } from 'Pages/InnerPages/ThirdPartyServiceProvider/InstaProvider';
import { Ticket } from 'Pages/InnerPages/Ticket/Add';
import { TicketReport } from 'Pages/InnerPages/Ticket/Index';
import UISetting from 'Pages/InnerPages/UISettings/Index';
import { UserActionHistory } from 'Pages/InnerPages/UserActionHistory/Index';
import { BankAdd } from 'Pages/InnerPages/UserBankDetail/UserBankDetailAdd';
import { IPRegistration } from 'Pages/InnerPages/UserOnbording/IPRegistration';

import { IPRegistrationOTP } from 'Pages/InnerPages/UserOnbording/IPRegistrationOTP';
import { IPChangeMobile } from 'Pages/InnerPages/UserOnbording/IPChangeMobile';
import { IPChangeMoblieVerify } from 'Pages/InnerPages/UserOnbording/IPChangeMoblieVerify';
import DownlineReport from 'Pages/InnerPages/Users/DownlineReport';
import InActiveUsers from 'Pages/InnerPages/Users/InActiveUsers';
import UserWiseBulkAction from 'Pages/InnerPages/Users/UserWiseBulkAction';
import UserWiseBulkActionSDAndDist from 'Pages/InnerPages/Users/UserWiseBulkActionSDAndDist';

import DeletedUsers from 'Pages/InnerPages/Users/DeletedUsers';

import { VirtualBalanceAdd } from 'Pages/InnerPages/VirtualBalance/Add';
import WalletTransfer from 'Pages/InnerPages/WalletTransfer/WalletTransfer';
import RetailerToAllFundTransfer from 'Pages/InnerPages/Fund/RetailerToAllFundTransfer';
import InstantPayeKYC from 'Pages/InnerPages/eKYC/InstantPay_eKYC';
import PaysprinteKYC from 'Pages/InnerPages/eKYC/Paysprint_eKYC';
import YesbankeKYC from 'Pages/InnerPages/eKYC/YesBank/Yesbank_eKYC';
import Yesbank_eKYC_BiomatricVerifcation from 'Pages/InnerPages/eKYC/YesBank/Yesbank_eKYC_BiomatricVerifcation';
import QuickeKYC from 'Pages/InnerPages/eKYC/Quick_eKYC';
import { IndexPaysprint } from 'Pages/InnerPages/eKYC/IndexPaysprint_eKYC';

import IndexCashFree from 'Pages/InnerPages/eKYC/Cashfree/IndexCashfree';
import { PADRSAadhar } from 'Pages/InnerPages/eKYC/PDRS_Aadhar';

import { FingpayInsuranceInitiate } from 'Pages/InnerPages/Insurance/FingpayInsuranceInitiate';


import InnerLayout from 'Pages/Layouts/InnerLayout';
import InnerLayoutRoute from 'Pages/Layouts/InnerLayoutRoute';
import { Route } from 'react-router-dom';


import { Add } from 'Pages/InnerPages/ChannelSlab/Add';
import { AssignEdit } from 'Pages/InnerPages/ChannelSlab/ChannalSlabAssignEdit';
import { ChannelSlabAssign } from 'Pages/InnerPages/ChannelSlab/ChannelSlabAssign';
import { ChannelSlabCommission } from 'Pages/InnerPages/ChannelSlab/ChannelSlabCommission';
import { DMTChannelSlabCommission } from 'Pages/InnerPages/ChannelSlab/DMTChannelSlabCommission';
import { AEPSIndex } from 'Pages/InnerPages/AEPS/AEPSIndex';






import { AdminDashboardV2 } from 'Pages/InnerPages/DashboradV2';
import { Request } from 'Pages/InnerPages/Fund/Request';
import { Upload } from 'Pages/InnerPages/KYC/Upload';

import { HLR } from 'Pages/InnerPages/Operators/HLR';
import { HLREdit } from 'Pages/InnerPages/Operators/HLREdit';
import { Operators } from 'Pages/InnerPages/Operators/Index';
import { OperatorAdd } from 'Pages/InnerPages/Operators/OperatorAdd';
import { OperatorEdit } from 'Pages/InnerPages/Operators/OperatorEdit';
import { AddRemainderOperatorAmount } from 'Pages/InnerPages/OperatorAmountWiseRemainderSetting/Add';






import { ProviderConfigSetup } from 'Pages/InnerPages/ProviderConfiguration/Index';
import { ProviderOperatorEdit } from 'Pages/InnerPages/ProviderConfiguration/ProviderConfigurationEdit';
import { ProviderDetailAdd } from 'Pages/InnerPages/ProviderDetails/ProviderDetailAdd';
import { ProviderDetailEdit } from 'Pages/InnerPages/ProviderDetails/ProviderDetailEdit';
import { ProviderAdd } from 'Pages/InnerPages/Providers/ProviderAdd';
import { ProviderEdit } from 'Pages/InnerPages/Providers/ProviderEdit';
import { Edit as SequenceEdit } from 'Pages/InnerPages/Sequence/Edit';
import { SequenceAdd } from 'Pages/InnerPages/Sequence/SequenceAdd';
import { DMTSlabCommission } from 'Pages/InnerPages/Slabs/DMTSlabCommission';

import { SeviceswiseWallet } from 'Pages/InnerPages/Services/SeviceswiseWallet';
import { ROfferCommissionAdd } from 'Pages/InnerPages/Slabs/ROfferCommissionAdd';

import { SlabAdd } from 'Pages/InnerPages/Slabs/SlabAdd';
import { SlabAssign } from 'Pages/InnerPages/Slabs/SlabAssign';
import { SlabAssignEdit } from 'Pages/InnerPages/Slabs/SlabAssignEdit';
import { SlabCommission } from 'Pages/InnerPages/Slabs/SlabCommission';
import { Transaction } from 'Pages/InnerPages/Transaction/Index';
import { BBPSTransaction } from 'Pages/InnerPages/Transaction/BBPSTransaction';
import { UpdatePassword } from 'Pages/InnerPages/Users/ChangePassword';
import { DatewiseUserReport } from 'Pages/InnerPages/Users/DatewiseUserReport';
import { IDFYProfileReport } from 'Pages/InnerPages/Users/IDFYProfileReport';
import { FailedSignUpUsers } from 'Pages/InnerPages/Users/FailedSignUpUsers';
import { Users } from 'Pages/InnerPages/Users/Index';
import { SelectProfile } from 'Pages/InnerPages/Users/Profile';
import { UserAdd } from 'Pages/InnerPages/Users/UserAdd';
import { UserEdit } from 'Pages/InnerPages/Users/UserEdit';
import UserTransfer from 'Pages/InnerPages/Users/UserTransfer';
import { UserwiseSaleTarget } from 'Pages/InnerPages/Users/UserwiseSaleTarget';
import { UserwiseSaleTargetReport } from 'Pages/InnerPages/Users/UserwiseSaleTargetReport';

import { TwoFA } from 'Pages/InnerPages/Users/2FA';

import { DigitalCertificateIndex } from 'Pages/InnerPages/Certificate/DigitalCertificateIndex';
import { VersionHistory } from 'Pages/InnerPages/VersionHistory/Index';

import { Index as ServiceWiseSliderIndex } from 'Pages/InnerPages/ServiceWiseSlider/Index';
import { VirtualAccountSetting } from 'Pages/InnerPages/VirtualAccount/Index';

import { PlanSetting } from 'Pages/InnerPages/PlanSetting/Index';

import { UserwiseLowBalance } from 'Pages/InnerPages/UserwiseLowBalance/Index';
import { DMTBankSetting } from 'Pages/InnerPages/DMTSettings/DMTBankSetting';
import { DMTTransaction } from 'Pages/InnerPages/Transaction/DMTTransaction';





import { YesBankAEPS } from 'Pages/InnerPages/AEPS/YesBankAEPS/YesBankAEPS';
import { YesBankAEPSAuthentication } from 'Pages/InnerPages/AEPS/YesBankAEPS/YesBankAEPSAuthentication';





import { DthCategoryChannel } from 'Pages/InnerPages/CategoryChannelMapping/Index';
import { DthPackageChannel } from 'Pages/InnerPages/DthPackageChannelMapping/Index';




import { PayoutSetting } from 'Pages/InnerPages/PayoutSetting/Index';
import { BulkPaySetting } from 'Pages/InnerPages/BulkPaySetting/Index';

import { RichTextMedia } from 'Pages/InnerPages/RichTextMedia/Index';


import { EKYCSetting } from 'Pages/InnerPages/eKYCSetting/Index';

import { UserWisePlanLimitSetting } from 'Pages/InnerPages/UserWisePlanLimitSetting/index';


import { KycIndex } from 'Pages/InnerPages/KYC/KycIndex';

import { Approve } from 'Pages/InnerPages/KYC/Approve';


import ManualSuccessFailReport from 'Pages/InnerPages/Report/ManualSuccessFailReport';
import PastSuccessFailReport from 'Pages/InnerPages/Report/PastSuccessFailReport';

import LiveTransaction from 'Pages/InnerPages/Report/LiveTransactions';
import { DMTReport } from 'Pages/InnerPages/Report/DMTReport';
import { IDPurchaseReport } from 'Pages/InnerPages/Report/IDPurchaseReport';
import { PaymentGatewayLog } from 'Pages/InnerPages/Report/PaymentGatewayLogReport';
import { IncomeReport } from 'Pages/InnerPages/Report/IncomeReport';
import { DMTResendWallet } from 'Pages/InnerPages/Report/DMTResendWallet';
import { RangewiseDMT } from 'Pages/InnerPages/Report/RangeWiseDMTReport';
import { UserWiseDMTRangeReport } from 'Pages/InnerPages/Report/UserWiseDMTRangeReport';

import { RangeWiseRecharge } from 'Pages/InnerPages/Report/RangeWiseRecharge';

import { AdminLedger } from 'Pages/InnerPages/Report/AdminLedger';
import { APIUserInitialLog } from 'Pages/InnerPages/Report/APIUserInitialLog';
import { GSTSummary } from 'Pages/InnerPages/Report/GSTsummary';
import { GSTHold } from 'Pages/InnerPages/Report/GSTHold';
import { UserReferralReport } from 'Pages/InnerPages/Report/UserReferralReport';
import { OperatorWiseUserWiseReport } from 'Pages/InnerPages/Report/OperatorWiseUserWiseReport';
import { ReferredUserReport } from 'Pages/InnerPages/Report/ReferredUserReport';

import { ReferAndEarn } from 'Pages/InnerPages/ReferAndEarn/Index';

import { LedgerReport } from 'Pages/InnerPages/Report/Ledger';
import { SelectByIDLog } from 'Pages/InnerPages/Report/LogByTransactionMasterID';
import { NPerformance } from 'Pages/InnerPages/Report/NPerformance';
import { PaymentGatewayReport } from 'Pages/InnerPages/Report/PaymentGatewayReport';
import { PayoutReport } from 'Pages/InnerPages/Report/PayoutReport';
import { VendorPayReport } from 'Pages/InnerPages/Report/VendorPayReport';
import { UserWisePlanViewReport } from 'Pages/InnerPages/Report/UserWisePlanViewReport';

import { Loan } from 'Pages/InnerPages/DealOffLoan/Index';

import { ProviderWisePerformanceGraph } from 'Pages/InnerPages/Report/ProviderWisePerformanceGraph';
import { OTTReport } from 'Pages/InnerPages/Report/OTTReport';


import { PendingDmt } from 'Pages/InnerPages/Report/PendingDmt';
import { PendingRecharge } from 'Pages/InnerPages/Report/PendingRecharge';

import { PendingRecon } from 'Pages/InnerPages/Report/PendingRecon';

import { ProviderLog } from 'Pages/InnerPages/Report/ProviderRequestResponse';
import { ViewAgentOnboarding } from 'Pages/InnerPages/IdfyUserOnboarding/ViewAgentOnboarding';
import { ManualAgentOnboarding } from 'Pages/InnerPages/IdfyUserOnboarding/ManualCreateOnboarding/ManualAgentOnboarding';

import { ProviderSale } from 'Pages/InnerPages/Report/ProviderSale';

import { ProviderWiseFailReport } from 'Pages/InnerPages/Report/ProviderWiseFailReason';
import { RetailerSearch } from 'Pages/InnerPages/Report/QuickSearch';
import { ActiveReport } from 'Pages/InnerPages/Report/ActiveReport';

import { Search } from 'Pages/InnerPages/Report/SearchConsumerNumber';
import { ServicewiseSale } from 'Pages/InnerPages/Report/ServicewiseSale';
import { SlabDetailByUser } from 'Pages/InnerPages/Report/SlabDetailByUser';
import { TransactionDetailReport } from 'Pages/InnerPages/Report/TransactionDetailReport';
import { SettlementReport } from 'Pages/InnerPages/Report/SettlementReport';


import { ProviderCommission } from 'Pages/InnerPages/Slabs/ProviderCommission';
import { ProviderStateConfigSetup } from 'Pages/InnerPages/ProviderStateConfiguration/Index';


import { CircleWiseOperatorLimitAdd } from 'Pages/InnerPages/CircleWiseOperatorLimit/CircleWiseOperatorLimitAdd';
import { OperatorLimitIndex } from 'Pages/InnerPages/CircleWiseOperatorLimit/Index';
import { CircleWiseCommission } from 'Pages/InnerPages/Slabs/CircleWiseCommission';
import { AddOperatorAmount } from 'Pages/InnerPages/OperatorAmountWiseSetting/Add';
import { ROfferRechargeReport } from 'Pages/InnerPages/Report/ROfferRechargeReport';
import { BBPSReport } from 'Pages/InnerPages/Report/BBPSReport';
import { AgentDetailReport } from 'Pages/InnerPages/Report/AgentDetailReport';





import YesBankSenderSearch from 'Pages/InnerPages/YesBankDMT/SenderSearch';

import { UserCreationSetting } from 'Pages/InnerPages/UserCreationSetting/Index';
import { IDPurchase } from 'Pages/InnerPages/UserCreation/Transaction';
import { FreeIDTransfer } from 'Pages/InnerPages/UserCreation/FreeIDTransfer';



import { UserWiseServiceSellReport } from 'Pages/InnerPages/Report/UserWiseServiceSellReport';
import { AddVoucher } from 'Pages/InnerPages/Voucher/AddVoucher';
import { ApproveVoucher } from 'Pages/InnerPages/Voucher/ApproveVoucher';
import { ReportVoucher } from 'Pages/InnerPages/Voucher/ReportVoucher';

import { UpdateAgent } from 'Pages/InnerPages/IdfyUserOnboarding/UpdateAgent';

const SenderAdd = lazy(() => import('Pages/InnerPages/YesBankDMT/SenderAdd'));

const BeneficiaryAdd = lazy(() =>
    import('Pages/InnerPages/YesBankDMT/BeneficiaryAdd')
);

const BeneficiaryVerify = lazy(() =>
    import('Pages/InnerPages/YesBankDMT/BeneficiaryVerify')
);



const UserwiseAccounting = lazy(() =>
    import('Pages/InnerPages/Report/UserwiseAccounting')
);
const UserwiseSale = lazy(() => import('Pages/InnerPages/Report/UserWiseSale'));
const OperatorSale = lazy(() => import('Pages/InnerPages/Report/OperatorSale'));
const SubscriptionChargeReport = lazy(() =>
    import('Pages/InnerPages/Report/SubscriptionChargeReport')
);

const AccountStatementReport = lazy(() =>
    import('Pages/InnerPages/Report/AccountStatementReport')
);

const SubscriptionPaymentReport = lazy(() =>
    import('Pages/InnerPages/Report/SubscriptionPaymentReport')
);

const ReporteKyc = lazy(() => import('Pages/InnerPages/eKYC/Report'));

const LoginHistory = lazy(() => import('Pages/InnerPages/Report/LoginHistory'));
const TDSReport = lazy(() => import('Pages/InnerPages/Report/TDSReport'));
const OperatorWiseGST = lazy(() =>
    import('Pages/InnerPages/Report/OperatorWiseGST')
);





const PurchaseReport = lazy(() =>
    import('Pages/InnerPages/Report/PurchaseReport')
);
const ChangeStatus = lazy(() => import('Pages/InnerPages/Report/ChangeStatus'));



const TwoFAReport = lazy(() => import('Pages/InnerPages/Report/TwoFAReport'));





const PayoutIndex = lazy(() => import('Pages/InnerPages/Payout/PayoutIndex'));

const PayoutIndexV1 = lazy(() =>
    import('Pages/InnerPages/PayoutV1/PayoutIndex')
);





const UserOnbordedReport = lazy(() =>
    import('Pages/InnerPages/Report/UserOnbordedReport')
);
const UserOnbordedStatusCheck = lazy(() =>
    import('Pages/InnerPages/Report/UserOnbordedStatusCheck')
);


const WalletTopUpIndex = lazy(() =>
    import('Pages/InnerPages/WalletTopUp/Index')
);
const PaysprintWalletTopUp = lazy(() =>
    import('Pages/InnerPages/WalletTopUp/PaysprintWalletTopUp')
);
const WalletTopUpReport = lazy(() =>
    import('Pages/InnerPages/Report/WalletTopUpReport')
);







const SenderBeneficiaryReport = lazy(() =>
    import('Pages/InnerPages/SenderBeneficiary/SenderBeneficiaryReport')
);



const InnerLayoutNavigator = () => {
    const RoutePath = [


        '/CFTransaction',
        '/EBTransaction',
        '/WalletTopUpIndex',
        '/PSWalletTopUp',
        '/WalletTopUpReport',
        '/OBPGTransaction',
        '/ProviderCommission',
        '/ProviderStateConfigSetup',
        '/TDS',

        '/PSRedirectURL',



        '/PayoutSetting',
        '/BulkPaySetting',

        '/RichTextMedia',
        '/OTTSetting',

        '/EAgentActivation',

        '/FPUserOnbording',
        '/FPValidateOTP',
        '/FPBiomatric',
        '/FPSendOTP',
        '/ViewAgentOnboarding',
        '/ManualAgentOnboarding',
        '/PSPayout',
        '/PayoutTransaction',
        '/PayoutTransactionV1',


        '/PayoutV1',
        '/UserwiseAccounting',
        '/PayoutBeneficiaryReport',

        '/IMPPayout',
        '/JJPayoutTransfer',
        '/EzyPayPayoutTransfer',
        '/PPPayout',
        '/ZPPayout',

        '/ACUserOnbording',
        '/ACValidateOTP',
        '/ACBiomatric',
        '/ACSendOTP',
        '/ACAEPS',



        '/OperatorWiseGST',
        '/PurchaseReport',

        '/AEPSIndex',
        '/Income',

        '/FPAuthentication',
        '/PSOnbording',

        '/PSPOnbording',


        '/IPMOnbording',


        '/MRegistration',


        '/PaysprintURLRedirect',
        '/URLRedirect',
        '/AdminDashboard',
        '/Search',
        '/Users/UserAlias',
        '/RetailerSearch',

        '/Pending',
        '/PendingDmt',

        '/PendingRecon',
        '/DMTResendWallet',
        '/Recharge',
        '/BBPSReport',
        '/AgentDetailReport',
        '/ROfferRechargeReport',
        '/RangewiseDMT',
        '/UserWiseDMTRangeReport',
        '/RangeWiseRecharge',
        '/TransactionDetail',
        '/AccountStatement',
        '/SettlementReport',
        '/Dashboard/Retailer',
        '/Fund',
        '/Fund/Request',
        '/Fund/Transfer',
        '/FundReverse',
        '/MoveFundsReport',
        '/Fund/Report',
        '/Transaction',
        '/BBPSTransaction',
        '/DMTTransaction',
        '/DMTTransaction2',



        '/PS_AccountOpen',
        '/UserOnbording',

        '/CP_UPITransfer',

        '/KYC',
        '/KYC/Upload',
        '/Users',
        '/Users/Approve',
        '/Users/Add',
        '/Users/DatewiseReport',
        '/IDFYProfile',
        '/MISInterestedAgents',
        '/Users/DeletedUsers',
        '/Users/Edit',
        '/ViewAgentOnboarding/Edit',
        '/UsersProfile',
        '/Subscription',
        '/Users/ChangePassword',
        '/Users/ChangeMpin',
        '/Users/Index',
        '/Users/UserTransfer',
        '/Users/UserwiseSaleTarget',
        '/Users/UserwiseSaleTargetReport',
        '/Services',
        '/Services/Index',
        '/Services/Add',
        '/Services/Edit',
        '/Operators',
        '/Operators/Index',
        '/Operators/Add',
        '/Operators/Edit',
        '/Operators/HLR',
        '/Operators/HLR/Edit',
        '/Providers',
        '/ProvidersAdd',
        '/Providers/Edit',
        '/ProvidersAdd/Add',

        '/ProviderDetails/Edit',
        '/ProviderConfiguration/Index',
        '/ProviderConfiguration/Edit',
        '/ProviderConfiguration',
        '/Slabs',
        '/Slabs/Edit',
        '/Slabs/Assign',
        '/Slabs/SlabAssignEdit',
        '/Slabs/Commission',
        '/Slabs/DMTCommission',
        '/ChannelSlabs/Commission',
        '/ChannelSlabs/DMTCommission',
        '/ProvidersSequence',
        '/Sequence',
        '/Sequence/Edit',
        '/Sequence/UserWise',
        '/UserWiseDMTSequenceAdd',
        '/PSTransaction',
        '/ROfferCommission',
        '/MDMT/Sender',
        '/MDMT/Sender/Search',
        '/MDMT/Transfer',
        '/MDMT/Sender/Add',
        '/MDMT/Sender/Verify',
        '/MDMT/Beneficiary/Add/',
        '/MDMT/Beneficiary/Verify/',

        '/AD_SenderSearch',
        '/AD_SenderAdd',
        '/AD_SenderVerify',
        '/AD_BeneficiaryAdd',
        '/AD_BeneficiaryVerify',



        '/PDMT/Transfer',
        '/PDMTSender',
        '/PDMTSender/Search',
        '/PDMTSender/Add',
        '/PDMTSender/Verify',
        '/PDMTBeneficiaryAdd/',
        '/PDMTBeneficiary/Verify/',

        '/ICashDMT/Transfer',
        '/ICashDMT/Sender',
        '/ICashDMT/Sender/Search',
        '/ICashDMT/Sender/Add',
        '/ICashDMT/Sender/Verify',
        '/ICashDMT/Beneficiary/Add/',
        '/ICashDMT/Beneficiary/Verify/',

        '/OB_Sender',
        '/OB_SenderAdd',
        '/OB_SenderVerify',
        '/OB_BeneficiaryAdd',
        '/OB_BeneficiaryVerify',



        // EzyPay
        '/EZPSenderSearch',
        '/EZPSenderAdd',
        '/EZPSenderVerify',
        '/EZPBeneficiaryAdd',
        '/EZPBeneficiaryVerify',

        //EzyPayV1
        '/EZPV1SenderSearch',
        '/EZPV1SenderAdd',
        '/EZPV1SenderVerify',
        '/EZPV1BeneficiaryAdd',
        '/EZPV1BeneficiaryVerify',

        //YesBank
        '/YBDMT_SenderSearch',
        '/YBDMT_SenderAdd',
        '/YBDMT_BeneficiaryAdd',
        '/YBDMT_BeneVerify',

        // EzyPaytmPay
        '/EZPTMSenderSearch',
        '/EZPTMSenderAdd',
        '/EZPTMSenderVerify',
        '/EZPTMBeneficiaryAdd',
        '/EZPTMBeneficiaryVerify',

        //InstantPay
        '/I_SenderSearch',
        '/I_SenderAdd',
        '/I_SenderVerify',
        '/I_BeneficiaryAdd',
        '/I_BeneficiaryDelete',

        //PayU
        '/PU_SenderSearch',
        '/PU_SenderAdd',
        '/PU_SenderVerify',
        '/PU_BeneficiaryAdd',
        '/PU_BeneficiaryVerify',

        //Paysprint
        '/PS_SenderSearch',
        '/PS_SenderAdd',
        '/PS_BeneficiaryAdd',

        //Icicicomposite
        '/IC_SenderAdd',
        '/IC_SenderVerify',
        '/IC_BeneAdd',
        '/IC_SenderSearch',
        '/IC_BeneVerify',

        //RazorpayDMT
        '/RP_SenderAdd',
        '/RP_SenderVerify',
        '/RP_BeneAdd',
        '/RP_SenderSearch',
        '/RP_BeneVerify',

        //JaiJui
        '/JJ_SenderAdd',
        '/JJ_SenderVerify',
        '/JJ_BeneAdd',
        '/JJ_SenderSearch',
        '/JJ_BeneVerify',
        //InstantPayPayout
        '/IP_SenderAdd',
        '/IP_SenderVerify',
        '/IP_BeneAdd',
        '/IP_SenderSearch',
        '/IP_BeneVerify',

        //PrimePay
        '/PP_SenderAdd',
        '/PP_SenderVerify',
        '/PP_BeneAdd',
        '/PP_SenderSearch',
        '/PP_BeneVerify',

        //Hypto
        '/H_SenderAdd',
        '/H_SenderVerify',
        '/H_BeneAdd',
        '/H_SenderSearch',
        '/H_BeneVerify',

        //Zplus Payout
        '/ZPP_SenderAdd',
        '/ZPP_SenderVerify',
        '/ZPP_BeneAdd',
        '/ZPP_SenderSearch',
        '/ZPP_BeneVerify',

        //Zplus
        '/ZP_DMTSenderSearch',
        '/ZP_SenderAdd',
        '/ZP_SenderVerify',
        '/ZP_BeneficiaryAdd',





        //Round Pay
        '/ROP_SenderAdd',
        '/ROP_SenderVerify',
        '/ROP_BeneAdd',
        '/ROP_BeneVerify',



        //VastWeb DMT
        '/VW_SenderAdd',
        '/VW_SenderVerify',
        '/VW_BeneAdd',

        //LevinDMT
        '/LF_SenderAdd',
        '/LF_SenderVerify',
        '/LF_BeneAdd',

        //IMPSGuru DMT
        '/IG_BeneAdd',
        '/IG_SenderAdd',



        //PAYTM CCF
        '/PC_SenderSearch',
        '/PC_SenderAdd',
        '/PC_SenderVerify',
        '/PC_BeneficiaryAdd',
        '/PC_BeneficiaryVerify',
        '/PC_BeneficiaryDelete',

        //WhiteLabelPAYTM CCF
        '/WPC_SenderSearch',
        '/WPC_SenderAdd',
        '/WPC_BeneficiaryDelete',
        '/WPC_BeneficiaryAdd',
        '/WPC_BeneficiaryVerify',

        //Mahagram
        '/MG_SenderSearch',
        '/MG_SenderAdd',
        '/MG_SenderVerify',
        '/MG_BeneficiaryAdd',
        '/MG_BeneficiaryVerify',
        '/MG_BeneGetOTP',

        //MasterMoney
        '/MMoney_SenderSearch',
        '/MMoney_SenderAdd',
        '/MMoney_BeneficiaryAdd',



        //MyNetLink

        '/MNLDMT_Transfer',
        '/MNLDMTSender',
        '/MNLDMTSender_Search',
        '/MNLDMTSender_Add',
        '/MNLDMTBeneficiaryAdd',

        //Novity5L
        '/NT5LDMT_SenderSearch',
        '/NT5LDMT_AgentRegistration',
        '/NT5LDMT_SenderAdd',
        '/NT5LDMT_BeneVerify',
        '/NT5LDMT_SenderVerify',
        '/NT5LDMT_BeneficiaryAdd',
        '/NT5LDMT_BeneDeleteVerify',

        //Novity Common
        '/NTDMT_SenderSearch',
        '/NTDMT_SenderAdd',
        '/NTDMT_SenderVerify',
        '/NTDMT_BeneficiaryAdd',
        '/NTDMT_BeneficiaryVerify',
        '/NTDMT_AgentRegistration',
        '/NTDMT_BeneVerify',
        '/NTDMT_BeneDeleteVerify',
        '/OperatorWiseUserWiseReport',
        '/UserReferralReport',
        '/ReferredUserReport',
        '/ReferAndEarn',

        '/IPGTransaction',
        '/PaymentGatewayTransaction',

        '/PGSuccess',
        '/PGFail',
        '/RTransaction',
        '/PaymentGatewayReport',
        '/PTransaction',


        '/UPIGatewayTransaction',
        '/PaymentGatewayLog',
        '/FPAEPS',
        '/IPRegistrationOTP',
        '/IPRegistration',

        '/IPChangeMobile',
        '/IPChangeMoblieVerify',
        '/QRCodeGenerate',
        '/DMTReport',
        '/ResponseAdd',
        '/Daybook',
        '/Daybook/Report',
        '/AEPSReport',
        '/MicroATMReport',
        '/POSReport',
        '/IPAdd',
        '/CallbackAdd',
        '/Log',
        '/Log/Report',
        '/ApiUserLog',
        '/CallbackMaster/Edit',
        '/CommissionStructure',
        '/KeySetting',
        '/KeyEdit',
        '/SMSAdd',
        '/MessageSetting',
        '/WebHookSetting',


        '/Complaints',
        '/IDPurchaseReport',
        '/AddBank',
        '/VirtualBalance',
        '/VirtualBalance/Add',
        '/SmsReport',
        '/OperatorUserwise',
        '/UserwiseServiceCharge',
        '/ServiceSubscriptionCharge',
        '/CreatePlan',
        '/CreatePlanIndex',
        '/AssignPlan',
        '/Plan/Edit',
        '/PendingSubcriptionCharge',
        '/SubscriptionChargeReport',
        '/SubscriptionPaymentReport',

        '/AccountStatementReport',
        '/SubscriptionPendingPayment',
        '/UserWiseBulkActionSDAndDist',
        '/SupportAccess',
        '/UserActionHistory',
        '/FundReverseRequestReport',
        '/Fund/ReverseRequest',
        '/Fund/ReverseTransferReport',
        '/ServiceChargeDeductionReport',
        '/CircleAPIAdd',
        '/WalletTransfer',
        '/SaleReport',
        '/LoginHistory',
        '/OperatorSale',
        '/Notification',



        '/LiveTransaction',

        '/ICICI_CompositeDMTDoc',



        '/CircleSwitching',

        '/ProviderLog',

        '/PendingIP',
        '/News/Index',

        '/EmailSetting',
        '/EmailSetting/Index',
        '/EmailSetting/Add',
        '/EmailSetting/Edit',
        '/DenominationDetails/Denomination',
        '/LiveTransaction',
        '/ReconciliationSettings',
        '/ReconciliationSettings/Index',
        '/ReconciliationSettings/Add',
        '/ReconciliationUpload',
        '/BBPSConfiguration',
        '/BBPSConfiguration/Add',
        '/BBPSConfiguration/Edit',
        '/BBPSConfiguration/Index',
        '/DMTSettings',
        '/DMTSettings/Index',
        '/DematSetting',

        '/AccountOpenSetting',
        '/UserWisePlanLimitSetting',


        '/VerificationSettings',
        '/ThirdPartyServiceProvider',
        '/ThirdPartyServiceProvider/Index',

        '/PayoutReport',
        '/Slider',
        '/DenominationDetails/DenominationMapping',
        '/UserLedger',
        '/UserWiseServiceSellReport',
        '/AdminLedger',
        '/ChannelSlabs',
        '/ChannelSlabAssign',
        '/ChannelSlabs/AssignEdit',
        '/MenuAdd',

        '/PushNotifications',

        '/AddPurchase',
        '/ProviderDayBook',
        '/ProviderSale',

        '/BulkMessageIndex',
        '/Payout',
        '/InvoiceSettings',
        '/InvoiceSettings/Index',
        '/PaymentGateWaySetting',
        '/UserwisePaymentGateway',

        '/ProviderWisePerformanceGraph',
        '/OTTReport',


        '/AEPSSetting',
        '/MicroATMSetting',

        '/Fund/GatewayTransaction',
        '/Invoice',
        '/GSTHold',
        '/GSTSummary',

        'Users/Inactive',
        '/RetailerDashboard',
        '/ClientDashboard',
        '/DistributorDashboard',
        '/SuperDistributorDashboard',

        '/KYCSetting',
        '/AdjustCommission',
        '/RangeWiseSequence',
        '/AccessDenied',
        '/EcollectionReport',
        '/ManualSuccessFailReport',
        '/PastSuccessFailReport',
        '/DownlineReport',
        '/Transaction/ThirdPartyService',
        '/UISetting',
        '/UserwiseSale',
        '/Company',
        '/EcollectionSetting',
        '/Ticket',
        '/TicketReport',
        '/Performance',
        '/OutsideRoleSetting',
        '/BalanceCapping',

        '/AdminAccounting',

        '/TwoFA',
        '/AddVoucher',
        '/ApproveVoucher',

        '/ReportVoucher',
        '/VersionHistory',

        '/DigitalCertificateIndex',
        '/DenominationDetails',
        '/ServiceWiseSlider',
        '/VirtualAccountSetting',
        '/CircleIndex',
        '/UserwiseLowBalance',
        '/PlanSetting',

        '/CMSDMT',
        '/DMTBankSetting',
        '/SenderBeneficiaryReport',
        '/SeviceswiseWallet',
        '/DthCategoryChannel',
        '/DthPackageChannel',
        '/KYC/Approve',
        '/ChangeStatus',

        '/UserOnboardedReport',
        '/UserOnbordedStatusCheck',
        '/RetailerToAllFundTransfer',
        '/InstantPayeKYC',

        '/UserwiseSequenceIndex',
        '/UserwiseCircleIndex/UserWiseCircleAdd',
        '/UserwiseCircleIndex',
        '/CircleWiseOperatorLimitAdd',
        '/OperatorLimitIndex',
        '/Slabs/CircleWiseCommission',
        '/EKYCSetting',


        '/KycIndex',


        '/AddOperatorAmount',
        '/AddRemainderOperatorAmount',
        '/PaysprinteKYC',
        '/YesbankeKYC',
        '/Yesbank_eKYC_BiomatricVerifcation',
        '/POSSetting',
        '/EkycPS',
        '/EkycCF',








        '/PSCreateVPA',
        '/PSDynamicQR',

        '/UserCreationSetting',
        '/IDPurchase',
        '/FreeIDTransfer',
        '/ReporteKyc',
        '/SmartPayTransaction',
        '/VendorPayReport',
        '/UserWisePlanViewReport',

        '/LoanRedirect',
        '/Loan',
        '/EkycPD',


        '/EkycQU',

        '/PSAuthenticationManual',


        '/CPAuthentication',
        '/FPInitiate',

        '/YBAEPS',
        '/YBAEPSAuthentication'
    ];
    return (
        <Route path={RoutePath}>
            <InnerLayout>



                <InnerLayoutRoute path="/EkycQU" exact component={QuickeKYC} />


                <InnerLayoutRoute path="/Loan" component={Loan} />
                <InnerLayoutRoute
                    path="/VendorPayReport"
                    component={VendorPayReport}
                />
                <InnerLayoutRoute
                    path="/UserWisePlanViewReport"
                    component={UserWisePlanViewReport}
                />
                <InnerLayoutRoute path="/ReporteKyc" component={ReporteKyc} />
                <InnerLayoutRoute
                    path="/FreeIDTransfer"
                    component={FreeIDTransfer}
                />
                <InnerLayoutRoute path="/IDPurchase" component={IDPurchase} />
                <InnerLayoutRoute
                    path="/UserCreationSetting"
                    component={UserCreationSetting}
                />
                <InnerLayoutRoute
                    path="/UserwiseAccounting"
                    component={UserwiseAccounting}
                />



                <InnerLayoutRoute
                    path="/FPInitiate"
                    exact
                    component={FingpayInsuranceInitiate}
                />

                <InnerLayoutRoute
                    path="/EkycPS"
                    exact
                    component={IndexPaysprint}
                />
                <InnerLayoutRoute
                    path="/EkycCF"
                    exact
                    component={IndexCashFree}
                />
                <InnerLayoutRoute
                    path="/EkycPD"
                    exact
                    component={PADRSAadhar}
                />
                <InnerLayoutRoute
                    path="/Slabs/CircleWiseCommission"
                    component={CircleWiseCommission}
                />
                <InnerLayoutRoute
                    path="/POSSetting"
                    exact
                    component={POSSetting}
                />
                <InnerLayoutRoute path="/KycIndex" exact component={KycIndex} />

                <InnerLayoutRoute
                    path="/PaysprinteKYC"
                    exact
                    component={PaysprinteKYC}
                />
                <InnerLayoutRoute
                    path="/YesbankeKYC"
                    exact
                    component={YesbankeKYC}
                />
                <InnerLayoutRoute
                    path="/Yesbank_eKYC_BiomatricVerifcation"
                    exact
                    component={Yesbank_eKYC_BiomatricVerifcation}
                />
                <InnerLayoutRoute
                    path="/InstantPayeKYC"
                    exact
                    component={InstantPayeKYC}
                />
                <InnerLayoutRoute
                    path="/AddOperatorAmount"
                    exact
                    component={AddOperatorAmount}
                />
                <InnerLayoutRoute
                    path="/AddRemainderOperatorAmount"
                    exact
                    component={AddRemainderOperatorAmount}
                />
                <InnerLayoutRoute
                    path="/EKYCSetting"
                    exact
                    component={EKYCSetting}
                />

                <InnerLayoutRoute
                    path="/OperatorLimitIndex"
                    exact
                    component={OperatorLimitIndex}
                />
                <InnerLayoutRoute
                    path="/CircleWiseOperatorLimitAdd"
                    exact
                    component={CircleWiseOperatorLimitAdd}
                />
                <InnerLayoutRoute
                    path="/UserwiseCircleIndex"
                    exact
                    component={UserwiseCircleIndex}
                />
                <InnerLayoutRoute
                    path="/UserwiseCircleIndex/UserWiseCircleAdd"
                    exact
                    component={UserWiseCircleAdd}
                />

                <InnerLayoutRoute
                    path="/RetailerToAllFundTransfer"
                    exact
                    component={RetailerToAllFundTransfer}
                />
                <InnerLayoutRoute
                    path="/ProviderCommission"
                    exact
                    component={ProviderCommission}
                />
                <InnerLayoutRoute
                    path="/ProviderStateConfigSetup"
                    exact
                    component={ProviderStateConfigSetup}
                />
                <InnerLayoutRoute
                    path="/ChangeStatus"
                    exact
                    component={ChangeStatus}
                />
                <InnerLayoutRoute
                    path="/DthPackageChannel"
                    exact
                    component={DthPackageChannel}
                />



                <InnerLayoutRoute
                    path="/DthCategoryChannel"
                    exact
                    component={DthCategoryChannel}
                />
                <InnerLayoutRoute path="/TDS" exact component={TDSReport} />


                <InnerLayoutRoute
                    path="/UserwiseLowBalance"
                    exact
                    component={UserwiseLowBalance}
                />
                <InnerLayoutRoute
                    path="/PlanSetting"
                    exact
                    component={PlanSetting}
                />
                <InnerLayoutRoute
                    path="/OperatorWiseGST"
                    exact
                    component={OperatorWiseGST}
                />
                <InnerLayoutRoute
                    path="/PurchaseReport"
                    exact
                    component={PurchaseReport}
                />
                <InnerLayoutRoute
                    path="/ServiceWiseSlider"
                    exact
                    component={ServiceWiseSliderIndex}
                />
                <InnerLayoutRoute
                    path="/VirtualAccountSetting"
                    exact
                    component={VirtualAccountSetting}
                />

                <InnerLayoutRoute
                    path="/WalletTopUpIndex"
                    exact
                    component={WalletTopUpIndex}
                />

                <InnerLayoutRoute
                    path="/WalletTopUpReport"
                    exact
                    component={WalletTopUpReport}
                />
                <InnerLayoutRoute
                    path="/SeviceswiseWallet"
                    exact
                    component={SeviceswiseWallet}
                />


                <InnerLayoutRoute
                    path="/DenominationDetails"
                    exact
                    component={DenominationDetails}
                />
                <InnerLayoutRoute
                    path="/DigitalCertificateIndex"
                    exact
                    component={DigitalCertificateIndex}
                />
                <InnerLayoutRoute path="/TwoFA" exact component={TwoFA} />
                <InnerLayoutRoute
                    path="/VersionHistory"
                    exact
                    component={VersionHistory}
                />
                <InnerLayoutRoute
                    path="/AdminAccounting"
                    exact
                    component={AdminAccounting}
                />




                <InnerLayoutRoute
                    path="/OutsideRoleSetting"
                    exact
                    component={OutsideRoleSetting}
                />
                <InnerLayoutRoute
                    path="/Performance"
                    exact
                    component={NPerformance}
                />
                <InnerLayoutRoute path="/Company" exact component={Company} />
                <InnerLayoutRoute
                    path="/UISetting"
                    exact
                    component={UISetting}
                />
                <InnerLayoutRoute
                    path="/DownlineReport"
                    exact
                    component={DownlineReport}
                />
                <InnerLayoutRoute
                    path="/ManualSuccessFailReport"
                    exact
                    component={ManualSuccessFailReport}
                />
                <InnerLayoutRoute
                    path="/PastSuccessFailReport"
                    exact
                    component={PastSuccessFailReport}
                />
                <InnerLayoutRoute
                    path="/EcollectionReport"
                    exact
                    component={EcollectionReport}
                />
                <InnerLayoutRoute
                    path="/PendingDmt"
                    exact
                    component={PendingDmt}
                />
                <InnerLayoutRoute
                    path="/AdjustCommission"
                    exact
                    component={AdjustCommission}
                />
                <InnerLayoutRoute
                    path="/RangeWiseSequence"
                    exact
                    component={RangeWiseSequence}
                />
                <InnerLayoutRoute
                    path="/AccessDenied"
                    exact
                    component={AccessDenied}
                />


                <InnerLayoutRoute
                    path="/UserOnboardedReport"
                    exact
                    component={UserOnbordedReport}
                />
                <InnerLayoutRoute
                    path="/UserOnbordedStatusCheck"
                    exact
                    component={UserOnbordedStatusCheck}
                />
                <InnerLayoutRoute
                    path="/KYCSetting"
                    exact
                    component={KYCSetting}
                />
                <InnerLayoutRoute
                    path="/RetailerDashboard"
                    exact
                    component={RetailerDashboard}
                />
                <InnerLayoutRoute
                    path="/ClientDashboard"
                    exact
                    component={RetailerDashboard}
                />
                <InnerLayoutRoute
                    path="/DistributorDashboard"
                    exact
                    component={DistributorDashboardV2}
                />
                <InnerLayoutRoute
                    path="/SuperDistributorDashboard"
                    exact
                    component={DistributorDashboardV2}
                />
                <InnerLayoutRoute
                    path="/ReconciliationUpload"
                    exact
                    component={ReconciliationUpload}
                />
                <InnerLayoutRoute
                    path="/ReconciliationSettings"
                    exact
                    component={ReconciliationSettings}
                />
                <InnerLayoutRoute
                    path="/ReconciliationSettings/Add"
                    component={ReconciliationSettingsAdd}
                />
                <InnerLayoutRoute
                    path="/ReconciliationSettings/Edit"
                    component={ReconciliationSettingsEdit}
                />
                <InnerLayoutRoute
                    path="/AdminDashboard"
                    exact
                    component={AdminDashboardV2}
                />
                <InnerLayoutRoute path="/Search" exact component={Search} />
                <InnerLayoutRoute
                    path="/RetailerSearch"
                    exact
                    component={RetailerSearch}
                />
                <InnerLayoutRoute
                    path="/Pending"
                    exact
                    component={PendingRecharge}
                />
                <InnerLayoutRoute
                    path="/PendingRecon"
                    exact
                    component={PendingRecon}
                />
                <InnerLayoutRoute
                    path="/DMTResendWallet"
                    exact
                    component={DMTResendWallet}
                />
                <InnerLayoutRoute
                    path="/RangewiseDMT"
                    exact
                    component={RangewiseDMT}
                />
                <InnerLayoutRoute
                    path="/UserWiseDMTRangeReport"
                    exact
                    component={UserWiseDMTRangeReport}
                />
                <InnerLayoutRoute
                    path="/RangeWiseRecharge"
                    exact
                    component={RangeWiseRecharge}
                />

                <InnerLayoutRoute
                    path="/Recharge"
                    exact
                    component={ActiveReport}
                />
                <InnerLayoutRoute
                    path="/ROfferRechargeReport"
                    exact
                    component={ROfferRechargeReport}
                />
                <InnerLayoutRoute
                    path="/BBPSReport"
                    exact
                    component={BBPSReport}
                />
                <InnerLayoutRoute
                    path="/AgentDetailReport"
                    exact
                    component={AgentDetailReport}
                />
                <InnerLayoutRoute
                    path="/TransactionDetail"
                    exact
                    component={TransactionDetailReport}
                />
                <InnerLayoutRoute
                    path="/AccountStatement"
                    component={TransactionDetailReport}
                />
                <InnerLayoutRoute
                    path="/SettlementReport"
                    component={SettlementReport}
                />
                <InnerLayoutRoute
                    exact
                    path="/Fund"
                    component={FundRequestIndex}
                />
                <InnerLayoutRoute
                    path="/Fund/Request"
                    exact
                    component={Request}
                />
                <InnerLayoutRoute
                    path="/Fund/Transfer"
                    exact
                    component={Transfer}
                />
                <InnerLayoutRoute
                    path="/FundReverse"
                    exact
                    component={Reverse}
                />
                <InnerLayoutRoute
                    path="/Fund/Report"
                    component={FundTransferReport}
                />
                <InnerLayoutRoute
                    path="/MoveFundsReport"
                    component={MoveFundsReport}
                />
                <InnerLayoutRoute
                    path="/PayoutSetting"
                    exact
                    component={PayoutSetting}
                />
                <InnerLayoutRoute
                    path="/Transaction"
                    exact
                    component={Transaction}
                />
                <InnerLayoutRoute
                    path="/BBPSTransaction"
                    exact
                    component={BBPSTransaction}
                />
                <InnerLayoutRoute
                    path="/DMTTransaction"
                    exact
                    component={() => <DMTTransaction isDMTOne="Yes" />}
                />
                <InnerLayoutRoute
                    path="/DMTTransaction2"
                    exact
                    component={() => <DMTTransaction isDMTOne="No" />}
                />



                <InnerLayoutRoute
                    path="/AEPSIndex"
                    exact
                    component={AEPSIndex}
                />
                <InnerLayoutRoute
                    path="/YBAEPS"
                    exact
                    component={YesBankAEPS}
                />
                <InnerLayoutRoute
                    path="/YBAEPSAuthentication"
                    exact
                    component={YesBankAEPSAuthentication}
                />

                <InnerLayoutRoute
                    path="/Transaction/ThirdPartyService"
                    component={InstaProvider}
                />
                <InnerLayoutRoute path="/KYC" exact component={Upload} />
                <InnerLayoutRoute path="/KYC/Upload" component={Upload} />
                <InnerLayoutRoute path="/KYC/Approve" component={Approve} />
                <InnerLayoutRoute exact path="/Users" component={Users} />

                <InnerLayoutRoute
                    path="/Users/Approve"
                    component={KYCApprove}
                />
                <InnerLayoutRoute
                    path="/Users/UserAlias"
                    component={UserAlias}
                />
                <InnerLayoutRoute path="/Users/Add" component={UserAdd} />
                <InnerLayoutRoute
                    path="/Users/DatewiseReport"
                    component={DatewiseUserReport}
                />
                <InnerLayoutRoute
                    path="/IDFYProfile"
                    component={IDFYProfileReport}
                />
                <InnerLayoutRoute
                    path="/MISInterestedAgents"
                    component={FailedSignUpUsers}
                />
                <InnerLayoutRoute path="/Users/Edit" component={UserEdit} />
                <InnerLayoutRoute
                    path="/ViewAgentOnboarding/Edit"
                    component={UpdateAgent}
                />
                <InnerLayoutRoute
                    path="/ManualAgentOnboarding"
                    component={ManualAgentOnboarding}
                />
                <InnerLayoutRoute
                    path="/UsersProfile"
                    component={SelectProfile}
                />
                <InnerLayoutRoute
                    path="/Users/ChangePassword"
                    component={UpdatePassword}
                />
                <InnerLayoutRoute path="/Users/Index" component={Users} />
                <InnerLayoutRoute
                    path="/Users/DeletedUsers"
                    exact
                    component={DeletedUsers}
                />
                <InnerLayoutRoute
                    path="/Users/UserTransfer"
                    exact
                    component={UserTransfer}
                />
                <InnerLayoutRoute
                    path="/Users/UserwiseSaleTarget"
                    component={UserwiseSaleTarget}
                />
                <InnerLayoutRoute
                    path="/Users/UserwiseSaleTargetReport"
                    component={UserwiseSaleTargetReport}
                />
                <InnerLayoutRoute exact path="/Services" component={Services} />
                <InnerLayoutRoute path="/Services/Index" component={Services} />
                <InnerLayoutRoute
                    path="/Operators"
                    exact
                    component={Operators}
                />
                <InnerLayoutRoute
                    path="/Operators/Index"
                    component={Operators}
                />
                <InnerLayoutRoute
                    path="/Operators/Add"
                    component={OperatorAdd}
                />
                <InnerLayoutRoute
                    path="/Operators/Edit"
                    component={OperatorEdit}
                />
                <InnerLayoutRoute path="/Operators/HLR" exact component={HLR} />
                <InnerLayoutRoute
                    path="/Operators/HLR/Edit"
                    component={HLREdit}
                />
                <InnerLayoutRoute
                    path="/ProvidersAdd/Add"
                    component={ProviderDetailAdd}
                />

                <InnerLayoutRoute
                    path="/ProvidersAdd"
                    exact
                    component={ProviderAdd}
                />
                <InnerLayoutRoute
                    path="/Providers/Edit"
                    component={ProviderEdit}
                />
                <InnerLayoutRoute
                    path="/ProviderDetails/Edit"
                    component={ProviderDetailEdit}
                />
                <InnerLayoutRoute
                    path="/ProviderConfiguration/Index"
                    component={ProviderConfigSetup}
                />
                <InnerLayoutRoute
                    path="/ProviderConfiguration/Edit"
                    component={ProviderOperatorEdit}
                />
                <InnerLayoutRoute
                    path="/ProviderConfiguration"
                    exact
                    component={ProviderAdd}
                />
                <InnerLayoutRoute
                    path="/Slabs/ROfferCommission"
                    component={ROfferCommissionAdd}
                />
                <InnerLayoutRoute path="/Slabs" exact component={SlabAdd} />
                <InnerLayoutRoute path="/Slabs/Assign" component={SlabAssign} />
                <InnerLayoutRoute
                    path="/Slabs/SlabAssignEdit"
                    component={SlabAssignEdit}
                />
                <InnerLayoutRoute
                    path="/Slabs/Commission"
                    component={SlabCommission}
                />
                <InnerLayoutRoute
                    path="/Slabs/DMTCommission"
                    component={DMTSlabCommission}
                />
                <InnerLayoutRoute
                    path="/ChannelSlabs/Commission"
                    component={ChannelSlabCommission}
                />
                <InnerLayoutRoute
                    path="/ChannelSlabs/DMTCommission"
                    component={DMTChannelSlabCommission}
                />
                <InnerLayoutRoute path="/ChannelSlabs" exact component={Add} />
                <InnerLayoutRoute
                    path="/ChannelSlabAssign"
                    exact
                    component={ChannelSlabAssign}
                />
                <InnerLayoutRoute
                    path="/ChannelSlabs/AssignEdit"
                    component={AssignEdit}
                />
                <InnerLayoutRoute
                    path="/ProvidersSequence"
                    component={SequenceAdd}
                />
                <InnerLayoutRoute
                    exact
                    path="/Sequence"
                    component={SequenceAdd}
                />
                <InnerLayoutRoute
                    path="/Sequence/Edit"
                    component={SequenceEdit}
                />
                <InnerLayoutRoute
                    path="/Sequence/UserWise"
                    component={SequenceUserWiseAdd}
                />
                <InnerLayoutRoute
                    path="/UserWiseDMTSequenceAdd"
                    component={UserWiseDMTSequenceAdd}
                />
                <InnerLayoutRoute
                    exact
                    path="/UserwiseSequenceIndex"
                    component={UserwiseSequenceIndex}
                />



                <InnerLayoutRoute path="/TwoFAReport" component={TwoFAReport} />

                <InnerLayoutRoute
                    path="/YBDMT_SenderAdd"
                    component={SenderAdd}
                />
                <InnerLayoutRoute
                    path="/YBDMT_BeneficiaryAdd"
                    component={BeneficiaryAdd}
                />
                <InnerLayoutRoute
                    path="/YBDMT_BeneVerify"
                    component={BeneficiaryVerify}
                />





                <InnerLayoutRoute
                    path="/PTransaction"
                    component={PTransaction}
                />



                <InnerLayoutRoute
                    exact
                    path="/PaymentGatewayTransaction"
                    component={GatewayTransaction}
                />

                <InnerLayoutRoute
                    exact
                    path="/PaymentGatewayReport"
                    component={PaymentGatewayReport}
                />
                <InnerLayoutRoute
                    exact
                    path="/UserReferralReport"
                    component={UserReferralReport}
                />
                <InnerLayoutRoute
                    exact
                    path="/OperatorWiseUserWiseReport"
                    component={OperatorWiseUserWiseReport}
                />
                <InnerLayoutRoute
                    exact
                    path="/ReferredUserReport"
                    component={ReferredUserReport}
                />
                <InnerLayoutRoute
                    exact
                    path="/ReferAndEarn"
                    component={ReferAndEarn}
                />
                <InnerLayoutRoute
                    exact
                    path="/PGSuccess"
                    component={PGSuccess}
                />
                <InnerLayoutRoute exact path="/PGFail" component={PGFail} />
                <InnerLayoutRoute
                    exact
                    path="/IPRegistrationOTP"
                    component={IPRegistrationOTP}
                />
                <InnerLayoutRoute
                    exact
                    path="/QRCodeGenerate"
                    component={QRCodeGenerate}
                />
                <InnerLayoutRoute
                    path="/IPRegistration"
                    component={IPRegistration}
                />

                <InnerLayoutRoute
                    exact
                    path="/IPChangeMobile"
                    component={IPChangeMobile}
                />
                <InnerLayoutRoute
                    path="/IPChangeMoblieVerify"
                    component={IPChangeMoblieVerify}
                />
                <InnerLayoutRoute path="/DMTReport" component={DMTReport} />
                <InnerLayoutRoute
                    path="/ResponseAdd"
                    component={ProviderResponseAdd}
                />
                <InnerLayoutRoute
                    path="/Daybook"
                    exact
                    component={DaybookReport}
                />
                <InnerLayoutRoute
                    path="/Daybook/Report"
                    component={DaybookReport}
                />
                <InnerLayoutRoute
                    path="/ProviderDayBook"
                    exact
                    component={ProviderDayBookReport}
                />
                <InnerLayoutRoute path="/AEPSReport" component={AEPSReport} />
                <InnerLayoutRoute path="/Income" component={IncomeReport} />
                <InnerLayoutRoute
                    path="/MicroATMReport"
                    component={MicroATMReport}
                />
                <InnerLayoutRoute path="/POSReport" component={POSReport} />

                <InnerLayoutRoute path="/IPAdd" exact component={IPAdd} />
                <InnerLayoutRoute
                    path="/CallbackAdd"
                    exact
                    component={CallbackAdd}
                />
                <InnerLayoutRoute path="/Log" exact component={SelectByIDLog} />
                <InnerLayoutRoute
                    path="/Log/Report"
                    component={SelectByIDLog}
                />
                <InnerLayoutRoute
                    path="/PaymentGatewayLog"
                    component={PaymentGatewayLog}
                />
                <InnerLayoutRoute
                    path="/ApiUserLog"
                    component={APIUserInitialLog}
                />
                <InnerLayoutRoute
                    path="/CallbackMaster/Edit"
                    component={CallbackEdit}
                />
                <InnerLayoutRoute
                    path="/CommissionStructure"
                    component={SlabDetailByUser}
                />
                <InnerLayoutRoute
                    path="/KeySetting"
                    component={SelectSettingMaster}
                />
                <InnerLayoutRoute path="/KeyEdit" component={SettingEdit} />
                <InnerLayoutRoute path="/SMSAdd" component={SMSAdd} />
                <InnerLayoutRoute
                    path="/MessageSetting"
                    component={MessageSetting}
                />
                <InnerLayoutRoute
                    path="/WebHookSetting"
                    component={WebHookSetting}
                />
                <InnerLayoutRoute path="/Complaints" component={Complaints} />
                <InnerLayoutRoute path="/AddBank" component={BankAdd} />
                <InnerLayoutRoute
                    exact
                    path="/VirtualBalance"
                    component={VirtualBalanceAdd}
                />
                <InnerLayoutRoute
                    path="/IDPurchaseReport"
                    component={IDPurchaseReport}
                />
                <InnerLayoutRoute
                    path="/VirtualBalance/Add"
                    component={VirtualBalanceAdd}
                />
                <InnerLayoutRoute path="/SmsReport" component={SmsReport} />
                <InnerLayoutRoute
                    path="/OperatorUserwise"
                    component={OperatorUserwise}
                />
                <InnerLayoutRoute
                    path="/UserwiseServiceCharge"
                    component={UserwiseServiceCharge}
                />
                <InnerLayoutRoute
                    path="/ServiceSubscriptionCharge"
                    component={ServiceSubscriptionCharge}
                />
                <InnerLayoutRoute path="/CreatePlan" component={CreatePlan} />
                <InnerLayoutRoute
                    path="/CreatePlanIndex"
                    component={CreatePlanIndex}
                />
                <InnerLayoutRoute
                    path="/Plan/Edit"
                    component={CreatePlanEdit}
                />
                <InnerLayoutRoute path="/AssignPlan" component={AssignPlan} />
                <InnerLayoutRoute
                    path="/Subscription"
                    component={Subscription}
                />
                <InnerLayoutRoute
                    path="/UserWisePlanLimitSetting"
                    component={UserWisePlanLimitSetting}
                />

                <InnerLayoutRoute
                    path="/PendingSubcriptionCharge"
                    component={PendingSubcriptionCharge}
                />
                <InnerLayoutRoute
                    path="/SubscriptionChargeReport"
                    component={SubscriptionChargeReport}
                />
                <InnerLayoutRoute
                    path="/AccountStatementReport"
                    component={AccountStatementReport}
                />
                <InnerLayoutRoute
                    path="/SubscriptionPaymentReport"
                    component={SubscriptionPaymentReport}
                />
                <InnerLayoutRoute
                    path="/SubscriptionPendingPayment"
                    component={SubscriptionPendingPayment}
                />
                <InnerLayoutRoute
                    path="/SupportAccess"
                    component={SupportAccess}
                />
                <InnerLayoutRoute
                    path="/UserActionHistory"
                    component={UserActionHistory}
                />
                <InnerLayoutRoute
                    path="/FundReverseRequestReport"
                    component={ReverseRequestReport}
                />
                <InnerLayoutRoute
                    path="/Fund/ReverseRequest"
                    component={ReverseRequest}
                />
                <InnerLayoutRoute
                    path="/Fund/ReverseTransferReport"
                    component={ReverseTransferReport}
                />
                <InnerLayoutRoute
                    path="/ServiceChargeDeductionReport"
                    component={ServiceChargeDeductionReport}
                />
                <InnerLayoutRoute
                    path="/CircleAPIAdd"
                    component={CircleAPIAdd}
                />
                <InnerLayoutRoute
                    path="/SaleReport"
                    component={ServicewiseSale}
                />
                <InnerLayoutRoute
                    path="/UserwiseSale"
                    component={UserwiseSale}
                />
                <InnerLayoutRoute
                    path="/ProviderSale"
                    component={ProviderSale}
                />
                <InnerLayoutRoute
                    path="/WalletTransfer"
                    component={WalletTransfer}
                />
                <InnerLayoutRoute
                    path="/Notification"
                    component={NotificationAdd}
                />




                <InnerLayoutRoute
                    path="/CircleSwitching"
                    component={CircleSwitching}
                />
                <InnerLayoutRoute path="/CircleIndex" component={CircleIndex} />
                <InnerLayoutRoute path="/ProviderLog" component={ProviderLog} />
                <InnerLayoutRoute
                    exact
                    path="/ViewAgentOnboarding"
                    component={ViewAgentOnboarding}
                />
                <InnerLayoutRoute path="/PendingIP" component={PendingIP} />
                <InnerLayoutRoute path="/News/Index" component={News} />

                <InnerLayoutRoute
                    path="/EmailSetting/Index"
                    component={EmailSetting}
                />
                <InnerLayoutRoute
                    path="/EmailSetting/Edit"
                    component={EmailEdit}
                />
                <InnerLayoutRoute
                    path="/EmailSetting/Add"
                    component={EmailAdd}
                />
                <InnerLayoutRoute
                    path="/DenominationDetails/Denomination"
                    component={DenominationAdd}
                />
                <InnerLayoutRoute
                    path="/LiveTransaction"
                    exact
                    component={LiveTransaction}
                />
                <InnerLayoutRoute
                    path="/BBPSConfiguration"
                    exact
                    component={BBPSConfiguration}
                />
                <InnerLayoutRoute
                    path="/BBPSConfiguration/Index"
                    exact
                    component={BBPSConfiguration}
                />
                <InnerLayoutRoute
                    path="/BBPSConfiguration/Add"
                    component={BBPSConfigurationAdd}
                />
                <InnerLayoutRoute
                    path="/BBPSConfiguration/Edit"
                    component={BBPSConfigurationEdit}
                />
                <InnerLayoutRoute
                    path="/DMTSettings"
                    exact
                    component={DMTSettings}
                />
                <InnerLayoutRoute
                    path="/DMTSettings/Index"
                    exact
                    component={DMTSettings}
                />
                <InnerLayoutRoute
                    path="/VerificationSettings"
                    exact
                    component={VerificationSettings}
                />

                <InnerLayoutRoute
                    path="/ThirdPartyServiceProvider"
                    exact
                    component={ThirdPartyServiceProvider}
                />
                <InnerLayoutRoute
                    path="/ThirdPartyServiceProvider/Index"
                    exact
                    component={ThirdPartyServiceProvider}
                />
                <InnerLayoutRoute
                    path="/ProviderWiseFailReport"
                    exact
                    component={ProviderWiseFailReport}
                />
                <InnerLayoutRoute
                    path="/Slider"
                    exact
                    component={SliderIndex}
                />
                <InnerLayoutRoute
                    path="/PushNotifications"
                    exact
                    component={PushNotificationsIndex}
                />
                <InnerLayoutRoute path="/UserLedger" component={LedgerReport} />
                <InnerLayoutRoute
                    path="/UserWiseServiceSellReport"
                    component={UserWiseServiceSellReport}
                />
                <InnerLayoutRoute path="/AdminLedger" component={AdminLedger} />
                <InnerLayoutRoute path="/AddPurchase" component={AddPurchase} />
                <InnerLayoutRoute
                    path="/PayoutReport"
                    component={PayoutReport}
                />
                <InnerLayoutRoute
                    path="/BulkPaySetting"
                    component={BulkPaySetting}
                />

                <InnerLayoutRoute
                    path="/RichTextMedia"
                    component={RichTextMedia}
                />

                <InnerLayoutRoute
                    path="/PayoutBeneficiaryReport"
                    component={BeneficiaryReport}
                />
                <InnerLayoutRoute
                    path="/ProviderWisePerformanceGraph"
                    component={ProviderWisePerformanceGraph}
                />
                <InnerLayoutRoute path="/OTTReport" component={OTTReport} />


                <InnerLayoutRoute
                    path="/DenominationDetails/DenominationMapping"
                    component={DenominationMapping}
                />
                <InnerLayoutRoute path="/MenuAdd" component={MenuAdd} />
                <InnerLayoutRoute
                    path="/BulkMessageIndex"
                    exact
                    component={BulkMessageIndex}
                />
                <InnerLayoutRoute
                    path="/Payout"
                    exact
                    component={PayoutIndex}
                />
                <InnerLayoutRoute
                    path="/PayoutTransaction"
                    exact
                    component={PayoutTransaction}
                />
                <InnerLayoutRoute
                    path="/PayoutV1"
                    exact
                    component={PayoutIndexV1}
                />
                <InnerLayoutRoute
                    path="/PayoutTransactionV1"
                    exact
                    component={PayoutTransactionV1}
                />
                <InnerLayoutRoute
                    path="/SmartPayTransaction"
                    exact
                    component={SmartPayTransaction}
                />



                <InnerLayoutRoute
                    path="/InvoiceSettings"
                    exact
                    component={InvoiceSettings}
                />


                <InnerLayoutRoute path="/Invoice" exact component={Invoice} />
                <InnerLayoutRoute
                    path="/InvoiceSettings/Index"
                    exact
                    component={InvoiceSettings}
                />
                <InnerLayoutRoute
                    path="/PaymentGateWaySetting"
                    exact
                    component={PaymentGateWaySetting}
                />
                <InnerLayoutRoute
                    path="/UserwisePaymentGateway"
                    exact
                    component={UserwisePaymentGateway}
                />
                <InnerLayoutRoute
                    path="/AEPSSetting"
                    exact
                    component={AEPSSetting}
                />
                <InnerLayoutRoute
                    path="/MicroATMSetting"
                    exact
                    component={MicroATMSetting}
                />
                <InnerLayoutRoute
                    path="/EcollectionSetting"
                    exact
                    component={EcollectionSetting}
                />
                <InnerLayoutRoute
                    path="/Fund/GatewayTransaction"
                    exact
                    component={GatewayTransaction}
                />
                <InnerLayoutRoute
                    path="/GSTSummary"
                    exact
                    component={GSTSummary}
                />
                <InnerLayoutRoute path="/GSTHold" exact component={GSTHold} />
                <InnerLayoutRoute
                    path="/Users/Inactive"
                    exact
                    component={InActiveUsers}
                />
                <InnerLayoutRoute path="/Ticket" component={Ticket} />
                <InnerLayoutRoute
                    path="/TicketReport"
                    component={TicketReport}
                />
                <InnerLayoutRoute
                    path="/BalanceCapping"
                    component={BalanceCappingUserwise}
                />
                <InnerLayoutRoute
                    path="/OperatorSale"
                    component={OperatorSale}
                />
                <InnerLayoutRoute
                    path="/LoginHistory"
                    component={LoginHistory}
                />
                <InnerLayoutRoute
                    path="/Users/UserWiseBulkAction"
                    exact
                    component={UserWiseBulkAction}
                />
                <InnerLayoutRoute
                    path="/UserWiseBulkActionSDAndDist"
                    exact
                    component={UserWiseBulkActionSDAndDist}
                />
                <InnerLayoutRoute
                    path="/DMTBankSetting"
                    exact
                    component={DMTBankSetting}
                />
                <InnerLayoutRoute path="/AddVoucher" component={AddVoucher} />
                <InnerLayoutRoute
                    path="/ApproveVoucher"
                    component={ApproveVoucher}
                />
                <InnerLayoutRoute
                    path="/ReportVoucher"
                    component={ReportVoucher}
                />
                <InnerLayoutRoute
                    path="/SenderBeneficiaryReport"
                    exact
                    component={SenderBeneficiaryReport}
                />




                <InnerLayoutRoute
                    path="/YBDMT_SenderSearch"
                    exact
                    component={() => (
                        <YesBankSenderSearch ActiveDMTName="YesBankDMT" />
                    )}
                />
            </InnerLayout>
        </Route>
    );
};

export default InnerLayoutNavigator;
