using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Service;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public ServiceController(IServiceService serviceService, IConfiguration configuration)
        {
            _serviceService = serviceService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetServiceRequest? getServiceRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                getServiceRequest = JsonConvert.DeserializeObject<GetServiceRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getServiceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var service = await _serviceService.GetServiceById(getServiceRequest.ServiceID);

                if (service == null)
                    return ResponseProcess.GetFailResponse(Messages.ServiceMessages.ServiceNotFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(service));
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
            GetServiceRequest? getServiceRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                getServiceRequest = JsonConvert.DeserializeObject<GetServiceRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getServiceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var services = await _serviceService.GetServices(getServiceRequest);

                if (services == null || services.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(services));
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
            CreateServiceRequest? createServiceRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                createServiceRequest = JsonConvert.DeserializeObject<CreateServiceRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || createServiceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _serviceService.CreateService(createServiceRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.ServiceMessages.ServiceCreatedSuccessfully);
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
            UpdateServiceRequest? updateServiceRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                updateServiceRequest = JsonConvert.DeserializeObject<UpdateServiceRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || updateServiceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _serviceService.UpdateService(updateServiceRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.ServiceMessages.ServiceUpdatedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>
        /// Save the display order of the whole service grid. Admin-only by deny-by-default: the
        /// action name starts with a write verb ("Update") and Service is a reference master, so
        /// ApiAccessPolicy already restricts it without a special entry.
        /// </summary>
        [HttpPost("UpdateSequence")]
        public async Task<APIResponse> UpdateSequence([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            UpdateServiceSequenceRequest? sequenceRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                sequenceRequest = JsonConvert.DeserializeObject<UpdateServiceSequenceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || sequenceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _serviceService.UpdateServiceSequence(sequenceRequest);
                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.ServiceMessages.ServiceUpdatedSuccessfully);
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
            DeleteServiceRequest? deleteServiceRequest;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);

                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                deleteServiceRequest = JsonConvert.DeserializeObject<DeleteServiceRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || deleteServiceRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                failureMessage = await _serviceService.DeleteService(deleteServiceRequest.ServiceID);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.ServiceMessages.ServiceDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("GetCatalog")]
        public async Task<APIResponse> GetCatalog([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                // Admin sees the full catalog (hidden + inactive included); everyone else only
                // what is meant for display.
                bool includeHidden = keyDataModel.UserTypeID == StaticModels.UserTypeCode.Admin;
                var catalog = await _serviceService.GetServiceCatalog(includeHidden, keyDataModel.UserID);

                if (catalog == null || catalog.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(catalog));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        [HttpPost("GetDropdown")]
        public async Task<APIResponse> GetDropdown([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;

            try
            {
                string decryptedKeyData = _encryptDecrypt.Decrypt(apiRequest.KeyData);
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(decryptedKeyData);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var dropdown = await _serviceService.GetServiceDropdown();

                if (dropdown == null || dropdown.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(dropdown));
                return ResponseProcess.GetSuccessResponse(encryptedResponse);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}