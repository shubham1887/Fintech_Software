using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.ProviderDetails;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ProviderDetailsController : ControllerBase
    {
        private readonly IProviderDetailsService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public ProviderDetailsController(IProviderDetailsService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetAll();
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetByProviderID")]
        public async Task<APIResponse> GetByProviderID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetProviderDetailsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetProviderDetailsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetByProviderID(request.ProviderID);
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetProviderDetailsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetProviderDetailsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetByID(request.ProviderDetailID);
                if (data == null) return ResponseProcess.GetFailResponse(Messages.ProviderDetailsMessages.NotFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateProviderDetailsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<CreateProviderDetailsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Create(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ProviderDetailsMessages.CreatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateProviderDetailsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<UpdateProviderDetailsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Update(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ProviderDetailsMessages.UpdatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteProviderDetailsRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<DeleteProviderDetailsRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Delete(request.ProviderDetailID);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ProviderDetailsMessages.DeletedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
