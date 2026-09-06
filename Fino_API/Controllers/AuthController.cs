// UPDATED AuthController with JWT Authentication
using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Login;
using FintechApp.Responses;
using FintechApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FintechApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = new Logger();

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

  
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<APIResponse> Login([FromForm] APIRequest apiRequest)
        {
            LoginRequest? loginRequest;

            try
            {
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                loginRequest = JsonConvert.DeserializeObject<LoginRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (loginRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            // Get IP Address
            loginRequest.IPAddress = IPAddressHelper.GetClientIPAddress(HttpContext);

            try
            {
                var (loginResponse, tokenResponse) = await _authService.Login(loginRequest);

                if (loginResponse == null || tokenResponse == null)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidCredentials);

                // Combine user info and tokens
                var combinedResponse = new
                {
                    user = loginResponse,
                    tokens = tokenResponse
                };

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(combinedResponse));
                return ResponseProcess.GetSuccessResponse(encryptedResponse, Messages.Common.Message.LoginSuccess);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>
        /// Refresh Token API - Get new Access + Refresh tokens
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<APIResponse> RefreshToken([FromForm] APIRequest apiRequest)
        {
            RefreshTokenRequest? refreshRequest;

            try
            {
                string decryptedRequest = _encryptDecrypt.Decrypt(apiRequest.Request);
                refreshRequest = JsonConvert.DeserializeObject<RefreshTokenRequest>(decryptedRequest);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (refreshRequest == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string ipAddress = IPAddressHelper.GetClientIPAddress(HttpContext);
                string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var tokenResponse = await _tokenService.RefreshToken(
                    refreshRequest,
                    ipAddress,
                    userAgent,
                    "Web"
                );

                if (tokenResponse == null)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.SessionExpired);

                string encryptedResponse = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(tokenResponse));
                return ResponseProcess.GetSuccessResponse(encryptedResponse, "Token refreshed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }

        /// <summary>
        /// Forgot Password — Step 1: send a reset OTP to a registered mobile number.
        /// </summary>
        [HttpPost("ForgotPasswordGenerateOTP")]
        [AllowAnonymous]
        public async Task<APIResponse> ForgotPasswordGenerateOTP([FromForm] APIRequest apiRequest)
        {
            ForgotPasswordGenerateOtpRequest? request;
            try { request = JsonConvert.DeserializeObject<ForgotPasswordGenerateOtpRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _authService.ForgotPasswordGenerateOtp(request.MobileNumber);
                if (!result.Equals("OK", StringComparison.OrdinalIgnoreCase)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(true.ToString(), "OTP sent to your registered mobile number.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Forgot Password — Step 2: verify the OTP; on success a temporary password is texted
        /// to the user and a change is forced at next login.
        /// </summary>
        [HttpPost("ForgotPasswordVerifyOTP")]
        [AllowAnonymous]
        public async Task<APIResponse> ForgotPasswordVerifyOTP([FromForm] APIRequest apiRequest)
        {
            ForgotPasswordVerifyOtpRequest? request;
            try { request = JsonConvert.DeserializeObject<ForgotPasswordVerifyOtpRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            try
            {
                string result = await _authService.ForgotPasswordVerifyOtp(request.MobileNumber, request.OTP);
                if (!result.Equals("OK", StringComparison.OrdinalIgnoreCase)) return ResponseProcess.GetFailResponse(result);
                return ResponseProcess.GetSuccessResponse(true.ToString(), "A new temporary password has been sent to your mobile number.");
            }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured); }
        }

        /// <summary>
        /// Logout API - Revokes all refresh tokens
        /// </summary>
        [HttpPost("logout")]
        [JwtAuthorization]
        public async Task<APIResponse> Logout()
        {
            try
            {
                // Get userId from JWT token (set by JwtAuthorizationAttribute)
                var userId = (int?)HttpContext.Items["UserId"];

                if (userId == null)
                    return ResponseProcess.GetUnauthorizedResponse(Messages.Common.Message.InvalidSession);

                var result = await _authService.Logout(userId.Value);

                if (result)
                    return ResponseProcess.GetSuccessResponse(string.Empty, Messages.Common.Message.LogoutSuccess);

                return ResponseProcess.GetFailResponse(Messages.Common.Message.Failed);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
        }
    }
}
