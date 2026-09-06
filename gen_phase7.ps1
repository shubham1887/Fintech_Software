
$base = "C:\Users\Shubham\OneDrive\Desktop\new Fintech application\Fino_API"

$entities = @(
    @{ N="eKYCSetting"; PK="eKYCSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="Slat";t="string"},@{n="Token";t="string"},@{n="AuthKey";t="string"},@{n="ChargeAmount";t="decimal"},@{n="EncryptionKey";t="string"},@{n="MerchantID";t="string"},@{n="Mode";t="string"},@{n="IsActive";t="bool"},@{n="IsActiveWeb";t="bool"}) },
    @{ N="MicroATMSetting"; PK="MicroATMSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="UserName";t="string"},@{n="Password";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="PublicKey";t="string"},@{n="MinimumAmount";t="decimal"},@{n="MaximumAmount";t="decimal"},@{n="DisplayName";t="string"},@{n="Sequence";t="int"},@{n="IsActive";t="bool"},@{n="IsActiveWeb";t="bool"}) },
    @{ N="SubscriptionMaster"; PK="SubscriptionMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="StartDate";t="DateTime"},@{n="EndDate";t="string"},@{n="UpdatedDOC";t="DateTime"},@{n="UpdatedDOCInt";t="int"},@{n="IsActive";t="bool"}) },
    @{ N="SupportUserAccessMaster"; PK="SupportUserAccessMasterID"; HasBase=$false; Filter="UserID"; FKs=@(); Fields=@(@{n="MenuMasterID";t="int"},@{n="UserID";t="int"},@{n="MenuGUID";t="string"}) },
    @{ N="ThirdPartyServiceProvider"; PK="ThirdPartyServiceProviderID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="Token";t="string"},@{n="DomainName";t="string"},@{n="AppID";t="string"},@{n="AllowedServices";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="UnitMaster"; PK="UnitMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"}) },
    @{ N="UrlTypeMaster"; PK="UrlTypeID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="UrlType";t="string"}) },
    @{ N="VerificationSetting"; PK="VerificationSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="Token";t="string"},@{n="CallBackURL";t="string"},@{n="UserName";t="string"},@{n="Sequence";t="int"},@{n="AuthCode";t="string"},@{n="Mode";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="VirtualBalance"; PK="VirtualBalanceID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Amount";t="decimal"},@{n="WalletID";t="int"},@{n="DOCType";t="string"},@{n="OpeningBalance";t="decimal"},@{n="ClosingBalance";t="decimal"}) },
    @{ N="UPITransferSetting"; PK="UPITransferSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="DisplayName";t="string"},@{n="Token";t="string"},@{n="CallBackURL";t="string"},@{n="UserName";t="string"},@{n="SenderLimit";t="decimal"},@{n="MaximumAmount";t="decimal"},@{n="MinAmount";t="decimal"},@{n="Sequence";t="int"},@{n="AuthCode";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="RemainderOperatorAmountWise"; PK="RemainderOperatorAmountWiseID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="OperatorID";t="int"},@{n="Amount";t="decimal"},@{n="Days";t="int"}) },
    @{ N="SDKSetting"; PK="SDKSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="URL";t="string"},@{n="PublicKey";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="PanCardSetting"; PK="PanCardSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="PublicKey";t="string"},@{n="Mode";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="LastChangeUserDetails"; PK="LastChangeUserDetailsID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="OnboardingUserID";t="int"},@{n="Change";t="string"},@{n="OldChanges";t="string"},@{n="NewChanges";t="string"}) },
    @{ N="LeadGenerationMaster"; PK="LeadGenerationMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="FormID";t="string"},@{n="MerchantName";t="string"},@{n="ConsumerName";t="string"},@{n="MobileNumber";t="string"},@{n="EmailID";t="string"},@{n="PanNumber";t="string"},@{n="Status";t="string"},@{n="LoanType";t="string"},@{n="Gender";t="string"},@{n="APIUserRequestID";t="string"},@{n="DOB";t="DateTime"}) },
    @{ N="BlackListBankMaster"; PK="BlackListBankMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="BankName";t="string"},@{n="IFSC";t="string"},@{n="AccountNumber";t="string"}) },
    @{ N="BulkPaySetting"; PK="BulkPaySettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="ActionURL";t="string"},@{n="StatusCheckURL";t="string"},@{n="IsActiveWeb";t="bool"},@{n="IsActive";t="bool"},@{n="MinimumAmount";t="decimal"},@{n="MaximumAmount";t="decimal"},@{n="Mode";t="string"}) },
    @{ N="CMSSetting"; PK="CMSSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="PublicKey";t="string"},@{n="IsActiveWeb";t="bool"},@{n="IsActive";t="bool"},@{n="Mode";t="string"}) },
    @{ N="ConsumerTypeMaster"; PK="ConsumerTypeMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="ConsumerType";t="string"}) },
    @{ N="DematSetting"; PK="DematSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="DisplayName";t="string"},@{n="UserName";t="string"},@{n="Token";t="string"},@{n="UTMSource";t="string"},@{n="EmailID";t="string"},@{n="Amount";t="decimal"},@{n="Sequence";t="int"},@{n="IsActiveWeb";t="bool"},@{n="IsActive";t="bool"}) },
    @{ N="DeviceWiseFailLoginHistory"; PK="DeviceWiseFailLoginHistoryID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="UserName";t="string"},@{n="DeviceID";t="string"}) },
    @{ N="LoanTypeMaster"; PK="LoanTypeMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="LoanType";t="string"}) },
    @{ N="MenuActionMapping"; PK="MenuActionMappingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Controller";t="string"},@{n="Action";t="string"},@{n="Description";t="string"},@{n="MenuMasterGUID";t="string"}) },
    @{ N="OTTSetting"; PK="OTTSettingID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="ActionURL";t="string"},@{n="StatusCheckURL";t="string"},@{n="IsActive";t="bool"},@{n="MinimumAmount";t="decimal"},@{n="MaximumAmount";t="decimal"},@{n="Mode";t="string"},@{n="DisplayName";t="string"}) },
    @{ N="PaySprintBank"; PK="ID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="BankName";t="string"},@{n="Sequence";t="int"}) },
    @{ N="POSSettingMaster"; PK="POSSettingMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="UserName";t="string"},@{n="Password";t="string"},@{n="MerchantID";t="string"},@{n="Token";t="string"},@{n="Slat";t="string"},@{n="PublicKey";t="string"},@{n="IsActiveWeb";t="bool"},@{n="IsActive";t="bool"},@{n="MinimumAmount";t="decimal"},@{n="MaximumAmount";t="decimal"}) },
    @{ N="SignUp"; PK="SignUpID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="MobileNumber";t="string"},@{n="PANNo";t="string"},@{n="ReferralCode";t="string"},@{n="EmployeeID";t="string"},@{n="OrderID";t="string"},@{n="ProfileID";t="string"},@{n="RedirectURL";t="string"},@{n="Status";t="string"},@{n="PinCode";t="string"}) },
    @{ N="Category"; PK="CategoryID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="CategoryName";t="string"}) },
    @{ N="ChannelCategory"; PK="ChannelCategoryID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Category";t="string"}) },
    @{ N="CreditInstitutionMaster"; PK="CreditInstitutionMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="Name";t="string"},@{n="MerchantID";t="string"},@{n="BranchID";t="string"},@{n="AccountNumber";t="string"},@{n="Token";t="string"},@{n="EncryptionKey";t="string"},@{n="IsActive";t="bool"},@{n="IsActiveWeb";t="bool"}) },
    @{ N="AndroidMenuMaster"; PK="AndroidMenuMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="MenuName";t="string"},@{n="Key";t="string"},@{n="URL";t="string"},@{n="UserTypeID";t="int"},@{n="Sequence";t="int"},@{n="SectionName";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="AndroidSectionMaster"; PK="AndroidSectionMasterID"; HasBase=$false; Filter=""; FKs=@(); Fields=@(@{n="SectionName";t="string"}) },
    @{ N="TransactionAPIUserRequestMaster"; PK="TransactionAPIUserRequestMasterID"; HasBase=$false; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="ApiUserRequestID";t="string"}) },
    @{ N="ProviderOperatorCircleMaster"; PK="ProviderOperatorCircleMasterID"; HasBase=$false; Filter=""; FKs=@(@{f="ProviderID";nav="ProviderMaster";t="ProviderMaster"},@{f="OperatorID";nav="OperatorMaster";t="OperatorMaster"}); Fields=@(@{n="ProviderID";t="int"},@{n="OperatorID";t="int"},@{n="StateID";t="int"},@{n="Opcode";t="string"},@{n="IsActive";t="bool"}) },
    @{ N="OnboardingUserDocument"; PK="OnboardingUserDocumentID"; HasBase=$false; Filter=""; FKs=@(@{f="OnboardingUserID";nav="OnboardingUser";t="OnboardingUser"}); Fields=@(@{n="Ref_ID";t="string"},@{n="Type";t="int"},@{n="OldImage";t="string"},@{n="NewImage";t="string"},@{n="OnboardingUserID";t="int"}) },
    @{ N="UPIVendorBeneficiaryMapping"; PK="UPIVendorBeneficiaryMappingID"; HasBase=$false; Filter=""; FKs=@(@{f="UpiBeneficiaryID";nav="UpiBeneficiary";t="UpiBeneficiary"}); Fields=@(@{n="ProviderID";t="int"},@{n="UPIVendorBeneficiaryID";t="string"},@{n="UpiBeneficiaryID";t="int"}) },
    # WITH BaseEntity
    @{ N="VirtualAccountMaster"; PK="VirtualAccountMasterID"; HasBase=$true; Filter=""; FKs=@(); Fields=@(@{n="Prefix";t="string"},@{n="BankName";t="string"},@{n="IFSC";t="string"}) },
    @{ N="DailyAuthenticationMaster"; PK="AuthenticationID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="ProviderID";nav="ProviderMaster";t="ProviderMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"}); Fields=@(@{n="UserID";t="int"},@{n="ProviderID";t="int"},@{n="ServiceID";t="int"},@{n="OperatorID";t="int"},@{n="Pipe";t="string"}) },
    @{ N="OperationalHistory"; PK="OperationalHistoryID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="APIType";t="string"},@{n="DocNo";t="string"}) },
    @{ N="SDKBBPSUserMapping"; PK="SDKBBPSUserMappingID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="ConfirmationCallbackURL";t="string"},@{n="IsActive";t="bool"},@{n="IsDelete";t="bool"}) },
    @{ N="UserwiseSaleTarget"; PK="UserwiseSaleTargetID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="WalletID";nav="WalletMaster";t="WalletMaster"}); Fields=@(@{n="UserID";t="int"},@{n="WalletID";t="int"},@{n="ParentID";t="int"},@{n="TargetAmount";t="decimal"},@{n="Commission";t="decimal"},@{n="CommissionType";t="string"},@{n="IsDelete";t="bool"}) },
    @{ N="UserwiseServiceChargeMaster"; PK="UserwiseServiceChargeMasterID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"},@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="ServiceID";t="int"},@{n="UserID";t="int"},@{n="OneTime";t="decimal"},@{n="IsDelete";t="bool"}) },
    @{ N="TargetSaleCommission"; PK="TargetSaleCommissionID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="WalletID";nav="WalletMaster";t="WalletMaster"}); Fields=@(@{n="UserID";t="int"},@{n="WalletID";t="int"},@{n="ParentID";t="int"},@{n="SaleAmount";t="decimal"},@{n="CommissionAmount";t="decimal"},@{n="Description";t="string"},@{n="SaleMonthDOC";t="string"},@{n="Status";t="string"}) },
    @{ N="UserActionHistory"; PK="UserHistoryID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="Action";t="string"},@{n="Description";t="string"},@{n="Status";t="string"},@{n="IPAddress";t="string"}) },
    @{ N="ReferralRegistration"; PK="ReferralRegistrationID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="MobileNumber";t="string"},@{n="ReferralCode";t="string"},@{n="CreatedByUserID";t="int"},@{n="IsBenefitUtilize";t="bool"},@{n="ReferralAmount";t="decimal"}) },
    @{ N="OverdraftDetail"; PK="OverdraftDetailID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="WalletID";nav="WalletMaster";t="WalletMaster"}); Fields=@(@{n="UserID";t="int"},@{n="WalletID";t="int"},@{n="OverdraftOpening";t="decimal"},@{n="OverdraftAmount";t="decimal"},@{n="OverdraftClosing";t="decimal"},@{n="UserRemark";t="string"},@{n="AdminRemark";t="string"},@{n="Status";t="string"},@{n="OpeningWalletBalance";t="decimal"}) },
    @{ N="OverdraftMaster"; PK="OverdraftMasterID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="WalletID";nav="WalletMaster";t="WalletMaster"}); Fields=@(@{n="UserID";t="int"},@{n="Overdraft";t="decimal"},@{n="MaxOverdraft";t="decimal"},@{n="WalletID";t="int"},@{n="IsActive";t="bool"}) },
    @{ N="TrustableDevice"; PK="ID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="IPAddress";t="string"},@{n="UserID";t="int"},@{n="IsVerified";t="bool"},@{n="Browser";t="string"},@{n="Device";t="string"}) },
    @{ N="UserMerchantMaster"; PK="UserMerchantMasterID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="PaySprintUPIMerchantCode";t="string"},@{n="PaySprintStaticQR";t="string"},@{n="PaySprintUPIRefid";t="string"}) },
    @{ N="UserWisePayementGatewaySetting"; PK="UserWisePayementGatewaySettingID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"},@{f="PaymentGatewaySettingID";nav="PaymentGatewaySetting";t="PaymentGatewaySetting"}); Fields=@(@{n="UserID";t="int"},@{n="PaymentGatewaySettingID";t="int"},@{n="MinimumAmount";t="decimal"},@{n="MaximumAmount";t="decimal"}) },
    @{ N="OnboardingUser"; PK="OnboardingUserID"; HasBase=$true; Filter=""; FKs=@(); Fields=@(@{n="ExternalID";t="string"},@{n="ProfileID";t="string"},@{n="ReferenceID";t="string"},@{n="Name";t="string"},@{n="DOB";t="string"},@{n="PanNo";t="string"},@{n="AadhaarNo";t="string"},@{n="MobileNumber";t="string"},@{n="EmailID";t="string"},@{n="State";t="string"},@{n="PinCode";t="string"},@{n="Status";t="string"},@{n="AdminStatus";t="string"},@{n="SupportStatus";t="string"},@{n="AdminRemark";t="string"},@{n="SupportRemark";t="string"},@{n="OrderID";t="string"},@{n="AccountNumber";t="string"},@{n="IFSC";t="string"},@{n="BankName";t="string"},@{n="SlabID";t="int"},@{n="AgentID";t="string"}) },
    @{ N="SmsTransactionMaster"; PK="SmsTransactionMasterID"; HasBase=$true; Filter=""; FKs=@(@{f="TransactionMasterID";nav="TransactionMaster";t="TransactionMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"}); Fields=@(@{n="TransactionMasterID";t="int"},@{n="ServiceID";t="int"},@{n="Amount";t="decimal"},@{n="BillNumber";t="string"},@{n="IsProcess";t="bool"},@{n="IsSuccess";t="bool"}) },
    @{ N="WalletServiceMapping"; PK="WalletServiceMappingID"; HasBase=$true; Filter=""; FKs=@(@{f="WalletID";nav="WalletMaster";t="WalletMaster"},@{f="ServiceID";nav="ServiceMaster";t="ServiceMaster"}); Fields=@(@{n="WalletID";t="int"},@{n="ServiceID";t="int"}) },
    @{ N="eKYCLog"; PK="eKYCLogID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="DocumentNo";t="string"},@{n="UserID";t="int"},@{n="Response";t="string"},@{n="DocumentType";t="string"}) },
    @{ N="eKYCMaster"; PK="eKYCMasterID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="ProviderID";t="int"},@{n="UserID";t="int"},@{n="OrderID";t="string"},@{n="OTPGenerateHash";t="string"},@{n="ChargeAmount";t="decimal"},@{n="OTPGenerateRequest";t="string"},@{n="OTPGenerateResponse";t="string"},@{n="VerifyRequest";t="string"},@{n="VerifyResponse";t="string"},@{n="OTPToken";t="string"},@{n="KYCToken";t="string"},@{n="wadh";t="string"}) },
    @{ N="BankRequestResponseLog"; PK="BankRequestResponseLogID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="ProviderID";t="int"},@{n="TransactionID";t="int"},@{n="ConsumerNumber";t="string"},@{n="Request";t="string"},@{n="Response";t="string"},@{n="IPAddress";t="string"},@{n="Through";t="string"},@{n="ServiceID";t="int"}) },
    @{ N="SupportUserAccessDetail"; PK="SupportUserAccessDetailID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="UserID";t="int"},@{n="Controller";t="string"},@{n="Action";t="string"},@{n="MenuGUID";t="string"}) },
    @{ N="LapuTransactionDetail"; PK="LapuTransactionDetailID"; HasBase=$true; Filter="UserID"; FKs=@(); Fields=@(@{n="UserID";t="int"},@{n="OperatorID";t="int"},@{n="ProivderID";t="int"},@{n="LapuMasterID";t="int"},@{n="LapuPartyMasterID";t="int"},@{n="LapuNo";t="string"},@{n="Status";t="string"},@{n="ConsumerNumber";t="string"},@{n="Amount";t="decimal"},@{n="OpeningBalance";t="decimal"},@{n="ClosingBalance";t="decimal"}) },
    @{ N="UpiSender"; PK="UpiSenderID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="Name";t="string"},@{n="Mobile";t="string"},@{n="GUID";t="string"},@{n="Address";t="string"},@{n="UserID";t="int"},@{n="IPAddress";t="string"},@{n="Pincode";t="string"}) },
    @{ N="UpiBeneficiary"; PK="UpiBeneficiaryID"; HasBase=$true; Filter="UserID"; FKs=@(@{f="UpiSenderID";nav="UpiSender";t="UpiSender"},@{f="UserID";nav="UserMaster";t="UserMaster"}); Fields=@(@{n="Name";t="string"},@{n="UpiID";t="string"},@{n="GUID";t="string"},@{n="SenderMobile";t="string"},@{n="IPAddress";t="string"},@{n="UpiSenderID";t="int"},@{n="UserID";t="int"}) }
)

function Get-Default($t) {
    if ($t -eq "string") { ' = string.Empty;' }
    elseif ($t -eq "DateTime") { ' = DateTime.MinValue;' }
    else { '' }
}

$dbSetEntries = @()
$ckEntries = @()
$diEntries = @()
$fkConfigs = @()

foreach ($e in $entities) {
    $n = $e.N
    $pk = $e.PK
    $hasBase = $e.HasBase
    $filter = $e.Filter
    $fks = $e.FKs
    $fields = $e.Fields
    $dbSetName = "${n}s"
    $ckName = "All${n}s"

    # ENTITY
    $baseClass = if ($hasBase) { " : BaseEntity" } else { "" }
    $navPropLines = $fks | ForEach-Object { "        public $($_.t) $($_.nav) { get; set; } = null!;" }
    $fieldPropLines = $fields | ForEach-Object { "        public $($_.t) $($_.n) { get; set; }$(Get-Default $_.t)" }
    $allProps = ($fieldPropLines + $navPropLines) -join [System.Environment]::NewLine
    $entityContent = "namespace FintechApp.Entities`n{`n    public class $n$baseClass`n    {`n        public int $pk { get; set; }`n$allProps`n    }`n}"
    Set-Content "$base\Entities\${n}.cs" -Value $entityContent -Encoding utf8

    # IREPO
    $filterLine = if ($filter) { "        Task<List<$n>> GetBy$filter(int $($filter.ToLower()));" } else { "" }
    $irepoContent = "using FintechApp.Entities;`nnamespace FintechApp.Interfaces.Repositories`n{`n    public interface I${n}Repository`n    {`n        Task<List<$n>> GetAll();`n        Task<${n}?> GetByID(int id);`n$filterLine`n        Task<int> Insert($n entity);`n        Task<int> Update($n entity);`n        Task<int> Delete(int id);`n    }`n}"
    Set-Content "$base\Interfaces\Repositories\I${n}Repository.cs" -Value $irepoContent -Encoding utf8

    # ISERVICE
    $filterSvcLine = if ($filter) { "        Task<List<${n}Response>> GetBy$filter(int $($filter.ToLower()));" } else { "" }
    $isvcContent = "using FintechApp.Requests.$n;`nusing FintechApp.Responses.$n;`nnamespace FintechApp.Interfaces.Services`n{`n    public interface I${n}Service`n    {`n        Task<List<${n}Response>> GetAll();`n        Task<${n}Response?> GetByID(int id);`n$filterSvcLine`n        Task<string> Create(Create${n}Request request);`n        Task<string> Update(Update${n}Request request);`n        Task<string> Delete(int id);`n    }`n}"
    Set-Content "$base\Interfaces\Services\I${n}Service.cs" -Value $isvcContent -Encoding utf8

    # MESSAGES
    $msgContent = "namespace FintechApp.Messages`n{`n    public static class ${n}Messages`n    {`n        public const string NotFound = `"$n not found.`";`n        public const string CreatedSuccessfully = `"$n created successfully.`";`n        public const string UpdatedSuccessfully = `"$n updated successfully.`";`n        public const string DeletedSuccessfully = `"$n deleted successfully.`";`n    }`n}"
    Set-Content "$base\Messages\${n}Messages.cs" -Value $msgContent -Encoding utf8

    # REQUESTS
    New-Item -ItemType Directory -Force -Path "$base\Requests\$n" | Out-Null
    New-Item -ItemType Directory -Force -Path "$base\Responses\$n" | Out-Null

    $filterGetProp = if ($filter) { " public int $filter { get; set; }" } else { "" }
    $getReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Get${n}Request { public int $pk { get; set; }$filterGetProp } public class Get${n}RequestValidator : AbstractValidator<Get${n}Request> { public Get${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Get${n}Request.cs" -Value $getReq -Encoding utf8

    $createPropParts = $fields | ForEach-Object { "public $($_.t) $($_.n) { get; set; }$(Get-Default $_.t)" }
    $createProps = $createPropParts -join " "
    $filterRule = if ($filter) { "RuleFor(x => x.$filter).GreaterThan(0);" } else { "" }
    $createReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Create${n}Request { $createProps } public class Create${n}RequestValidator : AbstractValidator<Create${n}Request> { public Create${n}RequestValidator() { $filterRule } } }"
    Set-Content "$base\Requests\$n\Create${n}Request.cs" -Value $createReq -Encoding utf8

    $updateProps = "public int $pk { get; set; } $createProps"
    $updateReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Update${n}Request { $updateProps } public class Update${n}RequestValidator : AbstractValidator<Update${n}Request> { public Update${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Update${n}Request.cs" -Value $updateReq -Encoding utf8

    $deleteReq = "using FluentValidation;`nnamespace FintechApp.Requests.$n { public class Delete${n}Request { public int $pk { get; set; } } public class Delete${n}RequestValidator : AbstractValidator<Delete${n}Request> { public Delete${n}RequestValidator() { RuleFor(x => x.$pk).GreaterThan(0); } } }"
    Set-Content "$base\Requests\$n\Delete${n}Request.cs" -Value $deleteReq -Encoding utf8

    # RESPONSE
    $respLines = @("        public int $pk { get; set; }")
    foreach ($f in $fields) { $respLines += "        public $($f.t) $($f.n) { get; set; }$(Get-Default $f.t)" }
    if ($hasBase) { $respLines += "        public DateTime DateTime { get; set; }"; $respLines += "        public DateTime UpdatedateTime { get; set; }" }
    $respBody = $respLines -join [System.Environment]::NewLine
    $respContent = "namespace FintechApp.Responses.$n {`n    public class ${n}Response {`n$respBody`n    }`n}"
    Set-Content "$base\Responses\$n\${n}Response.cs" -Value $respContent -Encoding utf8

    # REPOSITORY
    $softDelete = if ($hasBase) { "entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result;" } else { "_context.$dbSetName.Remove(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result;" }
    $getAllFilter = if ($hasBase) { ".AsNoTracking().Where(x => !x.IsDeleted).ToList()" } else { ".AsNoTracking().ToList()" }
    $filterRepoLine = if ($filter) { "        public async Task<List<$n>> GetBy$filter(int $($filter.ToLower())) { var list = await GetAll(); return list.Where(x => x.$filter == $($filter.ToLower())).ToList(); }" } else { "" }

    $repoContent = @"
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
namespace FintechApp.Repositories
{
    public class ${n}Repository : I${n}Repository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<$n>> _listCache;
        public ${n}Repository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<$n>>(); }
        public async Task<List<$n>> GetAll() => _listCache.GetOrCreate(CacheKeys.$ckName, () => _context.$dbSetName$getAllFilter) ?? new List<$n>();
        public async Task<${n}?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.$pk == id); }
$filterRepoLine
        public async Task<int> Insert($n entity) { await _context.$dbSetName.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result; }
        public async Task<int> Update($n entity) { _context.$dbSetName.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.$ckName); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.$dbSetName.FindAsync(id); if (entity == null) return 0; $softDelete }
    }
}
"@
    Set-Content "$base\Repositories\${n}Repository.cs" -Value $repoContent -Encoding utf8

    # SERVICE
    $mapParts = $fields | ForEach-Object { "$($_.n) = e.$($_.n)," }
    $mapBody = $mapParts -join " "
    $createParts = $fields | ForEach-Object { "$($_.n) = request.$($_.n)," }
    $createBody = $createParts -join " "
    $updateParts = $fields | ForEach-Object { "entity.$($_.n) = request.$($_.n);" }
    $updateBody = $updateParts -join " "
    $filterSvcLine2 = if ($filter) { "        public async Task<List<${n}Response>> GetBy$filter(int $($filter.ToLower())) { var list = await _repository.GetBy$filter($($filter.ToLower())); return list.Select(MapToResponse).ToList(); }" } else { "" }

    $svcContent = @"
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.$n;
using FintechApp.Responses.$n;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ${n}Service : I${n}Service
    {
        private readonly I${n}Repository _repository;
        public ${n}Service(I${n}Repository repository) { _repository = repository; }
        private static ${n}Response MapToResponse($n e) => new() { $pk = e.$pk, $mapBody };
        public async Task<List<${n}Response>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<${n}Response?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
$filterSvcLine2
        public async Task<string> Create(Create${n}Request request) { var entity = new $n { $createBody }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(Update${n}Request request) { var entity = await _repository.GetByID(request.$pk); if (entity == null) return Messages.${n}Messages.NotFound; $updateBody await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.${n}Messages.NotFound; }
    }
}
"@
    Set-Content "$base\Services\${n}Service.cs" -Value $svcContent -Encoding utf8

    # CONTROLLER
    $filterCtrlBlock = ""
    if ($filter) {
        $filterCtrlBlock = @"

        [HttpPost] public async Task<APIResponse> GetBy$filter([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; Get${n}Request? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Get${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetBy$filter(request.$filter); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Messages.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); }
        }
"@
    }

    $ctrlContent = @"
using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.$n;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ${n}Controller : ControllerBase
    {
        private readonly I${n}Service _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        public ${n}Controller(I${n}Service service, IConfiguration configuration) { _service = service; _logger = new Logger(); _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty); }
        [HttpPost] public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetAll(); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Messages.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Get${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Get${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { var data = await _service.GetByID(request.$pk); if (data == null) return ResponseProcess.GetFailResponse(Messages.${n}Messages.NotFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
$filterCtrlBlock
        [HttpPost] public async Task<APIResponse> Create([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Create${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Create${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Create(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.CreatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> Update([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Update${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Update${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Update(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.UpdatedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
        [HttpPost] public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; Delete${n}Request? request; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<Delete${n}Request>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Messages.ExceptionOccured); } if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Messages.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); try { fail = await _service.Delete(request.$pk); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.${n}Messages.DeletedSuccessfully); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Messages.ExceptionOccured); } }
    }
}
"@
    Set-Content "$base\Controllers\${n}Controller.cs" -Value $ctrlContent -Encoding utf8

    $dbSetEntries += "        public DbSet<$n> $dbSetName { get; set; }"
    $ckEntries += "        public const string $ckName = `"Cache_${ckName}`";"
    $diEntries += "builder.Services.AddScoped<I${n}Repository, ${n}Repository>();"
    $diEntries += "builder.Services.AddScoped<I${n}Service, ${n}Service>();"
    foreach ($fk in $fks) {
        $fkConfigs += "            modelBuilder.Entity<$n>().HasOne(x => x.$($fk.nav)).WithMany().HasForeignKey(x => x.$($fk.f)).OnDelete(DeleteBehavior.Restrict);"
    }
}

Write-Output "Generated $($entities.Count) entities"
$dbSetEntries | Set-Content "$env:TEMP\phase7_dbsets.txt" -Encoding utf8
$ckEntries | Set-Content "$env:TEMP\phase7_cachekeys.txt" -Encoding utf8
$diEntries | Set-Content "$env:TEMP\phase7_di.txt" -Encoding utf8
$fkConfigs | Set-Content "$env:TEMP\phase7_fkconfigs.txt" -Encoding utf8
Write-Output "DbSets: $($dbSetEntries.Count), CacheKeys: $($ckEntries.Count), DI: $($diEntries.Count), FKs: $($fkConfigs.Count)"
