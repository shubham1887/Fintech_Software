using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Login;
using FintechApp.Responses.Login;
using Microsoft.Extensions.Configuration;

namespace FintechApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IUserRepository _userRepository;
        private readonly IOTPMasterRepository _otpRepository;
        private readonly IMessageService _messageService;
        private readonly ILoginHistoryRepository _loginHistoryRepository;

        private const string ForgotOtpType = "ForgotPassword";

        public AuthService(
            IAuthRepository authRepository,
            ITokenRepository tokenRepository,
            IConfiguration configuration,
            JwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IOTPMasterRepository otpRepository,
            IMessageService messageService,
            ILoginHistoryRepository loginHistoryRepository)
        {
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _messageService = messageService;
            _loginHistoryRepository = loginHistoryRepository;

            string encryptionKey = configuration["Encryption:Key"] ?? string.Empty;
            string encryptionIV = configuration["Encryption:IV"] ?? string.Empty;
            _encryptDecrypt = new EncryptDecrypt(encryptionKey, encryptionIV);
        }

        public async Task<(LoginResponse?, TokenResponse?)> Login(LoginRequest request)
        {
            // Get user by mobile number
            var user = await _authRepository.GetUserByMobileNumber(request.MobileNumber);

            if (user == null)
                return (null, null);

            // Verify password
            if (!_encryptDecrypt.VerifyPassword(request.Password, user.PasswordHash))
            {
                // Log wrong password attempt
                await _authRepository.LogWrongPassword(user.UserMasterID, request.Password, request.IPAddress, request.MobileNumber);
                return (null, null);
            }

            // Check if user is active
            if (!user.IsActive)
                return (null, null);

            // Generate JWT tokens
            var tokenResponse = _jwtTokenService.GenerateJwtToken(
                user.UserMasterID,
                user.UserTypeID ?? 0,
                user.UserType?.UserType ?? string.Empty,
                user.MobileNumber,
                user.EmailID
            );

            // Store refresh token in database
            await _tokenRepository.CreateRefreshToken(
                user.UserMasterID,
                tokenResponse.RefreshToken,
                tokenResponse.JwtId,
                request.IPAddress,
                string.Empty, // UserAgent can be passed from request
                request.DeviceType
            );

            // Record a successful-login entry in login history (best-effort; never blocks login).
            try
            {
                await _loginHistoryRepository.Insert(new LoginHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = request.IPAddress ?? string.Empty,
                    Token = tokenResponse.JwtId ?? string.Empty, // JWT id (access token itself can exceed the 500-char column)
                    Through = string.IsNullOrWhiteSpace(request.DeviceType) ? "Web" : request.DeviceType,
                    Date = DateTime.UtcNow,
                    LoginMessage = "Login successful",
                    Location = string.Empty,
                    SimCardDetails = string.Empty,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                    IsDeleted = false
                });
            }
            catch
            {
                // Login history is non-critical; swallow so a logging failure never blocks sign-in.
            }

            // Return login response
            var loginResponse = new LoginResponse
            {
                UserID = user.UserMasterID,
                PersonName = user.PersonName,
                FirmName = user.FirmName,
                MobileNumber = user.MobileNumber,
                EmailID = user.EmailID,
                UserTypeID = user.UserTypeID ?? 0,
                UserType = user.UserType?.UserType ?? string.Empty,
                IsKYCApproved = user.IsKYCApproved,
                IsTwoFA = user.IsTwoFA
            };

            return (loginResponse, tokenResponse);
        }

        public async Task<bool> Logout(int userId)
        {
            // Revoke all refresh tokens for this user
            return await _tokenRepository.RevokeAllUserTokens(userId);
        }

        public async Task<string> ForgotPasswordGenerateOtp(string mobile)
        {
            mobile = (mobile ?? string.Empty).Trim();
            if (mobile.Length != 10 || !mobile.All(char.IsDigit))
                return "Please enter a valid 10-digit mobile number.";

            var user = await _userRepository.GetUserByMobile(mobile);
            if (user == null)
                return "This mobile number is not registered.";

            string otp = OtpHelper.GenerateOtp();
            await _otpRepository.Insert(new OTPMaster
            {
                OTP = otp,
                UserID = user.UserMasterID,
                Type = ForgotOtpType,
                MobileNumber = mobile,
                IsVerified = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            });

            _ = _messageService.SendEventAsync(ForgotOtpType, user.UserMasterID, mobile,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string>
                {
                    ["OTP"] = otp,
                    ["CustomerName"] = user.PersonName ?? string.Empty
                },
                fallbackSms: $"Your OTP to reset your password is {otp}. Do not share it with anyone.",
                isOtpDefault: true);

            return StaticModels.StatusType.Ok;
        }

        public async Task<string> ForgotPasswordVerifyOtp(string mobile, string otp)
        {
            mobile = (mobile ?? string.Empty).Trim();
            otp = (otp ?? string.Empty).Trim();
            if (mobile.Length == 0 || otp.Length == 0)
                return Messages.Common.Message.InvalidRequest;

            var user = await _userRepository.GetUserByMobile(mobile);
            if (user == null)
                return "This mobile number is not registered.";

            var record = await _otpRepository.GetActiveByMobileAndType(mobile, ForgotOtpType);
            if (record == null)
                return "OTP has expired or was not requested. Please resend.";
            if (record.OTP != otp)
                return "Incorrect OTP. Please try again.";

            record.IsVerified = true;
            record.UpdatedateTime = DateTime.UtcNow;
            await _otpRepository.Update(record);

            // Set a temporary password and force a change at next login.
            string tempPassword = OtpHelper.GetRandomPassword();
            user.PasswordHash = _encryptDecrypt.HashPassword(tempPassword);
            user.IsInitialPassword = true;
            user.UpdatedateTime = DateTime.UtcNow;
            bool updated = await _userRepository.UpdateUser(user);
            if (!updated)
                return "Could not reset the password. Please try again.";

            string firm = !string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName : user.PersonName;
            string msg = $"Your new temporary password for {firm} is {tempPassword}. Please change it after logging in.";
            _ = _messageService.SendEventAsync("New Login Credentials", user.UserMasterID, mobile,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string>
                {
                    ["Password"] = tempPassword,
                    ["Number"] = mobile,
                    ["CustomerName"] = user.PersonName ?? string.Empty
                },
                fallbackSms: msg);
            if (!string.IsNullOrWhiteSpace(user.EmailID))
                _ = _messageService.SendEmailAsync(user.EmailID, msg, "Password Reset");

            return StaticModels.StatusType.Ok;
        }
    }
}
