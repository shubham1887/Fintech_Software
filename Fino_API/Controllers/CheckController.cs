using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CheckController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public CheckController(IMessageService messageService, IConfiguration configuration)
        {
            _messageService = messageService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("CheckSms")] public async Task<APIResponse> CheckSms([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; CheckSmsRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<CheckSmsRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                string result = await _messageService.CheckSmsAsync(request.Mobile, request.Message, request.TemplateID ?? "");
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("CheckEmail")] public async Task<APIResponse> CheckEmail([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; CheckEmailRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<CheckEmailRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                string result = await _messageService.CheckEmailAsync(request.EmailID, request.Message, request.Subject ?? "Check");
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("CheckWhatsAppSms")] public async Task<APIResponse> CheckWhatsAppSms([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel; CheckWhatsAppRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<CheckWhatsAppRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
            try
            {
                string result = await _messageService.CheckWhatsAppAsync(request.WhatsAppNumber, request.Message);
                return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }

    public class CheckSmsRequest { public string Mobile { get; set; } = ""; public string Message { get; set; } = ""; public string? TemplateID { get; set; } }
    public class CheckEmailRequest { public string EmailID { get; set; } = ""; public string Message { get; set; } = ""; public string? Subject { get; set; } }
    public class CheckWhatsAppRequest { public string WhatsAppNumber { get; set; } = ""; public string Message { get; set; } = ""; }
}
