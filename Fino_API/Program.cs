// FIXED Program.cs with explicit service registrations
using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Middleware;
using FintechApp.Services;
using FintechApp.Interfaces.Services;
using FintechApp.Repositories;
using FintechApp.Interfaces.Repositories;
using Fino.PlanApi.Extensions;
using Fino.JaiJuiApi.Extensions;
using Fino.Pay1Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddOpenApi();

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add Memory Cache
builder.Services.AddMemoryCache();

// Generic in-memory list cache used by repositories (open generic — resolves any
// MemoryCacheProcessing<T>). The wrapper holds a static cache, so a singleton is safe.
builder.Services.AddSingleton(typeof(FintechApp.Processing.MemoryCacheProcessing<>));

// Add HttpClient factory
builder.Services.AddHttpClient();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                statusCode = 401,
                status = "UNAUTHORIZED",
                message = "Your session has expired. Please login again.",
                data = ""
            });
            return context.Response.WriteAsync(result);
        }
    };
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSFintechApp", policy =>
    {
        // Dev-friendly: allow any origin/port the UI dev server uses (tighten for production).
        policy.SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// ========== EXPLICIT SERVICE REGISTRATIONS ==========
// Register JwtTokenService as Singleton
builder.Services.AddSingleton<JwtTokenService>();

// Register Services
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICredentialService, CredentialService>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddScoped<IAppbarService, AppbarService>();
builder.Services.AddScoped<IPinGenerationService, PinGenerationService>();
builder.Services.AddScoped<IDMTSenderFlowService, DMTSenderFlowService>();
builder.Services.AddScoped<IDMTBeneficiaryFlowService, DMTBeneficiaryFlowService>();
builder.Services.AddScoped<IDMTAccountVerifyService, DMTAccountVerifyService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITodaySummaryService, TodaySummaryService>();
builder.Services.AddScoped<IAEPSService, AEPSService>();
builder.Services.AddScoped<ILocationPinCodeRepository, LocationPinCodeRepository>();
builder.Services.AddScoped<IAgentDetailsService, AgentDetailsService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IeKYCService, eKYCService>();
builder.Services.AddScoped<IMicroATMService, MicroATMService>();
builder.Services.AddScoped<IHitachiYesBankAEPSService, HitachiYesBankAEPSService>();
builder.Services.AddScoped<IFundService, FundService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
builder.Services.AddScoped<ICircleSwitchingService, CircleSwitchingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
// Maps app operators onto the plan provider's own operator codes (PlanApiCatalog).
builder.Services.AddScoped<IPlanOperatorCodeService, PlanOperatorCodeService>();
builder.Services.AddScoped<IUISettingService, UISettingService>();
builder.Services.AddScoped<IBBPSConfigurationService, BBPSConfigurationService>();
builder.Services.AddScoped<IIPMasterService, IPMasterService>();
builder.Services.AddScoped<ICallMeCategoryMasterService, CallMeCategoryMasterService>();
builder.Services.AddScoped<ICallMeRequestService, CallMeRequestService>();
builder.Services.AddScoped<ITicketMasterService, TicketMasterService>();
builder.Services.AddScoped<ITicketCategoryService, TicketCategoryService>();
builder.Services.AddScoped<IFundTransactionRepository, FundTransactionRepository>();
builder.Services.AddScoped<IFundTransactionService, FundTransactionService>();
// Fund-request Mode / SubMode masters (dynamic Add/Edit/Delete/View + dependent dropdowns)
builder.Services.AddScoped<IModeRepository, ModeRepository>();
builder.Services.AddScoped<IModeService, ModeService>();
builder.Services.AddScoped<ISubModeRepository, SubModeRepository>();
builder.Services.AddScoped<ISubModeService, SubModeService>();
builder.Services.AddScoped<IAEPSSettingService, AEPSSettingService>();
builder.Services.AddScoped<IProviderMasterService, ProviderMasterService>();
builder.Services.AddScoped<ITransactionMasterService, TransactionMasterService>();
builder.Services.AddScoped<ITransactionProcessingService, TransactionProcessingService>();
builder.Services.AddScoped<ITransactionReportService, TransactionReportService>();
// Aggregate ("analytics") report screens — service/user/operator roll-ups, network and slab reports.
builder.Services.AddScoped<IReportAnalyticsService, ReportAnalyticsService>();
builder.Services.AddScoped<IWalletToWalletTransferService, WalletToWalletTransferService>();
builder.Services.AddScoped<IUserAccountingService, UserAccountingService>();
builder.Services.AddScoped<IAccountProcessingService, AccountProcessingService>();
builder.Services.AddScoped<IAccountCoreProcessingService, AccountCoreProcessingService>();
// Config-driven provider gateway: routes, endpoint templates and response rules come from the
// admin panel, so recharges dispatch through whatever the admin has configured — no code change.
builder.Services.AddScoped<IProviderGatewayService, ProviderGatewayService>();
builder.Services.AddScoped<IServiceKindResolver, ServiceKindResolver>();
builder.Services.AddScoped<FintechApp.Interfaces.Providers.IRechargeProviderClient, FintechApp.Providers.DynamicRechargeProviderClient>();
// Slab commission engine: resolves retailer/distributor/super-distributor commission (or
// surcharge) from the user's assigned slab for every recharge-family transaction.
builder.Services.AddScoped<ISlabCommissionResolver, SlabCommissionResolver>();
builder.Services.AddScoped<IRechargeService, RechargeService>();
// Bank-transfer (DMT / payout / vendor-pay) provider seam. "DMTProvider:Mode" = "Simulation" wires
// the sandbox client that completes transfers end-to-end (dev/UAT); any other value (default) keeps
// the Unconfigured client, which fails-and-refunds so a production build never auto-succeeds transfers
// until a real aggregator is registered here.
// Bank-transfer (DMT / payout) provider seam — DB-driven. DynamicBankTransferProviderClient reads the
// gateways configured in the "DMT Settings" screen at call time and dispatches through the active one
// (e.g. JaiJui via the Fino.JaiJuiApi library, using that row's credentials). When no real gateway row
// is active it falls back to the sandbox (config DMTProvider:Mode=Simulation) or the Unconfigured client
// (fail-and-refund) — so a production build never auto-succeeds transfers until a real provider is set up.
builder.Services.AddScoped<FintechApp.Interfaces.Providers.IJaiJuiGatewayResolver, FintechApp.Providers.JaiJuiGatewayResolver>();
builder.Services.AddScoped<FintechApp.Providers.SimulationBankTransferProviderClient>();
builder.Services.AddScoped<FintechApp.Providers.UnconfiguredBankTransferProviderClient>();
builder.Services.AddScoped<FintechApp.Interfaces.Providers.IBankTransferProviderClient, FintechApp.Providers.DynamicBankTransferProviderClient>();
builder.Services.AddScoped<IBankTransferService, BankTransferService>();
builder.Services.AddScoped<IReconService, ReconService>();
builder.Services.AddScoped<IMenuService, MenuService>();
// Plan / DTH / R-Offer data provider (planapi.in) — separate Fino.PlanApi library
builder.Services.AddPlanApi();
// JaiJui DMT / money-transfer provider (jaijuirecharge.in) — separate Fino.JaiJuiApi library
builder.Services.AddJaiJuiApi();
// Pay1 Utility / BBPS bill payments (recharges.pay1.in) - separate Fino.Pay1Api library.
// Pay1 signs every call with sha1(partner_id + trans_id + api_key), which a URL template
// cannot express, so its protocol lives in the library and the gateway calls into it.
builder.Services.AddPay1Api();
builder.Services.AddScoped<FintechApp.Interfaces.Providers.ITransactionProviderClient, FintechApp.Providers.DynamicTransactionProviderClient>();
// Automated pending-recharge status checker (replaces the old FinovativeAPI Hangfire "Txn Status
// Check" recurring job). Re-enquires PENDING recharges against the provider every StatusCheck:IntervalMinutes.
builder.Services.AddHostedService<RechargeStatusCheckBackgroundService>();
builder.Services.AddScoped<IDMTSenderService, DMTSenderService>();
builder.Services.AddScoped<IDMTBeneficiaryService, DMTBeneficiaryService>();
builder.Services.AddScoped<ILoginHistoryService, LoginHistoryService>();
builder.Services.AddScoped<IStateMasterService, StateMasterService>();
builder.Services.AddScoped<IUserTypeMasterService, UserTypeMasterService>();
builder.Services.AddScoped<IUserCodeService, UserCodeService>();
builder.Services.AddScoped<IDMTBankService, DMTBankService>();
builder.Services.AddScoped<ISlabMasterService, SlabMasterService>();
builder.Services.AddScoped<IVerificationDocumentService, VerificationDocumentService>();
builder.Services.AddScoped<ISlabDetailService, SlabDetailService>();
builder.Services.AddScoped<IChannelSlabMasterService, ChannelSlabMasterService>();
builder.Services.AddScoped<IChannelSlabDetailService, ChannelSlabDetailService>();
builder.Services.AddScoped<IProviderConfigurationService, ProviderConfigurationService>();
builder.Services.AddScoped<IProviderDetailsService, ProviderDetailsService>();
builder.Services.AddScoped<IDMTSettingService, DMTSettingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INewsMasterService, NewsMasterService>();
builder.Services.AddScoped<ISettingMasterService, SettingMasterService>();
builder.Services.AddScoped<IEmailSettingService, EmailSettingService>();
builder.Services.AddScoped<IUserSlabMappingService, UserSlabMappingService>();
builder.Services.AddScoped<IUserChannelSlabMappingService, UserChannelSlabMappingService>();
builder.Services.AddScoped<ISliderMasterService, SliderMasterService>();
builder.Services.AddScoped<ISmsMasterService, SmsMasterService>();
builder.Services.AddScoped<IMessageSettingService, MessageSettingService>();
builder.Services.AddScoped<IDenominationMasterService, DenominationMasterService>();
builder.Services.AddScoped<ICompanyMasterService, CompanyMasterService>();
builder.Services.AddScoped<INPCIBankService, NPCIBankService>();
builder.Services.AddScoped<IOTPMasterService, OTPMasterService>();
builder.Services.AddScoped<IKycDocumentMasterService, KycDocumentMasterService>();
builder.Services.AddScoped<ILowBalanceConfigurationService, LowBalanceConfigurationService>();
builder.Services.AddScoped<IComplaintMasterService, ComplaintMasterService>();
builder.Services.AddScoped<IPayoutSettingService, PayoutSettingService>();
builder.Services.AddScoped<IPayoutBeneficiaryService, PayoutBeneficiaryService>();
builder.Services.AddScoped<ICircleWiseCommissionService, CircleWiseCommissionService>();
builder.Services.AddScoped<IUserCreationMasterService, UserCreationMasterService>();
builder.Services.AddScoped<IUserCreationSettingService, UserCreationSettingService>();
builder.Services.AddScoped<IUserBankDetailsService, UserBankDetailsService>();
builder.Services.AddScoped<IUserDocumentMasterService, UserDocumentMasterService>();
builder.Services.AddScoped<IKYCApprovedDetailsService, KYCApprovedDetailsService>();
builder.Services.AddScoped<IOperatorAmountWiseSettingService, OperatorAmountWiseSettingService>();
builder.Services.AddScoped<IAccountOpenSettingService, AccountOpenSettingService>();
builder.Services.AddScoped<IOperatorSequenceService, OperatorSequenceService>();
builder.Services.AddScoped<IOperatorStateMappingService, OperatorStateMappingService>();
builder.Services.AddScoped<ICashDepositSettingService, CashDepositSettingService>();
builder.Services.AddScoped<IAutoTransferUserwiseMappingService, AutoTransferUserwiseMappingService>();
builder.Services.AddScoped<IZoneMasterService, ZoneMasterService>();
builder.Services.AddScoped<IBBPSSettingsService, BBPSSettingsService>();
builder.Services.AddScoped<IInvoiceSettingService, InvoiceSettingService>();
builder.Services.AddScoped<IPaymentGatewaySettingService, PaymentGatewaySettingService>();
builder.Services.AddScoped<IUserCreationWalletService, UserCreationWalletService>();
builder.Services.AddScoped<IUserwiseLowBalanceService, UserwiseLowBalanceService>();
builder.Services.AddScoped<IUserWalletMappingService, UserWalletMappingService>();
builder.Services.AddScoped<IUserWalletMasterService, UserWalletMasterService>();
builder.Services.AddScoped<IUserServiceMappingService, UserServiceMappingService>();
builder.Services.AddScoped<IUserServiceAccessService, UserServiceAccessService>();
builder.Services.AddScoped<IKycApprovalService, KycApprovalService>();
builder.Services.AddScoped<IProviderStateConfigurationService, ProviderStateConfigurationService>();
builder.Services.AddScoped<IReconciliationSettingService, ReconciliationSettingService>();
builder.Services.AddScoped<IPasswordChangeHistoryService, PasswordChangeHistoryService>();
builder.Services.AddScoped<ICallbackMasterService, CallbackMasterService>();
// Enqueues a settled transaction for callback delivery (returns instantly); the queue is
// drained by ApiUserCallbackBackgroundService so a slow integrator's receiver never adds
// latency to the transaction that triggered it.
builder.Services.AddScoped<ICallbackDispatchService, CallbackDispatchService>();
builder.Services.AddSingleton<IApiUserCallbackQueue, ApiUserCallbackQueue>();
builder.Services.AddScoped<IApiUserCallbackSender, ApiUserCallbackSender>();
builder.Services.AddHostedService<ApiUserCallbackBackgroundService>();

// ── Inbound provider callbacks: one shared address, /api/Callback/p/{CallbackKey} ──
// The key identifies the provider; the router applies the same source check, verification,
// settlement and logging to every upstream. Parser ORDER IS THE MATCH ORDER — a provider with
// its own format goes above GenericProviderCallbackParser, which claims everything that is left.
builder.Services.AddScoped<IProviderCallbackParser, FintechApp.Services.Callbacks.Pay1CallbackParser>();
builder.Services.AddScoped<IProviderCallbackParser, FintechApp.Services.Callbacks.GenericProviderCallbackParser>();
builder.Services.AddScoped<IProviderCallbackRouter, ProviderCallbackRouter>();

// ── B2B (API-user) gateway: the /api/v1 contract ────────────────────────────────
// One credential gate, one response builder, one orchestration core — so the initial
// reply, the status check and the outbound callback all speak the identical contract.
builder.Services.AddScoped<IApiUserAuthService, ApiUserAuthService>();
builder.Services.AddScoped<IApiUserResponseBuilder, ApiUserResponseBuilder>();
builder.Services.AddScoped<IApiUserGatewayService, ApiUserGatewayService>();

builder.Services.AddScoped<IWebHookSettingService, WebHookSettingService>();
builder.Services.AddScoped<IWebHookLogService, WebHookLogService>();
builder.Services.AddScoped<IUserServiceChargeService, UserServiceChargeService>();
builder.Services.AddScoped<IUserOperatorLimitService, UserOperatorLimitService>();
builder.Services.AddScoped<IOperatorUserwiseService, OperatorUserwiseService>();
builder.Services.AddScoped<IOperatorSequenceUserWiseService, OperatorSequenceUserWiseService>();
builder.Services.AddScoped<IOperatorwiseCallbackSettingService, OperatorwiseCallbackSettingService>();
builder.Services.AddScoped<IProviderOperatorMasterService, ProviderOperatorMasterService>();
builder.Services.AddScoped<IProviderResponseMasterService, ProviderResponseMasterService>();
builder.Services.AddScoped<IProviderUserMappingService, ProviderUserMappingService>();
builder.Services.AddScoped<IRangeWiseSequenceService, RangeWiseSequenceService>();
builder.Services.AddScoped<IVoucherTypeService, VoucherTypeService>();
builder.Services.AddScoped<IVoucherMasterService, VoucherMasterService>();
builder.Services.AddScoped<IVoucherDetailsService, VoucherDetailsService>();
builder.Services.AddScoped<IUserBalanceCappingService, UserBalanceCappingService>();
builder.Services.AddScoped<IUserBalanceHistoryService, UserBalanceHistoryService>();
builder.Services.AddScoped<IUserwiseDMTMappingService, UserwiseDMTMappingService>();

// Register Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IOperatorRepository, OperatorRepository>();
builder.Services.AddScoped<IUISettingRepository, UISettingRepository>();
builder.Services.AddScoped<IUISettingService, UISettingService>();
builder.Services.AddScoped<IBBPSConfigurationRepository, BBPSConfigurationRepository>();
builder.Services.AddScoped<IIPMasterRepository, IPMasterRepository>();
builder.Services.AddScoped<ICallMeCategoryMasterRepository, CallMeCategoryMasterRepository>();
builder.Services.AddScoped<ITicketMasterRepository, TicketMasterRepository>();
builder.Services.AddScoped<ITicketCategoryRepository, TicketCategoryRepository>();
builder.Services.AddScoped<IFundTransactionRepository, FundTransactionRepository>();
builder.Services.AddScoped<IAEPSSettingRepository, AEPSSettingRepository>();
builder.Services.AddScoped<IProviderMasterRepository, ProviderMasterRepository>();
builder.Services.AddScoped<ITransactionMasterRepository, TransactionMasterRepository>();
builder.Services.AddScoped<IDMTSenderRepository, DMTSenderRepository>();
builder.Services.AddScoped<IDMTBeneficiaryRepository, DMTBeneficiaryRepository>();
builder.Services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
builder.Services.AddScoped<IStateMasterRepository, StateMasterRepository>();
builder.Services.AddScoped<IUserTypeMasterRepository, UserTypeMasterRepository>();
builder.Services.AddScoped<IUserCodeFormatRepository, UserCodeFormatRepository>();
builder.Services.AddScoped<IDMTBankRepository, DMTBankRepository>();
builder.Services.AddScoped<ISlabMasterRepository, SlabMasterRepository>();
builder.Services.AddScoped<IVerificationDocumentConfigRepository, VerificationDocumentConfigRepository>();
builder.Services.AddScoped<ISlabDetailRepository, SlabDetailRepository>();
builder.Services.AddScoped<IChannelSlabMasterRepository, ChannelSlabMasterRepository>();
builder.Services.AddScoped<IChannelSlabDetailRepository, ChannelSlabDetailRepository>();
builder.Services.AddScoped<IProviderConfigurationRepository, ProviderConfigurationRepository>();
builder.Services.AddScoped<IProviderDetailsRepository, ProviderDetailsRepository>();
builder.Services.AddScoped<IDMTSettingRepository, DMTSettingRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INewsMasterRepository, NewsMasterRepository>();
builder.Services.AddScoped<ISettingMasterRepository, SettingMasterRepository>();
builder.Services.AddScoped<IEmailSettingRepository, EmailSettingRepository>();
builder.Services.AddScoped<IUserSlabMappingRepository, UserSlabMappingRepository>();
builder.Services.AddScoped<IUserChannelSlabMappingRepository, UserChannelSlabMappingRepository>();
builder.Services.AddScoped<ISliderMasterRepository, SliderMasterRepository>();
builder.Services.AddScoped<ISmsMasterRepository, SmsMasterRepository>();
builder.Services.AddScoped<IMessageSettingRepository, MessageSettingRepository>();
builder.Services.AddScoped<IDenominationMasterRepository, DenominationMasterRepository>();
builder.Services.AddScoped<ICompanyMasterRepository, CompanyMasterRepository>();
builder.Services.AddScoped<INPCIBankRepository, NPCIBankRepository>();
builder.Services.AddScoped<IOTPMasterRepository, OTPMasterRepository>();
builder.Services.AddScoped<IKycDocumentMasterRepository, KycDocumentMasterRepository>();
builder.Services.AddScoped<ILowBalanceConfigurationRepository, LowBalanceConfigurationRepository>();
builder.Services.AddScoped<IComplaintMasterRepository, ComplaintMasterRepository>();
builder.Services.AddScoped<IPayoutSettingRepository, PayoutSettingRepository>();
builder.Services.AddScoped<IPayoutBeneficiaryRepository, PayoutBeneficiaryRepository>();
builder.Services.AddScoped<ICircleWiseCommissionRepository, CircleWiseCommissionRepository>();
builder.Services.AddScoped<IUserCreationMasterRepository, UserCreationMasterRepository>();
builder.Services.AddScoped<IUserCreationSettingRepository, UserCreationSettingRepository>();
builder.Services.AddScoped<IUserBankDetailsRepository, UserBankDetailsRepository>();
builder.Services.AddScoped<IUserDocumentMasterRepository, UserDocumentMasterRepository>();
builder.Services.AddScoped<IKYCApprovedDetailsRepository, KYCApprovedDetailsRepository>();
builder.Services.AddScoped<IOperatorAmountWiseSettingRepository, OperatorAmountWiseSettingRepository>();
builder.Services.AddScoped<IAccountOpenSettingRepository, AccountOpenSettingRepository>();
builder.Services.AddScoped<IOperatorSequenceRepository, OperatorSequenceRepository>();
builder.Services.AddScoped<IOperatorStateMappingRepository, OperatorStateMappingRepository>();
builder.Services.AddScoped<ICashDepositSettingRepository, CashDepositSettingRepository>();
builder.Services.AddScoped<IAutoTransferUserwiseMappingRepository, AutoTransferUserwiseMappingRepository>();
builder.Services.AddScoped<IZoneMasterRepository, ZoneMasterRepository>();
builder.Services.AddScoped<IBBPSSettingsRepository, BBPSSettingsRepository>();
builder.Services.AddScoped<IInvoiceSettingRepository, InvoiceSettingRepository>();
builder.Services.AddScoped<IPaymentGatewaySettingRepository, PaymentGatewaySettingRepository>();
builder.Services.AddScoped<IUserCreationWalletRepository, UserCreationWalletRepository>();
builder.Services.AddScoped<IUserwiseLowBalanceRepository, UserwiseLowBalanceRepository>();
builder.Services.AddScoped<IUserWalletMappingRepository, UserWalletMappingRepository>();
builder.Services.AddScoped<IUserWalletMasterRepository, UserWalletMasterRepository>();
builder.Services.AddScoped<IWalletMasterRepository, WalletMasterRepository>();
builder.Services.AddScoped<IWalletMasterService, WalletMasterService>();
// Deny-by-default role-based API access policy (singleton — reads static defaults + config overrides).
builder.Services.AddSingleton<IApiAccessPolicyService, ApiAccessPolicyService>();
// Deny-by-default DATA scope: a request body may only name users inside the caller's own branch
// of the hierarchy (scoped — walks the cached user tree per request).
builder.Services.AddScoped<IUserScopeService, UserScopeService>();
builder.Services.AddScoped<IUserServiceMappingRepository, UserServiceMappingRepository>();
builder.Services.AddScoped<IProviderStateConfigurationRepository, ProviderStateConfigurationRepository>();
builder.Services.AddScoped<IReconciliationSettingRepository, ReconciliationSettingRepository>();
builder.Services.AddScoped<IPasswordChangeHistoryRepository, PasswordChangeHistoryRepository>();
builder.Services.AddScoped<ICallbackMasterRepository, CallbackMasterRepository>();
builder.Services.AddScoped<IWebHookSettingRepository, WebHookSettingRepository>();
builder.Services.AddScoped<IWebHookLogRepository, WebHookLogRepository>();
builder.Services.AddScoped<IUserServiceChargeRepository, UserServiceChargeRepository>();
builder.Services.AddScoped<IUserOperatorLimitRepository, UserOperatorLimitRepository>();
builder.Services.AddScoped<IOperatorUserwiseRepository, OperatorUserwiseRepository>();
builder.Services.AddScoped<IOperatorSequenceUserWiseRepository, OperatorSequenceUserWiseRepository>();
builder.Services.AddScoped<IOperatorwiseCallbackSettingRepository, OperatorwiseCallbackSettingRepository>();
builder.Services.AddScoped<IProviderOperatorMasterRepository, ProviderOperatorMasterRepository>();
builder.Services.AddScoped<IProviderResponseMasterRepository, ProviderResponseMasterRepository>();
builder.Services.AddScoped<IProviderUserMappingRepository, ProviderUserMappingRepository>();
builder.Services.AddScoped<IRangeWiseSequenceRepository, RangeWiseSequenceRepository>();
builder.Services.AddScoped<IVoucherTypeRepository, VoucherTypeRepository>();
builder.Services.AddScoped<IVoucherMasterRepository, VoucherMasterRepository>();
builder.Services.AddScoped<IVoucherDetailsRepository, VoucherDetailsRepository>();
builder.Services.AddScoped<IUserBalanceCappingRepository, UserBalanceCappingRepository>();
builder.Services.AddScoped<IUserBalanceHistoryRepository, UserBalanceHistoryRepository>();
builder.Services.AddScoped<IUserBalanceMasterRepository, UserBalanceMasterRepository>();
builder.Services.AddScoped<IUserwiseDMTMappingRepository, UserwiseDMTMappingRepository>();

builder.Services.AddScoped<ICallMeRequestRepository, CallMeRequestRepository>();

// Phase 6 DI Registrations
builder.Services.AddScoped<IAPIUserCallbackRequestResponseLogRepository, APIUserCallbackRequestResponseLogRepository>();
builder.Services.AddScoped<IAPIUserCallbackRequestResponseLogService, APIUserCallbackRequestResponseLogService>();
builder.Services.AddScoped<IAPIUserInitialRequestResponseLogRepository, APIUserInitialRequestResponseLogRepository>();
builder.Services.AddScoped<IAPIUserInitialRequestResponseLogService, APIUserInitialRequestResponseLogService>();
builder.Services.AddScoped<IBulkMessageRepository, BulkMessageRepository>();
builder.Services.AddScoped<IBulkMessageService, BulkMessageService>();
builder.Services.AddScoped<ICashCollectionMasterRepository, CashCollectionMasterRepository>();
builder.Services.AddScoped<ICashCollectionMasterService, CashCollectionMasterService>();
builder.Services.AddScoped<ICashCollectionDetailRepository, CashCollectionDetailRepository>();
builder.Services.AddScoped<ICashCollectionDetailService, CashCollectionDetailService>();
builder.Services.AddScoped<ICircleAPIMasterRepository, CircleAPIMasterRepository>();
builder.Services.AddScoped<ICircleAPIMasterService, CircleAPIMasterService>();
builder.Services.AddScoped<ICircleWiseOperatorLimitRepository, CircleWiseOperatorLimitRepository>();
builder.Services.AddScoped<ICircleWiseOperatorLimitService, CircleWiseOperatorLimitService>();
builder.Services.AddScoped<ICircleWiseUserWiseOperatorOffRepository, CircleWiseUserWiseOperatorOffRepository>();
builder.Services.AddScoped<ICircleWiseUserWiseOperatorOffService, CircleWiseUserWiseOperatorOffService>();
builder.Services.AddScoped<ICreditCardSettingRepository, CreditCardSettingRepository>();
builder.Services.AddScoped<ICreditCardSettingService, CreditCardSettingService>();
builder.Services.AddScoped<IDateFormatMasterRepository, DateFormatMasterRepository>();
builder.Services.AddScoped<IDateFormatMasterService, DateFormatMasterService>();
builder.Services.AddScoped<IEcollectionSettingRepository, EcollectionSettingRepository>();
builder.Services.AddScoped<IEcollectionSettingService, EcollectionSettingService>();
builder.Services.AddScoped<IEcollectionDetailsRepository, EcollectionDetailsRepository>();
builder.Services.AddScoped<IEcollectionDetailsService, EcollectionDetailsService>();
builder.Services.AddScoped<IEnquiryDetailsRepository, EnquiryDetailsRepository>();
builder.Services.AddScoped<IEnquiryDetailsService, EnquiryDetailsService>();
builder.Services.AddScoped<IExternalMediaRepository, ExternalMediaRepository>();
builder.Services.AddScoped<IExternalMediaService, ExternalMediaService>();
builder.Services.AddScoped<IFingerPrintDeviceTypeRepository, FingerPrintDeviceTypeRepository>();
builder.Services.AddScoped<IFingerPrintDeviceTypeService, FingerPrintDeviceTypeService>();
builder.Services.AddScoped<IGSTHoldRepository, GSTHoldRepository>();
builder.Services.AddScoped<IGSTHoldService, GSTHoldService>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddScoped<IInboxService, InboxService>();
builder.Services.AddScoped<IInvoiceNumberHistoryRepository, InvoiceNumberHistoryRepository>();
builder.Services.AddScoped<IInvoiceNumberHistoryService, InvoiceNumberHistoryService>();
builder.Services.AddScoped<ILapuPartyMasterRepository, LapuPartyMasterRepository>();
builder.Services.AddScoped<ILapuPartyMasterService, LapuPartyMasterService>();
builder.Services.AddScoped<ILapuMasterRepository, LapuMasterRepository>();
builder.Services.AddScoped<ILapuMasterService, LapuMasterService>();
builder.Services.AddScoped<ILeadGenerationSettingRepository, LeadGenerationSettingRepository>();
builder.Services.AddScoped<ILeadGenerationSettingService, LeadGenerationSettingService>();
builder.Services.AddScoped<ILocationLookUpRepository, LocationLookUpRepository>();
builder.Services.AddScoped<ILocationLookUpService, LocationLookUpService>();
builder.Services.AddScoped<IMediaMasterRepository, MediaMasterRepository>();
builder.Services.AddScoped<IMediaMasterService, MediaMasterService>();
builder.Services.AddScoped<IMenuMasterRepository, MenuMasterRepository>();
builder.Services.AddScoped<IMenuMasterService, MenuMasterService>();
builder.Services.AddScoped<IPaymentGatewayLogRepository, PaymentGatewayLogRepository>();
builder.Services.AddScoped<IPaymentGatewayLogService, PaymentGatewayLogService>();
builder.Services.AddScoped<IPaymentModeMasterRepository, PaymentModeMasterRepository>();
builder.Services.AddScoped<IPaymentModeMasterService, PaymentModeMasterService>();
builder.Services.AddScoped<IPayoutBeneficiaryOTPRepository, PayoutBeneficiaryOTPRepository>();
builder.Services.AddScoped<IPayoutBeneficiaryOTPService, PayoutBeneficiaryOTPService>();
builder.Services.AddScoped<IPlanSettingRepository, PlanSettingRepository>();
builder.Services.AddScoped<IPlanSettingService, PlanSettingService>();
builder.Services.AddScoped<IProviderCallBackLogRepository, ProviderCallBackLogRepository>();
builder.Services.AddScoped<IProviderCallBackLogService, ProviderCallBackLogService>();
builder.Services.AddScoped<IProviderConfigurationAdminCommissionRepository, ProviderConfigurationAdminCommissionRepository>();
builder.Services.AddScoped<IProviderConfigurationAdminCommissionService, ProviderConfigurationAdminCommissionService>();
builder.Services.AddScoped<IProviderOperatorDenominationMappingRepository, ProviderOperatorDenominationMappingRepository>();
builder.Services.AddScoped<IProviderOperatorDenominationMappingService, ProviderOperatorDenominationMappingService>();
builder.Services.AddScoped<IProviderPurchaseRepository, ProviderPurchaseRepository>();
builder.Services.AddScoped<IProviderPurchaseService, ProviderPurchaseService>();
builder.Services.AddScoped<IProviderRequestResponseLogRepository, ProviderRequestResponseLogRepository>();
builder.Services.AddScoped<IProviderRequestResponseLogService, ProviderRequestResponseLogService>();
builder.Services.AddScoped<IProviderResponseTypeMasterRepository, ProviderResponseTypeMasterRepository>();
builder.Services.AddScoped<IProviderResponseTypeMasterService, ProviderResponseTypeMasterService>();
builder.Services.AddScoped<IRealProviderMappingRepository, RealProviderMappingRepository>();
builder.Services.AddScoped<IRealProviderMappingService, RealProviderMappingService>();
builder.Services.AddScoped<IViewProviderMappingRepository, ViewProviderMappingRepository>();
builder.Services.AddScoped<IRedeemcommissionRepository, RedeemcommissionRepository>();
builder.Services.AddScoped<IRedeemcommissionService, RedeemcommissionService>();
builder.Services.AddScoped<IRedirectUrlMasterRepository, RedirectUrlMasterRepository>();
builder.Services.AddScoped<IRedirectUrlMasterService, RedirectUrlMasterService>();
builder.Services.AddScoped<IRichTextMediaRepository, RichTextMediaRepository>();
builder.Services.AddScoped<IRichTextMediaService, RichTextMediaService>();
builder.Services.AddScoped<IROfferCommissionRepository, ROfferCommissionRepository>();
builder.Services.AddScoped<IROfferCommissionService, ROfferCommissionService>();
builder.Services.AddScoped<ISentSMSRepository, SentSMSRepository>();
builder.Services.AddScoped<ISentSMSService, SentSMSService>();
builder.Services.AddScoped<IServiceAndUserwiseSubscriptionRepository, ServiceAndUserwiseSubscriptionRepository>();
builder.Services.AddScoped<IServiceAndUserwiseSubscriptionService, ServiceAndUserwiseSubscriptionService>();
builder.Services.AddScoped<IServiceSubscriptionPlanMasterRepository, ServiceSubscriptionPlanMasterRepository>();
builder.Services.AddScoped<IServiceSubscriptionPlanMasterService, ServiceSubscriptionPlanMasterService>();
builder.Services.AddScoped<IServiceWiseSliderSettingRepository, ServiceWiseSliderSettingRepository>();
builder.Services.AddScoped<IServiceWiseSliderSettingService, ServiceWiseSliderSettingService>();
builder.Services.AddScoped<IPayoutVendorBeneficiaryMappingRepository, PayoutVendorBeneficiaryMappingRepository>();
builder.Services.AddScoped<IPayoutVendorBeneficiaryMappingService, PayoutVendorBeneficiaryMappingService>();
builder.Services.AddScoped<IBrowserUserAgentMasterRepository, BrowserUserAgentMasterRepository>();
builder.Services.AddScoped<IBrowserUserAgentMasterService, BrowserUserAgentMasterService>();

// If you still want to use dynamic registration, add it here as well
// builder.Services.AddDynamicDependencyInjection();

// Configure Cache Settings
var cacheExpiryMinutes = builder.Configuration.GetValue<int>("Cache:ExpiryTimeInMinutes");
if (cacheExpiryMinutes > 0)
{
    FintechApp.Processing.MemoryCacheSetting.ExpiryTimeInMinute = cacheExpiryMinutes;
}

// Phase 8 DI
builder.Services.AddScoped<IUserWisePlanLimitRepository, UserWisePlanLimitRepository>();
builder.Services.AddScoped<IUserWisePlanLimitService, UserWisePlanLimitService>();
builder.Services.AddScoped<IUserWisePlanViewRepository, UserWisePlanViewRepository>();
builder.Services.AddScoped<IUserWisePlanViewService, UserWisePlanViewService>();
builder.Services.AddScoped<ISenderRepository, SenderRepository>();
builder.Services.AddScoped<ISenderService, SenderService>();
builder.Services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<ISenderOTPRepository, SenderOTPRepository>();
builder.Services.AddScoped<ISenderOTPService, SenderOTPService>();
builder.Services.AddScoped<IBeneficiaryOTPRepository, BeneficiaryOTPRepository>();
builder.Services.AddScoped<IBeneficiaryOTPService, BeneficiaryOTPService>();
builder.Services.AddScoped<ITransactionDetailsRepository, TransactionDetailsRepository>();
builder.Services.AddScoped<ITransactionDetailsService, TransactionDetailsService>();
builder.Services.AddScoped<ITransactionProviderHistoryRepository, TransactionProviderHistoryRepository>();
builder.Services.AddScoped<ITransactionProviderHistoryService, TransactionProviderHistoryService>();
builder.Services.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<ITransactionManualUpdateRepository, TransactionManualUpdateRepository>();
builder.Services.AddScoped<ITransactionManualUpdateService, TransactionManualUpdateService>();
builder.Services.AddScoped<ITransactionStatusChangeRepository, TransactionStatusChangeRepository>();
builder.Services.AddScoped<ITransactionStatusChangeService, TransactionStatusChangeService>();
builder.Services.AddScoped<IThreewayReconCompletionRepository, ThreewayReconCompletionRepository>();
builder.Services.AddScoped<IThreewayReconCompletionService, ThreewayReconCompletionService>();
builder.Services.AddScoped<IUserActionMasterRepository, UserActionMasterRepository>();
builder.Services.AddScoped<IUserActionMasterService, UserActionMasterService>();
builder.Services.AddScoped<IAuthorizationMasterRepository, AuthorizationMasterRepository>();
builder.Services.AddScoped<IAuthorizationMasterService, AuthorizationMasterService>();
builder.Services.AddScoped<IUserPaymentCollectMappingRepository, UserPaymentCollectMappingRepository>();
builder.Services.AddScoped<IUserPaymentCollectMappingService, UserPaymentCollectMappingService>();
builder.Services.AddScoped<IUserPlanUtilizationRepository, UserPlanUtilizationRepository>();
builder.Services.AddScoped<IUserPlanUtilizationService, UserPlanUtilizationService>();
builder.Services.AddScoped<IMPinHistoryRepository, MPinHistoryRepository>();
builder.Services.AddScoped<IMPinHistoryService, MPinHistoryService>();
builder.Services.AddScoped<IWrongPasswordHistoryRepository, WrongPasswordHistoryRepository>();
builder.Services.AddScoped<IWrongPasswordHistoryService, WrongPasswordHistoryService>();
builder.Services.AddScoped<IFailedSignUpDataRepository, FailedSignUpDataRepository>();
builder.Services.AddScoped<IFailedSignUpDataService, FailedSignUpDataService>();
builder.Services.AddScoped<IServiceSubscriptionCreatePlanMasterRepository, ServiceSubscriptionCreatePlanMasterRepository>();
builder.Services.AddScoped<IServiceSubscriptionCreatePlanMasterService, ServiceSubscriptionCreatePlanMasterService>();
builder.Services.AddScoped<IServiceSubscriptionPlanDetailsRepository, ServiceSubscriptionPlanDetailsRepository>();
builder.Services.AddScoped<IServiceSubscriptionPlanDetailsService, ServiceSubscriptionPlanDetailsService>();
builder.Services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<IUserServiceSubscriptionDetailsRepository, UserServiceSubscriptionDetailsRepository>();
builder.Services.AddScoped<IUserServiceSubscriptionDetailsService, UserServiceSubscriptionDetailsService>();
builder.Services.AddScoped<IUserServiceSubscriptionChargeRepository, UserServiceSubscriptionChargeRepository>();
builder.Services.AddScoped<IUserServiceSubscriptionChargeService, UserServiceSubscriptionChargeService>();
builder.Services.AddScoped<IPrepaidActivationRepository, PrepaidActivationRepository>();
builder.Services.AddScoped<IPrepaidActivationService, PrepaidActivationService>();
builder.Services.AddScoped<IPureDMTTransferBeneDetailRepository, PureDMTTransferBeneDetailRepository>();
builder.Services.AddScoped<IPureDMTTransferBeneDetailService, PureDMTTransferBeneDetailService>();
builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
builder.Services.AddScoped<ICategoryChannelMappingRepository, CategoryChannelMappingRepository>();
builder.Services.AddScoped<ICategoryChannelMappingService, CategoryChannelMappingService>();

// Phase 7 DI
builder.Services.AddScoped<IeKYCSettingRepository, eKYCSettingRepository>();
builder.Services.AddScoped<IeKYCSettingService, eKYCSettingService>();
builder.Services.AddScoped<IMicroATMSettingRepository, MicroATMSettingRepository>();
builder.Services.AddScoped<IMicroATMSettingService, MicroATMSettingService>();
builder.Services.AddScoped<ISubscriptionMasterRepository, SubscriptionMasterRepository>();
builder.Services.AddScoped<ISubscriptionMasterService, SubscriptionMasterService>();
builder.Services.AddScoped<ISupportUserAccessMasterRepository, SupportUserAccessMasterRepository>();
builder.Services.AddScoped<ISupportUserAccessMasterService, SupportUserAccessMasterService>();
builder.Services.AddScoped<IThirdPartyServiceProviderRepository, ThirdPartyServiceProviderRepository>();
builder.Services.AddScoped<IThirdPartyServiceProviderService, ThirdPartyServiceProviderService>();
builder.Services.AddScoped<IUnitMasterRepository, UnitMasterRepository>();
builder.Services.AddScoped<IUnitMasterService, UnitMasterService>();
builder.Services.AddScoped<IUrlTypeMasterRepository, UrlTypeMasterRepository>();
builder.Services.AddScoped<IUrlTypeMasterService, UrlTypeMasterService>();
builder.Services.AddScoped<IVerificationSettingRepository, VerificationSettingRepository>();
builder.Services.AddScoped<IVerificationSettingService, VerificationSettingService>();
builder.Services.AddScoped<IVirtualBalanceRepository, VirtualBalanceRepository>();
builder.Services.AddScoped<IVirtualBalanceService, VirtualBalanceService>();
builder.Services.AddScoped<IUPITransferSettingRepository, UPITransferSettingRepository>();
builder.Services.AddScoped<IUPITransferSettingService, UPITransferSettingService>();
builder.Services.AddScoped<IRemainderOperatorAmountWiseRepository, RemainderOperatorAmountWiseRepository>();
builder.Services.AddScoped<IRemainderOperatorAmountWiseService, RemainderOperatorAmountWiseService>();
builder.Services.AddScoped<ISDKSettingRepository, SDKSettingRepository>();
builder.Services.AddScoped<ISDKSettingService, SDKSettingService>();
builder.Services.AddScoped<IPanCardSettingRepository, PanCardSettingRepository>();
builder.Services.AddScoped<IPanCardSettingService, PanCardSettingService>();
builder.Services.AddScoped<ILastChangeUserDetailsRepository, LastChangeUserDetailsRepository>();
builder.Services.AddScoped<ILastChangeUserDetailsService, LastChangeUserDetailsService>();
builder.Services.AddScoped<ILeadGenerationMasterRepository, LeadGenerationMasterRepository>();
builder.Services.AddScoped<ILeadGenerationMasterService, LeadGenerationMasterService>();
builder.Services.AddScoped<IBlackListBankMasterRepository, BlackListBankMasterRepository>();
builder.Services.AddScoped<IBlackListBankMasterService, BlackListBankMasterService>();
builder.Services.AddScoped<IBulkPaySettingRepository, BulkPaySettingRepository>();
builder.Services.AddScoped<IBulkPaySettingService, BulkPaySettingService>();
builder.Services.AddScoped<ICMSSettingRepository, CMSSettingRepository>();
builder.Services.AddScoped<ICMSSettingService, CMSSettingService>();
builder.Services.AddScoped<IWebsitePageRepository, WebsitePageRepository>();
builder.Services.AddScoped<IWebsitePageService, WebsitePageService>();
builder.Services.AddScoped<IConsumerTypeMasterRepository, ConsumerTypeMasterRepository>();
builder.Services.AddScoped<IConsumerTypeMasterService, ConsumerTypeMasterService>();
builder.Services.AddScoped<IDematSettingRepository, DematSettingRepository>();
builder.Services.AddScoped<IDematSettingService, DematSettingService>();
builder.Services.AddScoped<IDeviceWiseFailLoginHistoryRepository, DeviceWiseFailLoginHistoryRepository>();
builder.Services.AddScoped<IDeviceWiseFailLoginHistoryService, DeviceWiseFailLoginHistoryService>();
builder.Services.AddScoped<ILoanTypeMasterRepository, LoanTypeMasterRepository>();
builder.Services.AddScoped<ILoanTypeMasterService, LoanTypeMasterService>();
builder.Services.AddScoped<IMenuActionMappingRepository, MenuActionMappingRepository>();
builder.Services.AddScoped<IMenuActionMappingService, MenuActionMappingService>();
builder.Services.AddScoped<IOTTSettingRepository, OTTSettingRepository>();
builder.Services.AddScoped<IOTTSettingService, OTTSettingService>();
builder.Services.AddScoped<IPaySprintBankRepository, PaySprintBankRepository>();
builder.Services.AddScoped<IPaySprintBankService, PaySprintBankService>();
builder.Services.AddScoped<IPOSSettingMasterRepository, POSSettingMasterRepository>();
builder.Services.AddScoped<IPOSSettingMasterService, POSSettingMasterService>();
builder.Services.AddScoped<ISignUpRepository, SignUpRepository>();
builder.Services.AddScoped<ISignUpService, SignUpService>();
builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
builder.Services.AddScoped<IPendingRegistrationService, PendingRegistrationService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChannelCategoryRepository, ChannelCategoryRepository>();
builder.Services.AddScoped<IChannelCategoryService, ChannelCategoryService>();
builder.Services.AddScoped<ICreditInstitutionMasterRepository, CreditInstitutionMasterRepository>();
builder.Services.AddScoped<ICreditInstitutionMasterService, CreditInstitutionMasterService>();
builder.Services.AddScoped<IAndroidMenuMasterRepository, AndroidMenuMasterRepository>();
builder.Services.AddScoped<IAndroidMenuMasterService, AndroidMenuMasterService>();
builder.Services.AddScoped<IAndroidSectionMasterRepository, AndroidSectionMasterRepository>();
builder.Services.AddScoped<IAndroidSectionMasterService, AndroidSectionMasterService>();
builder.Services.AddScoped<ITransactionAPIUserRequestMasterRepository, TransactionAPIUserRequestMasterRepository>();
builder.Services.AddScoped<ITransactionAPIUserRequestMasterService, TransactionAPIUserRequestMasterService>();
builder.Services.AddScoped<IProviderOperatorCircleMasterRepository, ProviderOperatorCircleMasterRepository>();
builder.Services.AddScoped<IProviderOperatorCircleMasterService, ProviderOperatorCircleMasterService>();
builder.Services.AddScoped<IOnboardingUserDocumentRepository, OnboardingUserDocumentRepository>();
builder.Services.AddScoped<IOnboardingUserDocumentService, OnboardingUserDocumentService>();
builder.Services.AddScoped<IUPIVendorBeneficiaryMappingRepository, UPIVendorBeneficiaryMappingRepository>();
builder.Services.AddScoped<IUPIVendorBeneficiaryMappingService, UPIVendorBeneficiaryMappingService>();
builder.Services.AddScoped<IVirtualAccountMasterRepository, VirtualAccountMasterRepository>();
builder.Services.AddScoped<IVirtualAccountMasterService, VirtualAccountMasterService>();
builder.Services.AddScoped<IDailyAuthenticationMasterRepository, DailyAuthenticationMasterRepository>();
builder.Services.AddScoped<IDailyAuthenticationMasterService, DailyAuthenticationMasterService>();
builder.Services.AddScoped<IOperationalHistoryRepository, OperationalHistoryRepository>();
builder.Services.AddScoped<IOperationalHistoryService, OperationalHistoryService>();
builder.Services.AddScoped<ISDKBBPSUserMappingRepository, SDKBBPSUserMappingRepository>();
builder.Services.AddScoped<ISDKBBPSUserMappingService, SDKBBPSUserMappingService>();
builder.Services.AddScoped<IUserwiseSaleTargetRepository, UserwiseSaleTargetRepository>();
builder.Services.AddScoped<IUserwiseSaleTargetService, UserwiseSaleTargetService>();
builder.Services.AddScoped<IUserwiseServiceChargeMasterRepository, UserwiseServiceChargeMasterRepository>();
builder.Services.AddScoped<IUserwiseServiceChargeMasterService, UserwiseServiceChargeMasterService>();
builder.Services.AddScoped<ITargetSaleCommissionRepository, TargetSaleCommissionRepository>();
builder.Services.AddScoped<ITargetSaleCommissionService, TargetSaleCommissionService>();
builder.Services.AddScoped<IUserActionHistoryRepository, UserActionHistoryRepository>();
builder.Services.AddScoped<IUserActionHistoryService, UserActionHistoryService>();
builder.Services.AddScoped<IReferralRegistrationRepository, ReferralRegistrationRepository>();
builder.Services.AddScoped<IReferralRegistrationService, ReferralRegistrationService>();
builder.Services.AddScoped<IOverdraftDetailRepository, OverdraftDetailRepository>();
builder.Services.AddScoped<IOverdraftDetailService, OverdraftDetailService>();
builder.Services.AddScoped<IOverdraftMasterRepository, OverdraftMasterRepository>();
builder.Services.AddScoped<IOverdraftMasterService, OverdraftMasterService>();
builder.Services.AddScoped<ITrustableDeviceRepository, TrustableDeviceRepository>();
builder.Services.AddScoped<ITrustableDeviceService, TrustableDeviceService>();
builder.Services.AddScoped<IUserMerchantMasterRepository, UserMerchantMasterRepository>();
builder.Services.AddScoped<IUserMerchantMasterService, UserMerchantMasterService>();
builder.Services.AddScoped<IUserWisePayementGatewaySettingRepository, UserWisePayementGatewaySettingRepository>();
builder.Services.AddScoped<IUserWisePayementGatewaySettingService, UserWisePayementGatewaySettingService>();
builder.Services.AddScoped<IOnboardingUserRepository, OnboardingUserRepository>();
builder.Services.AddScoped<IOnboardingUserService, OnboardingUserService>();
builder.Services.AddScoped<ISmsTransactionMasterRepository, SmsTransactionMasterRepository>();
builder.Services.AddScoped<ISmsTransactionMasterService, SmsTransactionMasterService>();
builder.Services.AddScoped<IWalletServiceMappingRepository, WalletServiceMappingRepository>();
builder.Services.AddScoped<IWalletServiceMappingService, WalletServiceMappingService>();
builder.Services.AddScoped<IeKYCLogRepository, eKYCLogRepository>();
builder.Services.AddScoped<IeKYCLogService, eKYCLogService>();
builder.Services.AddScoped<IeKYCMasterRepository, eKYCMasterRepository>();
builder.Services.AddScoped<IeKYCMasterService, eKYCMasterService>();
builder.Services.AddScoped<IBankRequestResponseLogRepository, BankRequestResponseLogRepository>();
builder.Services.AddScoped<IBankRequestResponseLogService, BankRequestResponseLogService>();
builder.Services.AddScoped<ISupportUserAccessDetailRepository, SupportUserAccessDetailRepository>();
builder.Services.AddScoped<ISupportUserAccessDetailService, SupportUserAccessDetailService>();
builder.Services.AddScoped<ILapuTransactionDetailRepository, LapuTransactionDetailRepository>();
builder.Services.AddScoped<ILapuTransactionDetailService, LapuTransactionDetailService>();
builder.Services.AddScoped<IUpiSenderRepository, UpiSenderRepository>();
builder.Services.AddScoped<IUpiSenderService, UpiSenderService>();
builder.Services.AddScoped<IUpiBeneficiaryRepository, UpiBeneficiaryRepository>();
builder.Services.AddScoped<IUpiBeneficiaryService, UpiBeneficiaryService>();

var app = builder.Build();

// Warm up the EF Core model at startup. The model has 200+ entities, so the first
// on-demand build (triggered by the first DB query) is slow and made early requests
// (e.g. login) appear to hang. Building it here moves that cost to startup.
using (var warmupScope = app.Services.CreateScope())
{
    _ = warmupScope.ServiceProvider.GetRequiredService<FintechApp.Data.ApplicationDbContext>().Model;
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage(); // This will show detailed errors
}

// Use Custom Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Skip HTTPS redirection in Development so the Vite dev proxy can reach the API over
// http://localhost:5266 without a cross-origin 307 to the https port (which breaks CORS).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("CORSFintechApp");

// Serve uploaded images (service/operator icons) from <contentroot>/wwwroot/uploads.

// The folder is created here because WebRootPath is unset when wwwroot is absent at startup.
var staticWebRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(staticWebRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(staticWebRoot),
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();