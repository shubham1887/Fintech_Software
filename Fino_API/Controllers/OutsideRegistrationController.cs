using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.OutsideRegistration;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutsideRegistrationController : ControllerBase
    {
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;
        private readonly IStateMasterService _stateMasterService;
        private readonly IUserTypeMasterService _userTypeMasterService;
        private readonly IUserService _userService;
        private readonly ISettingMasterService _settingMasterService;
        private readonly IPendingRegistrationService _registrationService;

        public OutsideRegistrationController(
            IConfiguration configuration,
            IStateMasterService stateMasterService,
            IUserTypeMasterService userTypeMasterService,
            IUserService userService,
            ISettingMasterService settingMasterService,
            IPendingRegistrationService registrationService)
        {
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(configuration["Encryption:Key"] ?? string.Empty, configuration["Encryption:IV"] ?? string.Empty);
            _stateMasterService = stateMasterService;
            _userTypeMasterService = userTypeMasterService;
            _userService = userService;
            _settingMasterService = settingMasterService;
            _registrationService = registrationService;
        }

        // Public — no auth required
        [HttpPost("SelectState")]
        public async Task<APIResponse> SelectState([FromForm] APIRequest apiRequest)
        {
            var list = await _stateMasterService.GetAll();
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
        }

        // Public — returns all user types available for outside registration
        [HttpPost("SelectRoleForOutside")]
        public async Task<APIResponse> SelectRoleForOutside([FromForm] APIRequest apiRequest)
        {
            var list = await _userTypeMasterService.GetAll();
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
        }

        // Public — returns user types available for sign-up (referral code accepted but no filter applied in new system)
        [HttpPost("SelectRoleForSignUp")]
        public async Task<APIResponse> SelectRoleForSignUp([FromForm] APIRequest apiRequest)
        {
            RoleForSignUpRequest? req = null;
            try
            {
                if (!string.IsNullOrEmpty(apiRequest.Request))
                    req = JsonConvert.DeserializeObject<RoleForSignUpRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); }

            var list = await _userTypeMasterService.GetAll();
            if (list == null || list.Count == 0) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(list));
        }

        // Public — outside sign-up registration. Requires the mobile to be OTP-verified first;
        // rejects duplicates (mobile/email/PAN/Aadhaar/GST) against existing users and the pending queue.
        [HttpPost("Insert")]
        public async Task<APIResponse> Insert([FromForm] APIRequest apiRequest)
        {
            OutsideRegisterRequest? req;
            try { req = JsonConvert.DeserializeObject<OutsideRegisterRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _registrationService.Register(req);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(true.ToString(), "Registration submitted. Our team will review and approve your account shortly.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // Public — returns helpline number from SettingMaster (Key = "HelplineNumber")
        [HttpPost("SelectHelplineNumber")]
        public async Task<APIResponse> SelectHelplineNumber([FromForm] APIRequest apiRequest)
        {
            var settings = await _settingMasterService.GetAll();
            var setting = settings?.FirstOrDefault(s => s.Key.Equals("HelplineNumber", StringComparison.OrdinalIgnoreCase));
            if (setting == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(setting.Value);
        }

        // Public — returns Play Store URL from SettingMaster (Key = "PlayStoreURL")
        [HttpPost("SelectPlayStoreURL")]
        public async Task<APIResponse> SelectPlayStoreURL([FromForm] APIRequest apiRequest)
        {
            var settings = await _settingMasterService.GetAll();
            var setting = settings?.FirstOrDefault(s => s.Key.Equals("PlayStoreURL", StringComparison.OrdinalIgnoreCase));
            if (setting == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(setting.Value);
        }

        // Public — fetch city/state/district details for a given pincode
        [HttpPost("FetchDetailsForPincodeForOutSide")]
        public async Task<APIResponse> FetchDetailsForPincodeForOutSide([FromForm] APIRequest apiRequest)
        {
            FetchPincodeRequest? req;
            try { req = JsonConvert.DeserializeObject<FetchPincodeRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null || string.IsNullOrEmpty(req.Pincode)) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            var (fail, data) = await _userService.FetchDetailsForPincode(req.Pincode);
            if (!string.IsNullOrEmpty(fail)) return ResponseProcess.GetFailResponse(fail);
            if (data == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.DataNotFound);
            return ResponseProcess.GetSuccessResponse(JsonConvert.SerializeObject(data));
        }

        // Public — generate OTP for sign-up and deliver it to the (unregistered) mobile.
        [HttpPost("SignUpGenerateOTP")]
        public async Task<APIResponse> SignUpGenerateOTP([FromForm] APIRequest apiRequest)
        {
            SignUpGenerateOtpRequest? req;
            try { req = JsonConvert.DeserializeObject<SignUpGenerateOtpRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _registrationService.GenerateOtp(req.MobileNumber);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(true.ToString(), "OTP sent to your mobile number.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        // Public — verify the sign-up OTP; marks the mobile verified so registration can proceed.
        [HttpPost("SignUpVerifyOTP")]
        public async Task<APIResponse> SignUpVerifyOTP([FromForm] APIRequest apiRequest)
        {
            SignUpVerifyOtpRequest? req;
            try { req = JsonConvert.DeserializeObject<SignUpVerifyOtpRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (req == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _registrationService.VerifyOtp(req.MobileNumber, req.OTP);
                if (!result.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(true.ToString(), "Mobile number verified.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }
}
