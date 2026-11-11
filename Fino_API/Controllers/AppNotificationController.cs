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
    /// The notification bell, end to end: registering a device, reading the feed, marking it
    /// read, and (for the roles allowed to) composing an alert.
    ///
    /// Identity always comes from the decrypted, session-validated KeyData - never from a
    /// UserID on the request - so one user cannot read or address another's notifications.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class AppNotificationController : ControllerBase
    {
        private readonly IAppNotificationService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger = new();

        public AppNotificationController(IAppNotificationService service, IConfiguration configuration)
        {
            _service = service;
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>
        /// Who may broadcast. Admin and support address the platform; a super-distributor or
        /// distributor may address their own downline (the service clips it either way).
        /// </summary>
        private static bool CanCompose(KeyDataModel k) =>
            k.UserTypeID is UserTypeCode.Admin or UserTypeCode.SupportUser
                or UserTypeCode.SuperDistributor or UserTypeCode.Distributor;

        private KeyDataModel? ReadKey(APIRequest apiRequest)
        {
            try { return JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return null; }
        }

        private T? ReadBody<T>(APIRequest apiRequest) where T : class, new()
        {
            // An empty Request body is legitimate here (the bell asks for defaults), so a blank
            // payload becomes a default instance rather than a failure.
            if (string.IsNullOrWhiteSpace(apiRequest.Request)) return new T();
            try { return JsonConvert.DeserializeObject<T>(_encryptDecrypt.Decrypt(apiRequest.Request)) ?? new T(); }
            catch (Exception ex) { _logger.Write(ex); return null; }
        }

        // -------------------------------------------------------------------- feed

        [HttpPost("Feed")]
        public async Task<APIResponse> Feed([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            var request = ReadBody<GetNotificationFeedRequest>(apiRequest);
            if (key == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var feed = await _service.GetFeed(request, key);
                // An empty inbox is a valid answer, not "no record found" - the bell needs the
                // unread count even when there is nothing in the list.
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(feed)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("UnreadCount")]
        public async Task<APIResponse> UnreadCount([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            if (key == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                int count = await _service.GetUnreadCount(key.UserID);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new { UnreadCount = count })));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("MarkRead")]
        public async Task<APIResponse> MarkRead([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            var request = ReadBody<MarkNotificationReadRequest>(apiRequest);
            if (key == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                string result = await _service.MarkRead(request, key);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("Marked as read.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ------------------------------------------------------------------ device

        /// <summary>
        /// The app calls this on every launch and whenever Firebase rotates the token. Without
        /// it the platform has no address to push to - which is exactly why nothing arrived on
        /// Android before.
        /// </summary>
        [HttpPost("RegisterDevice")]
        public async Task<APIResponse> RegisterDevice([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            var request = ReadBody<RegisterDeviceRequest>(apiRequest);
            if (key == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                string result = await _service.RegisterDevice(request, key);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("Device registered.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // -------------------------------------------------------------------- send

        [HttpPost("Send")]
        public async Task<APIResponse> Send([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            var request = ReadBody<SendAppNotificationRequest>(apiRequest);
            if (key == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            if (!CanCompose(key)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                var (status, result) = await _service.Send(request, key);
                if (!status.EqualsIgnoreCase(StatusType.Ok) || result == null)
                    return ResponseProcess.GetFailResponse(status);

                string reach = result.PushConfigured
                    ? $"Alert sent to {result.RecipientCount} user(s); {result.DeviceCount} had a registered device."
                    : $"Alert saved for {result.RecipientCount} user(s). Push is not configured on this installation, so it will show in the app's bell only.";

                return ResponseProcess.GetSuccessResponse(reach);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>Admin's record of what was broadcast, to whom, and how much of it was read.</summary>
        [HttpPost("Sent")]
        public async Task<APIResponse> Sent([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            var request = ReadBody<GetSentNotificationsRequest>(apiRequest);
            if (key == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            if (key.UserTypeID != UserTypeCode.Admin && key.UserTypeID != UserTypeCode.SupportUser)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                var rows = await _service.GetSent(request);
                if (rows.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(rows)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Whether this install can push to a phone at all, and how many devices are registered.
        ///
        /// Exists because "the alert does not reach the phone" has one overwhelmingly common
        /// cause that is not a fault in the platform: Firebase has not been filled in for this
        /// client (server service account, and the four values in the app's hosted
        /// app-config.json). With nothing saying so, the composer looks like it is working and
        /// the tray stays empty, so it gets reported as a bug every time. The screens that send
        /// alerts read this and say it out loud instead.
        ///
        /// Admin / support only: it is a deployment fact, and they are the ones who can fix it.
        /// </summary>
        [HttpPost("PushStatus")]
        public async Task<APIResponse> PushStatus([FromForm] APIRequest apiRequest)
        {
            var key = ReadKey(apiRequest);
            if (key == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = key.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            if (key.UserTypeID != UserTypeCode.Admin && key.UserTypeID != UserTypeCode.SupportUser)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);

            try
            {
                var status = await _service.GetPushStatus();
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(status)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
