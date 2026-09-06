using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.PinGeneration;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PinGenerationController : ControllerBase
    {
        private readonly IPinGenerationService _pinService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public PinGenerationController(IPinGenerationService pinService, IConfiguration configuration)
        {
            _pinService = pinService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("GeneratePinOTP")]
        public async Task<APIResponse> GeneratePinOTP([FromForm] APIRequest apiRequest)
        {
            PinGenerationRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<PinGenerationRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _pinService.GeneratePinOTP(request);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("OTP sent successfully.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("ResendPinOTP")]
        public async Task<APIResponse> ResendPinOTP([FromForm] APIRequest apiRequest)
        {
            PinGenerationRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<PinGenerationRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _pinService.ResendPinOTP(request);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("OTP sent successfully.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("VerifyPinOTP")]
        public async Task<APIResponse> VerifyPinOTP([FromForm] APIRequest apiRequest)
        {
            PinVerifyRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<PinVerifyRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                var (fail, response) = await _pinService.VerifyPinOTP(request);
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("ForgotMpinGenerateOTP")]
        public async Task<APIResponse> ForgotMpinGenerateOTP([FromForm] APIRequest apiRequest)
        {
            ForgotMpinRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<ForgotMpinRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _pinService.ForgotMpinGenerateOTP(request);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse("OTP sent successfully.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
