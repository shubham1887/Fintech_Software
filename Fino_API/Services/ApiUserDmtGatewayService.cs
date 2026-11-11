using System.Globalization;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Requests.Beneficiary;
using FintechApp.Requests.DMTSender;
using FintechApp.Requests.Transaction;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserDmtGatewayService"/>
    public class ApiUserDmtGatewayService : IApiUserDmtGatewayService
    {
        /// <summary>Marks these transfers as originating from the B2B API in reports.</summary>
        private const string ThroughApi = "API";

        /// <summary>The only two rails a DMT transfer may take.</summary>
        private static readonly string[] PaymentTypes = { "IMPS", "NEFT" };
        private const string DefaultPaymentType = "IMPS";

        private readonly IApiUserAuthService _auth;
        private readonly IApiUserRequestSecurityService _security;
        private readonly IApiUserResponseBuilder _builder;
        private readonly IDMTSenderFlowService _senderFlow;
        private readonly IDMTBeneficiaryFlowService _beneficiaryFlow;
        private readonly IDMTAccountVerifyService _accountVerify;
        private readonly IDMTBankService _bankService;
        private readonly IDmtGatewayModeService _mode;
        private readonly IBankTransferService _transfer;
        private readonly ISenderRepository _senderRepo;
        private readonly IBeneficiaryRepository _beneficiaryRepo;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly ITransactionProcessingService _processing;
        private readonly ICallbackDispatchService _callbackDispatch;
        private readonly IAPIUserInitialRequestResponseLogService _initialLog;
        private readonly IResponseMessageRuleService _messageRules;
        private readonly Logger _logger = new();

        private readonly decimal _minAmount;
        private readonly decimal _maxAmount;
        private readonly decimal _senderMonthlyLimit;
        private readonly bool _statusLiveProbe;
        private readonly int _statusProbeMinAgeSeconds;

        public ApiUserDmtGatewayService(
            IApiUserAuthService auth,
            IApiUserRequestSecurityService security,
            IApiUserResponseBuilder builder,
            IDMTSenderFlowService senderFlow,
            IDMTBeneficiaryFlowService beneficiaryFlow,
            IDMTAccountVerifyService accountVerify,
            IDMTBankService bankService,
            IDmtGatewayModeService mode,
            IBankTransferService transfer,
            ISenderRepository senderRepo,
            IBeneficiaryRepository beneficiaryRepo,
            ITransactionMasterRepository txnRepo,
            IServiceRepository serviceRepo,
            ITransactionProcessingService processing,
            ICallbackDispatchService callbackDispatch,
            IAPIUserInitialRequestResponseLogService initialLog,
            IResponseMessageRuleService messageRules,
            IConfiguration configuration)
        {
            _auth = auth;
            _security = security;
            _builder = builder;
            _senderFlow = senderFlow;
            _beneficiaryFlow = beneficiaryFlow;
            _accountVerify = accountVerify;
            _bankService = bankService;
            _mode = mode;
            _transfer = transfer;
            _senderRepo = senderRepo;
            _beneficiaryRepo = beneficiaryRepo;
            _txnRepo = txnRepo;
            _serviceRepo = serviceRepo;
            _processing = processing;
            _callbackDispatch = callbackDispatch;
            _initialLog = initialLog;
            _messageRules = messageRules;

            var cfg = configuration.GetSection("ApiUser:Dmt");

            // A per-transfer floor and ceiling enforced at the API edge, ON TOP OF the wallet
            // balance, the slab limits and the per-sender cap. It is the cheapest possible brake on
            // a partner integration that goes wrong — a misplaced decimal point is refused before
            // anything is debited, and it costs one comparison.
            _minAmount = Math.Max(cfg.GetValue("MinAmount", 100m), 1m);
            _maxAmount = Math.Max(cfg.GetValue("MaxAmount", 25000m), _minAmount);

            // The fallback per-sender monthly cap, used only when the platform cannot resolve one.
            // Matches DMTSenderFlowService.DefaultSenderLimit so the API edge and the web flow
            // refuse the same transfer rather than disagreeing about the ceiling.
            _senderMonthlyLimit = Math.Max(cfg.GetValue("SenderMonthlyLimit", 25000m), _minAmount);

            var statusCfg = configuration.GetSection("ApiUser:Status");
            _statusLiveProbe = statusCfg.GetValue("LiveProbe", true);
            _statusProbeMinAgeSeconds = Math.Max(statusCfg.GetValue("ProbeMinAgeSeconds", 60), 0);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Remitter onboarding
        // ─────────────────────────────────────────────────────────────────────

        public async Task<ApiUserDmtAckResponse> RegisterSender(ApiUserDmtSenderRegisterRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                if (string.IsNullOrWhiteSpace(request.SenderName))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderNameRequired,
                        "Parameter 'sendername' is required.", request.RequestId);

                var (result, failure) = await _senderFlow.AddSender(new AddSenderRequest
                {
                    Name = Clean(request.SenderName),
                    Mobile = mobile,
                    Address = Clean(request.Address),
                    PinCode = Clean(request.Pincode),
                }, gate.userId, request.ClientIp);

                if (!failure.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, failure, request.RequestId);

                // "VERIFY_OTP" is the flow's way of saying the remitter exists but still owes us a
                // verification step. Translated here into the contract's boolean so an integrator
                // never has to match on an internal string.
                bool otpRequired = result.EqualsIgnoreCase("VERIFY_OTP");

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    otpRequired
                        ? "Sender registered. An OTP has been sent to the sender's mobile — verify it to activate."
                        : "Sender registered and active.",
                    request.RequestId);
                response.OtpRequired = otpRequired;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtAckResponse> VerifySenderOtp(ApiUserDmtSenderOtpRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                // A payout rail sends no sender OTP, so there is none to clear. An integrator who
                // built the four-step pure-DMT flow must not break when the platform switches
                // rails, so this succeeds instead of failing — the sender is already usable.
                if (await _mode.IsPayoutDmt())
                    return ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                        "Sender OTP is not required on this platform. The sender is already active.",
                        request.RequestId);

                if (string.IsNullOrWhiteSpace(request.Otp))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.OtpRequired,
                        "Parameter 'otp' is required.", request.RequestId);

                // Ownership before action: verifying an OTP for a remitter this API user never
                // registered would let one partner activate another's customer.
                if (await ResolveSender(gate.userId, mobile) == null)
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                string result = await _senderFlow.VerifyOTP(
                    new VerifySenderOTPRequest { Mobile = mobile, OTP = Clean(request.Otp) }, gate.userId);

                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, result, request.RequestId);

                return ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    "Sender verified. You may now register beneficiaries and transfer.", request.RequestId);
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtAckResponse> ResendSenderOtp(ApiUserDmtSenderOtpRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                if (await ResolveSender(gate.userId, mobile) == null)
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                string result = await _senderFlow.ResendOTP(new ResendSenderOTPRequest { Mobile = mobile }, gate.userId);
                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, result, request.RequestId);

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    "A fresh OTP has been sent to the sender's mobile.", request.RequestId);
                response.OtpRequired = true;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtSenderResponse> FetchSender(ApiUserDmtSenderFetchRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtSenderResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtSenderResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                var sender = await ResolveSender(gate.userId, mobile);
                if (sender == null)
                    return Fail<ApiUserDmtSenderResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                var (failure, details) = await _senderFlow.GetSender(
                    new SelectSenderRequest { Mobile = mobile }, gate.userId);

                if (!failure.EqualsIgnoreCase(StatusType.Ok) || details == null)
                    return Fail<ApiUserDmtSenderResponse>(ApiUserErrorCode.DmtRejected, failure, request.RequestId);

                var limit = await ResolveLimit(gate.userId, mobile, details.RemainingLimit);

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtSenderResponse(),
                    "Sender details fetched.", request.RequestId);
                response.SenderMobile = details.Mobile;
                response.SenderName = details.Name;
                response.Address = details.Address;
                response.Verified = sender.IsOTPVerified;
                response.ConsumedLimit = limit.consumed;
                response.RemainingLimit = limit.remaining;
                response.PerTransferLimit = details.DisplayLimit;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtSenderResponse>(ex, request.RequestId); }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Beneficiary management
        // ─────────────────────────────────────────────────────────────────────

        public async Task<ApiUserDmtBeneficiaryResponse> RegisterBeneficiary(ApiUserDmtBeneficiaryRegisterRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtBeneficiaryResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                string account = Clean(request.AccountNo);
                string ifsc = Clean(request.Ifsc).ToUpperInvariant();

                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                if (string.IsNullOrWhiteSpace(request.BeneficiaryName))
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.BeneficiaryRequired,
                        "Parameter 'beneficiaryname' is required.", request.RequestId);

                if (account.Length == 0)
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.AccountRequired,
                        "Parameter 'accountno' is required.", request.RequestId);

                var ifscError = ValidateIfsc<ApiUserDmtBeneficiaryResponse>(ifsc, request.RequestId);
                if (ifscError != null) return ifscError;

                var sender = await ResolveSender(gate.userId, mobile);
                if (sender == null)
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                if (!sender.IsOTPVerified)
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.SenderNotVerified,
                        "Sender is not OTP-verified. Verify the sender before adding beneficiaries.", request.RequestId);

                var (failure, added) = await _beneficiaryFlow.AddBeneficiary(new AddBeneficiaryRequest
                {
                    SenderGUID = sender.GUID,
                    SenderMobile = mobile,
                    Name = Clean(request.BeneficiaryName),
                    AccountNo = account,
                    IFSC = ifsc,
                    BankName = Clean(request.BankName),
                }, gate.userId, request.ClientIp);

                if (!failure.EqualsIgnoreCase(StatusType.Ok) || added == null)
                    return Fail<ApiUserDmtBeneficiaryResponse>(ApiUserErrorCode.DmtRejected, failure, request.RequestId);

                var stored = await ResolveBeneficiary(gate.userId, added.BeneficiaryGUID);

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtBeneficiaryResponse(),
                    added.Message, request.RequestId);
                response.OtpRequired = stored != null && !stored.IsOTPVerified;
                response.Beneficiary = stored != null ? Map(stored) : new ApiUserDmtBeneficiary
                {
                    BeneficiaryId = added.BeneficiaryGUID,
                    BeneficiaryName = Clean(request.BeneficiaryName),
                    AccountNo = account,
                    Ifsc = ifsc,
                    BankName = Clean(request.BankName),
                };
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtBeneficiaryResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtAckResponse> VerifyBeneficiaryOtp(ApiUserDmtBeneficiaryOtpRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                if (string.IsNullOrWhiteSpace(request.BeneficiaryId))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryRequired,
                        "Parameter 'beneficiaryid' is required.", request.RequestId);

                // As with the sender OTP: a payout rail issues none, so an integrator's existing
                // verify step succeeds rather than blocking a beneficiary that is already usable.
                if (await _mode.IsPayoutDmt())
                    return ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                        "Beneficiary OTP is not required on this platform. The beneficiary is already active.",
                        request.RequestId);

                if (string.IsNullOrWhiteSpace(request.Otp))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.OtpRequired,
                        "Parameter 'otp' is required.", request.RequestId);

                if (await ResolveBeneficiary(gate.userId, request.BeneficiaryId) == null)
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryNotFound,
                        "No beneficiary with this id exists under your account.", request.RequestId);

                string result = await _beneficiaryFlow.VerifyOTP(new VerifyBeneficiaryOTPRequest
                {
                    BeneficiaryGUID = Clean(request.BeneficiaryId),
                    OTP = Clean(request.Otp),
                }, gate.userId);

                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, result, request.RequestId);

                return ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    "Beneficiary verified. You may now transfer to this account.", request.RequestId);
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtAckResponse> ResendBeneficiaryOtp(ApiUserDmtBeneficiaryOtpRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                if (string.IsNullOrWhiteSpace(request.BeneficiaryId))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryRequired,
                        "Parameter 'beneficiaryid' is required.", request.RequestId);

                if (await ResolveBeneficiary(gate.userId, request.BeneficiaryId) == null)
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryNotFound,
                        "No beneficiary with this id exists under your account.", request.RequestId);

                string result = await _beneficiaryFlow.ResendOTP(
                    new ResendBeneficiaryOTPRequest { BeneficiaryGUID = Clean(request.BeneficiaryId) }, gate.userId);

                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, result, request.RequestId);

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    "A fresh OTP has been sent to the sender's mobile.", request.RequestId);
                response.OtpRequired = true;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtBeneficiaryListResponse> ListBeneficiaries(ApiUserDmtBeneficiaryListRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtBeneficiaryListResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtBeneficiaryListResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                var sender = await ResolveSender(gate.userId, mobile);
                if (sender == null)
                    return Fail<ApiUserDmtBeneficiaryListResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                // Read straight from the API user's OWN beneficiary rows rather than through the
                // shared flow query. Same data, but the scoping is structural: this can only ever
                // return rows belonging to the caller.
                var rows = (await _beneficiaryRepo.GetByUserID(gate.userId))
                    .Where(b => b.SenderID == sender.SenderID && b.IsActive && !b.IsDeleted)
                    .OrderByDescending(b => b.DateTime)
                    .Select(Map)
                    .ToList();

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtBeneficiaryListResponse(),
                    rows.Count > 0 ? "Beneficiaries fetched." : "No beneficiaries registered for this sender.",
                    request.RequestId);
                response.Count = rows.Count;
                response.Beneficiaries = rows;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtBeneficiaryListResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtAckResponse> DeleteBeneficiary(ApiUserDmtBeneficiaryRefRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtAckResponse>(gate, request.RequestId);

                if (string.IsNullOrWhiteSpace(request.BeneficiaryId))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryRequired,
                        "Parameter 'beneficiaryid' is required.", request.RequestId);

                if (await ResolveBeneficiary(gate.userId, request.BeneficiaryId) == null)
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.BeneficiaryNotFound,
                        "No beneficiary with this id exists under your account.", request.RequestId);

                string result = await _beneficiaryFlow.Delete(
                    new DeleteBeneficiaryByGUIDRequest { BeneficiaryGUID = Clean(request.BeneficiaryId) });

                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtAckResponse>(ApiUserErrorCode.DmtRejected, result, request.RequestId);

                return ApiUserResponseFactory.Ok(new ApiUserDmtAckResponse(),
                    "Beneficiary deleted.", request.RequestId);
            }
            catch (Exception ex) { return Crash<ApiUserDmtAckResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtVerifyAccountResponse> VerifyBeneficiaryAccount(ApiUserDmtBeneficiaryVerifyRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtVerifyAccountResponse>(gate, request.RequestId);

                string mobile = Clean(request.SenderMobile);
                string account = Clean(request.AccountNo);
                string ifsc = Clean(request.Ifsc).ToUpperInvariant();

                if (!IsMobile(mobile))
                    return Fail<ApiUserDmtVerifyAccountResponse>(ApiUserErrorCode.SenderMobileRequired,
                        "Parameter 'sendermobile' must be a 10-digit mobile number.", request.RequestId);

                if (account.Length == 0)
                    return Fail<ApiUserDmtVerifyAccountResponse>(ApiUserErrorCode.AccountRequired,
                        "Parameter 'accountno' is required.", request.RequestId);

                var ifscError = ValidateIfsc<ApiUserDmtVerifyAccountResponse>(ifsc, request.RequestId);
                if (ifscError != null) return ifscError;

                var sender = await ResolveSender(gate.userId, mobile);
                if (sender == null)
                    return Fail<ApiUserDmtVerifyAccountResponse>(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.", request.RequestId);

                var (failure, verified) = await _accountVerify.VerifyAccount(new VerifyAccountRequest
                {
                    SenderGUID = sender.GUID,
                    SenderMobile = mobile,
                    AccountNo = account,
                    IFSC = ifsc,
                    BankName = Clean(request.BankName),
                    PaymentType = DefaultPaymentType,
                }, gate.userId, request.ClientIp);

                if (!failure.EqualsIgnoreCase(StatusType.Ok) || verified == null)
                    return Fail<ApiUserDmtVerifyAccountResponse>(ApiUserErrorCode.DmtRejected, failure, request.RequestId);

                // The enquiry itself succeeded but the bank may still have declined to confirm a
                // name. That is a FAILED verification, not a broken request, so it carries the
                // charge and the bank's own message rather than a contract error code.
                bool confirmed = verified.Status.EqualsIgnoreCase(StatusType.Success);

                var response = ApiUserResponseFactory.Apply(new ApiUserDmtVerifyAccountResponse(),
                    confirmed ? ApiUserStatus.Success : ApiUserStatus.Failed,
                    ApiUserErrorCode.None,
                    verified.Message,
                    request.RequestId);
                response.VerifiedName = verified.VerifiedName;
                response.BankName = verified.BankName;
                response.Charge = verified.Charge;
                response.Balance = verified.ClosingBalance;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtVerifyAccountResponse>(ex, request.RequestId); }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Money movement
        // ─────────────────────────────────────────────────────────────────────

        public async Task<ApiUserDmtTransactionResponse> Transfer(ApiUserDmtTransferRequest request)
        {
            int userId = 0;
            int txnId = 0;
            var response = FailTxn(ApiUserErrorCode.SystemError,
                "An error occurred. Please try again.", request.RequestId, ApiUserEventType.Initial);

            try
            {
                var gate = await Gate(request);
                if (!gate.ok)
                {
                    response = FailTxn(gate.errorCode, gate.message, request.RequestId, ApiUserEventType.Initial);
                    return response;
                }
                userId = gate.userId;

                // requestid is mandatory here and nowhere else: it is the idempotency key, and
                // without it a retried HTTP call would silently become a second transfer.
                if (string.IsNullOrWhiteSpace(request.RequestId))
                {
                    response = FailTxn(ApiUserErrorCode.RequestIdRequired,
                        "Parameter 'requestid' is required and must be unique for every transfer.",
                        request.RequestId, ApiUserEventType.Initial);
                    return response;
                }

                // Idempotency. Replay the original result instead of debiting twice — the safest
                // possible answer to a network retry, and it keeps the integrator's reconciliation
                // keyed on their own reference.
                var existing = await _txnRepo.GetByApiUserRequestID(userId, request.RequestId);
                if (existing != null)
                {
                    txnId = existing.TransactionMasterID;
                    response = await BuildTxn(existing, ApiUserEventType.Initial, duplicate: true,
                        messageOverride: $"Duplicate requestid. Returning the original transfer ({existing.Status}).",
                        requestIdOverride: request.RequestId);
                    return response;
                }

                var validation = await ValidateTransfer(request, userId);
                if (validation.error != null)
                {
                    response = validation.error;
                    return response;
                }

                var target = validation.target!;

                var (failure, data) = await _transfer.Transfer(new BankTransferRequest
                {
                    Amount = target.Amount,
                    AccountNo = target.AccountNo,
                    IFSC = target.Ifsc,
                    BankName = target.BankName,
                    BeneficiaryName = target.BeneficiaryName,
                    Mobile = target.SenderMobile,
                    SenderID = target.SenderGuid,
                    BeneficiaryID = target.BeneficiaryGuid,
                    PaymentType = target.PaymentType,
                    Remark = Clean(request.Remark),
                    Through = ThroughApi,
                    APIUserRequestID = request.RequestId,
                    Latitude = Clean(request.Latitude),
                    Longitude = Clean(request.Longitude),
                }, TransferKind.DMT, userId, request.ClientIp);

                // Rejected before a transaction existed (balance, limits, service off…). Nothing was
                // debited, and it will never complete: FAILED / TXN_REJECTED.
                if (!failure.EqualsIgnoreCase(StatusType.Ok) || data == null)
                {
                    response = FailTxn(ApiUserErrorCode.TransactionRejected, failure,
                        request.RequestId, ApiUserEventType.Initial);
                    return response;
                }

                txnId = data.TransactionMasterID;

                // Build the reply from the STORED transaction, not from the in-flight DTO, so the
                // initial reply, a later status check and the callback can never disagree. The
                // balance the transfer pipeline just read is authoritative, so it wins.
                var txn = await _txnRepo.GetByID(txnId);
                response = txn != null
                    ? await BuildTxn(txn, ApiUserEventType.Initial,
                        balanceOverride: data.ClosingBalance, messageOverride: data.Message,
                        requestIdOverride: request.RequestId)
                    : FailTxn(ApiUserErrorCode.SystemError,
                        "Transfer was created but could not be read back.", request.RequestId, ApiUserEventType.Initial);

                // Already final on the synchronous reply — push the callback too, so an integrator
                // who only listens on their callback URL is never left waiting for a settlement
                // that already happened.
                if (response.Final) await _callbackDispatch.DispatchForTransaction(txnId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                response = FailTxn(ApiUserErrorCode.SystemError,
                    "An error occurred. Please try again.", request.RequestId, ApiUserEventType.Initial);
                return response;
            }
            finally
            {
                // Apply the admin's rewrites HERE rather than leaving it to the outgoing filter.
                // The filter runs after this method has already serialised the reply into the
                // audit log, so a message rewritten only there would go out in our words while
                // the integrator's own "Transaction log" screen still showed the provider's —
                // which is the one place a leaked "Low balance" is guaranteed to be read.
                // Doing it before the log keeps the wire, the log and the callback identical.
                await ApplyMessageRules(response);

                // Audit every transfer attempt, including the rejected ones — a rejected transfer is
                // exactly what an integrator opens a ticket about.
                await LogInitial(userId, txnId, request, response);
            }
        }

        public async Task<ApiUserDmtTransactionResponse> Status(ApiUserDmtStatusRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok)
                    return FailTxn(gate.errorCode, gate.message, request.RequestId, ApiUserEventType.Status);

                bool hasTxnId = int.TryParse(Clean(request.TxnId), out int txnId) && txnId > 0;
                if (string.IsNullOrWhiteSpace(request.RequestId) && !hasTxnId)
                    return FailTxn(ApiUserErrorCode.RequestIdRequired,
                        "Send 'requestid' (your own reference) or 'txnid' (our transaction id).",
                        request.RequestId, ApiUserEventType.Status);

                // requestid first: it is the integrator's own key and the one they always have.
                TransactionMaster? txn = null;
                if (!string.IsNullOrWhiteSpace(request.RequestId))
                    txn = await _txnRepo.GetByApiUserRequestID(gate.userId, request.RequestId);
                if (txn == null && hasTxnId)
                    txn = await _txnRepo.GetByID(txnId);

                // Scope check — one API user must never read another's transfer by id.
                if (txn == null || txn.UserID != gate.userId)
                    return FailTxn(ApiUserErrorCode.TransactionNotFound,
                        "No transfer found for the given requestid / txnid.",
                        request.RequestId, ApiUserEventType.Status);

                txn = await ProbeIfPending(txn, gate.userId, request.ClientIp) ?? txn;

                return await BuildTxn(txn, ApiUserEventType.Status, requestIdOverride: request.RequestId);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return FailTxn(ApiUserErrorCode.SystemError,
                    "An error occurred. Please try again.", request.RequestId, ApiUserEventType.Status);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Reference data
        // ─────────────────────────────────────────────────────────────────────

        public async Task<ApiUserBalanceResponse> Balance(ApiUserDmtQueryRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok)
                    return ApiUserResponseFactory.Fail(new ApiUserBalanceResponse(),
                        gate.errorCode, gate.message, request.RequestId);

                return await _builder.Balance(gate.userId, request.RequestId);
            }
            catch (Exception ex) { return Crash<ApiUserBalanceResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtBankListResponse> Banks(ApiUserDmtQueryRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtBankListResponse>(gate, request.RequestId);

                var banks = (await _bankService.GetAll())
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.Name)
                    .Select(b => new ApiUserDmtBank
                    {
                        BankId = b.DMTBankID,
                        BankName = b.Name,
                        DefaultIfsc = b.DefaultIFSC,
                        Imps = b.IsIMPS,
                    })
                    .ToList();

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtBankListResponse(),
                    "Bank list fetched.", request.RequestId);
                response.Count = banks.Count;
                response.Banks = banks;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtBankListResponse>(ex, request.RequestId); }
        }

        public async Task<ApiUserDmtIfscResponse> Ifsc(ApiUserDmtIfscRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok) return Fail<ApiUserDmtIfscResponse>(gate, request.RequestId);

                string ifsc = Clean(request.Ifsc).ToUpperInvariant();
                var ifscError = ValidateIfsc<ApiUserDmtIfscResponse>(ifsc, request.RequestId);
                if (ifscError != null) return ifscError;

                var (failure, branch) = await _beneficiaryFlow.CheckBranch(new CheckBranchRequest { IFSC = ifsc });
                if (!failure.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserDmtIfscResponse>(ApiUserErrorCode.IfscInvalid, failure, request.RequestId);

                var response = ApiUserResponseFactory.Ok(new ApiUserDmtIfscResponse(),
                    "Branch details fetched.", request.RequestId);
                response.Ifsc = string.IsNullOrWhiteSpace(branch.IFSC) ? ifsc : branch.IFSC;
                response.BankName = branch.BankName;
                response.Branch = branch.Branch;
                response.Address = branch.Address;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserDmtIfscResponse>(ex, request.RequestId); }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  The gate — authentication THEN request-signature verification
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs both security layers in the only order that works: the credential check resolves
        /// WHICH user is calling, and only then can the signature be verified, because the HMAC
        /// key is that user's own API token. A failure from either layer is returned with its own
        /// contract code so an integrator can tell a bad token from a bad signature.
        /// </summary>
        private async Task<(bool ok, string errorCode, string message, int userId)> Gate(
            ApiUserDmtRequestBase request)
        {
            var auth = await _auth.Authenticate(request.Mobile, request.Token, request.ClientIp);
            if (!auth.Succeeded) return (false, auth.ErrorCode, auth.Message, 0);

            int userId = auth.User!.UserMasterID;

            var security = _security.Verify(request.Security, userId, auth.User.APIToken);
            if (!security.Succeeded) return (false, security.ErrorCode, security.Message, 0);

            return (true, string.Empty, string.Empty, userId);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Validation
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Everything that must hold before a rupee moves. Returns the resolved sender and
        /// beneficiary, or the exact contract error to hand back — one place, so the error codes
        /// stay consistent and no caller can reach the transfer pipeline through a shortcut.
        /// </summary>
        private async Task<(ApiUserDmtTransactionResponse? error, TransferTarget? target)> ValidateTransfer(
            ApiUserDmtTransferRequest request, int userId)
        {
            (ApiUserDmtTransactionResponse? error, TransferTarget? target) Error(string code, string message) =>
                (FailTxn(code, message, request.RequestId, ApiUserEventType.Initial), null);

            // WHICH RAIL IS LIVE decides what a transfer needs. A payout rail keeps no remitter
            // and no beneficiary registration, so insisting on either would refuse a transfer the
            // rail would have accepted. Everything below branches on this one fact.
            bool payoutRail = await _mode.IsPayoutDmt();

            // MANDATORY on both rails. On a pure rail it names a registered, verified remitter.
            // On a payout rail it does not have to be registered anywhere — any valid number is
            // accepted — but it is still required, because it is the contact number filed against
            // the transfer and the only customer-side identifier a dispute can be traced by.
            string mobile = Clean(request.SenderMobile);
            if (!IsMobile(mobile))
                return Error(ApiUserErrorCode.SenderMobileRequired,
                    "Parameter 'sendermobile' must be a 10-digit mobile number.");

            if (!decimal.TryParse(Clean(request.Amount), out decimal amount) || amount <= 0)
                return Error(ApiUserErrorCode.AmountInvalid,
                    "Parameter 'amount' must be a number greater than zero.");

            // Round to paise before anything downstream sees it: an unrounded amount would be
            // debited at one precision and reported at another.
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            if (amount < _minAmount)
                return Error(ApiUserErrorCode.AmountBelowLimit, $"Minimum transfer amount is {_minAmount:0.##}.");

            if (amount > _maxAmount)
                return Error(ApiUserErrorCode.AmountAboveLimit,
                    $"Maximum transfer amount is {_maxAmount:0.##} per transaction.");

            string paymentType = Clean(request.PaymentType).ToUpperInvariant();
            if (paymentType.Length == 0) paymentType = DefaultPaymentType;
            if (!PaymentTypes.Contains(paymentType))
                return Error(ApiUserErrorCode.PaymentTypeInvalid,
                    $"Parameter 'paymenttype' must be one of: {string.Join(", ", PaymentTypes)}.");

            string locationFailure = ValidateLocation(request.Latitude, request.Longitude);
            if (locationFailure.Length > 0)
                return Error(ApiUserErrorCode.LocationInvalid, locationFailure);

            bool inlineDestination = Clean(request.AccountNo).Length > 0 || Clean(request.Ifsc).Length > 0;

            // An inline destination on a pure rail is refused, never ignored: that rail can only
            // send to an account it registered, so quietly dropping the fields would transfer to
            // whatever the beneficiaryid happens to point at.
            if (inlineDestination && !payoutRail)
                return Error(ApiUserErrorCode.BeneficiaryRequired,
                    "This platform runs pure DMT: register the beneficiary and send 'beneficiaryid'. " +
                    "Inline 'accountno' / 'ifsc' are accepted only on a payout rail.");

            if (string.IsNullOrWhiteSpace(request.BeneficiaryId) && !inlineDestination)
                return Error(ApiUserErrorCode.BeneficiaryRequired,
                    payoutRail
                        ? "Send 'beneficiaryid', or name the destination inline with 'accountno', 'ifsc' and 'beneficiaryname'."
                        : "Parameter 'beneficiaryid' is required.");

            // ── The sender ───────────────────────────────────────────────
            // Pure rail: mandatory and must be verified. Payout rail: looked up only if the
            // caller named one, and carried onto the transaction for their reconciliation.
            Sender? sender = null;
            if (mobile.Length > 0)
            {
                sender = await ResolveSender(userId, mobile);
                if (sender == null && !payoutRail)
                    return Error(ApiUserErrorCode.SenderNotFound,
                        "No sender is registered under your account with this mobile number.");

                if (sender != null && !sender.IsOTPVerified && !payoutRail)
                    return Error(ApiUserErrorCode.SenderNotVerified, "Sender is not OTP-verified.");
            }

            // ── The destination ──────────────────────────────────────────
            string accountNo, ifsc, bankName, beneficiaryName, beneficiaryGuid;

            if (!string.IsNullOrWhiteSpace(request.BeneficiaryId))
            {
                var beneficiary = await ResolveBeneficiary(userId, request.BeneficiaryId);
                if (beneficiary == null)
                    return Error(ApiUserErrorCode.BeneficiaryNotFound,
                        "No beneficiary with this id exists under your account.");

                // The beneficiary must belong to THIS sender. Without this a caller could pair any
                // sender they own with any beneficiary they own and send money the remitter never
                // authorised — and the per-sender limit would be charged to the wrong customer.
                // Only meaningful when a sender is in play at all.
                if (sender != null && beneficiary.SenderID != sender.SenderID)
                    return Error(ApiUserErrorCode.BeneficiaryNotFound,
                        "This beneficiary is not registered under the given sender.");

                if (!beneficiary.IsOTPVerified && !payoutRail)
                    return Error(ApiUserErrorCode.BeneficiaryNotVerified, "Beneficiary is not OTP-verified.");

                if (!beneficiary.IsActive || beneficiary.IsDeleted)
                    return Error(ApiUserErrorCode.BeneficiaryNotFound, "This beneficiary is no longer active.");

                accountNo = beneficiary.AccountNo;
                ifsc = beneficiary.IFSC;
                bankName = beneficiary.BankName;
                beneficiaryName = beneficiary.Name;
                beneficiaryGuid = beneficiary.GUID;
            }
            else
            {
                // Inline, payout rail only (guarded above).
                accountNo = Clean(request.AccountNo);
                if (accountNo.Length < 5 || accountNo.Length > 25 || !accountNo.All(char.IsAsciiLetterOrDigit))
                    return Error(ApiUserErrorCode.AccountRequired,
                        "Parameter 'accountno' must be 5-25 characters, digits and letters only — no spaces or dashes.");

                ifsc = Clean(request.Ifsc).ToUpperInvariant();
                if (ifsc.Length == 0)
                    return Error(ApiUserErrorCode.IfscRequired, "Parameter 'ifsc' is required with 'accountno'.");
                if (!IsWellFormedIfsc(ifsc))
                    return Error(ApiUserErrorCode.IfscInvalid,
                        "Parameter 'ifsc' must be a valid 11-character IFSC, e.g. HDFC0000123.");

                beneficiaryName = Clean(request.BeneficiaryName);
                if (beneficiaryName.Length == 0)
                    return Error(ApiUserErrorCode.BeneficiaryRequired,
                        "Parameter 'beneficiaryname' is required when you name the destination inline.");

                // Bank name is a convenience, not a control — the rails route on the IFSC. A
                // failed lookup is not an error.
                bankName = Clean(request.BankName);
                if (bankName.Length == 0)
                {
                    try
                    {
                        var (branchFailure, branch) = await _beneficiaryFlow.CheckBranch(new CheckBranchRequest { IFSC = ifsc });
                        if (branchFailure.EqualsIgnoreCase(StatusType.Ok)) bankName = Clean(branch.BankName);
                    }
                    catch (Exception ex) { _logger.Write(ex); }
                }

                beneficiaryGuid = string.Empty;
            }

            // The regulatory per-sender cap, checked last because it is the only rule that costs a
            // database aggregate — and checked BEFORE the wallet is touched, so a transfer over the
            // cap never has to be refunded. It is a cap on a REMITTER, so it applies only when
            // there is one; a payout rail transfer with no sender is bounded by the per-transfer
            // ceiling and the wallet instead.
            if (sender != null)
            {
                var limit = await ResolveLimit(userId, mobile, null);
                if (limit.limit > 0 && amount > limit.remaining)
                    return Error(ApiUserErrorCode.AmountAboveLimit,
                        $"This transfer would exceed the sender's limit. Remaining limit is {limit.remaining:0.##}.");
            }

            return (null, new TransferTarget(accountNo, ifsc, bankName, beneficiaryName,
                mobile, sender?.GUID ?? string.Empty, beneficiaryGuid, amount, paymentType));
        }

        /// <summary>
        /// A validated transfer, flattened. Carrying the resolved destination rather than the
        /// Sender/Beneficiary rows is what lets one call site serve both rails: on a payout rail
        /// there may be no such rows at all.
        /// </summary>
        private sealed record TransferTarget(
            string AccountNo,
            string Ifsc,
            string BankName,
            string BeneficiaryName,
            string SenderMobile,
            string SenderGuid,
            string BeneficiaryGuid,
            decimal Amount,
            string PaymentType);

        /// <summary>
        /// Checks the outlet coordinates when the caller sends them. Returns the failure text, or
        /// empty when there is nothing wrong.
        ///
        /// Both or neither: the pipeline stores the pair as a single "lat,long" string, so a
        /// longitude on its own used to be dropped without a word — the integrator believed they
        /// were capturing location for every transfer and the audit column stayed empty. And a
        /// value outside the coordinate range is not a location at all; storing it would put
        /// something in the audit trail that looks like evidence and is not.
        /// </summary>
        private static string ValidateLocation(string? latitude, string? longitude)
        {
            string lat = Clean(latitude);
            string lng = Clean(longitude);

            if (lat.Length == 0 && lng.Length == 0) return string.Empty; // Optional — nothing sent.

            if (lat.Length == 0 || lng.Length == 0)
                return "Send 'latitude' and 'longitude' together, or neither.";

            if (!decimal.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal latValue)
                || latValue < -90m || latValue > 90m)
                return "Parameter 'latitude' must be a number between -90 and 90.";

            if (!decimal.TryParse(lng, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal lngValue)
                || lngValue < -180m || lngValue > 180m)
                return "Parameter 'longitude' must be a number between -180 and 180.";

            return string.Empty;
        }

        /// <summary>
        /// IFSC shape: four letters, a zero, then six alphanumerics. Catching a malformed IFSC
        /// here gives the integrator a precise reason instead of an upstream decline after the
        /// wallet has been debited.
        /// </summary>
        private static bool IsWellFormedIfsc(string ifsc) =>
            ifsc.Length == 11
            && ifsc.Take(4).All(char.IsAsciiLetterUpper)
            && ifsc[4] == '0'
            && ifsc.Skip(5).All(char.IsAsciiLetterOrDigit);

        /// <summary>
        /// Validates IFSC shape: four letters, a zero, then six alphanumerics. Catching a malformed
        /// IFSC here saves a chargeable round trip to the bank and gives the integrator a precise
        /// reason instead of an upstream decline.
        /// </summary>
        private T? ValidateIfsc<T>(string ifsc, string requestId) where T : ApiUserEnvelope, new()
        {
            if (ifsc.Length == 0)
                return Fail<T>(ApiUserErrorCode.IfscRequired, "Parameter 'ifsc' is required.", requestId);

            bool wellFormed = ifsc.Length == 11
                && ifsc.Take(4).All(char.IsAsciiLetterUpper)
                && ifsc[4] == '0'
                && ifsc.Skip(5).All(char.IsAsciiLetterOrDigit);

            return wellFormed
                ? null
                : Fail<T>(ApiUserErrorCode.IfscInvalid,
                    "Parameter 'ifsc' must be a valid 11-character IFSC, e.g. HDFC0000123.", requestId);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Scoped lookups — every read is pinned to the calling API user
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// The remitter with this mobile REGISTERED BY THIS API USER, or null. Scoping every read
        /// to the caller is what keeps one partner from reading, activating or transacting on
        /// another partner's customers.
        /// </summary>
        private async Task<Sender?> ResolveSender(int userId, string mobile) =>
            (await _senderRepo.GetByUserID(userId))
                .FirstOrDefault(s => s.Mobile == mobile && !s.IsDeleted);

        /// <summary>The beneficiary with this reference under this API user, or null.</summary>
        private async Task<Beneficiary?> ResolveBeneficiary(int userId, string beneficiaryId)
        {
            string id = Clean(beneficiaryId);
            if (id.Length == 0) return null;

            return (await _beneficiaryRepo.GetByUserID(userId))
                .FirstOrDefault(b => b.GUID == id && !b.IsDeleted);
        }

        /// <summary>
        /// The remitter's cap, what they have already sent inside the current period, and what is
        /// left. The period is the calendar month, which is how the per-sender DMT cap is defined.
        ///
        /// The cap is NEVER allowed to resolve to zero. A zero would read as "no limit configured"
        /// at the call site and wave every transfer through — so a lookup that fails for any
        /// reason (the flow service's own KYC gate, a missing setting row, an unregistered
        /// remitter) falls back to the configured default instead of disabling the control. A
        /// limit check that can be switched off by an unrelated failure is not a control at all.
        /// </summary>
        private async Task<(decimal limit, decimal consumed, decimal remaining)> ResolveLimit(
            int userId, string mobile, decimal? knownLimit)
        {
            decimal limit = knownLimit ?? 0m;

            if (limit <= 0)
            {
                // The flow service owns limit resolution (provider setting → global setting →
                // default), so ask it rather than re-deriving the precedence here and drifting.
                var (failure, details) = await _senderFlow.GetSender(
                    new SelectSenderRequest { Mobile = mobile }, userId);

                if (failure.EqualsIgnoreCase(StatusType.Ok) && details != null && details.RemainingLimit > 0)
                    limit = details.RemainingLimit;
            }

            if (limit <= 0) limit = _senderMonthlyLimit;

            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            decimal consumed = await _txnRepo.GetDMTConsumedBySender(mobile, monthStart);

            return (limit, consumed, Math.Max(limit - consumed, 0m));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Response shaping
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Turns a stored transfer into the DMT contract. The shared builder produces the common
        /// transaction fields — so DMT and recharge can never disagree about what <c>status</c>,
        /// <c>final</c> or <c>balance</c> mean — and the DMT block is layered on top from the same
        /// stored row.
        /// </summary>
        private async Task<ApiUserDmtTransactionResponse> BuildTxn(
            TransactionMaster txn,
            string eventType,
            decimal? balanceOverride = null,
            string? messageOverride = null,
            bool duplicate = false,
            string? requestIdOverride = null)
        {
            var common = await _builder.FromTransaction(
                txn, eventType, balanceOverride, messageOverride, duplicate, requestIdOverride);

            var response = new ApiUserDmtTransactionResponse
            {
                Status = common.Status,
                StatusCode = common.StatusCode,
                ErrorCode = common.ErrorCode,
                Message = common.Message,
                RequestId = common.RequestId,
                Event = common.Event,
                Final = common.Final,
                Duplicate = common.Duplicate,
                TxnId = common.TxnId,
                ProviderRef = common.ProviderRef,
                Service = common.Service,
                ServiceId = common.ServiceId,
                Operator = common.Operator,
                OperatorCode = common.OperatorCode,
                Number = common.Number,
                Amount = common.Amount,
                DebitAmount = common.DebitAmount,
                Balance = common.Balance,
                Date = common.Date,

                SenderMobile = txn.DMT_SenderNumber,
                BeneficiaryId = txn.DMT_BeneficiaryID,
                BeneficiaryName = txn.DMT_BeneficiaryName,
                AccountNo = txn.ConsumerNumber,
                Ifsc = txn.DMT_IFSC,
                BankName = txn.DMT_Bank,
                PaymentType = txn.DMT_PaymentType,
            };

            // The UTR is the bank's reference and only exists once the transfer actually settled.
            // Reporting the internal provider reference while PENDING would have integrators quote
            // a number the beneficiary's bank cannot trace.
            response.Utr = ApiUserStatus.IsFinal(txn.Status) ? common.ProviderRef : string.Empty;

            // Charges are carried explicitly as well as folded into debitamount, so an integrator
            // reconciling commission never has to derive them by subtraction.
            response.Charge = txn.Charge != 0 ? txn.Charge : Math.Max(txn.DebitAmount - txn.Amount, 0m);

            return response;
        }

        /// <summary>
        /// Re-enquire a still-pending transfer with the upstream provider so the answer is the
        /// bank's, not just our stored record. Skipped for a transfer younger than
        /// ProbeMinAgeSeconds (the provider has not decided yet, and a poll-happy integrator would
        /// otherwise hammer the upstream) or with no provider assigned.
        /// </summary>
        private async Task<TransactionMaster?> ProbeIfPending(TransactionMaster txn, int userId, string ipAddress)
        {
            if (!_statusLiveProbe) return null;
            if (ApiUserStatus.IsFinal(txn.Status)) return null;
            if (txn.ProviderID <= 0) return null;
            if ((DateTime.Now - txn.DateTime).TotalSeconds < _statusProbeMinAgeSeconds) return null;

            try
            {
                var svc = await _serviceRepo.GetServiceById(txn.ServiceID);
                await _processing.CheckProviderStatus(
                    txn.TransactionMasterID, ServiceKinds.KindOf(svc), userId, ipAddress);

                // CheckProviderStatus settles in the database and fires the callback itself when
                // the probe was decisive; re-read so we report what it wrote.
                return await _txnRepo.GetByID(txn.TransactionMasterID);
            }
            catch (Exception ex)
            {
                // A provider that is unreachable must not turn a status enquiry into an error —
                // report the transfer as we know it and let the caller poll again.
                _logger.Write(ex);
                return null;
            }
        }

        private static ApiUserDmtBeneficiary Map(Beneficiary b) => new()
        {
            BeneficiaryId = b.GUID,
            BeneficiaryName = b.Name,
            AccountNo = b.AccountNo,
            Ifsc = b.IFSC,
            BankName = b.BankName,
            Verified = b.IsOTPVerified,
            NameVerified = b.IsNameVerified,
        };

        private static ApiUserDmtTransactionResponse FailTxn(
            string errorCode, string message, string requestId, string eventType)
        {
            string status = ApiUserResponseFactory.StatusFor(errorCode);
            var response = ApiUserResponseFactory.Apply(
                new ApiUserDmtTransactionResponse(), status, errorCode, message, requestId);
            response.Event = eventType;
            response.Final = ApiUserStatus.IsFinal(status);
            return response;
        }

        private static T Fail<T>(string errorCode, string message, string requestId)
            where T : ApiUserEnvelope, new() =>
            ApiUserResponseFactory.Fail(new T(), errorCode, message, requestId);

        private static T Fail<T>((bool ok, string errorCode, string message, int userId) gate, string requestId)
            where T : ApiUserEnvelope, new() =>
            ApiUserResponseFactory.Fail(new T(), gate.errorCode, gate.message, requestId);

        private T Crash<T>(Exception ex, string requestId) where T : ApiUserEnvelope, new()
        {
            _logger.Write(ex);
            return ApiUserResponseFactory.Fail(new T(),
                ApiUserErrorCode.SystemError, "An error occurred. Please try again.", requestId);
        }

        private static string Clean(string? value) => (value ?? string.Empty).Trim();

        private static bool IsMobile(string value) => value.Length == 10 && value.All(char.IsAsciiDigit);

        /// <summary>
        /// Puts the reply's one human-readable field through the admin's rewrite rules
        /// (Settings → Response Messages) on the API channel, in place.
        ///
        /// Provider wording was already rewritten at the gateway, where the provider is known;
        /// this second pass is for the messages WE wrote — limit refusals, validation, system
        /// errors — and for anything a provider path missed. It runs before the audit log is
        /// written, so the integrator never reads a different message than the one we sent.
        /// Never throws: a rewrite problem must not change the outcome of a transfer.
        /// </summary>
        private async Task ApplyMessageRules(ApiUserDmtTransactionResponse response)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(response.Message)) return;
                response.Message = await _messageRules.Apply(
                    response.Message, 0, response.ServiceId, MessageChannel.Api);
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        /// <summary>
        /// Writes the request/response pair to APIUserInitialRequestResponseLog. Purely an audit
        /// trail — a logging failure never affects the caller's result.
        /// </summary>
        private async Task LogInitial(int userId, int txnId, ApiUserDmtTransferRequest request,
            ApiUserDmtTransactionResponse response)
        {
            try
            {
                if (userId <= 0) return; // Unauthenticated noise is not worth an audit row.
                await _initialLog.Create(new CreateAPIUserInitialRequestResponseLogRequest
                {
                    UserID = userId,
                    TransactionMasterID = txnId,
                    APIUserRequestID = request.RequestId,
                    ConsumerNumber = Clean(request.BeneficiaryId),
                    Request = request.RawRequest,
                    Response = JsonConvert.SerializeObject(response, Formatting.None),
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
