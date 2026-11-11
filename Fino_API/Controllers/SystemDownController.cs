using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.SystemDown;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The scheduled system-down window: a from/to clock window during which the platform refuses
    /// financial transactions, with a named set of services allowed to keep running.
    ///
    /// Two audiences, so two access levels. SelectPolicy / SavePolicy are the operator's and
    /// assert the admin tier here as well as through ApiAccessPolicy - a window that any role
    /// could switch on would be a way to stop the whole network trading. Status is read by every
    /// signed-in client to decide whether to show its maintenance banner, so it is open to all
    /// roles and says nothing an unauthenticated caller could use.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class SystemDownController : ControllerBase
    {
        private const string AdminOnly = "Only an administrator can change the system-down window.";

        private readonly ISystemDowntimeService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public SystemDownController(ISystemDowntimeService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        private (KeyDataModel? keyData, T? request, APIResponse? error) Decode<T>(APIRequest apiRequest) where T : class, new()
        {
            KeyDataModel? keyDataModel;
            T? mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                // Read endpoints are legitimately called with an empty body.
                string body = _encryptDecrypt.Decrypt(apiRequest.Request ?? string.Empty);
                mainRequest = string.IsNullOrWhiteSpace(body) ? new T() : JsonConvert.DeserializeObject<T>(body);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null) return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return (null, null, ResponseProcess.GetFailResponse(fail));

            return (keyDataModel, mainRequest ?? new T(), null);
        }

        private static bool IsAdmin(KeyDataModel keyData) =>
            keyData.UserTypeID == UserTypeCode.Admin || keyData.UserTypeID == UserTypeCode.SupportUser;

        private APIResponse Ok(object data) =>
            ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));

        /// <summary>The window, its hours, its message and the per-service exception list.</summary>
        [HttpPost("SelectPolicy")]
        public async Task<APIResponse> SelectPolicy([FromForm] APIRequest apiRequest)
        {
            var (keyData, _, error) = Decode<SystemDownReadRequest>(apiRequest);
            if (error != null) return error;
            if (!IsAdmin(keyData!)) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoPermission);
            try { return Ok(await _service.SelectPolicy()); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>Save the switch, the hours, the message and the exception list as one change.</summary>
        [HttpPost("SavePolicy")]
        public async Task<APIResponse> SavePolicy([FromForm] APIRequest apiRequest)
        {
            var (keyData, request, error) = Decode<SystemDownSaveRequest>(apiRequest);
            if (error != null) return error;
            if (keyData!.UserTypeID != UserTypeCode.Admin) return ResponseProcess.GetFailResponse(AdminOnly);
            try
            {
                string fail = await _service.SavePolicy(request!);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(Messages.Common.Message.SavedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Is the platform down right now, and what should the user be told? Every role, because
        /// every role needs the banner.
        /// </summary>
        [HttpPost("Status")]
        public async Task<APIResponse> Status([FromForm] APIRequest apiRequest)
        {
            var (_, _, error) = Decode<SystemDownReadRequest>(apiRequest);
            if (error != null) return error;
            try { return Ok(await _service.SelectStatus()); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
