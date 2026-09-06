using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.LoginHistory;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class LoginHistoryController : ControllerBase
    {
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public LoginHistoryController(ILoginHistoryService loginHistoryService, IConfiguration configuration)
        {
            _loginHistoryService = loginHistoryService;
            _logger = new Logger();
            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetLoginHistoryRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetLoginHistoryRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var result = await _loginHistoryService.GetAll(request);
                if (result == null || result.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(result));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateLoginHistoryRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<CreateLoginHistoryRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            var validator = new CreateLoginHistoryRequestValidator();
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return ResponseProcess.GetFailResponse(validationResult.Errors[0].ErrorMessage);

            try
            {
                string result = await _loginHistoryService.Create(request);
                if (!result.EqualsIgnoreCase(StatusType.Ok))
                    return ResponseProcess.GetFailResponse(result);

                return ResponseProcess.GetSuccessResponse(Messages.Common.Message.SavedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
