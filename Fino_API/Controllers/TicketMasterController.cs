using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.TicketMaster;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class TicketMasterController : ControllerBase
    {
        private readonly ITicketMasterService _ticketMasterService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public TicketMasterController(ITicketMasterService ticketMasterService, IConfiguration configuration)
        {
            _ticketMasterService = ticketMasterService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            GetTicketMasterRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketMasterRequest>(
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
                var ticket = await _ticketMasterService.GetTicketMasterById(getRequest.TicketMasterID);

                if (ticket == null)
                    return ResponseProcess.GetFailResponse(Messages.TicketMasterMessages.TicketNotFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(ticket));
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
            GetTicketMasterRequest? getRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                getRequest = JsonConvert.DeserializeObject<GetTicketMasterRequest>(
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
                var tickets = await _ticketMasterService.GetTicketMasters(getRequest);

                if (tickets == null || tickets.Count == 0)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(tickets));
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
            CreateTicketMasterRequest? createRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                createRequest = JsonConvert.DeserializeObject<CreateTicketMasterRequest>(
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
                failureMessage = await _ticketMasterService.CreateTicketMaster(createRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketCreatedSuccessfully);
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
            UpdateTicketMasterRequest? updateRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                updateRequest = JsonConvert.DeserializeObject<UpdateTicketMasterRequest>(
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
                failureMessage = await _ticketMasterService.UpdateTicketMaster(updateRequest);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketUpdatedSuccessfully);
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
            DeleteTicketMasterRequest? deleteRequest;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(
                    _encryptDecrypt.Decrypt(apiRequest.KeyData));
                deleteRequest = JsonConvert.DeserializeObject<DeleteTicketMasterRequest>(
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
                failureMessage = await _ticketMasterService.DeleteTicketMaster(deleteRequest.TicketMasterID);

                if (!failureMessage.EqualsIgnoreCase(StaticModels.StatusType.Ok))
                    return ResponseProcess.GetFailResponse(failureMessage);

                return ResponseProcess.GetSuccessResponse(Messages.TicketMasterMessages.TicketDeletedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}