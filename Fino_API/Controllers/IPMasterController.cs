using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.IPMaster;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class IPMasterController : ControllerBase
    {
        private readonly IIPMasterService _ipMasterService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public IPMasterController(IIPMasterService ipMasterService, IConfiguration configuration)
        {
            _ipMasterService = ipMasterService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetIPMasterRequest? getIPMasterRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                getIPMasterRequest = JsonConvert.DeserializeObject<GetIPMasterRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getIPMasterRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var ipMaster = await _ipMasterService.GetIPMasterById(getIPMasterRequest.IpMasterID);

                if (ipMaster == null)
                    return ResponseProcess.GetFailResponse(Messages.IPMasterMessages.IPMasterNotFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(ipMaster));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetIPMasterRequest? getIPMasterRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                getIPMasterRequest = JsonConvert.DeserializeObject<GetIPMasterRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getIPMasterRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var ipMasters = await _ipMasterService.GetIPMasters(getIPMasterRequest);

                if (ipMasters == null || ipMasters.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(ipMasters));
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
            CreateIPMasterRequest? createIPMasterRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                createIPMasterRequest = JsonConvert.DeserializeObject<CreateIPMasterRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || createIPMasterRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _ipMasterService.CreateIPMaster(createIPMasterRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.IPMasterMessages.IPMasterCreatedSuccessfully);
            }
            catch (ArgumentException ex)
            {
                return ResponseProcess.GetFailResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateIPMasterRequest? updateIPMasterRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                updateIPMasterRequest = JsonConvert.DeserializeObject<UpdateIPMasterRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || updateIPMasterRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _ipMasterService.UpdateIPMaster(updateIPMasterRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.IPMasterMessages.IPMasterUpdatedSuccessfully);
            }
            catch (ArgumentException ex)
            {
                return ResponseProcess.GetFailResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            DeleteIPMasterRequest? deleteIPMasterRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                deleteIPMasterRequest = JsonConvert.DeserializeObject<DeleteIPMasterRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || deleteIPMasterRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _ipMasterService.DeleteIPMaster(deleteIPMasterRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.IPMasterMessages.IPMasterDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}