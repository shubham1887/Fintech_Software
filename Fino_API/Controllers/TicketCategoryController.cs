using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TicketCategory;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TicketCategoryController : ControllerBase
    {
        private readonly ITicketCategoryService _ticketCategoryService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TicketCategoryController(ITicketCategoryService ticketCategoryService, IConfiguration configuration)
        {
            _ticketCategoryService = ticketCategoryService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        // Everyone raising a ticket needs to read the category list; only the admin defines it.
        private static bool IsAdmin(KeyDataModel k) => k.UserTypeID == StaticModels.UserTypeCode.Admin;

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetTicketCategoryRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketCategoryRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var category = await _ticketCategoryService.GetTicketCategoryById(getRequest.TicketCategoryID);

                if (category == null)
                    return ResponseProcess.GetFailResponse(Messages.TicketCategoryMessages.TicketCategoryNotFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(category));
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
            GetTicketCategoryRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketCategoryRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || getRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var categories = await _ticketCategoryService.GetTicketCategories(getRequest);

                if (categories == null || categories.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(categories));
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
            CreateTicketCategoryRequest? createRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                createRequest = JsonConvert.DeserializeObject<CreateTicketCategoryRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || createRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                if (!IsAdmin(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
                failureMessage = await _ticketCategoryService.CreateTicketCategory(createRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketCategoryMessages.TicketCategoryCreatedSuccessfully);
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
            UpdateTicketCategoryRequest? updateRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateTicketCategoryRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || updateRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                if (!IsAdmin(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
                failureMessage = await _ticketCategoryService.UpdateTicketCategory(updateRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketCategoryMessages.TicketCategoryUpdatedSuccessfully);
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
            DeleteTicketCategoryRequest? deleteRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<DeleteTicketCategoryRequest>(
                    _encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (keyDataModel == null || deleteRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                if (!IsAdmin(keyDataModel)) return ResponseProcess.GetFailResponse(Messages.Common.Message.Unauthorized);
                failureMessage = await _ticketCategoryService.DeleteTicketCategory(deleteRequest.TicketCategoryID);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketCategoryMessages.TicketCategoryDeletedSuccessfully);
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
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
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
                var dropdown = await _ticketCategoryService.GetTicketCategoryDropdown();

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