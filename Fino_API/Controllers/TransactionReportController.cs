using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TransactionReport;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TransactionReportController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ITransactionReportService _reportService;
        private readonly IReportAnalyticsService _analytics;

        public TransactionReportController(IConfiguration configuration, ITransactionReportService reportService, IReportAnalyticsService analytics)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _reportService = reportService;
            _analytics = analytics;
        }

        /// <summary>
        /// Decrypt → validate → run one aggregate report → serialise. Every roll-up screen posts
        /// the same TxnReportRequest and receives the same wrapper shape, so the plumbing that
        /// used to be copy-pasted per endpoint lives here once.
        /// </summary>
        private async Task<APIResponse> Analytics<T>(
            APIRequest apiRequest,
            Func<TxnReportRequest, int, int, Task<(string fail, Responses.TransactionReport.AnalyticsWrapper<T> data)>> run)
        {
            KeyDataModel? keyDataModel;
            TxnReportRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await run(req ?? new TxnReportRequest(), keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        /// <summary>Same plumbing for the reports that return raw transaction rows.</summary>
        private async Task<APIResponse> Txn(
            APIRequest apiRequest,
            Func<TxnReportRequest, int, int, Task<(string fail, Responses.TransactionReport.TxnReportWrapper data)>> run)
        {
            KeyDataModel? keyDataModel;
            TxnReportRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await run(req ?? new TxnReportRequest(), keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ════════════════════════════════════════════════════════════════
        // CRITICAL — 21 fully implemented endpoints
        // ════════════════════════════════════════════════════════════════

        [HttpPost("SelectReport")]
        public async Task<APIResponse> SelectReport([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectReport(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectSummary")]
        public async Task<APIResponse> SelectSummary([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectSummary(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("Recharge")]
        public async Task<APIResponse> Recharge([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.Recharge(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("Prepaid")]
        public async Task<APIResponse> Prepaid([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.Prepaid(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("AEPS")]
        public async Task<APIResponse> AEPS([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.AEPS(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("MicroATM")]
        public async Task<APIResponse> MicroATM([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.MicroATM(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("CashDeposit")]
        public async Task<APIResponse> CashDeposit([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.CashDeposit(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("LeadGeneration")]
        public async Task<APIResponse> LeadGeneration([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.LeadGeneration(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("LiveTransaction")]
        public async Task<APIResponse> LiveTransaction([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.LiveTransaction(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("ManualSuccessFail")]
        public async Task<APIResponse> ManualSuccessFail([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.ManualSuccessFail(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("LapuwiseReport")]
        public async Task<APIResponse> LapuwiseReport([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.LapuwiseReport(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectByUserWise")]
        public async Task<APIResponse> SelectByUserWise([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectByUserWise(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectByUserWiseByServiceID")]
        public async Task<APIResponse> SelectByUserWiseByServiceID([FromForm] APIRequest apiRequest)
        {
            TxnReportByServiceRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportByServiceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectByUserWiseByServiceID(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectByUserWiseByTransactionMasterID")]
        public async Task<APIResponse> SelectByUserWiseByTransactionMasterID([FromForm] APIRequest apiRequest)
        {
            TxnReportByServiceRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportByServiceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectByUserWiseByTransactionMasterID(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SpecificUserReport")]
        public async Task<APIResponse> SpecificUserReport([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SpecificUserReport(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("MonthlySummary")]
        public async Task<APIResponse> MonthlySummary([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.MonthlySummary(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("GSTsummary")]
        public async Task<APIResponse> GSTsummary([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.GSTsummary(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("GSTOperatorWise")]
        public async Task<APIResponse> GSTOperatorWise([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.GSTOperatorWise(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("Income")]
        public async Task<APIResponse> Income([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.Income(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("AmountWiseInsightsSelect")]
        public async Task<APIResponse> AmountWiseInsightsSelect([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.AmountWiseInsightsSelect(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("StateWiseSummary")]
        public async Task<APIResponse> StateWiseSummary([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.StateWiseSummary(req, keyDataModel.UserID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // ════════════════════════════════════════════════════════════════
        // STUBS — endpoints that require separate BAL/provider integration
        // ════════════════════════════════════════════════════════════════

        [HttpPost("SelectForCMSReport")] public async Task<APIResponse> SelectForCMSReport([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForPOSReport")] public async Task<APIResponse> SelectForPOSReport([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("Payout")]
        public async Task<APIResponse> Payout([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.Payout(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForVendorPayReport")] public async Task<APIResponse> SelectForVendorPayReport([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("WalleTopUp")] public async Task<APIResponse> WalleTopUp([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("PanCard")] public async Task<APIResponse> PanCard([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForRetailerDashborad")] public async Task<APIResponse> SelectForRetailerDashborad([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectPendingTransaction")] public async Task<APIResponse> SelectPendingTransaction([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectPendingProviderTransaction")] public async Task<APIResponse> SelectPendingProviderTransaction([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // Pending Recharge queue for the admin page — recharge transactions still PENDING/PROCESS.
        [HttpPost("SelectPendingTransactionWithFilter")]
        public async Task<APIResponse> SelectPendingTransactionWithFilter([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectPendingTransactionWithFilter(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // DMT transactions still awaiting a final status.
        [HttpPost("SelectDMTPendingTransactionWithFilter")] public Task<APIResponse> SelectDMTPendingTransactionWithFilter([FromForm] APIRequest apiRequest) => Txn(apiRequest, _reportService.SelectDMTPendingTransactionWithFilter);

        // The pending queue for ANY service kind - ServiceType names it (RECHARGE / DMT / BBPS /
        // PAYOUT / AEPS / MATM / CASHDEPOSIT), blank for all of them. The two endpoints above are
        // kept because the mobile app and the B2B integrations call them by name.
        [HttpPost("SelectPendingByKind")] public Task<APIResponse> SelectPendingByKind([FromForm] APIRequest apiRequest) => Txn(apiRequest, _reportService.SelectPendingByKind);

        // Which pending queues this install has, and what is waiting in each.
        [HttpPost("SelectPendingQueues")]
        public async Task<APIResponse> SelectPendingQueues([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                // The hub is legitimately opened with no filters at all.
                string body = _encryptDecrypt.Decrypt(apiRequest.Request ?? string.Empty);
                req = string.IsNullOrWhiteSpace(body) ? new TxnReportRequest() : JsonConvert.DeserializeObject<TxnReportRequest>(body);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectPendingQueues(
                req ?? new TxnReportRequest(), keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectDMTPending")] public async Task<APIResponse> SelectDMTPending([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUPIPendingTransactionWithFilter")] public async Task<APIResponse> SelectUPIPendingTransactionWithFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUPIPending")] public async Task<APIResponse> SelectUPIPending([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectCMSOfflineDMT")] public async Task<APIResponse> SelectCMSOfflineDMT([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForFilter")] public async Task<APIResponse> SelectForFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDMTForReceipt")] public async Task<APIResponse> SelectDMTForReceipt([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectDMTForReceiptForWeb")] public async Task<APIResponse> SelectDMTForReceiptForWeb([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForReceipt")] public async Task<APIResponse> SelectForReceipt([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForBusPrint")] public async Task<APIResponse> SelectForBusPrint([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectAEPSForReceipt")] public async Task<APIResponse> SelectAEPSForReceipt([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectMicroATMForReceipt")] public async Task<APIResponse> SelectMicroATMForReceipt([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectCreditCardForReceipt")] public async Task<APIResponse> SelectCreditCardForReceipt([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectByConsumerNumber")] public async Task<APIResponse> SelectByConsumerNumber([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectChangeStatusByCallback")] public async Task<APIResponse> SelectChangeStatusByCallback([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForAdminDashborad")] public async Task<APIResponse> SelectForAdminDashborad([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // Service-wise sale — one row per service, with success / fail / pending splits.
        [HttpPost("ServicewiseSale")] public Task<APIResponse> ServicewiseSale([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.ServicewiseSale);

        [HttpPost("SelectRoleWithCommission")] public async Task<APIResponse> SelectRoleWithCommission([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("ProviderFailure")] public async Task<APIResponse> ProviderFailure([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("ProviderSale")] public async Task<APIResponse> ProviderSale([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; TxnReportRequest? req; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); var (failMsg, data) = await _reportService.ProviderSale(req, keyDataModel.UserID, keyDataModel.UserTypeID); if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg); return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data)); }

        // Manually-settled transactions, newest settlement first.
        [HttpPost("SelectPastSuccessFail")] public Task<APIResponse> SelectPastSuccessFail([FromForm] APIRequest apiRequest) => Txn(apiRequest, _reportService.SelectPastSuccessFail);

        // User-wise sale — one row per transacting user, with wallet movement and commission.
        [HttpPost("UserwiseSale")] public Task<APIResponse> UserwiseSale([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.UserwiseSale);

        // Top performers — highest sale first, ranked.
        [HttpPost("SelectTopPerformer")] public Task<APIResponse> SelectTopPerformer([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.TopPerformer);

        // Operator-wise sale — user x service x operator, with commission percentages.
        [HttpPost("OperatorSale")] public Task<APIResponse> OperatorSale([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.OperatorSale);

        [HttpPost("SelectRangewiseDMT")] public async Task<APIResponse> SelectRangewiseDMT([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        // DMT totals bucketed by ticket size, per user.
        [HttpPost("SelectUserWiseDMTRangeReport")] public Task<APIResponse> SelectUserWiseDMTRangeReport([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.UserwiseDMTRange);

        // Recharge totals bucketed by ticket size.
        [HttpPost("SelectRangewiseRecharge")] public Task<APIResponse> SelectRangewiseRecharge([FromForm] APIRequest apiRequest) => Analytics(apiRequest, _analytics.RangewiseRecharge);

        [HttpPost("SelectResendDMTTransaction")] public async Task<APIResponse> SelectResendDMTTransaction([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("ProviderFailureGraph")] public async Task<APIResponse> ProviderFailureGraph([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("PurchaseReport")] public async Task<APIResponse> PurchaseReport([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; TxnReportRequest? req; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); var (failMsg, data) = await _reportService.PurchaseReport(req, keyDataModel.UserID, keyDataModel.UserTypeID); if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg); return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data)); }

        [HttpPost("SelectForFilterByUserID")] public async Task<APIResponse> SelectForFilterByUserID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForFilterByOperatorID")] public async Task<APIResponse> SelectForFilterByOperatorID([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("Search")]
        public async Task<APIResponse> Search([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.Search(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForRechargeFilter")]
        public async Task<APIResponse> SelectForRechargeFilter([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectForRechargeFilter(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForBBPS")]
        public async Task<APIResponse> SelectForBBPS([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectForBBPS(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForROfferRechargeFilter")] public async Task<APIResponse> SelectForROfferRechargeFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForDMTFilter")]
        public async Task<APIResponse> SelectForDMTFilter([FromForm] APIRequest apiRequest)
        {
            TxnReportRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await _reportService.SelectForDMTFilter(req, keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("SelectForCreditCardFilter")] public async Task<APIResponse> SelectForCreditCardFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectSuccessPayoutTransaction")] public async Task<APIResponse> SelectSuccessPayoutTransaction([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectForRechargeCompactFilter")] public async Task<APIResponse> SelectForRechargeCompactFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectSuccessUPITransferTransaction")] public async Task<APIResponse> SelectSuccessUPITransferTransaction([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectUPITransferForFilter")] public async Task<APIResponse> SelectUPITransferForFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectAccountOpening")] public async Task<APIResponse> SelectAccountOpening([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectOTTTransferForFilter")] public async Task<APIResponse> SelectOTTTransferForFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }

        [HttpPost("SelectBusBookingTransferForFilter")] public async Task<APIResponse> SelectBusBookingTransferForFilter([FromForm] APIRequest apiRequest) { KeyDataModel? keyDataModel; try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); } catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); } if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest); string fail = keyDataModel.IsValidModalState(); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return await Task.FromResult(ResponseProcess.GetFailResponse("Feature under development.")); }
    }
}
