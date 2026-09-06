using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.UserAccounting;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization(UserTypeCode.Admin)] // Admin-only (menu: UserwiseAccounting = role 1)
    public class UserAccountingController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IUserAccountingService _service;

        public UserAccountingController(IConfiguration configuration, IUserAccountingService service)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _service = service;
        }

        // Per-user accounting reconciliation: Opening/Purchase/Sale/Commission/Surcharge/Adjustments/Closing/Difference
        [HttpPost("UserwiseAccounting")]
        public async Task<APIResponse> UserwiseAccounting([FromForm] APIRequest apiRequest)
        {
            UserwiseAccountingRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UserwiseAccountingRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _service.UserwiseAccounting(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Failed transactions created on a past date but adjusted on the selected date (refund reflected today)
        [HttpPost("SelectForFilterByOldAmountAdjustment")]
        public async Task<APIResponse> SelectForFilterByOldAmountAdjustment([FromForm] APIRequest apiRequest)
        {
            UserAccountingCompRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UserAccountingCompRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _service.SelectForFilterByOldAmountAdjustment(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Failed transactions created on the selected date but adjusted on a different (later) date
        [HttpPost("SelectForFilterByNewAmountAdjustment")]
        public async Task<APIResponse> SelectForFilterByNewAmountAdjustment([FromForm] APIRequest apiRequest)
        {
            UserAccountingCompRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UserAccountingCompRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _service.SelectForFilterByNewAmountAdjustment(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Transactions whose status was manually changed on the selected date
        [HttpPost("ManualSuccessFail")]
        public async Task<APIResponse> ManualSuccessFail([FromForm] APIRequest apiRequest)
        {
            UserAccountingCompRequest? req;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                req = JsonConvert.DeserializeObject<UserAccountingCompRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            var (failureMessage, data) = await _service.ManualSuccessFail(req, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(failureMessage);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
    }
}
