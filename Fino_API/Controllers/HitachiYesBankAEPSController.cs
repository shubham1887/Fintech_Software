using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.HitachiYesBankAEPS;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class HitachiYesBankAEPSController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IHitachiYesBankAEPSService _hitachiYesBankAEPSService;

        public HitachiYesBankAEPSController(IConfiguration configuration, IHitachiYesBankAEPSService hitachiYesBankAEPSService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
            _hitachiYesBankAEPSService = hitachiYesBankAEPSService;
        }

        [HttpPost("Authentication")]
        public async Task<APIResponse> Authentication([FromForm] APIRequest apiRequest)
        {
            YesBank_AuthenticationRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_AuthenticationRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            mainRequest.IPAddress = keyDataModel.IPAddress;

            var (failureMessage, data) = await _hitachiYesBankAEPSService.Authentication(mainRequest, keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                data.Message = failureMessage;
                data.Status = "FAILURE";
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(data));
            }

            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("CashWithdrawal")]
        public async Task<APIResponse> CashWithdrawal([FromForm] APIRequest apiRequest)
        {
            YesBank_CashWithdrawalRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_CashWithdrawalRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string ipAddress = keyDataModel.IPAddress;
            string userAgent = Request.Headers["User-Agent"].ToString();

            var (failureMessage, data) = await _hitachiYesBankAEPSService.CashWithdrawal(mainRequest, keyDataModel.UserID, ipAddress, userAgent);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                data.Message = failureMessage;
                data.Status = "FAILURE";
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(data));
            }

            data.Message = "Cash withdrawal successful.";
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("BalanceEnquiry")]
        public async Task<APIResponse> BalanceEnquiry([FromForm] APIRequest apiRequest)
        {
            YesBank_BalanceEnquiryRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_BalanceEnquiryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string ipAddress = keyDataModel.IPAddress;
            string userAgent = Request.Headers["User-Agent"].ToString();

            var (failureMessage, data) = await _hitachiYesBankAEPSService.BalanceEnquiry(mainRequest, keyDataModel.UserID, ipAddress, userAgent);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                data.Message = failureMessage;
                data.Status = "FAILURE";
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(data));
            }

            data.Message = "Balance enquiry successful.";
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        [HttpPost("MiniStatement")]
        public async Task<APIResponse> MiniStatement([FromForm] APIRequest apiRequest)
        {
            YesBank_MinistatementRequest? mainRequest;
            KeyDataModel? keyDataModel;
            try
            {
                mainRequest = JsonConvert.DeserializeObject<YesBank_MinistatementRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (mainRequest == null || keyDataModel == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailResponse(fail);

            string ipAddress = keyDataModel.IPAddress;
            string userAgent = Request.Headers["User-Agent"].ToString();

            var (failureMessage, data) = await _hitachiYesBankAEPSService.MiniStatement(mainRequest, keyDataModel.UserID, ipAddress, userAgent);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                data.Message = failureMessage;
                data.Status = "FAILURE";
                return ResponseProcess.GetFailResponse(JsonConvert.SerializeObject(data));
            }

            data.Message = "Mini statement retrieved successfully.";
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }
    }
}
