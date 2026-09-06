using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Credential;
using FintechApp.Responses;
using FintechApp.StaticModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    public class CredentialController : ControllerBase
    {
        private readonly ICredentialService _credentialService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public CredentialController(ICredentialService credentialService, IConfiguration configuration)
        {
            _credentialService = credentialService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<APIResponse> Login([FromForm] APIRequest apiRequest)
        {
            LoginCredentialRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<LoginCredentialRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                var (failMsg, response) = await _credentialService.Login(request, ipAddress);
                if (!failMsg.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(failMsg);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [AllowAnonymous]
        [HttpPost("VerifyTwoFAatLogin")]
        public async Task<APIResponse> VerifyTwoFAatLogin([FromForm] APIRequest apiRequest)
        {
            VerifyTwoFARequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<VerifyTwoFARequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                var (failMsg, response) = await _credentialService.VerifyTwoFAatLogin(request, ipAddress);
                if (!failMsg.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(failMsg);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<APIResponse> ForgotPassword([FromForm] APIRequest apiRequest)
        {
            ForgotPasswordCredentialRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<ForgotPasswordCredentialRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _credentialService.ForgotPassword(request, ipAddress);
                if (!result.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt("Password sent successfully."));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPasswordGenerateOTP")]
        public async Task<APIResponse> ForgotPasswordGenerateOTP([FromForm] APIRequest apiRequest)
        {
            ForgotPasswordCredentialRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<ForgotPasswordCredentialRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _credentialService.ForgotPasswordGenerateOTP(request, ipAddress);
                if (!result.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt("OTP sent successfully."));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [AllowAnonymous]
        [HttpPost("VerifyForgotPasswordOTP")]
        public async Task<APIResponse> VerifyForgotPasswordOTP([FromForm] APIRequest apiRequest)
        {
            ForgotPasswordOTPRequest? request;
            try
            {
                request = JsonConvert.DeserializeObject<ForgotPasswordOTPRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _credentialService.VerifyForgotPasswordOTP(request, ipAddress);
                if (!result.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt("Password reset successfully."));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("NewDeviceLogin")]
        public async Task<APIResponse> NewDeviceLogin([FromForm] APIRequest apiRequest)
        {
            NewDeviceLoginRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<NewDeviceLoginRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                var (failMsg, response) = await _credentialService.NewDeviceLogin(request, ipAddress);
                if (!failMsg.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(failMsg);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [AllowAnonymous]
        [HttpPost("RefreshJWTToken")]
        public async Task<APIResponse> RefreshJWTToken([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                string refreshToken = HttpContext.Request.Headers["RefreshToken"].ToString();
                var (failMsg, response) = await _credentialService.RefreshJWTToken(keyDataModel.UserID, refreshToken);
                if (!failMsg.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(failMsg);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("InitialChangePassword")]
        public async Task<APIResponse> InitialChangePassword([FromForm] APIRequest apiRequest)
        {
            UpdatePasswordCredentialRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<UpdatePasswordCredentialRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _credentialService.InitialChangePassword(request, keyDataModel.UserID, ipAddress);
                if (!result.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt("Password changed successfully."));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("Logout")]
        public async Task<APIResponse> Logout([FromForm] APIRequest apiRequest)
        {
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || keyDataModel.UserID <= 0)
                return ResponseProcess.GetSuccessResponse(string.Empty);

            try
            {
                await _credentialService.Logout(keyDataModel.UserID);
                return ResponseProcess.GetSuccessResponse(string.Empty);
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetSuccessResponse(string.Empty); }
        }

        [HttpPost("ChangePasswordAfterExpiry")]
        public async Task<APIResponse> ChangePasswordAfterExpiry([FromForm] APIRequest apiRequest)
        {
            ChangePasswordAfterExpiryRequest? request;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                request = JsonConvert.DeserializeObject<ChangePasswordAfterExpiryRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
            try
            {
                string result = await _credentialService.ChangePasswordAfterExpiry(request, ipAddress);
                if (!result.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt("Password changed successfully."));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        [HttpPost("FetchDetailsForPincode")]
        public async Task<APIResponse> FetchDetailsForPincode([FromForm] APIRequest apiRequest)
        {
            string? pincode;
            KeyDataModel? keyDataModel;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(_encryptDecrypt.Decrypt(apiRequest.KeyData));
                var req = JsonConvert.DeserializeObject<PincodeRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
                pincode = req?.Pincode;
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (keyDataModel == null || string.IsNullOrWhiteSpace(pincode)) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            string fail = keyDataModel.IsValidModalState();
            if (!fail.EqualsIgnoreCase(StatusType.Ok)) return ResponseProcess.GetFailResponse(fail);

            try
            {
                var (failMsg, response) = await _credentialService.FetchDetailsForPincode(pincode);
                if (!failMsg.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    return ResponseProcess.GetFailResponse(failMsg);
                return ResponseProcess.GetSuccessResponse(_encryptDecrypt.Encrypt(JsonConvert.SerializeObject(response)));
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }
    }

    public class PincodeRequest { public string Pincode { get; set; } = string.Empty; }
}
