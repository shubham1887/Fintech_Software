using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.CashCollection;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CashCollectionController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly ICashCollectionMasterService _cashCollectionService;

        public CashCollectionController(IConfiguration configuration, ICashCollectionMasterService cashCollectionService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _cashCollectionService = cashCollectionService;
        }

        [HttpPost("Add")]
        public async Task<APIResponse> Add([FromForm] APIRequest apiRequest)
        {
            AddCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<AddCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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

            var (failureMessage, result) = await _cashCollectionService.AddCashCollection(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
        }

        [HttpPost("Reverse")]
        public async Task<APIResponse> Reverse([FromForm] APIRequest apiRequest)
        {
            ReverseCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<ReverseCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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

            var (failureMessage, result) = await _cashCollectionService.ReverseCashCollection(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
        }

        [HttpPost("SendCollectionMessage")]
        public async Task<APIResponse> SendCollectionMessage([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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

            string failureMessage = await _cashCollectionService.SendCollectionMessage(mainRequest.UserID, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject("Outstanding message send successfully"));
        }

        [HttpPost("SelectByUserID")]
        public async Task<APIResponse> SelectByUserID([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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

            var (failureMessage, data) = await _cashCollectionService.SelectUserOutstanding(mainRequest.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("OutstandingReport")]
        public async Task<APIResponse> OutstandingReport([FromForm] APIRequest apiRequest)
        {
            SelectCashCollectionRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<SelectCashCollectionRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
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

            var (failureMessage, wrapper) = await _cashCollectionService.OutstandingReport(mainRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(wrapper));
        }
    }
}
