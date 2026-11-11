using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Requests.Beneficiary;
using FintechApp.Requests.Transaction;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserPayoutGatewayService"/>
    public class ApiUserPayoutGatewayService : IApiUserPayoutGatewayService
    {
        /// <summary>Marks these transfers as originating from the B2B API in reports.</summary>
        private const string ThroughApi = "API";

        /// <summary>
        /// The rails a payout may take. RTGS is here and absent from DMT because payout tickets
        /// are the API user's own funds and are not capped at the DMT per-transfer ceiling.
        /// </summary>
        private static readonly string[] PaymentTypes = { "IMPS", "NEFT", "RTGS" };
        private const string DefaultPaymentType = "IMPS";

        private readonly IApiUserAuthService _auth;
        private readonly IApiUserRequestSecurityService _security;
        private readonly IApiUserResponseBuilder _builder;
        private readonly IBankTransferService _transfer;
        private readonly IDMTBeneficiaryFlowService _beneficiaryFlow;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly ITransactionProcessingService _processing;
        private readonly ICallbackDispatchService _callbackDispatch;
        private readonly IAPIUserInitialRequestResponseLogService _initialLog;
        private readonly Logger _logger = new();

        private readonly decimal _minAmount;
        private readonly decimal _maxAmount;
        private readonly bool _statusLiveProbe;
        private readonly int _statusProbeMinAgeSeconds;

        public ApiUserPayoutGatewayService(
            IApiUserAuthService auth,
            IApiUserRequestSecurityService security,
            IApiUserResponseBuilder builder,
            IBankTransferService transfer,
            IDMTBeneficiaryFlowService beneficiaryFlow,
            ITransactionMasterRepository txnRepo,
            IServiceRepository serviceRepo,
            ITransactionProcessingService processing,
            ICallbackDispatchService callbackDispatch,
            IAPIUserInitialRequestResponseLogService initialLog,
            IConfiguration configuration)
        {
            _auth = auth;
            _security = security;
            _builder = builder;
            _transfer = transfer;
            _beneficiaryFlow = beneficiaryFlow;
            _txnRepo = txnRepo;
            _serviceRepo = serviceRepo;
            _processing = processing;
            _callbackDispatch = callbackDispatch;
            _initialLog = initialLog;

            var cfg = configuration.GetSection("ApiUser:Payout");

            // A per-transfer floor and ceiling enforced at the API edge, ON TOP OF the wallet
            // balance and the slab limits. It is the cheapest possible brake on a partner
            // integration that goes wrong — a misplaced decimal point is refused before anything
            // is debited, and it costs one comparison.
            _minAmount = Math.Max(cfg.GetValue("MinAmount", 1m), 1m);
            _maxAmount = Math.Max(cfg.GetValue("MaxAmount", 200000m), _minAmount);

            var statusCfg = configuration.GetSection("ApiUser:Status");
            _statusLiveProbe = statusCfg.GetValue("LiveProbe", true);
            _statusProbeMinAgeSeconds = Math.Max(statusCfg.GetValue("ProbeMinAgeSeconds", 60), 0);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Money movement
        // ─────────────────────────────────────────────────────────────────────

        public async Task<ApiUserPayoutTransactionResponse> Transfer(ApiUserPayoutTransferRequest request)
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
                // without it a retried HTTP call would silently become a second payout.
                if (string.IsNullOrWhiteSpace(request.RequestId))
                {
                    response = FailTxn(ApiUserErrorCode.RequestIdRequired,
                        "Parameter 'requestid' is required and must be unique for every payout.",
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
                        messageOverride: $"Duplicate requestid. Returning the original payout ({existing.Status}).",
                        requestIdOverride: request.RequestId);
                    return response;
                }

                var validation = await ValidateTransfer(request);
                if (validation.error != null)
                {
                    response = validation.error;
                    return response;
                }

                // Route through the payout service in the catalogue. Resolved here rather than
                // taken from the caller: an integrator must not be able to point a payout at the
                // DMT service and inherit its wallet, limits and commission slabs.
                var (serviceError, serviceId) = await ResolvePayoutServiceId(request.RequestId);
                if (serviceError != null)
                {
                    response = serviceError;
                    return response;
                }

                var (failure, data) = await _transfer.Transfer(new BankTransferRequest
                {
                    ServiceID = serviceId,
                    Amount = validation.amount,
                    AccountNo = validation.accountNo,
                    IFSC = validation.ifsc,
                    BankName = validation.bankName,
                    BeneficiaryName = validation.beneficiaryName,
                    Mobile = Clean(request.Mobile),
                    PaymentType = validation.paymentType,
                    Remark = Clean(request.Remark),
                    Through = ThroughApi,
                    APIUserRequestID = request.RequestId,
                    Latitude = Clean(request.Latitude),
                    Longitude = Clean(request.Longitude),
                }, TransferKind.Payout, userId, request.ClientIp);

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
                        "Payout was created but could not be read back.", request.RequestId, ApiUserEventType.Initial);

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
                // Audit every payout attempt, including the rejected ones — a rejected payout is
                // exactly what an integrator opens a ticket about.
                await LogInitial(userId, txnId, request, response);
            }
        }

        public async Task<ApiUserPayoutTransactionResponse> Status(ApiUserPayoutStatusRequest request)
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

                // Scope check — one API user must never read another's payout by id.
                if (txn == null || txn.UserID != gate.userId)
                    return FailTxn(ApiUserErrorCode.TransactionNotFound,
                        "No payout found for the given requestid / txnid.",
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

        public async Task<ApiUserBalanceResponse> Balance(ApiUserPayoutQueryRequest request)
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

        public async Task<ApiUserPayoutIfscResponse> Ifsc(ApiUserPayoutIfscRequest request)
        {
            try
            {
                var gate = await Gate(request);
                if (!gate.ok)
                    return ApiUserResponseFactory.Fail(new ApiUserPayoutIfscResponse(),
                        gate.errorCode, gate.message, request.RequestId);

                string ifsc = Clean(request.Ifsc).ToUpperInvariant();
                if (!IsWellFormedIfsc(ifsc))
                    return Fail<ApiUserPayoutIfscResponse>(
                        ifsc.Length == 0 ? ApiUserErrorCode.IfscRequired : ApiUserErrorCode.IfscInvalid,
                        ifsc.Length == 0
                            ? "Parameter 'ifsc' is required."
                            : "Parameter 'ifsc' must be a valid 11-character IFSC, e.g. HDFC0000123.",
                        request.RequestId);

                var (failure, branch) = await _beneficiaryFlow.CheckBranch(new CheckBranchRequest { IFSC = ifsc });
                if (!failure.EqualsIgnoreCase(StatusType.Ok))
                    return Fail<ApiUserPayoutIfscResponse>(ApiUserErrorCode.IfscInvalid, failure, request.RequestId);

                var response = ApiUserResponseFactory.Ok(new ApiUserPayoutIfscResponse(),
                    "Branch details fetched.", request.RequestId);
                response.Ifsc = string.IsNullOrWhiteSpace(branch.IFSC) ? ifsc : branch.IFSC;
                response.BankName = branch.BankName;
                response.Branch = branch.Branch;
                response.Address = branch.Address;
                return response;
            }
            catch (Exception ex) { return Crash<ApiUserPayoutIfscResponse>(ex, request.RequestId); }
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
        private async Task<(bool ok, string errorCode, string message, int userId)> Gate(ApiUserPayoutRequestBase request)
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
        /// Everything that must hold before a rupee moves. One place, so the error codes stay
        /// consistent and no caller can reach the transfer pipeline through a shortcut.
        ///
        /// Note what is absent compared with DMT: no sender lookup, no beneficiary lookup, no OTP
        /// state and no per-remitter monthly cap. A payout has no remitter — the destination
        /// account arrives on this request and is validated on the spot.
        /// </summary>
        private async Task<(ApiUserPayoutTransactionResponse? error, string accountNo, string ifsc,
            string beneficiaryName, string bankName, decimal amount, string paymentType)>
            ValidateTransfer(ApiUserPayoutTransferRequest request)
        {
            (ApiUserPayoutTransactionResponse? error, string, string, string, string, decimal, string)
                Error(string code, string message) =>
                (FailTxn(code, message, request.RequestId, ApiUserEventType.Initial),
                    string.Empty, string.Empty, string.Empty, string.Empty, 0m, string.Empty);

            string accountNo = Clean(request.AccountNo);
            if (accountNo.Length == 0)
                return Error(ApiUserErrorCode.AccountRequired, "Parameter 'accountno' is required.");

            // Digits only, and long enough to be a real account. A space or a dash copied out of a
            // passbook would otherwise be dispatched to the bank verbatim and declined upstream,
            // after the wallet had already been debited.
            if (accountNo.Length < 5 || accountNo.Length > 25 || !accountNo.All(char.IsAsciiLetterOrDigit))
                return Error(ApiUserErrorCode.AccountRequired,
                    "Parameter 'accountno' must be 5-25 characters, digits and letters only — no spaces or dashes.");

            string ifsc = Clean(request.Ifsc).ToUpperInvariant();
            if (ifsc.Length == 0)
                return Error(ApiUserErrorCode.IfscRequired, "Parameter 'ifsc' is required.");
            if (!IsWellFormedIfsc(ifsc))
                return Error(ApiUserErrorCode.IfscInvalid,
                    "Parameter 'ifsc' must be a valid 11-character IFSC, e.g. HDFC0000123.");

            string beneficiaryName = Clean(request.BeneficiaryName);
            if (beneficiaryName.Length == 0)
                return Error(ApiUserErrorCode.BeneficiaryNameRequired,
                    "Parameter 'beneficiaryname' is required — payout names the account holder inline.");

            if (!decimal.TryParse(Clean(request.Amount), out decimal amount) || amount <= 0)
                return Error(ApiUserErrorCode.AmountInvalid,
                    "Parameter 'amount' must be a number greater than zero.");

            // Round to paise before anything downstream sees it: an unrounded amount would be
            // debited at one precision and reported at another.
            amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

            if (amount < _minAmount)
                return Error(ApiUserErrorCode.AmountBelowLimit, $"Minimum payout amount is {_minAmount:0.##}.");

            if (amount > _maxAmount)
                return Error(ApiUserErrorCode.AmountAboveLimit,
                    $"Maximum payout amount is {_maxAmount:0.##} per transaction.");

            string paymentType = Clean(request.PaymentType).ToUpperInvariant();
            if (paymentType.Length == 0) paymentType = DefaultPaymentType;
            if (!PaymentTypes.Contains(paymentType))
                return Error(ApiUserErrorCode.PaymentTypeInvalid,
                    $"Parameter 'paymenttype' must be one of: {string.Join(", ", PaymentTypes)}.");

            // Bank name is a convenience, not a control: resolve it from the IFSC when the caller
            // omits it so reports and the beneficiary's own records read properly. A lookup that
            // fails is NOT an error — the IFSC is what the rails actually route on.
            string bankName = Clean(request.BankName);
            if (bankName.Length == 0)
            {
                try
                {
                    var (failure, branch) = await _beneficiaryFlow.CheckBranch(new CheckBranchRequest { IFSC = ifsc });
                    if (failure.EqualsIgnoreCase(StatusType.Ok)) bankName = Clean(branch.BankName);
                }
                catch (Exception ex) { _logger.Write(ex); }
            }

            return (null, accountNo, ifsc, beneficiaryName, bankName, amount, paymentType);
        }

        /// <summary>
        /// The ServiceID the payout rides on: the first active service the admin mapped to the
        /// PAYOUT report kind. Resolved from the catalogue because ServiceMaster rows are created
        /// per install, so no id can be hard-coded in a product that ships to many clients.
        /// </summary>
        private async Task<(ApiUserPayoutTransactionResponse? error, int serviceId)> ResolvePayoutServiceId(string requestId)
        {
            var services = await _serviceRepo.GetAllServices();
            var payout = services
                .Where(s => s.IsActive && !s.IsDelete && ServiceKinds.Matches(s, ServiceKinds.Payout))
                .OrderBy(s => s.ServiceID)
                .FirstOrDefault();

            if (payout == null)
                return (FailTxn(ApiUserErrorCode.PayoutUnavailable,
                    "Payout is not enabled on this platform. Contact support.",
                    requestId, ApiUserEventType.Initial), 0);

            return (null, payout.ServiceID);
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

        // ─────────────────────────────────────────────────────────────────────
        //  Response shaping
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Turns a stored payout into the payout contract. The shared builder produces the common
        /// transaction fields — so payout, DMT and recharge can never disagree about what
        /// <c>status</c>, <c>final</c> or <c>balance</c> mean — and the payout block is layered on
        /// top from the same stored row.
        /// </summary>
        private async Task<ApiUserPayoutTransactionResponse> BuildTxn(
            TransactionMaster txn,
            string eventType,
            decimal? balanceOverride = null,
            string? messageOverride = null,
            bool duplicate = false,
            string? requestIdOverride = null)
        {
            var common = await _builder.FromTransaction(
                txn, eventType, balanceOverride, messageOverride, duplicate, requestIdOverride);

            var response = new ApiUserPayoutTransactionResponse
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

                AccountNo = txn.ConsumerNumber,
                Ifsc = txn.DMT_IFSC,
                BeneficiaryName = txn.DMT_BeneficiaryName,
                BankName = txn.DMT_Bank,
                PaymentType = txn.DMT_PaymentType,
            };

            // The UTR is the bank's reference and only exists once the payout actually settled.
            // Reporting the internal provider reference while PENDING would have integrators quote
            // a number the beneficiary's bank cannot trace.
            response.Utr = ApiUserStatus.IsFinal(txn.Status) ? common.ProviderRef : string.Empty;

            // Charges are carried explicitly as well as folded into debitamount, so an integrator
            // reconciling commission never has to derive them by subtraction.
            response.Charge = txn.Charge != 0 ? txn.Charge : Math.Max(txn.DebitAmount - txn.Amount, 0m);

            return response;
        }

        /// <summary>
        /// Re-enquire a still-pending payout with the upstream provider so the answer is the
        /// bank's, not just our stored record. Skipped for a payout younger than
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
                // report the payout as we know it and let the caller poll again.
                _logger.Write(ex);
                return null;
            }
        }

        private static ApiUserPayoutTransactionResponse FailTxn(
            string errorCode, string message, string requestId, string eventType)
        {
            string status = ApiUserResponseFactory.StatusFor(errorCode);
            var response = ApiUserResponseFactory.Apply(
                new ApiUserPayoutTransactionResponse(), status, errorCode, message, requestId);
            response.Event = eventType;
            response.Final = ApiUserStatus.IsFinal(status);
            return response;
        }

        private static T Fail<T>(string errorCode, string message, string requestId)
            where T : ApiUserEnvelope, new() =>
            ApiUserResponseFactory.Fail(new T(), errorCode, message, requestId);

        private T Crash<T>(Exception ex, string requestId) where T : ApiUserEnvelope, new()
        {
            _logger.Write(ex);
            return ApiUserResponseFactory.Fail(new T(),
                ApiUserErrorCode.SystemError, "An error occurred. Please try again.", requestId);
        }

        private static string Clean(string? value) => (value ?? string.Empty).Trim();

        /// <summary>
        /// Writes the request/response pair to APIUserInitialRequestResponseLog. Purely an audit
        /// trail — a logging failure never affects the caller's result.
        /// </summary>
        private async Task LogInitial(int userId, int txnId, ApiUserPayoutTransferRequest request,
            ApiUserPayoutTransactionResponse response)
        {
            try
            {
                if (userId <= 0) return; // Unauthenticated noise is not worth an audit row.
                await _initialLog.Create(new CreateAPIUserInitialRequestResponseLogRequest
                {
                    UserID = userId,
                    TransactionMasterID = txnId,
                    APIUserRequestID = request.RequestId,
                    ConsumerNumber = Clean(request.AccountNo),
                    Request = request.RawRequest,
                    Response = JsonConvert.SerializeObject(response, Formatting.None),
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
