using FintechApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<UserMaster> UserMasters { get; set; }
        public DbSet<UserTypeMaster> UserTypeMasters { get; set; }
        public DbSet<UserCodeFormat> UserCodeFormats { get; set; }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<WrongPasswordLog> WrongPasswordLogs { get; set; }
        public DbSet<MenuItem> MenuItem { get; set; }

        public DbSet<SubMenuItem> SubMenuItem { get; set; }
        public DbSet<ServiceMaster> ServiceMasters { get; set; }
        public DbSet<OperatorMaster> OperatorMasters { get; set; }
        public DbSet<UISettingMaster> UISettingMasters { get; set; }
        public DbSet<IPMaster> IPMasters { get; set; }
        public DbSet<CallMeRequest> CallMeRequests { get; set; }

        public DbSet<BBPSConfiguration> BBPSConfigurations { get; set; }
        public DbSet<CallMeCategoryMaster> CallMeCategoryMaster { get; set; }
        public DbSet<WalletMaster> WalletMaster { get; set; }
        public DbSet<UserBalanceMaster> UserBalanceMaster { get; set; }
        public DbSet<TicketMaster> TicketMasters { get; set; }

        public DbSet<TicketCategory> TicketCategories { get; set; }
        public DbSet<FundTransaction> FundTransactions { get; set; }
        public DbSet<AEPSSetting> AEPSSettings { get; set; }
        public DbSet<ProviderMaster> ProviderMasters { get; set; }
        public DbSet<TransactionMaster> TransactionMasters { get; set; }
        public DbSet<DMTSender> DMTSenders { get; set; }
        public DbSet<DMTBeneficiary> DMTBeneficiaries { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<DMTBank> DMTBanks { get; set; }
        public DbSet<SlabMaster> SlabMasters { get; set; }
        public DbSet<SlabDetail> SlabDetails { get; set; }
        public DbSet<VerificationDocumentConfig> VerificationDocumentConfigs { get; set; }
        public DbSet<ChannelSlabMaster> ChannelSlabMasters { get; set; }
        public DbSet<ChannelSlabDetail> ChannelSlabDetails { get; set; }
        public DbSet<ProviderConfiguration> ProviderConfigurations { get; set; }
        public DbSet<ProviderDetails> ProviderDetails { get; set; }
        public DbSet<DMTSetting> DMTSettings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // In-app alert feed (the bell) + the devices a push can reach.
        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<AppNotificationRecipient> AppNotificationRecipients { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }

        public DbSet<NewsMaster> NewsMasters { get; set; }
        public DbSet<SettingMaster> SettingMasters { get; set; }
        public DbSet<EmailSettingMaster> EmailSettingMasters { get; set; }
        public DbSet<UserSlabMapping> UserSlabMappings { get; set; }
        public DbSet<UserChannelSlabMapping> UserChannelSlabMappings { get; set; }
        public DbSet<SliderMaster> SliderMasters { get; set; }
        public DbSet<SmsMaster> SmsMasters { get; set; }
        public DbSet<MessageSetting> MessageSettings { get; set; }
        public DbSet<DenominationMaster> DenominationMasters { get; set; }
        public DbSet<CompanyMaster> CompanyMasters { get; set; }
        public DbSet<NPCIBank> NPCIBanks { get; set; }
        public DbSet<OTPMaster> OTPMasters { get; set; }
        public DbSet<KycDocumentMaster> KycDocumentMasters { get; set; }
        public DbSet<LowBalanceConfiguration> LowBalanceConfigurations { get; set; }
        public DbSet<ComplaintMaster> ComplaintMasters { get; set; }
        public DbSet<PayoutSetting> PayoutSettings { get; set; }
        public DbSet<PayoutBeneficiary> PayoutBeneficiaries { get; set; }
        public DbSet<CircleWiseCommission> CircleWiseCommissions { get; set; }
        public DbSet<UserCreationMaster> UserCreationMasters { get; set; }
        public DbSet<UserCreationSetting> UserCreationSettings { get; set; }
        public DbSet<UserBankDetails> UserBankDetails { get; set; }
        public DbSet<UserDocumentMaster> UserDocumentMasters { get; set; }
        public DbSet<KYCApprovedDetails> KYCApprovedDetails { get; set; }
        public DbSet<OperatorAmountWiseSetting> OperatorAmountWiseSettings { get; set; }
        public DbSet<AccountOpenSetting> AccountOpenSettings { get; set; }
        public DbSet<OperatorSequence> OperatorSequences { get; set; }
        public DbSet<OperatorStateMapping> OperatorStateMappings { get; set; }
        public DbSet<CashDepositSetting> CashDepositSettings { get; set; }
        public DbSet<AutoTransferUserwiseMapping> AutoTransferUserwiseMappings { get; set; }

        // Phase 5 DbSets
        public DbSet<ZoneMaster> ZoneMasters { get; set; }
        public DbSet<BBPSSettings> BBPSSettings { get; set; }
        public DbSet<InvoiceSetting> InvoiceSettings { get; set; }
        public DbSet<PaymentGatewaySetting> PaymentGatewaySettings { get; set; }
        public DbSet<UserCreationWallet> UserCreationWallets { get; set; }
        public DbSet<UserwiseLowBalance> UserwiseLowBalances { get; set; }
        public DbSet<UserWalletMapping> UserWalletMappings { get; set; }
        public DbSet<UserWalletMaster> UserWalletMasters { get; set; }
        // Audit of every hold placed on or released from a wallet (see WalletHoldHistory).
        public DbSet<WalletHoldHistory> WalletHoldHistories { get; set; }
        public DbSet<UserServiceMapping> UserServiceMappings { get; set; }
        public DbSet<ProviderStateConfiguration> ProviderStateConfigurations { get; set; }
        public DbSet<ReconciliationSetting> ReconciliationSettings { get; set; }
        public DbSet<PasswordChangeHistory> PasswordChangeHistories { get; set; }
        public DbSet<CallbackMaster> CallbackMasters { get; set; }
        public DbSet<WebHookSetting> WebHookSettings { get; set; }
        public DbSet<WebHookLog> WebHookLogs { get; set; }
        public DbSet<UserServiceCharge> UserServiceCharges { get; set; }
        public DbSet<UserOperatorLimit> UserOperatorLimits { get; set; }
        public DbSet<OperatorUserwise> OperatorUserwises { get; set; }
        public DbSet<OperatorSequenceUserWise> OperatorSequenceUserWises { get; set; }
        public DbSet<OperatorwiseCallbackSetting> OperatorwiseCallbackSettings { get; set; }
        public DbSet<ProviderOperatorMaster> ProviderOperatorMasters { get; set; }
        public DbSet<ProviderResponseMaster> ProviderResponseMasters { get; set; }

        /// <summary>Admin-authored rewrites for the words a user is shown (see ResponseMessageRule).</summary>
        public DbSet<ResponseMessageRule> ResponseMessageRules { get; set; }
        public DbSet<ProviderUserMapping> ProviderUserMappings { get; set; }
        public DbSet<RangeWiseSequence> RangeWiseSequences { get; set; }
        public DbSet<VoucherType> VoucherTypes { get; set; }
        public DbSet<VoucherMaster> VoucherMasters { get; set; }
        public DbSet<VoucherDetails> VoucherDetails { get; set; }
        public DbSet<UserBalanceCapping> UserBalanceCappings { get; set; }
        public DbSet<UserBalanceHistory> UserBalanceHistories { get; set; }
        public DbSet<UserwiseDMTMapping> UserwiseDMTMappings { get; set; }

        // Phase 6 DbSets
        public DbSet<APIUserCallbackRequestResponseLog> APIUserCallbackRequestResponseLogs { get; set; }
        public DbSet<APIUserInitialRequestResponseLog> APIUserInitialRequestResponseLogs { get; set; }
        public DbSet<BulkMessage> BulkMessages { get; set; }
        public DbSet<CashCollectionMaster> CashCollectionMasters { get; set; }
        public DbSet<CashCollectionDetail> CashCollectionDetails { get; set; }
        public DbSet<CircleAPIMaster> CircleAPIMasters { get; set; }
        public DbSet<CircleWiseOperatorLimit> CircleWiseOperatorLimits { get; set; }
        public DbSet<CircleWiseUserWiseOperatorOff> CircleWiseUserWiseOperatorOffs { get; set; }
        public DbSet<CreditCardSetting> CreditCardSettings { get; set; }
        public DbSet<DateFormatMaster> DateFormatMasters { get; set; }
        public DbSet<EcollectionSetting> EcollectionSettings { get; set; }
        public DbSet<EcollectionDetails> EcollectionDetails { get; set; }
        public DbSet<EnquiryDetails> EnquiryDetails { get; set; }
        public DbSet<ExternalMedia> ExternalMedias { get; set; }
        public DbSet<FingerPrintDeviceType> FingerPrintDeviceTypes { get; set; }
        public DbSet<GSTHold> GSTHolds { get; set; }
        public DbSet<Inbox> Inboxes { get; set; }
        public DbSet<InvoiceNumberHistory> InvoiceNumberHistories { get; set; }
        public DbSet<LapuPartyMaster> LapuPartyMasters { get; set; }
        public DbSet<LapuMaster> LapuMasters { get; set; }
        public DbSet<LeadGenerationSetting> LeadGenerationSettings { get; set; }
        public DbSet<LocationLookUp> LocationLookUps { get; set; }
        public DbSet<MediaMaster> MediaMasters { get; set; }
        public DbSet<MenuMaster> MenuMasters { get; set; }
        public DbSet<PaymentGatewayLog> PaymentGatewayLogs { get; set; }
        public DbSet<PaymentModeMaster> PaymentModeMasters { get; set; }
        public DbSet<PayoutBeneficiaryOTP> PayoutBeneficiaryOTPs { get; set; }
        public DbSet<PlanSetting> PlanSettings { get; set; }
        public DbSet<ProviderCallBackLog> ProviderCallBackLogs { get; set; }
        public DbSet<ProviderConfigurationAdminCommission> ProviderConfigurationAdminCommissions { get; set; }
        public DbSet<ProviderOperatorDenominationMapping> ProviderOperatorDenominationMappings { get; set; }
        public DbSet<ProviderPurchase> ProviderPurchases { get; set; }
        public DbSet<ProviderRequestResponseLog> ProviderRequestResponseLogs { get; set; }
        public DbSet<ProviderResponseTypeMaster> ProviderResponseTypeMasters { get; set; }
        public DbSet<RealProviderMapping> RealProviderMappings { get; set; }
        public DbSet<ViewProviderMapping> ViewProviderMappings { get; set; }
        public DbSet<Redeemcommission> Redeemcommissions { get; set; }
        public DbSet<RedirectUrlMaster> RedirectUrlMasters { get; set; }
        public DbSet<RichTextMedia> RichTextMedias { get; set; }
        public DbSet<ROfferCommission> ROfferCommissions { get; set; }
        public DbSet<SentSMS> SentSMS { get; set; }
        public DbSet<ServiceAndUserwiseSubscription> ServiceAndUserwiseSubscriptions { get; set; }
        public DbSet<ServiceSubscriptionPlanMaster> ServiceSubscriptionPlanMasters { get; set; }
        public DbSet<ServiceWiseSliderSetting> ServiceWiseSliderSettings { get; set; }
        public DbSet<PayoutVendorBeneficiaryMapping> PayoutVendorBeneficiaryMappings { get; set; }
        public DbSet<BrowserUserAgentMaster> BrowserUserAgentMasters { get; set; }

        // Phase 7
        public DbSet<eKYCSetting> eKYCSettings { get; set; }
        public DbSet<MicroATMSetting> MicroATMSettings { get; set; }
        public DbSet<SubscriptionMaster> SubscriptionMasters { get; set; }
        public DbSet<SupportUserAccessMaster> SupportUserAccessMasters { get; set; }
        public DbSet<ThirdPartyServiceProvider> ThirdPartyServiceProviders { get; set; }
        public DbSet<UnitMaster> UnitMasters { get; set; }
        public DbSet<UrlTypeMaster> UrlTypeMasters { get; set; }
        public DbSet<VerificationSetting> VerificationSettings { get; set; }
        public DbSet<VirtualBalance> VirtualBalances { get; set; }
        public DbSet<UPITransferSetting> UPITransferSettings { get; set; }
        public DbSet<RemainderOperatorAmountWise> RemainderOperatorAmountWises { get; set; }
        public DbSet<SDKSetting> SDKSettings { get; set; }
        public DbSet<PanCardSetting> PanCardSettings { get; set; }
        public DbSet<LastChangeUserDetails> LastChangeUserDetailss { get; set; }
        public DbSet<LeadGenerationMaster> LeadGenerationMasters { get; set; }
        public DbSet<BlackListBankMaster> BlackListBankMasters { get; set; }
        public DbSet<BulkPaySetting> BulkPaySettings { get; set; }
        public DbSet<CMSSetting> CMSSettings { get; set; }
        public DbSet<WebsitePageMaster> WebsitePageMasters { get; set; }
        public DbSet<ConsumerTypeMaster> ConsumerTypeMasters { get; set; }
        public DbSet<DematSetting> DematSettings { get; set; }
        public DbSet<DeviceWiseFailLoginHistory> DeviceWiseFailLoginHistorys { get; set; }
        public DbSet<LoanTypeMaster> LoanTypeMasters { get; set; }
        public DbSet<MenuActionMapping> MenuActionMappings { get; set; }
        public DbSet<OTTSetting> OTTSettings { get; set; }
        public DbSet<PaySprintBank> PaySprintBanks { get; set; }
        public DbSet<POSSettingMaster> POSSettingMasters { get; set; }
        public DbSet<SignUp> SignUps { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<Category> Categorys { get; set; }
        public DbSet<ChannelCategory> ChannelCategorys { get; set; }
        public DbSet<CreditInstitutionMaster> CreditInstitutionMasters { get; set; }
        public DbSet<AndroidMenuMaster> AndroidMenuMasters { get; set; }
        public DbSet<AndroidSectionMaster> AndroidSectionMasters { get; set; }
        public DbSet<TransactionAPIUserRequestMaster> TransactionAPIUserRequestMasters { get; set; }
        public DbSet<ProviderOperatorCircleMaster> ProviderOperatorCircleMasters { get; set; }
        public DbSet<OnboardingUserDocument> OnboardingUserDocuments { get; set; }
        public DbSet<UPIVendorBeneficiaryMapping> UPIVendorBeneficiaryMappings { get; set; }
        public DbSet<VirtualAccountMaster> VirtualAccountMasters { get; set; }
        public DbSet<DailyAuthenticationMaster> DailyAuthenticationMasters { get; set; }
        public DbSet<OperationalHistory> OperationalHistorys { get; set; }
        public DbSet<SDKBBPSUserMapping> SDKBBPSUserMappings { get; set; }
        public DbSet<UserwiseSaleTarget> UserwiseSaleTargets { get; set; }
        public DbSet<UserwiseServiceChargeMaster> UserwiseServiceChargeMasters { get; set; }
        public DbSet<TargetSaleCommission> TargetSaleCommissions { get; set; }
        public DbSet<UserActionHistory> UserActionHistorys { get; set; }
        // Every change the panel makes to a user record - who, what, from what to what, why, from where.
        public DbSet<UserChangeHistory> UserChangeHistorys { get; set; }
        public DbSet<ReferralRegistration> ReferralRegistrations { get; set; }
        public DbSet<OverdraftDetail> OverdraftDetails { get; set; }
        public DbSet<OverdraftMaster> OverdraftMasters { get; set; }
        public DbSet<TrustableDevice> TrustableDevices { get; set; }
        public DbSet<UserQuickService> UserQuickServices { get; set; }
        public DbSet<UserTwoFactor> UserTwoFactors { get; set; }

        // ── Dynamic tax engine (GST / TDS) ──────────────────────────────────────────────
        // Masters are effective-dated: a rate or rule is never edited in place, the old row is
        // closed and a new one opened, so a past transaction still answers with what it was
        // actually charged. TransactionTaxDetails is the stamp reports READ instead of
        // recomputing tax — see that entity's comment for why that distinction matters.
        public DbSet<TaxRateMaster> TaxRateMasters { get; set; }
        public DbSet<HSNSACMaster> HSNSACMasters { get; set; }
        public DbSet<TDSSectionMaster> TDSSectionMasters { get; set; }
        public DbSet<TaxRuleMaster> TaxRuleMasters { get; set; }
        public DbSet<PartyTaxProfile> PartyTaxProfiles { get; set; }
        public DbSet<PartyServiceRelationship> PartyServiceRelationships { get; set; }
        public DbSet<TaxRuleAudit> TaxRuleAudits { get; set; }
        public DbSet<TransactionTaxDetail> TransactionTaxDetails { get; set; }

        // ── GST documents (Phase 2) ─────────────────────────────────────────────────────
        // Sale and purchase share TaxInvoiceHeaders/Lines so both sides carry the same columns —
        // gross value, taxable value and the basis between them. Comparing two tables that have
        // drifted apart is how the 18cr-against-19cr mismatch stayed invisible.
        public DbSet<TaxDocumentSeries> TaxDocumentSeries { get; set; }
        public DbSet<TaxInvoiceHeader> TaxInvoiceHeaders { get; set; }
        public DbSet<TaxInvoiceLine> TaxInvoiceLines { get; set; }
        public DbSet<TaxInvoiceAnnexureLine> TaxInvoiceAnnexureLines { get; set; }
        public DbSet<EInvoiceLog> EInvoiceLogs { get; set; }
        public DbSet<ExchangePartnerLeg> ExchangePartnerLegs { get; set; }
        public DbSet<UserMerchantMaster> UserMerchantMasters { get; set; }
        public DbSet<UserWisePayementGatewaySetting> UserWisePayementGatewaySettings { get; set; }
        public DbSet<OnboardingUser> OnboardingUsers { get; set; }
        public DbSet<SmsTransactionMaster> SmsTransactionMasters { get; set; }
        public DbSet<WalletServiceMapping> WalletServiceMappings { get; set; }
        public DbSet<eKYCLog> eKYCLogs { get; set; }
        public DbSet<eKYCMaster> eKYCMasters { get; set; }
        public DbSet<BankRequestResponseLog> BankRequestResponseLogs { get; set; }
        public DbSet<SupportUserAccessDetail> SupportUserAccessDetails { get; set; }
        public DbSet<LapuTransactionDetail> LapuTransactionDetails { get; set; }
        public DbSet<UpiSender> UpiSenders { get; set; }
        public DbSet<UpiBeneficiary> UpiBeneficiarys { get; set; }

        // Phase 8
        public DbSet<UserWisePlanLimit> UserWisePlanLimits { get; set; }
        public DbSet<UserWisePlanView> UserWisePlanViews { get; set; }
        public DbSet<Sender> Senders { get; set; }
        public DbSet<Beneficiary> Beneficiarys { get; set; }
        public DbSet<SenderOTP> SenderOTPs { get; set; }
        public DbSet<BeneficiaryOTP> BeneficiaryOTPs { get; set; }
        public DbSet<TransactionDetails> TransactionDetailss { get; set; }
        public DbSet<TransactionProviderHistory> TransactionProviderHistorys { get; set; }
        public DbSet<PaymentGateway> PaymentGateways { get; set; }
        public DbSet<TransactionManualUpdate> TransactionManualUpdates { get; set; }
        public DbSet<TransactionStatusChange> TransactionStatusChanges { get; set; }
        public DbSet<ThreewayReconCompletion> ThreewayReconCompletions { get; set; }
        public DbSet<UserActionMaster> UserActionMasters { get; set; }
        public DbSet<AuthorizationMaster> AuthorizationMasters { get; set; }
        public DbSet<UserPaymentCollectMapping> UserPaymentCollectMappings { get; set; }
        public DbSet<UserPlanUtilization> UserPlanUtilizations { get; set; }
        public DbSet<MPinHistory> MPinHistorys { get; set; }
        public DbSet<WrongPasswordHistory> WrongPasswordHistorys { get; set; }
        public DbSet<FailedSignUpData> FailedSignUpDatas { get; set; }
        public DbSet<ServiceSubscriptionCreatePlanMaster> ServiceSubscriptionCreatePlanMasters { get; set; }
        public DbSet<ServiceSubscriptionPlanDetails> ServiceSubscriptionPlanDetailss { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserServiceSubscriptionDetails> UserServiceSubscriptionDetailss { get; set; }
        public DbSet<UserServiceSubscriptionCharge> UserServiceSubscriptionCharges { get; set; }
        public DbSet<PrepaidActivation> PrepaidActivations { get; set; }
        public DbSet<PureDMTTransferBeneDetail> PureDMTTransferBeneDetails { get; set; }
        public DbSet<SubCategory> SubCategorys { get; set; }
        public DbSet<CategoryChannelMapping> CategoryChannelMappings { get; set; }
        public DbSet<LocationPinCodeMaster> LocationPinCodeMasters { get; set; }

        // Fund-request Mode / SubMode masters
        public DbSet<Mode> Modes { get; set; }
        public DbSet<SubMode> SubModes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit primary keys for entities whose key column does not follow the
            // <Entity>ID naming convention and have no [Key] attribute.
            modelBuilder.Entity<DailyAuthenticationMaster>().HasKey(x => x.AuthenticationID);
            modelBuilder.Entity<TransactionDetails>().HasKey(x => x.TransactionDetailID);
            modelBuilder.Entity<TransactionProviderHistory>().HasKey(x => x.RechargeProviderHistoryID);
            modelBuilder.Entity<UrlTypeMaster>().HasKey(x => x.UrlTypeID);
            modelBuilder.Entity<UserActionHistory>().HasKey(x => x.UserHistoryID);

            // The user change log is read by (user, date) and by (actor, date) - the two questions
            // the report asks. The table is append-only, so the indexes cost nothing on writes.
            modelBuilder.Entity<UserChangeHistory>().HasIndex(x => new { x.UserID, x.DateTime });
            modelBuilder.Entity<UserChangeHistory>().HasIndex(x => new { x.ChangedByUserID, x.DateTime });

            // Fund-request SubMode → Mode (Restrict: a mode with sub modes cannot be hard-deleted;
            // the service also soft-blocks deletion while sub modes exist).
            modelBuilder.Entity<SubMode>()
                .HasOne(x => x.Mode).WithMany().HasForeignKey(x => x.ModeID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMaster>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();

            // A user pins a service once. Saving the shortcut list replaces it wholesale, so this
            // index is what stops a double-submit (or two tabs) leaving the same tile twice.
            modelBuilder.Entity<UserQuickService>()
                .HasIndex(q => new { q.UserID, q.ServiceID })
                .IsUnique();

            // One authenticator enrolment per user. Enrolling again replaces that row rather than
            // adding a second secret — two live secrets would mean a phone the user thought they
            // had revoked still produces working codes.
            modelBuilder.Entity<UserTwoFactor>()
                .HasIndex(t => t.UserID)
                .IsUnique();

            // ── Tax engine ──────────────────────────────────────────────────────────────
            // One stamp per transaction, enforced. The engine resolves a transaction once and
            // writes what it decided; a second stamp would mean two different answers on record
            // for the same transaction and no way to say which one the return was filed on. The
            // unique index is also what makes a retry of the stamping job safe.
            modelBuilder.Entity<TransactionTaxDetail>()
                .HasIndex(t => t.TransactionMasterID)
                .IsUnique();

            // The resolver's hot path: every lookup filters on direction, service and date.
            modelBuilder.Entity<TaxRuleMaster>()
                .HasIndex(r => new { r.Direction, r.ServiceID, r.EffectiveFrom });

            // One tax profile per party.
            modelBuilder.Entity<PartyTaxProfile>()
                .HasIndex(p => p.UserID)
                .IsUnique();

            // A party's relationship on a service, over time — the Bharti Cellular lookup.
            modelBuilder.Entity<PartyServiceRelationship>()
                .HasIndex(p => new { p.UserID, p.ServiceID, p.EffectiveFrom });

            modelBuilder.Entity<TaxRateMaster>().HasIndex(r => new { r.TaxType, r.Code, r.EffectiveFrom });
            modelBuilder.Entity<HSNSACMaster>().HasIndex(h => h.Code);
            modelBuilder.Entity<TDSSectionMaster>().HasIndex(s => new { s.SectionCode, s.EffectiveFrom });
            modelBuilder.Entity<TaxRuleAudit>().HasIndex(a => new { a.EntityName, a.EntityID, a.ChangedOn });

            // ── GST documents ───────────────────────────────────────────────────────────
            // One live counter per series per registration per financial year. A second row for
            // the same three would be a second counter handing out the same numbers.
            modelBuilder.Entity<TaxDocumentSeries>()
                .HasIndex(s => new { s.SeriesCode, s.CompanyGSTIN, s.FinancialYear })
                .IsUnique();

            // No duplicate invoice number under one registration — the most expensive GST error
            // available. Filtered to sales because a purchase carries the SUPPLIER's number, and
            // two suppliers may legitimately both have an "INV-001".
            modelBuilder.Entity<TaxInvoiceHeader>()
                .HasIndex(h => new { h.CompanyGSTIN, h.DocumentType, h.DocumentNo })
                .IsUnique()
                .HasFilter("[Direction] = 'SALE' AND [IsDeleted] = 0");

            // The same supplier bill cannot be entered twice — the control that stops a purchase
            // register inflating, which is half of what the 1 crore question was about.
            modelBuilder.Entity<TaxInvoiceHeader>()
                .HasIndex(h => new { h.PartyUserID, h.DocumentNo })
                .IsUnique()
                .HasFilter("[Direction] = 'PURCHASE' AND [IsDeleted] = 0");

            modelBuilder.Entity<TaxInvoiceHeader>().HasIndex(h => new { h.Direction, h.PeriodKey, h.Status });
            modelBuilder.Entity<TaxInvoiceLine>().HasIndex(l => l.TaxInvoiceHeaderID);
            modelBuilder.Entity<TaxInvoiceAnnexureLine>().HasIndex(a => a.TaxInvoiceHeaderID);

            // A source can be invoiced ONCE. These two indexes are what make invoice generation
            // safe to re-run: a half-finished batch is simply run again, because the sources it
            // already covered cannot be picked up a second time. Filtered because exactly one of
            // the two ids is set on any given row.
            modelBuilder.Entity<TaxInvoiceAnnexureLine>()
                .HasIndex(a => a.TransactionMasterID)
                .IsUnique()
                .HasFilter("[TransactionMasterID] > 0 AND [IsDeleted] = 0");

            modelBuilder.Entity<TaxInvoiceAnnexureLine>()
                .HasIndex(a => a.FundTransactionID)
                .IsUnique()
                .HasFilter("[FundTransactionID] > 0 AND [IsDeleted] = 0");

            // Drives the daily "IRN still pending and the 30-day limit is approaching" alert.
            modelBuilder.Entity<EInvoiceLog>().HasIndex(e => new { e.Status, e.ReportingDeadline });
            modelBuilder.Entity<EInvoiceLog>().HasIndex(e => e.TaxInvoiceHeaderID);

            // One leg per wallet movement per direction, so replaying the swap detector cannot
            // double-count a barter — and double-counting turnover is as wrong as missing it.
            modelBuilder.Entity<ExchangePartnerLeg>()
                .HasIndex(l => new { l.FundTransactionID, l.Direction })
                .IsUnique();
            modelBuilder.Entity<ExchangePartnerLeg>().HasIndex(l => new { l.DocumentStatus, l.PeriodKey });

            // One row per physical device. The same token arriving again is an update, not a
            // second row — otherwise a reinstalled app would be pushed to twice.
            modelBuilder.Entity<UserDevice>()
                .HasIndex(d => d.FcmToken)
                .IsUnique();

            modelBuilder.Entity<UserDevice>()
                .HasIndex(d => new { d.UserID, d.IsActive });

            // The bell asks exactly one question — "my unread, newest first" — so that is the index.
            modelBuilder.Entity<AppNotificationRecipient>()
                .HasIndex(r => new { r.UserID, r.IsRead });

            modelBuilder.Entity<AppNotificationRecipient>()
                .HasIndex(r => r.AppNotificationID);

            modelBuilder.Entity<AppNotificationRecipient>()
                .HasOne(r => r.AppNotification)
                .WithMany()
                .HasForeignKey(r => r.AppNotificationID)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for RefreshTokens (JWT)
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => new { rt.UserID, rt.IsRevoked, rt.IsUsed });

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.JwtId);


            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.UserID);

            modelBuilder.Entity<WrongPasswordLog>()
                .HasIndex(w => new { w.UserID, w.DateTime });

            modelBuilder.Entity<TransactionMaster>()
                .HasIndex(t => t.UserID);

            modelBuilder.Entity<TransactionMaster>()
                .HasIndex(t => t.Status);

            modelBuilder.Entity<TransactionMaster>()
                .HasIndex(t => t.ProviderRequestID);

            modelBuilder.Entity<TransactionMaster>()
                .HasOne(t => t.UserMaster)
                .WithMany()
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionMaster>()
                .HasOne(t => t.ServiceMaster)
                .WithMany()
                .HasForeignKey(t => t.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionMaster>()
                .HasOne(t => t.OperatorMaster)
                .WithMany()
                .HasForeignKey(t => t.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionMaster>()
                .HasOne(t => t.WalletMaster)
                .WithMany()
                .HasForeignKey(t => t.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DMTSender>()
                .HasIndex(s => s.Mobile);

            modelBuilder.Entity<DMTSender>()
                .HasOne(s => s.UserMaster)
                .WithMany()
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DMTBeneficiary>()
                .HasIndex(b => new { b.SenderID, b.AccountNo });

            modelBuilder.Entity<DMTBeneficiary>()
                .HasOne(b => b.DMTSender)
                .WithMany()
                .HasForeignKey(b => b.SenderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DMTBeneficiary>()
                .HasOne(b => b.UserMaster)
                .WithMany()
                .HasForeignKey(b => b.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoginHistory>()
                .HasIndex(l => new { l.UserID, l.Date });

            modelBuilder.Entity<LoginHistory>()
                .HasOne(l => l.UserMaster)
                .WithMany()
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderMaster>()
                .HasIndex(p => p.ProviderName);

            modelBuilder.Entity<SlabDetail>()
                .HasOne(s => s.SlabMaster)
                .WithMany()
                .HasForeignKey(s => s.SlabMasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SlabDetail>()
                .HasOne(s => s.ServiceMaster)
                .WithMany()
                .HasForeignKey(s => s.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SlabDetail>()
                .HasOne(s => s.OperatorMaster)
                .WithMany()
                .HasForeignKey(s => s.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChannelSlabDetail>()
                .HasOne(c => c.ChannelSlabMaster)
                .WithMany()
                .HasForeignKey(c => c.ChannelSlabMasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChannelSlabDetail>()
                .HasOne(c => c.ServiceMaster)
                .WithMany()
                .HasForeignKey(c => c.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChannelSlabDetail>()
                .HasOne(c => c.OperatorMaster)
                .WithMany()
                .HasForeignKey(c => c.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderConfiguration>()
                .HasOne(p => p.OperatorMaster)
                .WithMany()
                .HasForeignKey(p => p.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderConfiguration>()
                .HasOne(p => p.ProviderMaster)
                .WithMany()
                .HasForeignKey(p => p.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderConfiguration>()
                .HasOne(p => p.ServiceMaster)
                .WithMany()
                .HasForeignKey(p => p.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderDetails>()
                .HasOne(p => p.ProviderMaster)
                .WithMany()
                .HasForeignKey(p => p.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderDetails>()
                .HasOne(p => p.ServiceMaster)
                .WithMany()
                .HasForeignKey(p => p.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsMaster>()
                .HasOne(n => n.UserMaster)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSlabMapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSlabMapping>()
                .HasOne(u => u.SlabMaster)
                .WithMany()
                .HasForeignKey(u => u.SlabMasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserChannelSlabMapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserChannelSlabMapping>()
                .HasOne(u => u.ChannelSlabMaster)
                .WithMany()
                .HasForeignKey(u => u.ChannelSlabMasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LowBalanceConfiguration>()
                .HasOne(l => l.UserMaster)
                .WithMany()
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LowBalanceConfiguration>()
                .HasOne(l => l.WalletMaster)
                .WithMany()
                .HasForeignKey(l => l.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComplaintMaster>()
                .HasOne(c => c.UserMaster)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComplaintMaster>()
                .HasOne(c => c.ServiceMaster)
                .WithMany()
                .HasForeignKey(c => c.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComplaintMaster>()
                .HasOne(c => c.OperatorMaster)
                .WithMany()
                .HasForeignKey(c => c.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CircleWiseCommission>()
                .HasOne(c => c.SlabMaster)
                .WithMany()
                .HasForeignKey(c => c.SlabMasterID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CircleWiseCommission>()
                .HasOne(c => c.OperatorMaster)
                .WithMany()
                .HasForeignKey(c => c.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCreationMaster>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCreationMaster>()
                .HasOne(u => u.ServiceMaster)
                .WithMany()
                .HasForeignKey(u => u.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCreationMaster>()
                .HasOne(u => u.WalletMaster)
                .WithMany()
                .HasForeignKey(u => u.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserCreationSetting>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserBankDetails>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserDocumentMaster>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequence>()
                .HasOne(o => o.ServiceMaster)
                .WithMany()
                .HasForeignKey(o => o.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequence>()
                .HasOne(o => o.OperatorMaster)
                .WithMany()
                .HasForeignKey(o => o.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequence>()
                .HasOne(o => o.ProviderMaster)
                .WithMany()
                .HasForeignKey(o => o.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorStateMapping>()
                .HasOne(o => o.OperatorMaster)
                .WithMany()
                .HasForeignKey(o => o.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AutoTransferUserwiseMapping>()
                .HasOne(a => a.UserMaster)
                .WithMany()
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Phase 5 FK constraints
            modelBuilder.Entity<UserCreationWallet>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseLowBalance>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseLowBalance>()
                .HasOne(u => u.WalletMaster)
                .WithMany()
                .HasForeignKey(u => u.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserWalletMapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserWalletMapping>()
                .HasOne(u => u.WalletMaster)
                .WithMany()
                .HasForeignKey(u => u.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserWalletMaster>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserWalletMaster>()
                .HasOne(u => u.WalletMaster)
                .WithMany()
                .HasForeignKey(u => u.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserServiceMapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserServiceMapping>()
                .HasOne(u => u.ServiceMaster)
                .WithMany()
                .HasForeignKey(u => u.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderStateConfiguration>()
                .HasOne(p => p.ProviderMaster)
                .WithMany()
                .HasForeignKey(p => p.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReconciliationSetting>()
                .HasOne(r => r.ProviderMaster)
                .WithMany()
                .HasForeignKey(r => r.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordChangeHistory>()
                .HasOne(p => p.UserMaster)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CallbackMaster>()
                .HasOne(c => c.UserMaster)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CallbackMaster>()
                .HasOne(c => c.ServiceMaster)
                .WithMany()
                .HasForeignKey(c => c.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOperatorLimit>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOperatorLimit>()
                .HasOne(u => u.OperatorMaster)
                .WithMany()
                .HasForeignKey(u => u.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserOperatorLimit>()
                .HasOne(u => u.ServiceMaster)
                .WithMany()
                .HasForeignKey(u => u.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorUserwise>()
                .HasOne(o => o.UserMaster)
                .WithMany()
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorUserwise>()
                .HasOne(o => o.OperatorMaster)
                .WithMany()
                .HasForeignKey(o => o.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorUserwise>()
                .HasOne(o => o.ServiceMaster)
                .WithMany()
                .HasForeignKey(o => o.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequenceUserWise>()
                .HasOne(o => o.UserMaster)
                .WithMany()
                .HasForeignKey(o => o.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequenceUserWise>()
                .HasOne(o => o.ServiceMaster)
                .WithMany()
                .HasForeignKey(o => o.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequenceUserWise>()
                .HasOne(o => o.OperatorMaster)
                .WithMany()
                .HasForeignKey(o => o.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OperatorSequenceUserWise>()
                .HasOne(o => o.ProviderMaster)
                .WithMany()
                .HasForeignKey(o => o.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderResponseMaster>()
                .HasOne(p => p.ProviderMaster)
                .WithMany()
                .HasForeignKey(p => p.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderUserMapping>()
                .HasOne(p => p.UserMaster)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RangeWiseSequence>()
                .HasOne(r => r.ProviderMaster)
                .WithMany()
                .HasForeignKey(r => r.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RangeWiseSequence>()
                .HasOne(r => r.ServiceMaster)
                .WithMany()
                .HasForeignKey(r => r.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RangeWiseSequence>()
                .HasOne(r => r.OperatorMaster)
                .WithMany()
                .HasForeignKey(r => r.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VoucherMaster>()
                .HasOne(v => v.VoucherType)
                .WithMany()
                .HasForeignKey(v => v.VoucherTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VoucherDetails>()
                .HasOne(v => v.UserMaster)
                .WithMany()
                .HasForeignKey(v => v.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserBalanceCapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserBalanceCapping>()
                .HasOne(u => u.WalletMaster)
                .WithMany()
                .HasForeignKey(u => u.WalletID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseDMTMapping>()
                .HasOne(u => u.UserMaster)
                .WithMany()
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseDMTMapping>()
                .HasOne(u => u.ServiceMaster)
                .WithMany()
                .HasForeignKey(u => u.ServiceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseDMTMapping>()
                .HasOne(u => u.OperatorMaster)
                .WithMany()
                .HasForeignKey(u => u.OperatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserwiseDMTMapping>()
                .HasOne(u => u.ProviderMaster)
                .WithMany()
                .HasForeignKey(u => u.ProviderID)
                .OnDelete(DeleteBehavior.Restrict);

            // Phase 6 FK Configurations
            modelBuilder.Entity<APIUserCallbackRequestResponseLog>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<APIUserInitialRequestResponseLog>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashCollectionMaster>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CircleWiseOperatorLimit>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CircleWiseOperatorLimit>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CircleWiseUserWiseOperatorOff>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CircleWiseUserWiseOperatorOff>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EcollectionDetails>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GSTHold>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GSTHold>()
                .HasOne(x => x.StateGst).WithMany().HasForeignKey(x => x.StateID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inbox>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Inbox>()
                .HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Inbox>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceNumberHistory>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LapuMaster>()
                .HasOne(x => x.LapuPartyMaster).WithMany().HasForeignKey(x => x.LapuPartyMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LapuMaster>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayoutBeneficiaryOTP>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderConfigurationAdminCommission>()
                .HasOne(x => x.ProviderMaster).WithMany().HasForeignKey(x => x.ProviderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderConfigurationAdminCommission>()
                .HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderConfigurationAdminCommission>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderOperatorDenominationMapping>()
                .HasOne(x => x.ProviderMaster).WithMany().HasForeignKey(x => x.ProviderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderOperatorDenominationMapping>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderOperatorDenominationMapping>()
                .HasOne(x => x.DenominationMaster).WithMany().HasForeignKey(x => x.DenominationMasterID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderPurchase>()
                .HasOne(x => x.ProviderMaster).WithMany().HasForeignKey(x => x.ProviderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderPurchase>()
                .HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderRequestResponseLog>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RealProviderMapping>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ViewProviderMapping>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Redeemcommission>()
                .HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ROfferCommission>()
                .HasOne(x => x.SlabMaster).WithMany().HasForeignKey(x => x.SlabMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ROfferCommission>()
                .HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ROfferCommission>()
                .HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceWiseSliderSetting>()
                .HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PayoutVendorBeneficiaryMapping>()
                .HasOne(x => x.PayoutBeneficiary).WithMany().HasForeignKey(x => x.PayoutBeneficaryID).OnDelete(DeleteBehavior.Restrict);

            // Phase 7 FKs
            modelBuilder.Entity<ProviderOperatorCircleMaster>().HasOne(x => x.ProviderMaster).WithMany().HasForeignKey(x => x.ProviderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProviderOperatorCircleMaster>().HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OnboardingUserDocument>().HasOne(x => x.OnboardingUser).WithMany().HasForeignKey(x => x.OnboardingUserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UPIVendorBeneficiaryMapping>().HasOne(x => x.UpiBeneficiary).WithMany().HasForeignKey(x => x.UpiBeneficiaryID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyAuthenticationMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyAuthenticationMaster>().HasOne(x => x.ProviderMaster).WithMany().HasForeignKey(x => x.ProviderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyAuthenticationMaster>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OperationalHistory>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SDKBBPSUserMapping>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserwiseSaleTarget>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserwiseSaleTarget>().HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserwiseServiceChargeMaster>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserwiseServiceChargeMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TargetSaleCommission>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TargetSaleCommission>().HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserActionHistory>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReferralRegistration>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OverdraftDetail>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OverdraftDetail>().HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OverdraftMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<OverdraftMaster>().HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TrustableDevice>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserMerchantMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserWisePayementGatewaySetting>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserWisePayementGatewaySetting>().HasOne(x => x.PaymentGatewaySetting).WithMany().HasForeignKey(x => x.PaymentGatewaySettingID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SmsTransactionMaster>().HasOne(x => x.TransactionMaster).WithMany().HasForeignKey(x => x.TransactionMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SmsTransactionMaster>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WalletServiceMapping>().HasOne(x => x.WalletMaster).WithMany().HasForeignKey(x => x.WalletID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WalletServiceMapping>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<eKYCLog>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<eKYCMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BankRequestResponseLog>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupportUserAccessDetail>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UpiSender>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UpiBeneficiary>().HasOne(x => x.UpiSender).WithMany().HasForeignKey(x => x.UpiSenderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UpiBeneficiary>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);

            // Phase 8 FKs
            modelBuilder.Entity<UserWisePlanLimit>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserWisePlanView>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sender>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Beneficiary>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Beneficiary>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SenderOTP>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SenderOTP>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BeneficiaryOTP>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BeneficiaryOTP>().HasOne(x => x.Beneficiary).WithMany().HasForeignKey(x => x.BeneficiaryID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BeneficiaryOTP>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TransactionProviderHistory>().HasOne(x => x.TransactionMaster).WithMany().HasForeignKey(x => x.TransactionMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentGateway>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentGateway>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TransactionManualUpdate>().HasOne(x => x.TransactionMaster).WithMany().HasForeignKey(x => x.TransactionMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TransactionManualUpdate>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.ManualUpdatedBy).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TransactionStatusChange>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TransactionStatusChange>().HasOne(x => x.TransactionMaster).WithMany().HasForeignKey(x => x.TransactionMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ThreewayReconCompletion>().HasOne(x => x.TransactionMaster).WithMany().HasForeignKey(x => x.TransactionMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ThreewayReconCompletion>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ThreewayReconCompletion>().HasOne(x => x.OperatorMaster).WithMany().HasForeignKey(x => x.OperatorID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserActionMaster>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AuthorizationMaster>().HasOne(x => x.UserTypeMaster).WithMany().HasForeignKey(x => x.UserTypeID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserCodeFormat>().HasOne(x => x.UserType).WithMany().HasForeignKey(x => x.UserTypeID).OnDelete(DeleteBehavior.Restrict);
            // One code format per user type, and one user code (AliasName) per user — the running
            // sequence may never hand the same code to two users. Blank aliases (legacy rows that
            // predate auto-generation) are excluded from the unique index by the filter.
            modelBuilder.Entity<UserCodeFormat>().HasIndex(x => x.UserTypeID).IsUnique();
            modelBuilder.Entity<UserMaster>().HasIndex(x => x.AliasName).IsUnique()
                .HasFilter("[AliasName] IS NOT NULL AND [AliasName] <> ''");
            modelBuilder.Entity<ServiceSubscriptionPlanDetails>().HasOne(x => x.ServiceSubscriptionCreatePlanMaster).WithMany().HasForeignKey(x => x.ServiceSubscriptionCreatePlanMasterID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserServiceSubscriptionDetails>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserServiceSubscriptionDetails>().HasOne(x => x.ServiceMaster).WithMany().HasForeignKey(x => x.ServiceID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PrepaidActivation>().HasOne(x => x.UserMaster).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SubCategory>().HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CategoryChannelMapping>().HasOne(x => x.ChannelCategory).WithMany().HasForeignKey(x => x.ChannelCategoryID).OnDelete(DeleteBehavior.Restrict);

            // Turn off cascade delete for every relationship still defaulting to Cascade.
            // Many tables reference shared principals (UserMaster, ServiceMaster, ...), which
            // SQL Server rejects as "multiple cascade paths". The app soft-deletes (IsDeleted),
            // and all explicit relationships already use Restrict, so this matches intent.
            foreach (var fk in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys())
                         .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
                         .ToList())
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}