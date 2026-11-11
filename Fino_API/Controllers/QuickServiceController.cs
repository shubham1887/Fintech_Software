using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.QuickService;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// A user's own "Quick Services" shortcuts.
    ///
    /// Every action works on the caller and only the caller: the user id comes from the signed
    /// token (cross-checked against KeyData by <see cref="JwtAuthorizationAttribute"/>) and no
    /// endpoint here takes a target user, so there is no id to tamper with.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class QuickServiceController : ControllerBase
    {
        private readonly IUserQuickServiceService _quickService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public QuickServiceController(IUserQuickServiceService quickService, IConfiguration configuration)
        {
            _quickService = quickService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Select")]
        public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var (failureMessage, data) = await _quickService.Select(keyDataModel.UserID, keyDataModel.UserTypeID);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Save")]
        public async Task<APIResponse> Save([FromForm] APIRequest apiRequest)
        {
            SaveQuickServiceRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                request = JsonConvert.DeserializeObject<SaveQuickServiceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null || keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                string result = await _quickService.Save(keyDataModel.UserID, keyDataModel.UserTypeID, request);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(Messages.Common.Message.UpdatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
