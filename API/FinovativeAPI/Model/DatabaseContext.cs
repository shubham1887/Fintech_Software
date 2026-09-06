using Microsoft.EntityFrameworkCore;
using Project.Processing;
using System;
using System.Linq;

namespace Project.Model
{
    public class DatabaseContext : DbContext
    {
        private readonly string Constr = "";

        public DatabaseContext()
        {
            Constr = ConnectionClass.connectionString;
        }

        public DatabaseContext(DbContextOptions options)
           : base(options)
        {
        }

        private bool isFundOrCashCollectionTable(string tableName)
        {
            if (tableName.EqualsIgnoreCase("FundTransaction"))
            {
                return true;
            }
            if (tableName.EqualsIgnoreCase("CashCollectionDetail"))
            {
                return true;
            }

            return false;
        }

        private bool isArchiveTable(Object table)
        {
            string tableName = table.GetType().Name;
            if (tableName.EqualsIgnoreCase("Archive_TransactionMaster"))
            {
                return true;
            }
            if (tableName.EqualsIgnoreCase("Archive_TransactionDetails"))
            {
                return true;
            }

            return false;
        }

        private bool isValid(Object table)
        {
            if (table == null)
            {
                return false;
            }

            if (!Project.StaticModel.Company.BackDateFundTransfer.EqualsIgnoreCase("Yes"))
            {
                return false;
            }

            if (!isFundOrCashCollectionTable(table.GetType().Name))
            {
                return false;
            }
            DateTime dt = ((BaseEntity)table).DOC;
            DateTime MagicValueForNullDateTimeInNonNullableColumns = DateTime.MinValue;

            if (dt == MagicValueForNullDateTimeInNonNullableColumns)
            {
                return false;
            }

            return true;
        }

        private bool isValidDate(DateTime dt)
        {
            if (dt == DateTime.MinValue)
                return false;
            return true;
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                bool isArchive = isArchiveTable((BaseEntity)entityEntry.Entity);
                if (!isArchive)
                {
                    ((BaseEntity)entityEntry.Entity).UpdatedDOC = DateTime.Now;
                }

                if (entityEntry.State == EntityState.Added)
                {
                    bool MinDOCflag = isValidDate(((BaseEntity)entityEntry.Entity).DOC);
                    if (!MinDOCflag)
                    {
                        if (!isValid(entityEntry.Entity))
                        {
                            ((BaseEntity)entityEntry.Entity).DOC = DateTime.Now;
                            ((BaseEntity)entityEntry.Entity).DOCInt = Int32.Parse(((BaseEntity)entityEntry.Entity).DOC.ToString("yyyyMMdd"));
                        }
                    }
                }
            }
            return base.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Constr);
            }
        }
        
        public DbSet<SignUp> SignUp { get; set; }
        public DbSet<TransactionStatusChange> TransactionStatusChange { get; set; }
        public DbSet<ProviderUserMapping> ProviderUserMapping { get; set; }
        public DbSet<NPCIBank> NPCIBank { get; set; }
        public DbSet<UserServiceSubscriptionDetails> UserServiceSubscriptionDetails { get; set; }
        public DbSet<UserServiceSubscriptionCharge> UserServiceSubscriptionCharge { get; set; }
        public DbSet<ServiceAndUserwiseSubscription> ServiceAndUserwiseSubscription { get; set; }
        public DbSet<UserSubscriptionPlanMapping> UserSubscriptionPlanMapping { get; set; }
        public DbSet<SDKSetting> SDKSetting { get; set; }
        public DbSet<PanMitraOnboardedUser> PanMitraOnboardedUser { get; set; }
        public DbSet<SDKBBPSUserMapping> SDKBBPSUserMapping { get; set; }
        public DbSet<Archive_TransactionDetails> Archive_TransactionDetails { get; set; }
        public DbSet<Archive_TransactionMaster> Archive_TransactionMaster { get; set; }
        public DbSet<RedirectUrlMaster> RedirectUrlMaster { get; set; }
        public DbSet<UserCreationMaster> UserCreationMaster { get; set; }
        public DbSet<FingerPrintDeviceType> FingerPrintDeviceType { get; set; }    
        public DbSet<DematGrowthLossMaster> DematGrowthLossMaster { get; set; }
        public DbSet<UserCreationWallet> UserCreationWallet { get; set; }
        public DbSet<UserCreationSetting> UserCreationSetting { get; set; }
        public DbSet<UserMerchantMaster> UserMerchantMaster { get; set; }
        public DbSet<SmsTransactionMaster> SmsTransactionMaster { get; set; }
        public DbSet<DematSetting> DematSetting { get; set; }
        public DbSet<CMSSetting> CMSSetting { get; set; }
        public DbSet<MosambeeAEPSBank> MosambeeAEPSBank { get; set; }
        public DbSet<PaySprintBank> PaySprintBank { get; set; }
        public DbSet<eKYCLog> eKYCLog { get; set; }
        public DbSet<PayoutBeneficiaryOTP> PayoutBeneficiaryOTP { get; set; }
        public DbSet<POSSettingMaster> POSSettingMaster { get; set; }
        public DbSet<PayoutBeneficiary> PayoutBeneficiary { get; set; }
        public DbSet<AutoTransferUserwiseMapping> AutoTransferUserwiseMapping { get; set; }
        public DbSet<RemainderOperatorAmountWise> RemainderOperatorAmountWise { get; set; }
        public DbSet<ZoneMaster> ZoneMaster { get; set; }
        public DbSet<WebHookLog> WebHookLog { get; set; }
        public DbSet<AccountOpenSetting> AccountOpenSetting { get; set; }
        public DbSet<eKYCMaster> eKYCMaster { get; set; }
        public DbSet<WebHookSetting> WebHookSetting { get; set; }
        public DbSet<eKYCSetting> eKYCSetting { get; set; }
        public DbSet<KYCApprovedDetails> KYCApprovedDetails { get; set; }
        public DbSet<UserPaymentCollectMapping> UserPaymentCollectMapping { get; set; }
        public DbSet<UPITransferSetting> UPITransferSetting { get; set; }
        public DbSet<InvoiceNumberHistory> InvoiceNumberHistory { get; set; }
        public DbSet<CashFreeBeneficiary> CashFreeBeneficiary { get; set; }
        public DbSet<UserwiseDMTMapping> UserwiseDMTMapping { get; set; }
        public DbSet<PureDMTTransferBeneDetail> PureDMTTransferBeneDetail { get; set; }
        public DbSet<ReferralRegistration> ReferralRegistration { get; set; }
        public DbSet<CircleWiseCommission> CircleWiseCommission { get; set; }
        public DbSet<CircleWiseOperatorLimit> CircleWiseOperatorLimit { get; set; }
        public DbSet<RangeWiseSequence> RangeWiseSequence { get; set; }
        public DbSet<UserWisePayementGatewaySetting> UserWisePayementGatewaySetting { get; set; }
        public DbSet<EnquiryDetails> EnquiryDetails { get; set; }
        public DbSet<CircleWiseUserWiseOperatorOff> CircleWiseUserWiseOperatorOff { get; set; }
        public DbSet<OperatorAmountWiseSetting> OperatorAmountWiseSetting { get; set; }
        public DbSet<Redeemcommission> Redeemcommission { get; set; }
        public DbSet<ProviderConfigurationAdminCommission> ProviderConfigurationAdminCommission { get; set; }
        public DbSet<CashCollectionDetail> CashCollectionDetail { get; set; }
        public DbSet<OverdraftDetail> OverdraftDetail { get; set; }
        public DbSet<OverdraftMaster> OverdraftMaster { get; set; }
        public DbSet<CashDepositSetting> CashDepositSetting { get; set; }
        public DbSet<PaysprintOnboardedUser> PaysprintOnboardedUser { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<CreditCardSetting> CreditCardSetting { get; set; }
        public DbSet<ResponseKeyValue> ResponseKeyValue { get; set; }
        public DbSet<OperatorwiseCallbackSetting> OperatorwiseCallbackSetting { get; set; }
        public DbSet<UserTypeMaster> UserTypeMaster { get; set; }
        public DbSet<PayoutSetting> PayoutSetting { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Inbox> Inbox { get; set; }
        public DbSet<OperatorMaster> OperatorMaster { get; set; }
        public DbSet<ServiceMaster> ServiceMaster { get; set; }
        public DbSet<ProviderMaster> ProviderMaster { get; set; }
        public DbSet<BulkPaySetting> BulkPaySetting { get; set; }
        public DbSet<OTTSetting> OTTSetting { get; set; }
        public DbSet<BusSettingMaster> BusSettingMaster { get; set; }
        public DbSet<ProviderStateConfiguration> ProviderStateConfiguration { get; set; }
        public DbSet<LocationLookUp> LocationLookUp { get; set; }
        public DbSet<SentSMS> SentSms { get; set; }
        public DbSet<SlabMaster> SlabMaster { get; set; }
        public DbSet<SMSMaster> SmsMaster { get; set; }
        public DbSet<StateMaster> StateMaster { get; set; }
        public DbSet<UserActionMaster> UserActionMaster { get; set; }
        public DbSet<WalletMaster> WalletMaster { get; set; }
        public DbSet<WalletServiceMapping> WalletServiceMapping { get; set; }
        public DbSet<PayoutVendorBeneficiaryMapping> PayoutVendorBeneficiaryMapping { get; set; }
        public DbSet<TrustableDevice> TrustableDevice { get; set; }
        public DbSet<TransactionMaster> TransactionMaster { get; set; }
        public DbSet<FundTransaction> FundTransaction { get; set; }
        public DbSet<UserWalletMaster> UserWalletMaster { get; set; }
        public DbSet<UserBalanceHistory> UserBalanceHistory { get; set; }
        public DbSet<UserServiceMapping> UserServiceMapping { get; set; }
        public DbSet<ProviderConfiguration> ProviderConfiguration { get; set; }
        public DbSet<ProviderOperatorMaster> ProviderOperatorMaster { get; set; }
        public DbSet<ProviderDetails> ProviderDetails { get; set; }
        public DbSet<UserServiceCharge> UserServiceCharge { get; set; }
        public DbSet<TransactionDetails> TransactionDetails { get; set; }
        public DbSet<ProviderRequestResponseLog> ProviderRequestResponseLog { get; set; }
        public DbSet<BankRequestResponseLog> BankRequestResponseLog { get; set; }
        public DbSet<UPIVendorBeneficiaryMapping> UPIVendorBeneficiaryMapping { get; set; }
     
        public DbSet<MediaMaster> MediaMaster { get; set; }
        public DbSet<UserSlabMapping> UserSlabMapping { get; set; }

        public DbSet<AuthorizationMaster> AuthorizationMaster { get; set; }

        public DbSet<UrlTypeMaster> UrlTypeMaster { get; set; }

        public DbSet<Sender> Sender { get; set; }

        public DbSet<SenderOTP> SenderOTP { get; set; }

        public DbSet<Beneficiary> Beneficiary { get; set; }

        public DbSet<BeneficiaryOTP> BeneficiaryOTP { get; set; }

        public DbSet<DMTBank> DMTBank { get; set; }

        public DbSet<DateFormatMaster> DateFormatMaster { get; set; }

        public DbSet<SlabDetail> SlabDetail { get; set; }
        public DbSet<OperatorSequence> OperatorSequence { get; set; }

        public DbSet<ProviderResponseMaster> ProviderResponseMaster { get; set; }

        public DbSet<UserOperatorLimit> UserOperatorLimit { get; set; }

        public DbSet<ProviderResponseTypeMaster> ProviderResponseTypeMaster { get; set; }
        public DbSet<TransactionProviderHistory> TransactionProviderHistory { get; set; }
        public DbSet<ProviderCallBackLog> ProviderCallBackLog { get; set; }

        public DbSet<UserDocumentMaster> UserDocumentMaster { get; set; }

        public DbSet<TempTransactionMaster> TempTransactionMaster { get; set; }

        public DbSet<LoginHistrory> LoginHistrory { get; set; }

        public DbSet<OTPMaster> OTPMaster { get; set; }

        public DbSet<MessageSetting> MessageSetting { get; set; }

        public DbSet<OperatorStateMapping> OperatorStateMapping { get; set; }
        public DbSet<CallbackMaster> CallbackMaster { get; set; }
        public DbSet<APIUserCallbackRequestResponseLog> APIUserCallbackRequestResponseLog { get; set; }

        public DbSet<IPMaster> IPMaster { get; set; }
        public DbSet<SettingMaster> SettingMaster { get; set; }
        public DbSet<APIUserInitialRequestResponseLog> APIUserInitialRequestResponseLog { get; set; }

        public DbSet<CashCollectionMaster> CashCollectionMaster { get; set; }

        public DbSet<ComplaintMaster> ComplaintMaster { get; set; }

        public DbSet<OperatorSequenceUserWise> OperatorSequenceUserWise { get; set; }

        public DbSet<PaymentModeMaster> PaymentModeMaster { get; set; }
        public DbSet<UserBankDetails> UserBankDetails { get; set; }
        public DbSet<ExternalMedia> ExternalMedia { get; set; }

        public DbSet<VirtualBalance> VirtualBalance { get; set; }
        public DbSet<OperatorUserwise> OperatorUserwise { get; set; }

        public DbSet<UserwiseLowBalance> UserwiseLowBalance { get; set; }
        public DbSet<PlanSetting> PlanSetting { get; set; }
        public DbSet<VerificationSetting> VerificationSetting { get; set; }
        public DbSet<UserwiseServiceChargeMaster> UserwiseServiceChargeMaster { get; set; }

        public DbSet<SupportUserAccessMaster> SupportUserAccessMaster { get; set; }
        public DbSet<SupportUserAccessDetail> SupportUserAccessDetail { get; set; }

        public DbSet<RefreshTokenMaster> RefreshTokenMaster { get; set; }

        public DbSet<UserActionHistory> UserActionHistory { get; set; }
        public DbSet<MenuMaster> MenuMaster { get; set; }
        public DbSet<MenuActionMapping> MenuActionMapping { get; set; }
        public DbSet<ProviderOperatorCircleMaster> ProviderOperatorCircleMaster { get; set; }

        public DbSet<CircleAPIMaster> CircleAPIMaster { get; set; }
        public DbSet<CompanyMaster> CompanyMaster { get; set; }
        public DbSet<Notification> Notification { get; set; }

        public DbSet<SliderMaster> SliderMaster { get; set; }
        public DbSet<NewsMaster> NewsMaster { get; set; }
        public DbSet<DenominationMaster> DenominationMaster { get; set; }
        public DbSet<EmailSettingMaster> EmailSettingMaster { get; set; }
        public DbSet<BBPSConfiguration> BBPSConfiguration { get; set; }
        public DbSet<ChannelSlabMaster> ChannelSlabMaster { get; set; }

        public DbSet<DailyAuthenticationMaster> DailyAuthenticationMaster { get; set; }
        public DbSet<ServiceSubscriptionCreatePlanMaster> ServiceSubscriptionCreatePlanMaster { get; set; }
        public DbSet<ServiceSubscriptionPlanDetails> ServiceSubscriptionPlanDetails { get; set; }
        public DbSet<VendorBeneficiaryMapping> VendorBeneficiaryMapping { get; set; }

        public DbSet<ChannelSlabDetail> ChannelSlabDetail { get; set; }
        public DbSet<UserChannelSlabMapping> UserChannelSlabMapping { get; set; }
        public DbSet<CallMeCategoryMaster> CallMeCategoryMaster { get; set; }
        public DbSet<CallMeRequest> CallMeRequest { get; set; }
        public DbSet<ProviderPurchase> ProviderPurchase { get; set; }
        public DbSet<ReconciliationSetting> ReconciliationSetting { get; set; }
        public DbSet<DMTSetting> DMTSetting { get; set; }

        public DbSet<TicketMaster> TicketMaster { get; set; }
        public DbSet<OfflineBookingDetails> OfflineBookingDetails { get; set; }

        public DbSet<ProviderOperatorDenominationMapping> ProviderOperatorDenominationMapping { get; set; }
        public DbSet<RealProviderMapping> RealProviderMapping { get; set; }

        public DbSet<BulkMessage> BulkMessage { get; set; }

        public DbSet<InvoiceSetting> InvoiceSetting { get; set; }
        public DbSet<UnitMaster> UnitMaster { get; set; }
        public DbSet<ThirdPartyServiceProvider> ThirdPartyServiceProvider { get; set; }
        public DbSet<OfflineProductMaster> OfflineProductMaster { get; set; }
        public DbSet<OfflineProductMasterImages> OfflineProductMasterImages { get; set; }
        public DbSet<OfflineProductUIControlMapping> OfflineProductUIControlMapping { get; set; }
        public DbSet<PaymentGatewaySetting> PaymentGatewaySetting { get; set; }
        public DbSet<ViewProviderMapping> ViewProviderMapping { get; set; }
        public DbSet<KycDocumentMaster> KycDocumentMaster { get; set; }
        public DbSet<EcollectionDetails> EcollectionDetails { get; set; }
        public DbSet<TransactionManualUpdate> TransactionManualUpdate { get; set; }

        public DbSet<UpiSender> UpiSender { get; set; }
        public DbSet<UpiBeneficiary> UpiBeneficiary { get; set; }

        public DbSet<BrowserUserAgentMaster> BrowserUserAgentMaster { get; set; }
        public DbSet<UISettingMaster> UISettingMaster { get; set; }

        public DbSet<TicketCategory> TicketCategory { get; set; }
        public DbSet<LowBalanceConfiguration> LowBalanceConfiguration { get; set; }

        public DbSet<PaymentGateway> PaymentGateway { get; set; }

        public DbSet<CyberPlateSetting> CyberPlateSetting { get; set; }
        public DbSet<EcollectionSetting> EcollectionSetting { get; set; }

        public DbSet<ProviderDetailsSample> ProviderDetailsSample { get; set; }
        public DbSet<SubscriptionMaster> SubscriptionMaster { get; set; }
        public DbSet<UserBalanceCapping> UserBalanceCapping { get; set; }
        public DbSet<LapuPartyMaster> LapuPartyMaster { get; set; }
        public DbSet<LapuMaster> LapuMaster { get; set; }
        public DbSet<LapuTransactionDetail> LapuTransactionDetail { get; set; }
        public DbSet<ProviderResponseMasterSample> ProviderResponseMasterSample { get; set; }
        public DbSet<PasswordChangeHistory> PasswordChangeHistory { get; set; }
        public DbSet<WrongPasswordHistory> WrongPasswordHistory { get; set; }

        public DbSet<PaymentGatewayLog> PaymentGatewayLog { get; set; }

        public DbSet<ROfferCommission> ROfferCommission { get; set; }
        public DbSet<PrepaidActivation> PrepaidActivation { get; set; }

        public DbSet<AEPSSetting> AEPSSetting { get; set; }

        public DbSet<RichTextMedia> RichTextMedia { get; set; }
        public DbSet<ProviderConfigurationSample> ProviderConfigurationSample { get; set; }
        public DbSet<ServiceWiseSliderSetting> ServiceWiseSliderSetting { get; set; }
        public DbSet<VirtualAccountMaster> VirtualAccountMaster { get; set; }
        public DbSet<OperationalHistory> OperationalHistory { get; set; }
        public DbSet<MicroATMSetting> MicroATMSetting { get; set; }
        public DbSet<ProductImageMapping> ProductImageMapping { get; set; }
        public DbSet<Category> Category { get; set; }

        public DbSet<SubCategory> SubCategory { get; set; }

        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<ProductDetails> ProductDetails { get; set; }
        public DbSet<UserwiseSaleTarget> UserwiseSaleTarget { get; set; }
        public DbSet<TargetSaleCommission> TargetSaleCommission { get; set; }
        public DbSet<ChannelCategory> ChannelCategory { get; set; }
        public DbSet<CategoryChannelMapping> CategoryChannelMapping { get; set; }
        public DbSet<DthPackageChannelMapping> DthPackageChannelMapping { get; set; }
        public DbSet<OfflineSlabDetail> OfflineSlabDetail { get; set; }
        public DbSet<GSTHold> GSTHold { get; set; }
        public DbSet<PanCardSetting> PanCardSetting { get; set; }
        public DbSet<LeadGenerationSetting> LeadGenerationSetting { get; set; }

        public DbSet<AndroidMenuMaster> AndroidMenuMaster { get; set; }
        public DbSet<AndroidSectionMaster> AndroidSectionMaster { get; set; }

        public DbSet<ServiceSubscriptionPlanMaster> ServiceSubscriptionPlanMaster { get; set; }
        public DbSet<CreditInstitutionMaster> CreditInstitutionMaster { get; set; }
        public DbSet<UserWisePlanLimit> UserWisePlanLimit { get; set; }
        public DbSet<UserPlanUtilization> UserPlanUtilization { get; set; }
        public DbSet<BlackListBankMaster> BlackListBankMaster { get; set; }
        public DbSet<VoucherType> VoucherType { get; set; }
        public DbSet<VoucherMaster> VoucherMaster { get; set; }
        public DbSet<VoucherDetails> VoucherDetails { get; set; }
        public DbSet<ThreewayReconCompletion> ThreewayReconCompletion { get; set; }    
        public DbSet<LeadGenerationMaster> LeadGenerationMaster { get; set; }    
        public DbSet<LoanTypeMaster> LoanTypeMaster { get; set; }    
        public DbSet<TransactionAPIUserRequestMaster> TransactionAPIUserRequestMaster { get; set; }

        public DbSet<ConsumerTypeMaster> ConsumerTypeMaster { get; set; }
        public DbSet<UserWisePlanView> UserWisePlanView { get; set; }
        public DbSet<BBPSSettings> BBPSSettings { get; set; }
        public DbSet<MPinHistory> MPinHistory { get; set; }
        public DbSet<Location_PinCodeMaster> Location_PinCodeMaster { get; set; }
        public DbSet<FailedSignUpData> FailedSignUpData { get; set; }
        
        

        public DbSet<OnboardingUserDocument> OnboardingUserDocument { get; set; }
        public DbSet<OnboardingUser> OnboardingUser { get; set; }
        public DbSet<LastChangeUserDetails> LastChangeUserDetails { get; set; }
        public DbSet<DeviceWiseFailLoginHistory> DeviceWiseFailLoginHistory { get; set; }
        public DbSet<LOBMaster> LOBMaster { get; set; }
        public DbSet<LOB_Details> LOB_Details { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserMaster>()
              .HasIndex(u => u.MobileNumber)
              .IsUnique();

            modelBuilder.Entity<UserMaster>()
             .HasIndex(u => u.EmailID)
             .IsUnique();
        }
    }
}