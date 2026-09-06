using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Tax;
using FintechApp.Requests.TransactionMaster;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TaxController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ITransactionMasterService _transactionService;

        private readonly IReportAnalyticsService _analytics;

        public TaxController(IConfiguration configuration, ITransactionMasterService transactionService, IReportAnalyticsService analytics)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _transactionService = transactionService;
            _analytics = analytics;
        }

        /// <summary>Decrypt → validate → run one aggregate report → serialise.</summary>
        private async Task<APIResponse> Report<T>(
            APIRequest apiRequest,
            Func<Requests.TransactionReport.TxnReportRequest, int, int, Task<(string fail, Responses.TransactionReport.AnalyticsWrapper<T> data)>> run)
        {
            KeyDataModel? keyDataModel;
            Requests.TransactionReport.TxnReportRequest? req;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<Requests.TransactionReport.TxnReportRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failMsg, data) = await run(req ?? new Requests.TransactionReport.TxnReportRequest(), keyDataModel.UserID, keyDataModel.UserTypeID);
            if (!string.IsNullOrEmpty(failMsg)) return ResponseProcess.GetFailResponse(failMsg);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // TDS report — aggregates RetailerTDS/DistributerTDS/SuperDistributerTDS from TransactionMaster for given month/year
        // TDS report — every transaction that deducted tax at source in the window, with the
        // retailer / distributor / super-distributor split. Takes the same date range as every
        // other report screen (the old Month/Year-only contract could not be called from the UI).
        [HttpPost("SelectTDS")]
        public Task<APIResponse> SelectTDS([FromForm] APIRequest apiRequest)
            => Report(apiRequest, _analytics.Tds);

        [HttpPost("SelectGSTDetailReportForAdmin")]
        public async Task<APIResponse> SelectGSTDetailReportForAdmin([FromForm] APIRequest apiRequest)
        {
            TaxGstAdminRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<TaxGstAdminRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            if (req.Month < 1 || req.Month > 12 || req.Year < 2000)
                return ResponseProcess.GetFailResponse("Invalid month or year.");

            var fromDate = new DateTime(req.Year, req.Month, 1);
            var toDate = new DateTime(req.Year, req.Month, DateTime.DaysInMonth(req.Year, req.Month));

            var txns = await _transactionService.GetAll(new GetTransactionRequest
            {
                UserID = req.UserID,
                Status = "SUCCESS",
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 1,
                PageSize = 10000
            });

            if (txns == null || txns.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);

            var grouped = txns.GroupBy(t => t.UserID).Select(g => new
            {
                UserID = g.Key,
                DistributerUserID = g.First().DistributerUserID,
                SuperDistributerUserID = g.First().SuperDistributerUserID,
                SuccessCount = g.Count(),
                SuccessAmt = g.Sum(t => t.Amount),
                GST = g.Sum(t => t.AdminGST + t.RetailerGST + t.DistributerGST + t.SuperDistributerGST),
                Commission = g.Sum(t => t.RetailerCommission + t.DistributerCommission + t.SuperDistributerCommission + t.AdminCommission)
            }).ToList();

            var result = new
            {
                Count = grouped.Count,
                TotalSuccessAmt = grouped.Sum(g => g.SuccessAmt),
                TotalGST = grouped.Sum(g => g.GST),
                TotalCommission = grouped.Sum(g => g.Commission),
                IsExcelDownload = req.IsExcelDownload,
                List = grouped
            };

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
        }

        // GST detail with invoice number generation — requires InvoiceNumberHistory and InvoiceSetting infrastructure (pending)
        [HttpPost("SelectGSTDetail")]
        public async Task<APIResponse> SelectGSTDetail([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "SelectGSTDetail pending: GST invoice detail with sequential invoice number generation requires IInvoiceNumberHistoryService and IInvoiceSettingService infrastructure not yet implemented."));
        }

        // GST detail by transaction with invoice numbers — same infrastructure dependency as SelectGSTDetail
        [HttpPost("SelectGSTDetailByTransaction")]
        public async Task<APIResponse> SelectGSTDetailByTransaction([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "SelectGSTDetailByTransaction pending: transaction-level GST invoice with InvoiceNumberHistory tracking not yet implemented."));
        }

        // GST invoice HTML generation (category-wise) — requires DinkToPdf, InvoiceSetting, and HTML invoice template from SettingMaster
        [HttpPost("SelectGSTCategoryWise")]
        public async Task<APIResponse> SelectGSTCategoryWise([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "SelectGSTCategoryWise pending: GST invoice PDF generation requires DinkToPdf, InvoiceSetting, and GSTInvoice HTML template from SettingMaster — not yet implemented."));
        }

        // GST invoice V2 (P2ABanking/RCM category) — same infrastructure as SelectGSTCategoryWise
        [HttpPost("SelectGSTCategoryWiseV2")]
        public async Task<APIResponse> SelectGSTCategoryWiseV2([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            return await Task.FromResult(ResponseProcess.GetFailResponse(
                "SelectGSTCategoryWiseV2 pending: P2ABanking/RCM category GST invoice PDF generation requires DinkToPdf and InvoiceSetting infrastructure — not yet implemented."));
        }
    }
}
