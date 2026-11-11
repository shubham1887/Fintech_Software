using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.ResponseMessageRule;
using FintechApp.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    /// <summary>
    /// Admin screen behind "Response Messages": the rules that decide what a user is told when a
    /// provider answers in its own words. Unlisted in ApiAccessPolicy, so deny-by-default keeps the
    /// whole controller with Admin/Support — these rules decide what every user of the platform
    /// reads, which is not a retailer's setting.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class ResponseMessageRuleController : ControllerBase
    {
        private readonly IResponseMessageRuleService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public ResponseMessageRuleController(IResponseMessageRuleService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("GetAll")]
        public async Task<APIResponse> GetAll([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                var data = await _service.GetAll();
                if (data.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.NoRecordFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("GetById")]
        public async Task<APIResponse> GetById([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; GetResponseMessageRuleRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<GetResponseMessageRuleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                var data = await _service.GetByID(request.ResponseMessageRuleID);
                if (data == null) return ResponseProcess.GetFailResponse(Messages.ResponseMessageRuleMessages.NotFound);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Create")]
        public async Task<APIResponse> Create([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; CreateResponseMessageRuleRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<CreateResponseMessageRuleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                fail = await _service.Create(request);
                if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(Messages.ResponseMessageRuleMessages.CreatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Update")]
        public async Task<APIResponse> Update([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; UpdateResponseMessageRuleRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<UpdateResponseMessageRuleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                fail = await _service.Update(request);
                if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(Messages.ResponseMessageRuleMessages.UpdatedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Delete")]
        public async Task<APIResponse> Delete([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; DeleteResponseMessageRuleRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<DeleteResponseMessageRuleRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                fail = await _service.Delete(request.ResponseMessageRuleID);
                if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(Messages.ResponseMessageRuleMessages.DeletedSuccessfully);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Dry-run a sample provider message through the live rules. This is what makes the screen
        /// safe to use: an operator can see exactly what a customer would be shown BEFORE a rule
        /// goes anywhere near a real transaction.
        /// </summary>
        [HttpPost("Test")]
        public async Task<APIResponse> Test([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; TestResponseMessageRequest? request;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); request = JsonConvert.DeserializeObject<TestResponseMessageRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StaticModels.StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                var data = await _service.Test(request);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
