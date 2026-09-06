using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.ChannelSlabMaster;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ChannelSlabMasterController : ControllerBase
    {
        private readonly IChannelSlabMasterService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public ChannelSlabMasterController(IChannelSlabMasterService service, IConfiguration configuration)
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

        [HttpPost("GetByUplineUser")]
        public async Task<APIResponse> GetByUplineUser([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetChannelSlabMasterRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetChannelSlabMasterRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetByUplineUser(request.UplineUserID);
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetDropdown")]
        public async Task<APIResponse> GetDropdown([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetChannelSlabMasterRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<GetChannelSlabMasterRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = await _service.GetDropdown(request.UplineUserID);
                if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            CreateChannelSlabMasterRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<CreateChannelSlabMasterRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Create(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ChannelSlabMasterMessages.CreatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateChannelSlabMasterRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<UpdateChannelSlabMasterRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Update(request);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ChannelSlabMasterMessages.UpdatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteChannelSlabMasterRequest? request;
            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                request = JsonConvert.DeserializeObject<DeleteChannelSlabMasterRequest>(decryptedRequest);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _service.Delete(request.ChannelSlabMasterID);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
                return ResponseProcess.GetSuccessResponse(Messages.ChannelSlabMasterMessages.DeletedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
