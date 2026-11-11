using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.ProviderGateway;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    // Admin utilities for the config-driven provider gateway: live balance enquiry,
    // response-rule dry-run, and the placeholder catalogue that powers the UI helper chips.
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization(UserTypeCode.Admin)]
    public class ProviderGatewayController : ControllerBase
    {
        private readonly IProviderGatewayService _service;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public ProviderGatewayController(IProviderGatewayService service, IConfiguration configuration)
        {
            _service = service;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
        }

        [HttpPost("CheckBalance")]
        public async Task<APIResponse> CheckBalance([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            ProviderBalanceRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<ProviderBalanceRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var (fail, data) = await _service.CheckBalance(request.ProviderID);
                if (!fail.EqualsIgnoreCase(StatusType.Ok) || data == null) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // Re-enquire every active provider's balance in one call and return the per-provider outcome.
        // The dashboard's refresh button uses this: "Provider Stock" reads the stored balance on each
        // provider row, which is only as fresh as the last enquiry, so a refresh that only re-read the
        // database showed the same stale figures back. One enquiry per provider takes a moment, which
        // is why this is a deliberate button rather than something the 30-second poll does.
        [HttpPost("RefreshAllBalances")]
        public async Task<APIResponse> RefreshAllBalances([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var (fail, data) = await _service.CheckAllBalances();
                if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("TestResponse")]
        public async Task<APIResponse> TestResponse([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            TestProviderResponseRequest? request;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<TestProviderResponseRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var (fail, data) = await _service.TestResponse(request);
                if (!fail.EqualsIgnoreCase(StatusType.Ok) || data == null) return ResponseProcess.GetFailResponse(fail);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Placeholders")]
        public APIResponse Placeholders([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try { keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }

            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string failureMessage = keyDataModel.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);

            try
            {
                var data = _service.GetPlaceholders();
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
