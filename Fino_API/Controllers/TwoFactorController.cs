using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TwoFactor;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Authenticator-app ("Google Authenticator") two-step verification.
    ///
    /// Every action except <see cref="ClearForUser"/> works on the caller and only the caller: the
    /// user id comes from the signed token (cross-checked against KeyData by
    /// <see cref="JwtAuthorizationAttribute"/>) and no endpoint takes a target user, so there is
    /// no id to tamper with. ClearForUser is the operator's "lost the phone" path and is the one
    /// action here that names somebody else — hence the explicit admin check inside it.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TwoFactorController : ControllerBase
    {
        private readonly ITwoFactorService _twoFactorService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TwoFactorController(ITwoFactorService twoFactorService, IConfiguration configuration)
        {
            _twoFactorService = twoFactorService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        /// <summary>Whether the caller has an authenticator set up, for the Security screen.</summary>
        [HttpPost("Status")]
        public async Task<APIResponse> Status([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = Decode(apiRequest);
            if (error != null) return error;

            try
            {
                var data = await _twoFactorService.Status(keyDataModel!.UserID);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Mint a secret and hand back the QR payload. Two-step verification is NOT on yet after
        /// this call — <see cref="Confirm"/> is what switches it on.
        /// </summary>
        [HttpPost("BeginSetup")]
        public async Task<APIResponse> BeginSetup([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, error) = Decode(apiRequest);
            if (error != null) return error;

            try
            {
                var (failureMessage, data) = await _twoFactorService.BeginSetup(keyDataModel!.UserID);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok) || data == null)
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Prove the app is generating matching codes, switch two-step verification on, and return
        /// the recovery codes — the only time they are readable.
        /// </summary>
        [HttpPost("Confirm")]
        public async Task<APIResponse> Confirm([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, request, error) = Decode<ConfirmTwoFactorRequest>(apiRequest);
            if (error != null) return error;

            try
            {
                var (failureMessage, data) = await _twoFactorService.Confirm(keyDataModel!.UserID, request!);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok) || data == null)
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(
                    _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)),
                    Messages.TwoFactorMessages.EnabledSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>Switch two-step verification off. Needs the account password AND a live code.</summary>
        [HttpPost("Disable")]
        public async Task<APIResponse> Disable([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, request, error) = Decode<DisableTwoFactorRequest>(apiRequest);
            if (error != null) return error;

            try
            {
                string result = await _twoFactorService.Disable(keyDataModel!.UserID, request!);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(Messages.TwoFactorMessages.DisabledSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Clear somebody else's enrolment — the "lost the phone, cannot find the recovery codes"
        /// path. Admin only: this is the one way into an account that removes the second step
        /// without presenting a code, so it is checked here as well as by ApiAccessPolicy, which
        /// scopes the action to Admin independently of the rest of this controller.
        /// </summary>
        [HttpPost("ClearForUser")]
        public async Task<APIResponse> ClearForUser([FromForm] APIRequest apiRequest)
        {
            var (keyDataModel, request, error) = Decode<ClearTwoFactorRequest>(apiRequest);
            if (error != null) return error;

            if (keyDataModel!.UserTypeID != UserTypeCode.Admin && keyDataModel.UserTypeID != UserTypeCode.SupportUser)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.NoPermission);

            if (request!.UserID <= 0)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidData);

            try
            {
                string result = await _twoFactorService.ClearForUser(request.UserID);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(Messages.TwoFactorMessages.ClearedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // ─── Envelope decoding ───────────────────────────────────────────────

        /// <summary>Decrypt and validate the session envelope for an action that takes no body.</summary>
        private (KeyDataModel? keyData, APIResponse? error) Decode(APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null)
                return (null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return (null, ResponseProcess.GetFailResponse(fail));

            return (keyDataModel, null);
        }

        private (KeyDataModel? keyData, T? request, APIResponse? error) Decode<T>(APIRequest apiRequest) where T : class
        {
            KeyDataModel? keyDataModel;
            T? mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<T>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured));
            }

            if (keyDataModel == null || mainRequest == null)
                return (null, null, ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest));

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return (null, null, ResponseProcess.GetFailResponse(fail));

            return (keyDataModel, mainRequest, null);
        }
    }
}
