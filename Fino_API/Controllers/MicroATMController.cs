using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.MicroATM;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class MicroATMController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IMicroATMService _microATMService;

        public MicroATMController(IConfiguration configuration, IMicroATMService microATMService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
            _microATMService = microATMService;
        }

        [HttpPost("Initiate")]
        public async Task<APIResponse> Initiate([FromForm] APIRequest apiRequest)
        {
            InitiateImproveRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<InitiateImproveRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            if (!keyDataModel.DeviceType.Equals("Android", StringComparison.OrdinalIgnoreCase))
                return ResponseProcess.GetFailResponse("MicroATM is only available on Android platform.");

            string ipAddress = keyDataModel.IPAddress;

            var (failureMessage, data) = await _microATMService.Initiate(mainRequest, keyDataModel.UserID, ipAddress);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("TransactionStatus")]
        public async Task<APIResponse> TransactionStatus([FromForm] APIRequest apiRequest)
        {
            TransactionStatusRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TransactionStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, responseJson) = await _microATMService.TransactionStatusImprove(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(responseJson);
        }

        [HttpPost("TransactionStatusImprove")]
        public async Task<APIResponse> TransactionStatusImprove([FromForm] APIRequest apiRequest)
        {
            TransactionStatusRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<TransactionStatusRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, responseJson) = await _microATMService.TransactionStatusImprove(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(responseJson);
        }
    }
}
