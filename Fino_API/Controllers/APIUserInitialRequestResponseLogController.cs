using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class APIUserInitialRequestResponseLogController : ControllerBase
    {
        private readonly IAPIUserInitialRequestResponseLogService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public APIUserInitialRequestResponseLogController(IAPIUserInitialRequestResponseLogService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("Select")] public async Task<APIResponse> Select([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; SelectAPIUserLogRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<SelectAPIUserLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            // An API partner reads its own traffic only; the operator's desk reads everyone's.
            bool isAdmin = keyDataModel.UserTypeID == StaticModels.UserTypeCode.Admin || keyDataModel.UserTypeID == StaticModels.UserTypeCode.SupportUser;
            if (!isAdmin) request.UserID = keyDataModel.UserID;
            try
            {
                var data = await _service.Select(request);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetAll")] public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetAll(); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetById")] public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetByID(request.APIUserInitialRequestResponseLogID); if (data == null) return ResponseProcess.GetFailResponse(Messages.APIUserInitialRequestResponseLogMessages.NotFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetByUserID")] public async Task<APIResponse> GetByUserID([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetByUserID(request.UserID); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // The API traffic for ONE transaction — what the integrator sent us and what we replied,
        // which is what an API user's "Log" action must show instead of our upstream provider call.
        [HttpPost("GetByTransaction")] public async Task<APIResponse> GetByTransaction([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { var data = await _service.GetByTransaction(request.TransactionMasterID, keyDataModel.UserID, keyDataModel.UserTypeID); if (data == null || data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound); return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data))); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
        [HttpPost("Create")] public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; CreateAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<CreateAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { fail = await _service.Create(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.APIUserInitialRequestResponseLogMessages.CreatedSuccessfully); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Update")] public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; UpdateAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<UpdateAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { fail = await _service.Update(request); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.APIUserInitialRequestResponseLogMessages.UpdatedSuccessfully); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Delete")] public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; DeleteAPIUserInitialRequestResponseLogRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<DeleteAPIUserInitialRequestResponseLogRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try { fail = await _service.Delete(request.APIUserInitialRequestResponseLogID); if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail); return ResponseProcess.GetSuccessResponse(Messages.APIUserInitialRequestResponseLogMessages.DeletedSuccessfully); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
