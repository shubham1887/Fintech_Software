using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.AppNotification;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The original "App Alert" endpoint, kept so already-deployed panels and API users do not
    /// break. It is now a thin alias over <see cref="AppNotificationController"/>: the alert is
    /// stored, scoped to the sender's downline, and pushed over FCM HTTP v1.
    ///
    /// New callers should use AppNotification/Send, which accepts an explicit audience (and an
    /// explicit user list) instead of the single "UserTypeID, where 0 means everyone" field that
    /// caused alerts to reach the whole platform.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class PushNotificationsController : ControllerBase
    {
        private readonly IAppNotificationService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger = new();

        public PushNotificationsController(IAppNotificationService service, IConfiguration configuration)
        {
            _service = service;
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        private static bool CanCompose(KeyDataModel k) =>
            k.UserTypeID is UserTypeCode.Admin or UserTypeCode.SupportUser
                or UserTypeCode.SuperDistributor or UserTypeCode.Distributor;

        [HttpPost("SendPushNotification")]
        public async Task<APIResponse> SendPushNotification([FromForm] APIRequest apiRequest)
        {
            SendAppNotificationRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SendAppNotificationRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            if (!CanCompose(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                var (status, result) = await _service.Send(request, keyDataModel);
                if (!status.EqualsIgnoreCase(StatusType.Ok) || result == null)
                    return ResponseProcess.GetFailResponse(status);

                return ResponseProcess.GetSuccessResponse(
                    $"Alert sent to {result.RecipientCount} user(s); {result.DeviceCount} had a registered device.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
