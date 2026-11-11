// UPDATED AuthController with JWT Authentication
using FintechApp.Authorization;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Requests.Login;
using FintechApp.Requests.TwoFactor;
using FintechApp.Requests.User;
using FintechApp.Responses;
using FintechApp.Responses.Login;
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

                if (loginResponse == null)
                    return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidCredentials);

                // Password accepted, second step outstanding. This is a SUCCESS response carrying
                // no tokens — reporting it as invalid credentials (which is what a null token used
                // to mean here) would tell the user their own password was wrong.
                if (tokenResponse == null)
                {
                    if (!loginResponse.IsTwoFA)
                        return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidCredentials);

                    string twoFaPayload = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new
                    {
                        user = loginResponse,
                        tokens = (TokenResponse?)null
                    }));
                    return ResponseProcess.GetSuccessResponse(twoFaPayload, Messages.TwoFactorMessages.Required);
                }

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
        /// Second step of sign-in: mobile + password + the authenticator code (or a recovery code).
        ///
        /// Anonymous, like <see cref="Login"/> — there is no session yet. The credentials are
        /// re-presented rather than the server keeping a half-built login, so nothing partial
        /// exists to be stolen or to expire.
        /// </summary>
        [HttpPost("login-2fa")]
        [AllowAnonymous]
        public async Task<APIResponse> LoginTwoFactor([FromForm] APIRequest apiRequest)
        {
            TwoFactorLoginRequest? request;

            try
            {
                request = JsonConvert.DeserializeObject<TwoFactorLoginRequest>(_encryptDecrypt.Decrypt(apiRequest.Request));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured);
            }

            if (request == null)
                return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);
            if (string.IsNullOrWhiteSpace(request.Code))
                return ResponseProcess.GetFailResponse(Messages.TwoFactorMessages.CodeRequired);

            request.IPAddress = IPAddressHelper.GetClientIPAddress(HttpContext);

            try
            {
                var (loginResponse, tokenResponse) = await _authService.LoginWithTwoFactor(request);

                // One message for a wrong password and a wrong code alike. Telling them apart
                // would turn this endpoint into a way to confirm a password without the phone.
                if (loginResponse == null || tokenResponse == null)
                    return ResponseProcess.GetFailResponse(Messages.TwoFactorMessages.InvalidCode);

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
        /// Continue as the other role this mobile number holds.
        ///
        /// Authenticated: the person is already signed in as their primary account and is
        /// choosing which of their own logins to work as. The service checks the target really
        /// is one of theirs before issuing anything.
        /// </summary>
        [HttpPost("SwitchProfile")]
        [JwtAuthorization]
        public async Task<APIResponse> SwitchProfile([FromForm] APIRequest apiRequest)
        {
            SwitchProfileRequest? request;
            try { request = JsonConvert.DeserializeObject<SwitchProfileRequest>(_encryptDecrypt.Decrypt(apiRequest.Request)); }
            catch (Exception ex) { _logger.Write(ex); return ResponseProcess.GetFailResponse(Messages.Common.Message.ExceptionOccured); }
            if (request == null) return ResponseProcess.GetFailResponse(Messages.Common.Message.InvalidRequest);

            // Identity from the signed token, never from the body — the body only names the
            // destination, and the service re-checks that it belongs to this person.
            var userId = (int?)HttpContext.Items["UserId"];
            if (userId == null || userId <= 0)
                return ResponseProcess.GetUnauthorizedResponse(Messages.Common.Message.InvalidSession);

            try
            {
                var (loginResponse, tokenResponse, failure) = await _authService.SwitchProfile(
                    userId.Value, request.UserID, "Web", IPAddressHelper.GetClientIPAddress(HttpContext));

                if (loginResponse == null || tokenResponse == null)
                    return ResponseProcess.GetFailResponse(string.IsNullOrWhiteSpace(failure) ? Messages.Common.Message.Failed : failure);

                string payload = _encryptDecrypt.Encrypt(JsonConvert.SerializeObject(new
                {
                    user = loginResponse,
                    tokens = tokenResponse
                }));
                return ResponseProcess.GetSuccessResponse(payload, Messages.Common.Message.LoginSuccess);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ResponseProcess.GetErrorResponse(Messages.Common.Message.ExceptionOccured);
            }
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
