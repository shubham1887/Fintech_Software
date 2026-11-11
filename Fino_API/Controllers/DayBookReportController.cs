using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.DayBookReport;
using FintechApp.Requests.FundTransaction;
using FintechApp.Requests.Operator;
using FintechApp.Requests.ProviderMaster;
using FintechApp.Requests.TransactionMaster;
using FintechApp.Responses;
using FintechApp.Responses.DayBookReport;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class DayBookReportController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IFundTransactionService _fundTransactionService;
        private readonly ITransactionMasterService _transactionService;
        private readonly IProviderMasterService _providerService;
        private readonly IOperatorService _operatorService;
        private readonly IServiceService _serviceService;

        private readonly IReportAnalyticsService _analytics;

        public DayBookReportController(
            IConfiguration configuration,
            IFundTransactionService fundTransactionService,
            ITransactionMasterService transactionService,
            IProviderMasterService providerService,
            IOperatorService operatorService,
            IServiceService serviceService, IReportAnalyticsService analytics)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _fundTransactionService = fundTransactionService;
            _transactionService = transactionService;
            _providerService = providerService;
            _operatorService = operatorService;
            _serviceService = serviceService;
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

        // User daybook — wallet fund transactions for the current user (sender or receiver)
        // Operator-wise daybook: every transaction in the window, grouped by operator and split
        // into success / failed / pending with the commission earned on each. Scoped to the
        // caller's own branch of the network by the analytics service.
        [HttpPost("Select")]
        public Task<APIResponse> Select([FromForm] APIRequest apiRequest)
            => Report(apiRequest, _analytics.Daybook);

        // Profit & loss: opening / purchase / sale / closing, every commission tier, the GST on
        // both sides, the margin between them and the net profit. GroupBy chooses the granularity
        // (Date, Service, Operator or User); Date is the default.
        //
        // It lives on this controller because it is the daybook's figures read as a P&L, and
        // because this controller is already granted to the accounting desk - the role the report
        // was asked for.
        [HttpPost("ProfitLoss")]
        public Task<APIResponse> ProfitLoss([FromForm] APIRequest apiRequest)
            => Report(apiRequest, _analytics.ProfitLoss);

        // Provider-wise daybook (Admin only) — aggregates TransactionMaster by Provider+Operator:
        // hits/amounts per status (Success/Failed/Pending), commissions and provider profit.
        [HttpPost("ProviderDaybook")]
        public async Task<APIResponse> ProviderDaybook([FromForm] APIRequest apiRequest)
        {
            ProviderDayBookRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<ProviderDayBookRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            // Provider profit is sensitive — Admin (UserTypeID 1) only
            if (keyDataModel.UserTypeID != 1) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            var wrapper = await BuildProviderDaybook(req, providerIDFilter: req.ProviderID);
            if (wrapper.list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }

        // Provider-wise daybook filtered to a single ProviderID — operator breakdown for that provider.
        [HttpPost("SelectByProviderID")]
        public async Task<APIResponse> SelectByProviderID([FromForm] APIRequest apiRequest)
        {
            ProviderDayBookRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<ProviderDayBookRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            if (req.ProviderID <= 0) return ResponseProcess.GetFailResponse("ProviderID is required.");

            var wrapper = await BuildProviderDaybook(req, providerIDFilter: req.ProviderID);
            if (wrapper.list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }

        // Shared aggregation: groups transactions in the date range by Provider+Operator with a Total row.
        private async Task<ProviderDayBookWrapper> BuildProviderDaybook(ProviderDayBookRequest req, int providerIDFilter)
        {
            var wrapper = new ProviderDayBookWrapper { IsExcelDownload = req.IsExcelDownload };

            DateTime? fromDate = DateTime.TryParse(req.FromDate, out var fd) ? fd.Date : null;
            DateTime? toDate = DateTime.TryParse(req.ToDate, out var td) ? td.Date.AddDays(1) : null;

            var txns = await _transactionService.GetAll(new GetTransactionRequest
            {
                UserID = 0,
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = 1,
                PageSize = int.MaxValue
            });

            if (providerIDFilter > 0)
                txns = txns.Where(t => t.ProviderID == providerIDFilter).ToList();
            // Optional service / operator / entity narrowing (drives the daybook filter bar).
            if (req.ServiceID > 0)
                txns = txns.Where(t => t.ServiceID == req.ServiceID).ToList();
            if (req.OperatorID > 0)
                txns = txns.Where(t => t.OperatorID == req.OperatorID).ToList();
            if (req.UserID > 0)
                txns = txns.Where(t => t.UserID == req.UserID).ToList();

            if (txns.Count == 0) return wrapper;

            // Name lookups
            var providers = await _providerService.GetAll(new GetProviderMasterRequest { IsActive = true });
            var providerNames = providers.GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
            var operators = await _operatorService.GetOperatorDropdown();
            var operatorNames = operators.GroupBy(o => o.OperatorID).ToDictionary(g => g.Key, g => g.First().OperatorName);
            var services = await _serviceService.GetServiceDropdown();
            var serviceNames = services.GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);

            // Group by provider + service + operator so the client can render ONE provider row and
            // drill into its per-service / per-operator sale on demand.
            var grouped = txns
                .GroupBy(t => new { t.ProviderID, t.ServiceID, t.OperatorID })
                .Select(g =>
                {
                    var success = g.Where(x => x.Status == StatusType.Success).ToList();
                    var failed = g.Where(x => x.Status == StatusType.Failed).ToList();
                    var pending = g.Where(x => x.Status == StatusType.Pending).ToList();
                    decimal successAmount = success.Sum(x => x.Amount);
                    // AdminProfit is stamped by the recharge pipeline only; the bank-transfer rail
                    // writes AdminCommission (the same net figure) and leaves AdminProfit at 0.
                    decimal profit = success.Sum(x => x.AdminProfit != 0m ? x.AdminProfit : x.AdminCommission);
                    return new ProviderDayBookViewModel
                    {
                        ProviderID = g.Key.ProviderID,
                        ProviderName = providerNames.TryGetValue(g.Key.ProviderID, out var pn) ? pn : g.Key.ProviderID.ToString(),
                        ServiceID = g.Key.ServiceID,
                        ServiceName = serviceNames.TryGetValue(g.Key.ServiceID, out var sn) ? sn : g.Key.ServiceID.ToString(),
                        OperatorID = g.Key.OperatorID,
                        Operator = operatorNames.TryGetValue(g.Key.OperatorID, out var on) ? on : g.Key.OperatorID.ToString(),
                        TotalHits = g.Count(),
                        TotalAmount = g.Sum(x => x.Amount),
                        SuccessHits = success.Count,
                        SuccessAmount = successAmount,
                        FailedHits = failed.Count,
                        FailedAmount = failed.Sum(x => x.Amount),
                        PendingHits = pending.Count,
                        PendingAmount = pending.Sum(x => x.Amount),
                        AdminCommission = success.Sum(x => x.AdminCommission),
                        RetailerCommission = success.Sum(x => x.RetailerCommission),
                        DistributerCommission = success.Sum(x => x.DistributerCommission),
                        SuperDistributerCommission = success.Sum(x => x.SuperDistributerCommission),
                        ProviderCommission = success.Sum(x => x.ProviderCommission),
                        Profit = profit,
                        Percentage = successAmount != 0 ? Math.Round(profit / successAmount * 100, 2) : 0
                    };
                })
                .OrderBy(x => x.ProviderName).ThenBy(x => x.Operator)
                .ToList();

            // Total row
            decimal totalSuccessAmount = grouped.Sum(x => x.SuccessAmount);
            decimal totalProfit = grouped.Sum(x => x.Profit);
            grouped.Add(new ProviderDayBookViewModel
            {
                Operator = "Total",
                TotalHits = grouped.Sum(x => x.TotalHits),
                TotalAmount = grouped.Sum(x => x.TotalAmount),
                SuccessHits = grouped.Sum(x => x.SuccessHits),
                SuccessAmount = totalSuccessAmount,
                FailedHits = grouped.Sum(x => x.FailedHits),
                FailedAmount = grouped.Sum(x => x.FailedAmount),
                PendingHits = grouped.Sum(x => x.PendingHits),
                PendingAmount = grouped.Sum(x => x.PendingAmount),
                AdminCommission = grouped.Sum(x => x.AdminCommission),
                RetailerCommission = grouped.Sum(x => x.RetailerCommission),
                DistributerCommission = grouped.Sum(x => x.DistributerCommission),
                SuperDistributerCommission = grouped.Sum(x => x.SuperDistributerCommission),
                ProviderCommission = grouped.Sum(x => x.ProviderCommission),
                Profit = totalProfit,
                Percentage = totalSuccessAmount != 0 ? Math.Round(totalProfit / totalSuccessAmount * 100, 2) : 0
            });

            wrapper.list = grouped;
            wrapper.Count = grouped.Count;
            return wrapper;
        }
    }
}
