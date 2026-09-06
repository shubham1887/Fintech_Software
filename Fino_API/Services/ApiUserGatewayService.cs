using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Requests.Transaction;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserGatewayService"/>
    public class ApiUserGatewayService : IApiUserGatewayService
    {
        /// <summary>Marks these transactions as originating from the B2B API in reports.</summary>
        private const string ThroughApi = "API";

        private readonly IApiUserAuthService _auth;
        private readonly IApiUserResponseBuilder _builder;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IOperatorRepository _operatorRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IRechargeService _rechargeService;
        private readonly ITransactionProcessingService _processing;
        private readonly ICallbackDispatchService _callbackDispatch;
        private readonly IAPIUserInitialRequestResponseLogService _initialLog;
        private readonly Logger _logger = new();

        private readonly bool _statusLiveProbe;
        private readonly int _statusProbeMinAgeSeconds;

        public ApiUserGatewayService(
            IApiUserAuthService auth,
            IApiUserResponseBuilder builder,
            ITransactionMasterRepository txnRepo,
            IOperatorRepository operatorRepo,
            IServiceRepository serviceRepo,
            IRechargeService rechargeService,
            ITransactionProcessingService processing,
            ICallbackDispatchService callbackDispatch,
            IAPIUserInitialRequestResponseLogService initialLog,
            IConfiguration configuration)
        {
            _auth = auth;
            _builder = builder;
            _txnRepo = txnRepo;
            _operatorRepo = operatorRepo;
            _serviceRepo = serviceRepo;
            _rechargeService = rechargeService;
            _processing = processing;
            _callbackDispatch = callbackDispatch;
            _initialLog = initialLog;

            var cfg = configuration.GetSection("ApiUser:Status");
            _statusLiveProbe = cfg.GetValue("LiveProbe", true);
            _statusProbeMinAgeSeconds = Math.Max(cfg.GetValue("ProbeMinAgeSeconds", 60), 0);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Recharge — the initial request
        // ─────────────────────────────────────────────────────────────────────────

        public async Task<ApiUserTransactionResponse> Recharge(ApiUserRechargeRequest request)
        {
            int userId = 0;
            var response = ApiUserResponseFactory.FailTransaction(
                ApiUserErrorCode.SystemError, "An error occurred. Please try again.",
                request.RequestId, ApiUserEventType.Initial);
            int txnId = 0;

            try
            {
                var auth = await _auth.Authenticate(request.UserId, request.Token, request.ClientIp);
                if (!auth.Succeeded)
                {
                    response = ApiUserResponseFactory.FailTransaction(
                        auth.ErrorCode, auth.Message, request.RequestId, ApiUserEventType.Initial);
                    return response;
                }
                userId = auth.User!.UserMasterID;

                // requestid is mandatory here and nowhere else: it is the idempotency key, and
                // without it a retried HTTP call would silently become a second recharge.
                if (string.IsNullOrWhiteSpace(request.RequestId))
                {
                    response = ApiUserResponseFactory.FailTransaction(
                        ApiUserErrorCode.RequestIdRequired,
                        "Parameter 'requestid' is required and must be unique for every transaction.",
                        request.RequestId, ApiUserEventType.Initial);
                    return response;
                }

                // Idempotency. Replay the original result instead of charging twice — the safest
                // possible answer to a network retry, and it keeps the integrator's reconciliation
                // keyed on their own reference.
                var existing = await _txnRepo.GetByApiUserRequestID(userId, request.RequestId);
                if (existing != null)
                {
                    txnId = existing.TransactionMasterID;
                    response = await _builder.FromTransaction(
                        existing, ApiUserEventType.Initial, duplicate: true,
                        messageOverride: $"Duplicate requestid. Returning the original transaction ({existing.Status}).");
                    return response;
                }

                var validation = await Validate(request);
                if (validation.error != null)
                {
                    response = validation.error;
                    return response;
                }
                var op = validation.op!;

                var rechargeRequest = new RechargeRequest
                {
                    OperatorID = op.OperatorID,
                    ServiceID = op.ServiceID,
                    ConsumerNumber = request.Number.Trim(),
                    Amount = validation.amount,
                    MobileNo = string.IsNullOrWhiteSpace(request.Mobile) ? request.Number.Trim() : request.Mobile.Trim(),
                    Pin = request.Pin,
                    Through = ThroughApi,
                    APIUserRequestID = request.RequestId,
                    Param1 = request.Param1,
                    Param2 = request.Param2,
                    Param3 = request.Param3,
                    BBPS_CustomerName = request.CustomerName,
                };

                var (failureMessage, data) = await _rechargeService.Recharge(rechargeRequest, userId, request.ClientIp);

                // Rejected before a transaction existed (balance, limits, PIN, operator off…).
                // Nothing was charged, and it will never complete: FAILED / TXN_REJECTED.
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok) || data == null)
                {
                    response = ApiUserResponseFactory.FailTransaction(
                        ApiUserErrorCode.TransactionRejected, failureMessage, request.RequestId, ApiUserEventType.Initial);
                    return response;
                }

                txnId = data.TransactionMasterID;

                // Build the reply from the STORED transaction, not from the in-flight DTO, so the
                // initial reply, a later status check and the callback can never disagree. The
                // balance the recharge pipeline just read is authoritative, so it wins.
                var txn = await _txnRepo.GetByID(txnId);
                response = txn != null
                    ? await _builder.FromTransaction(txn, ApiUserEventType.Initial,
                        balanceOverride: data.ClosingBalance, messageOverride: data.Message,
                        requestIdOverride: request.RequestId)
                    : ApiUserResponseFactory.FailTransaction(
                        ApiUserErrorCode.SystemError, "Transaction was created but could not be read back.",
                        request.RequestId, ApiUserEventType.Initial);

                // Already final on the synchronous reply — push the callback too, so an integrator
                // who only listens on their callback URL is never left waiting for a settlement
                // that already happened.
                if (response.Final)
                    await _callbackDispatch.DispatchForTransaction(txnId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                response = ApiUserResponseFactory.FailTransaction(
                    ApiUserErrorCode.SystemError, "An error occurred. Please try again.",
                    request.RequestId, ApiUserEventType.Initial);
                return response;
            }
            finally
            {
                // Audit every initial request, including the rejected ones — a rejected call is
                // exactly what an integrator opens a ticket about.
                await LogInitial(userId, txnId, request, response);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Status check
        // ─────────────────────────────────────────────────────────────────────────

        public async Task<ApiUserTransactionResponse> Status(ApiUserStatusRequest request)
        {
            try
            {
                var auth = await _auth.Authenticate(request.UserId, request.Token, request.ClientIp);
                if (!auth.Succeeded)
                    return ApiUserResponseFactory.FailTransaction(
                        auth.ErrorCode, auth.Message, request.RequestId, ApiUserEventType.Status);

                int userId = auth.User!.UserMasterID;

                bool hasTxnId = int.TryParse((request.TxnId ?? string.Empty).Trim(), out int txnId) && txnId > 0;
                if (string.IsNullOrWhiteSpace(request.RequestId) && !hasTxnId)
                    return ApiUserResponseFactory.FailTransaction(
                        ApiUserErrorCode.RequestIdRequired,
                        "Send 'requestid' (your own reference) or 'txnid' (our transaction id).",
                        request.RequestId, ApiUserEventType.Status);

                // requestid first: it is the integrator's own key and the one they always have.
                TransactionMaster? txn = null;
                if (!string.IsNullOrWhiteSpace(request.RequestId))
                    txn = await _txnRepo.GetByApiUserRequestID(userId, request.RequestId);
                if (txn == null && hasTxnId)
                    txn = await _txnRepo.GetByID(txnId);

                // Scope check — one API user must never read another's transaction by id.
                if (txn == null || txn.UserID != userId)
                    return ApiUserResponseFactory.FailTransaction(
                        ApiUserErrorCode.TransactionNotFound,
                        "No transaction found for the given requestid / txnid.",
                        request.RequestId, ApiUserEventType.Status);

                txn = await ProbeIfPending(txn, userId, request.ClientIp) ?? txn;

                return await _builder.FromTransaction(
                    txn, ApiUserEventType.Status, requestIdOverride: request.RequestId);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ApiUserResponseFactory.FailTransaction(
                    ApiUserErrorCode.SystemError, "An error occurred. Please try again.",
                    request.RequestId, ApiUserEventType.Status);
            }
        }

        /// <summary>
        /// Re-enquire a still-pending transaction with the upstream provider so the answer is the
        /// operator's, not just our stored record. Skipped for a transaction younger than
        /// ProbeMinAgeSeconds (the provider has not decided yet, and a poll-happy integrator would
        /// otherwise hammer the upstream) or with no provider assigned. Returns the refreshed row.
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
                // report the transaction as we know it and let the caller poll again.
                _logger.Write(ex);
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Balance / operators
        // ─────────────────────────────────────────────────────────────────────────

        public async Task<ApiUserBalanceResponse> Balance(ApiUserQueryRequest request)
        {
            try
            {
                var auth = await _auth.Authenticate(request.UserId, request.Token, request.ClientIp);
                if (!auth.Succeeded)
                    return ApiUserResponseFactory.Fail(new ApiUserBalanceResponse(), auth.ErrorCode, auth.Message, request.RequestId);

                return await _builder.Balance(auth.User!.UserMasterID, request.RequestId);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ApiUserResponseFactory.Fail(new ApiUserBalanceResponse(),
                    ApiUserErrorCode.SystemError, "An error occurred. Please try again.", request.RequestId);
            }
        }

        public async Task<ApiUserOperatorListResponse> Operators(ApiUserQueryRequest request)
        {
            try
            {
                var auth = await _auth.Authenticate(request.UserId, request.Token, request.ClientIp);
                if (!auth.Succeeded)
                    return ApiUserResponseFactory.Fail(new ApiUserOperatorListResponse(), auth.ErrorCode, auth.Message, request.RequestId);

                return await _builder.Operators(request.RequestId);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ApiUserResponseFactory.Fail(new ApiUserOperatorListResponse(),
                    ApiUserErrorCode.SystemError, "An error occurred. Please try again.", request.RequestId);
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parameter validation for /recharge. Returns the resolved operator, or the exact
        /// contract error to hand back — one place, so the error codes stay consistent.
        /// </summary>
        private async Task<(ApiUserTransactionResponse? error, OperatorMaster? op, decimal amount)> Validate(
            ApiUserRechargeRequest request)
        {
            ApiUserTransactionResponse Error(string code, string message) =>
                ApiUserResponseFactory.FailTransaction(code, message, request.RequestId, ApiUserEventType.Initial);

            if (string.IsNullOrWhiteSpace(request.Number))
                return (Error(ApiUserErrorCode.NumberRequired, "Parameter 'number' is required."), null, 0);

            if (!decimal.TryParse((request.Amount ?? string.Empty).Trim(), out decimal amount) || amount <= 0)
                return (Error(ApiUserErrorCode.AmountInvalid, "Parameter 'amount' must be a number greater than zero."), null, 0);

            if (string.IsNullOrWhiteSpace(request.Operator))
                return (Error(ApiUserErrorCode.OperatorRequired, "Parameter 'operator' is required."), null, 0);

            var op = await _operatorRepo.GetOperatorByOpcode(request.Operator.Trim());
            if (op == null || !op.IsActive)
                return (Error(ApiUserErrorCode.OperatorInvalid,
                    $"Unknown or inactive operator '{request.Operator.Trim()}'. Call /api/v1/operators for valid codes."), null, 0);

            return (null, op, amount);
        }

        /// <summary>
        /// Writes the initial request/response pair to APIUserInitialRequestResponseLog. Purely an
        /// audit trail — a logging failure never affects the caller's result.
        /// </summary>
        private async Task LogInitial(int userId, int txnId, ApiUserRechargeRequest request, ApiUserTransactionResponse response)
        {
            try
            {
                if (userId <= 0) return; // Unauthenticated noise is not worth an audit row.
                await _initialLog.Create(new CreateAPIUserInitialRequestResponseLogRequest
                {
                    UserID = userId,
                    TransactionMasterID = txnId,
                    APIUserRequestID = request.RequestId,
                    ConsumerNumber = request.Number ?? string.Empty,
                    Request = request.RawRequest,
                    Response = JsonConvert.SerializeObject(response, Formatting.None),
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
