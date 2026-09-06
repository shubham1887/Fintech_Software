using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TransactionReport;
using FintechApp.Responses;
using FintechApp.Responses.TransactionReport;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The two cross-tab sale grids: sale per day split by service kind, and yesterday-vs-today
    /// sale per user. Both are aggregates, so they delegate to <see cref="IReportAnalyticsService"/>,
    /// which scopes the data to the caller's own branch of the network.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TransactionWiseReportController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IReportAnalyticsService _analytics;

        public TransactionWiseReportController(IConfiguration configuration, IReportAnalyticsService analytics)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _analytics = analytics;
        }

        // Daily sale, one column per service kind (Mobile / DTH / Postpaid / Electricity / DMT / AEPS / PG).
        [HttpPost("SelectOperatorWisesUserWiseReport")]
        public Task<APIResponse> SelectOperatorWisesUserWiseReport([FromForm] APIRequest apiRequest)
            => Run(apiRequest, _analytics.DailyServiceSale);

        // Yesterday vs today sale, per user, split by service kind.
        [HttpPost("SelectUserWiseOperatorWiseSellReport")]
        public Task<APIResponse> SelectUserWiseOperatorWiseSellReport([FromForm] APIRequest apiRequest)
            => Run(apiRequest, _analytics.UserwiseServiceSell);

        private async Task<APIResponse> Run<T>(
            APIRequest apiRequest,
            Func<TxnReportRequest, int, int, Task<(string fail, AnalyticsWrapper<T> data)>> run)
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
    }
}
