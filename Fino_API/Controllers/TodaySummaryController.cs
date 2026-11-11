using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Dashboard;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TodaySummaryController : ControllerBase
    {
        private readonly ITodaySummaryService _todaySummaryService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TodaySummaryController(ITodaySummaryService todaySummaryService, IConfiguration configuration)
        {
            _todaySummaryService = todaySummaryService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            SelectDashboardRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SelectDashboardRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                // "Select" is the summary the dashboard asks for, whoever is looking at it — so the
                // ROLE decides which report that is, here, rather than trusting the caller to pick.
                // It used to run the panel-wide report for everyone, so a retailer, a distributor or
                // an API user opened their dashboard and saw the whole platform's float and fund
                // movement as if it were their own account.
                var (failureMessage, data) = await ForCaller(keyDataModel.UserTypeID, keyDataModel.UserID, request.Date);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// The summary that belongs to this role.
        ///
        /// Only the operator roles (admin, support, accountant) are asking "what does the PLATFORM
        /// hold" — for them the panel-wide report is the right answer and matches the network-wide
        /// balance strip above it. Every other role is asking about their OWN wallet, so they get
        /// the per-account report, scoped to the id the JWT was issued for.
        ///
        /// The distributor variant adds the lines a tier with a downline needs (fund transferred
        /// down, downline reversals); every remaining role — retailer, API user, client, field
        /// officer — gets the retailer variant, which is the plain own-account view.
        /// </summary>
        private Task<(string failureMessage, List<Responses.TodaySummary.TodaySummaryViewModel> data)> ForCaller(
            int userTypeId, int userId, string date) => userTypeId switch
        {
            UserTypeCode.Admin or UserTypeCode.SupportUser or UserTypeCode.Accountant
                => _todaySummaryService.Select(userId, date),
            UserTypeCode.SuperDistributor or UserTypeCode.Distributor or UserTypeCode.SDFOS or UserTypeCode.DFOS
                => _todaySummaryService.SelectForDistributor(userId, date),
            _ => _todaySummaryService.SelectForRetailer(userId, date),
        };

        [HttpPost("SelectForRetailer")]
        public async Task<APIResponse> SelectForRetailer([FromForm] APIRequest apiRequest)
        {
            SelectDashboardRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SelectDashboardRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var (failureMessage, data) = await _todaySummaryService.SelectForRetailer(keyDataModel.UserID, request.Date);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("SelectForDistributor")]
        public async Task<APIResponse> SelectForDistributor([FromForm] APIRequest apiRequest)
        {
            SelectDashboardRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SelectDashboardRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var (failureMessage, data) = await _todaySummaryService.SelectForDistributor(keyDataModel.UserID, request.Date);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
