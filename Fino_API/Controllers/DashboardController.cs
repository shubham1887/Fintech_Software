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
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public DashboardController(IDashboardService dashboardService, IConfiguration configuration)
        {
            _dashboardService = dashboardService;
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
                // "Select" is the dashboard the client asks for, whoever is looking at it — so the
                // ROLE decides which dashboard that is, here, rather than trusting the caller to pick.
                // It used to run the platform-wide report for everyone, so an API user, a client or a
                // field officer saw the whole panel's day turnover in their "Today's sale" and
                // "Failed" tiles: a freshly created account that had never transacted still showed
                // figures. (KeyData is cross-checked against the signed token by JwtAuthorization,
                // so UserTypeID here is the token's.)
                var (failureMessage, data) = await ForCaller(keyDataModel.UserTypeID, keyDataModel.UserID, request.Date);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// The dashboard that belongs to this role.
        ///
        /// Only the operator roles (admin, support, accountant) are asking "what did the PLATFORM
        /// do today" — for them the panel-wide report is the right answer, and it is the one that
        /// matches the provider-stock and network-float panels beside it. Every other role is
        /// asking about their OWN account, so they get a report scoped to the id the JWT was
        /// issued for: the distributor variant for a tier that has a downline, and the plain
        /// own-account variant for everyone else — retailer, API user, client, field officer.
        ///
        /// Mirrors TodaySummaryController.ForCaller; keep the two in step.
        /// </summary>
        private Task<(string failureMessage, Responses.Dashboard.DashboardViewModel? data)> ForCaller(
            int userTypeId, int userId, string date) => userTypeId switch
        {
            UserTypeCode.Admin or UserTypeCode.SupportUser or UserTypeCode.Accountant
                => _dashboardService.Select(userId, date),
            UserTypeCode.SuperDistributor or UserTypeCode.Distributor or UserTypeCode.SDFOS or UserTypeCode.DFOS
                => _dashboardService.SelectForDistributor(userId, date),
            _ => _dashboardService.SelectForRetailer(userId, date),
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
                var (failureMessage, data) = await _dashboardService.SelectForRetailer(keyDataModel.UserID, request.Date);
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
                var (failureMessage, data) = await _dashboardService.SelectForDistributor(keyDataModel.UserID, request.Date);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
