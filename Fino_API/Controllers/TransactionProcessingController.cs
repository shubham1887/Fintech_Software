using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TransactionProcessing;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TransactionProcessingController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ITransactionProcessingService _txnService;
        private readonly IRechargeService _rechargeService;
        private readonly IBankTransferService _bankTransferService;

        public TransactionProcessingController(IConfiguration configuration, ITransactionProcessingService txnService, IRechargeService rechargeService, IBankTransferService bankTransferService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
            _txnService = txnService;
            _rechargeService = rechargeService;
            _bankTransferService = bankTransferService;
        }

        // Shared money-out handler for DMT / Payout / VendorPay (same pipeline, different kind).
        private async Task<APIResponse> HandleBankTransfer(APIRequest apiRequest, Requests.Transaction.TransferKind kind)
        {
            Requests.Transaction.BankTransferRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<Requests.Transaction.BankTransferRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            var (failureMessage, data) = await _bankTransferService.Transfer(req, kind, k.UserID, ipAddress);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Shared provider status-check / resend handler for a single transaction.
        // Manual settlement moves money — marking a SUCCESS transaction FAILED refunds the wallet
        // and reverses commission — and takes an arbitrary TransactionID, so it must never be
        // reachable by the user who owns the transaction. Admin/Support only.
        private static bool IsSettlementAuthorized(KeyDataModel k)
            => k.UserTypeID == UserTypeCode.Admin || k.UserTypeID == UserTypeCode.SupportUser;

        private async Task<APIResponse> HandleProviderStatus(APIRequest apiRequest, string serviceType, bool isResend)
        {
            TxnSelectByIDRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = isResend
                ? await _txnService.ResendToProvider(req.ID, serviceType, k.UserID, ip)
                : await _txnService.CheckProviderStatus(req.ID, serviceType, k.UserID, ip);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse($"{serviceType} {(isResend ? "resend" : "status check")} processed.");
        }

        // Shared provider status-check handler for a batch of transactions.
        private async Task<APIResponse> HandleProviderStatusBatch(APIRequest apiRequest, string serviceType)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.CheckProviderStatusBatch(req, serviceType, k.UserID, ip);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse($"{serviceType} batch status check processed.");
        }

        // Shared provider batch-resend handler.
        private async Task<APIResponse> HandleProviderResendBatch(APIRequest apiRequest, string serviceType)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.ResendToProviderBatch(req, serviceType, k.UserID, ip);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse($"{serviceType} batch resend processed.");
        }

        // ─── CRITICAL: Select ─────────────────────────────────────────────────────────

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            TxnSelectRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (f, data) = await _txnService.Select(mainRequest, keyDataModel.UserID);
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ─── CRITICAL: SelectByID ─────────────────────────────────────────────────────

        [HttpPost("SelectByID")]
        public async Task<APIResponse> SelectByID([FromForm] APIRequest apiRequest)
        {
            TxnSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (f, data) = await _txnService.SelectByID(mainRequest);
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ─── CRITICAL: Update (UpdateStatusManually) ─────────────────────────────────

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            TxnUpdateStatusRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnUpdateStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.Update(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction status updated successfully.");
        }

        // ─── CRITICAL: UpdateManualSuccessFail ───────────────────────────────────────

        [HttpPost("UpdateManualSuccessFail")]
        public async Task<APIResponse> UpdateManualSuccessFail([FromForm] APIRequest apiRequest)
        {
            TxnUpdateStatusRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnUpdateStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.UpdateManualSuccessFail(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction updated and wallet credited.");
        }

        // ─── CRITICAL: RechargeRefundAll ─────────────────────────────────────────────

        [HttpPost("RechargeRefundAll")]
        public async Task<APIResponse> RechargeRefundAll([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.RechargeRefundAll(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("All pending recharges refunded.");
        }

        // ─── CRITICAL: CMSRefundAll ───────────────────────────────────────────────────

        [HttpPost("CMSRefundAll")]
        public async Task<APIResponse> CMSRefundAll([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.CMSRefundAll(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("CMS refund completed.");
        }

        // ─── CRITICAL: DMTTransactionRefund ──────────────────────────────────────────

        [HttpPost("DMTTransactionRefund")]
        public async Task<APIResponse> DMTTransactionRefund([FromForm] APIRequest apiRequest)
        {
            TxnBatchIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnBatchIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.DMTTransactionRefund(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("DMT transaction refunded.");
        }

        // ─── CRITICAL: AllProviderWisePerformance ─────────────────────────────────────

        [HttpPost("AllProviderWisePerformance")]
        public async Task<APIResponse> AllProviderWisePerformance([FromForm] APIRequest apiRequest)
        {
            TxnProviderWiseRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnProviderWiseRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (f, data) = await _txnService.AllProviderWisePerformance(mainRequest);
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ─── CRITICAL: TransactionResend ─────────────────────────────────────────────

        [HttpPost("TransactionResend")]
        public async Task<APIResponse> TransactionResend([FromForm] APIRequest apiRequest)
        {
            TxnSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.TransactionResend(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction resent successfully.");
        }

        // ─── CRITICAL: TransactionResendForSingleProvider ────────────────────────────

        [HttpPost("TransactionResendForSingleProvider")]
        public async Task<APIResponse> TransactionResendForSingleProvider([FromForm] APIRequest apiRequest)
        {
            TxnDMTResendRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnDMTResendRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.TransactionResendForSingleProvider(mainRequest, keyDataModel.UserID, ipAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction resent to provider.");
        }

        // ─── CRITICAL: DMTTransactionResend ──────────────────────────────────────────

        [HttpPost("DMTTransactionResend")]
        public async Task<APIResponse> DMTTransactionResend([FromForm] APIRequest apiRequest)
        {
            TxnDMTResendRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnDMTResendRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.DMTTransactionResend(mainRequest, keyDataModel.UserID, ipAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("DMT transaction resent.");
        }

        // ─── CRITICAL: DMTTransactionResendAll ───────────────────────────────────────

        [HttpPost("DMTTransactionResendAll")]
        public async Task<APIResponse> DMTTransactionResendAll([FromForm] APIRequest apiRequest)
        {
            List<TxnDMTResendRequest>? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<List<TxnDMTResendRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.DMTTransactionResendAll(mainRequest, keyDataModel.UserID, ipAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("DMT transactions resent.");
        }

        // ─── CRITICAL: DMTCMSTransactionResendAll ────────────────────────────────────

        [HttpPost("DMTCMSTransactionResendAll")]
        public async Task<APIResponse> DMTCMSTransactionResendAll([FromForm] APIRequest apiRequest)
        {
            List<TxnDMTResendRequest>? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<List<TxnDMTResendRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.DMTCMSTransactionResendAll(mainRequest, keyDataModel.UserID, ipAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("DMT-CMS transactions resent.");
        }

        // ─── CRITICAL: GetAEPSTransactionStatus ──────────────────────────────────────

        [HttpPost("GetAEPSTransactionStatus")]
        public async Task<APIResponse> GetAEPSTransactionStatus([FromForm] APIRequest apiRequest)
        {
            TxnSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.GetAEPSTransactionStatus(mainRequest, keyDataModel.UserID, ipAddress);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("AEPS status checked.");
        }

        // ─── CRITICAL: GetBBPSTransactionStatus ──────────────────────────────────────

        [HttpPost("GetBBPSTransactionStatus")]
        public async Task<APIResponse> GetBBPSTransactionStatus([FromForm] APIRequest apiRequest)
        {
            TxnSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.GetBBPSTransactionStatus(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("BBPS status checked.");
        }

        // ─── CRITICAL: GetTransactionStatus ──────────────────────────────────────────

        [HttpPost("GetTransactionStatus")]
        public async Task<APIResponse> GetTransactionStatus([FromForm] APIRequest apiRequest)
        {
            TxnSelectByIDRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TxnSelectByIDRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (mainRequest == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string result = await _txnService.GetTransactionStatus(mainRequest, keyDataModel.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction status checked.");
        }

        // ─── Remaining stubs (not in CRITICAL list) ──────────────────────────────────

        // Processing-time distribution of successful transactions (analytics, read-only)
        [HttpPost("HourlyPerformance")]
        public async Task<APIResponse> HourlyPerformance([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? k;
            try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            var data = await _txnService.HourlyPerformance();
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Average processing time over the last 12 hours in 2-hour intervals (analytics, read-only)
        [HttpPost("Last12hrPerformance")]
        public async Task<APIResponse> Last12hrPerformance([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? k;
            try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            var data = await _txnService.Last12hrPerformance();
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Average processing time for a single provider on a given date (analytics, read-only)
        [HttpPost("ProviderWisePerformance")]
        public async Task<APIResponse> ProviderWisePerformance([FromForm] APIRequest apiRequest)
        {
            TxnSingleProviderPerfRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnSingleProviderPerfRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            var data = await _txnService.ProviderWisePerformance(req);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Recharge pipeline: validate → balance check → debit wallet → dispatch to operator → settle.
        // Operator dispatch is delegated to IRechargeProviderClient (auto-refunds if no provider is configured).
        [HttpPost("Recharge")]
        public async Task<APIResponse> Recharge([FromForm] APIRequest apiRequest)
        {
            Requests.Transaction.RechargeRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<Requests.Transaction.RechargeRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            var (failureMessage, data) = await _rechargeService.Recharge(req, k.UserID, ipAddress);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            // A FAILED provider outcome is still a successful pipeline run (debit + refund happened) —
            // return the structured result so the client sees the settled status.
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Manually settle a transaction (SUCCESS finalises, FAILED/REFUND credits wallet + reverses commission)
        [HttpPost("UpdateStatusManually")]
        public async Task<APIResponse> UpdateStatusManually([FromForm] APIRequest apiRequest)
        {
            TxnUpdateStatusRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnUpdateStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            if (!IsSettlementAuthorized(k)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            string result = await _txnService.UpdateStatusManually(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Transaction status updated and settled.");
        }

        // Manually settle a transaction stuck in a provider-pending state
        [HttpPost("UpdateStatusManuallyPenidngProvider")]
        public async Task<APIResponse> UpdateStatusManuallyPenidngProvider([FromForm] APIRequest apiRequest)
        {
            TxnUpdateStatusRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnUpdateStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            if (!IsSettlementAuthorized(k)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            string result = await _txnService.UpdateStatusManuallyPendingProvider(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Provider-pending transaction settled.");
        }

        [HttpPost("Resend")] public async Task<APIResponse> Resend([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "RECHARGE", isResend: true);

        // Shift: re-dispatch a stuck recharge through an admin-chosen provider (rather than the one
        // the switching sequence picked). Moves money, so admin/support only.
        [HttpPost("ShiftRecharge")]
        public async Task<APIResponse> ShiftRecharge([FromForm] APIRequest apiRequest)
        {
            TxnShiftRequest? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnShiftRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            if (!IsSettlementAuthorized(k)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.ShiftToProvider(req.TransactionID, req.ProviderID, "RECHARGE", k.UserID, ip);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Recharge shifted to the selected provider.");
        }

        [HttpPost("ShiftAll")]
        public async Task<APIResponse> ShiftAll([FromForm] APIRequest apiRequest)
        {
            List<TxnShiftRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnShiftRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);
            if (!IsSettlementAuthorized(k)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? string.Empty;
            string result = await _txnService.ShiftToProviderBatch(req, "RECHARGE", k.UserID, ip);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("Selected recharges shifted to the provider.");
        }

        [HttpPost("StatusCheckAll")] public async Task<APIResponse> StatusCheckAll([FromForm] APIRequest apiRequest) => await HandleProviderStatusBatch(apiRequest, "RECHARGE");

        // Bulk-refund all selected pending transactions (wallet credit + commission reversal)
        [HttpPost("RefundAll")]
        public async Task<APIResponse> RefundAll([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string result = await _txnService.RefundAll(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("All pending transactions refunded.");
        }

        // Bulk-refund all selected provider-pending transactions
        [HttpPost("RefundAllProviderPenidng")]
        public async Task<APIResponse> RefundAllProviderPenidng([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string result = await _txnService.RefundAllProviderPending(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("All provider-pending transactions refunded.");
        }

        [HttpPost("ResendAll")] public async Task<APIResponse> ResendAll([FromForm] APIRequest apiRequest) => await HandleProviderResendBatch(apiRequest, "RECHARGE");

        // Bulk-mark all selected pending transactions as SUCCESS
        [HttpPost("SuccessAll")]
        public async Task<APIResponse> SuccessAll([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string result = await _txnService.SuccessAll(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("All pending transactions marked success.");
        }

        // Bulk-mark all selected provider-pending transactions as SUCCESS
        [HttpPost("SuccessAllProviderPenidng")]
        public async Task<APIResponse> SuccessAllProviderPenidng([FromForm] APIRequest apiRequest)
        {
            List<TxnBatchIDRequest>? req;
            KeyDataModel? k;
            try
            {
                k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<List<TxnBatchIDRequest>>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (k == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string f = k.IsValidModalState();
            if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f);

            string result = await _txnService.SuccessAllProviderPending(req, k.UserID);
            if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
            return ResponseProcess.GetSuccessResponse("All provider-pending transactions marked success.");
        }

        // Bank payout pipeline (validate → balance → debit → dispatch → settle; provider seam)
        [HttpPost("Payout")]
        public async Task<APIResponse> Payout([FromForm] APIRequest apiRequest)
            => await HandleBankTransfer(apiRequest, Requests.Transaction.TransferKind.Payout);

        // Domestic Money Transfer pipeline (validate → balance → debit → dispatch → settle; provider seam)
        [HttpPost("MoneyTransfer")]
        public async Task<APIResponse> MoneyTransfer([FromForm] APIRequest apiRequest)
            => await HandleBankTransfer(apiRequest, Requests.Transaction.TransferKind.DMT);

        [HttpPost("GetDMTTransctionStatus")] public async Task<APIResponse> GetDMTTransctionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "DMT", isResend: false);

        [HttpPost("GetPayoutTransactionStatus")] public async Task<APIResponse> GetPayoutTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "PAYOUT", isResend: false);

        [HttpPost("GetVendorPayTransactionStatus")] public async Task<APIResponse> GetVendorPayTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "VENDORPAY", isResend: false);

        [HttpPost("GetCreditCardTransactionStatus")] public async Task<APIResponse> GetCreditCardTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "CREDITCARD", isResend: false);

        [HttpPost("GetCashDespositTransactionStatus")] public async Task<APIResponse> GetCashDespositTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "CASHDEPOSIT", isResend: false);

        [HttpPost("GetMicroATMTransactionStatus")] public async Task<APIResponse> GetMicroATMTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "MATM", isResend: false);

        [HttpPost("GetWalleTopUpTransactionStatus")] public async Task<APIResponse> GetWalleTopUpTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "WALLETTOPUP", isResend: false);

        [HttpPost("GetDMTTransctionStatusCheckAll")] public async Task<APIResponse> GetDMTTransctionStatusCheckAll([FromForm] APIRequest apiRequest) => await HandleProviderStatusBatch(apiRequest, "DMT");

        [HttpPost("GetRechargeTransctionStatus")] public async Task<APIResponse> GetRechargeTransctionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "RECHARGE", isResend: false);

        [HttpPost("SelectTransferModeByIFSC")] public async Task<APIResponse> SelectTransferModeByIFSC([FromForm] APIRequest apiRequest) { KeyDataModel? k; try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string f = k.IsValidModalState(); if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("VerifyOTPRefund")] public async Task<APIResponse> VerifyOTPRefund([FromForm] APIRequest apiRequest) { KeyDataModel? k; try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string f = k.IsValidModalState(); if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("VerifyOTPRefundForCreditCard")] public async Task<APIResponse> VerifyOTPRefundForCreditCard([FromForm] APIRequest apiRequest) { KeyDataModel? k; try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string f = k.IsValidModalState(); if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectOperatorByConsumerNumber")] public async Task<APIResponse> SelectOperatorByConsumerNumber([FromForm] APIRequest apiRequest) { KeyDataModel? k; try { k = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (k == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string f = k.IsValidModalState(); if (!f.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(f); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("GetTransactionRefund")] public async Task<APIResponse> GetTransactionRefund([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "DMT", isResend: false);

        [HttpPost("GetUPITransactionStatus")] public async Task<APIResponse> GetUPITransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "UPI", isResend: false);

        [HttpPost("UPITransactionResendAll")] public async Task<APIResponse> UPITransactionResendAll([FromForm] APIRequest apiRequest) => await HandleProviderResendBatch(apiRequest, "UPI");

        [HttpPost("UPIStatusCheckAll")] public async Task<APIResponse> UPIStatusCheckAll([FromForm] APIRequest apiRequest) => await HandleProviderStatusBatch(apiRequest, "UPI");

        // Vendor payment pipeline (validate → balance → debit → dispatch → settle; provider seam)
        [HttpPost("VendorPay")]
        public async Task<APIResponse> VendorPay([FromForm] APIRequest apiRequest)
            => await HandleBankTransfer(apiRequest, Requests.Transaction.TransferKind.VendorPay);

        [HttpPost("GetLeadGenerationTransactionStatus")] public async Task<APIResponse> GetLeadGenerationTransactionStatus([FromForm] APIRequest apiRequest) => await HandleProviderStatus(apiRequest, "LEADGEN", isResend: false);
    }
}
