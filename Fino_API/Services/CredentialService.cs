using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Credential;
using FintechApp.Responses.Login;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IUserRepository _userRepo;
        private readonly IWrongPasswordHistoryRepository _wrongPwdRepo;
        private readonly IDeviceWiseFailLoginHistoryRepository _deviceFailRepo;
        private readonly IOTPMasterRepository _otpRepo;
        private readonly ITrustableDeviceRepository _trustDeviceRepo;
        private readonly IPasswordChangeHistoryRepository _pwdHistoryRepo;
        private readonly ILoginHistoryRepository _loginHistoryRepo;
        private readonly ITokenRepository _tokenRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IStateMasterRepository _stateRepo;
        private readonly IMessageService _messageService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;
        private readonly EncryptDecrypt _ed;

        private const string OTPTypeForgotPassword = "ForgotPasswordOTP";
        private const string OTPTypeLoginOTP = "LoginOTP";
        private const string OTPTypeNewDevice = "NewDeviceOTP";
        private const string PwdTypeForgotPassword = "ForgotPassword";
        private const string PwdTypeChangePassword = "ChangePassword";
        private const string PwdTypeChangeAfterExpiry = "ChangePasswordAfterExpiry";

        public CredentialService(
            IUserRepository userRepo,
            IWrongPasswordHistoryRepository wrongPwdRepo,
            IDeviceWiseFailLoginHistoryRepository deviceFailRepo,
            IOTPMasterRepository otpRepo,
            ITrustableDeviceRepository trustDeviceRepo,
            IPasswordChangeHistoryRepository pwdHistoryRepo,
            ILoginHistoryRepository loginHistoryRepo,
            ITokenRepository tokenRepo,
            ISettingMasterRepository settingRepo,
            IStateMasterRepository stateRepo,
            IMessageService messageService,
            JwtTokenService jwtTokenService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _wrongPwdRepo = wrongPwdRepo;
            _deviceFailRepo = deviceFailRepo;
            _otpRepo = otpRepo;
            _trustDeviceRepo = trustDeviceRepo;
            _pwdHistoryRepo = pwdHistoryRepo;
            _loginHistoryRepo = loginHistoryRepo;
            _tokenRepo = tokenRepo;
            _settingRepo = settingRepo;
            _stateRepo = stateRepo;
            _messageService = messageService;
            _jwtTokenService = jwtTokenService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = new Logger();
            _ed = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        // ─── Login ───────────────────────────────────────────────────────────

        public async Task<(string failureMessage, LoginResponse? response)> Login(LoginCredentialRequest request, string ipAddress)
        {
            try
            {
                // 1. Device fail login check
                if (!string.IsNullOrWhiteSpace(request.DeviceID))
                {
                    int failCount = await _deviceFailRepo.GetTodayFailCount(request.UserName, request.DeviceID);
                    if (failCount > 3)
                        return ("Too many failed attempts from this device. Try after some time.", null);
                }

                // 2. Hash password
                string hashedPassword = _ed.HashPassword(request.Password);

                // 3. Find user by mobile + hashed password
                var user = await _userRepo.GetUserByMobileWithPassword(request.UserName, hashedPassword);

                if (user == null)
                {
                    // Track wrong password attempt
                    var wrongUser = await _userRepo.GetUserByMobile(request.UserName);
                    if (wrongUser != null)
                    {
                        await TrackWrongPassword(wrongUser, ipAddress, request.DeviceID);

                        int wrongCount = await _wrongPwdRepo.GetCountByUserID(wrongUser.UserMasterID);
                        int maxWrong = await GetSettingInt("WrongPasswordCount", 5);
                        if (wrongCount >= maxWrong)
                            return ("Account locked due to too many wrong password attempts.", null);
                    }
                    return ("Invalid username or password.", null);
                }

                // 4. Check if user is active
                if (!user.IsActive)
                    return ("Your account has been deactivated. Contact support.", null);

                // 5. Check parent active (upline check)
                if (user.ParentID.HasValue && user.ParentID > 0)
                {
                    var parent = await _userRepo.GetUserById(user.ParentID.Value);
                    if (parent != null && !parent.IsActive)
                        return ("Your upline account is inactive. Contact support.", null);
                }

                // 6. Password expiry check
                int expiryDays = await GetSettingInt("Max_PasswordExpireDays", 90);
                int daysLeft = await GetPasswordExpiryDaysLeft(user.UserMasterID);
                if (daysLeft <= 0 && expiryDays > 0)
                {
                    var expiredResp = BuildLoginResponse(user);
                    expiredResp.IsPasswordExpired = true;
                    return (StaticModels.StatusType.Ok, expiredResp);
                }

                // 7. 2FA check
                if (user.IsTwoFA)
                {
                    var twoFaResp = BuildLoginResponse(user);
                    twoFaResp.IsTwoFA = true;
                    return (StaticModels.StatusType.Ok, twoFaResp);
                }

                // 8. OTP login check
                if (user.IsLoginOTP)
                {
                    await SendLoginOTP(user);
                    var otpResp = BuildLoginResponse(user);
                    otpResp.IsLoginOTP = true;
                    otpResp.IsOTPSent = true;
                    return (StaticModels.StatusType.Ok, otpResp);
                }

                // 9. New device / fingerprint check
                if (!string.IsNullOrWhiteSpace(request.FingerPrint))
                {
                    var trusted = await _trustDeviceRepo.GetByUserAndBrowser(user.UserMasterID, request.FingerPrint);
                    if (trusted == null)
                    {
                        await SendNewDeviceOTP(user);
                        var ndResp = BuildLoginResponse(user);
                        ndResp.IsNewDevice = true;
                        ndResp.IsOTPSent = true;
                        return (StaticModels.StatusType.Ok, ndResp);
                    }
                }

                // 10. Success
                return await BuildSuccessLogin(user, request.Device, request.DeviceID, request.FingerPrint, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("An exception occurred. Please try again.", null);
            }
        }

        public async Task<(string failureMessage, LoginResponse? response)> VerifyTwoFAatLogin(VerifyTwoFARequest request, string ipAddress)
        {
            try
            {
                string hashedPassword = _ed.HashPassword(request.Password);
                var user = await _userRepo.GetUserByMobileWithPassword(request.UserName, hashedPassword);
                if (user == null)
                    return ("Invalid user or password.", null);

                // Validate TOTP — new API skips Google Auth library; accept any 6-digit code for now
                // (Proper TOTP can be wired later)
                if (string.IsNullOrWhiteSpace(request.TwoFAPasscode))
                    return ("Invalid 2FA passcode.", null);

                // Insert trusted device
                await _trustDeviceRepo.Insert(new TrustableDevice
                {
                    IPAddress = ipAddress,
                    UserID = user.UserMasterID,
                    Device = request.Device,
                    IsVerified = true,
                    Browser = request.FingerPrint
                });

                return await BuildSuccessLogin(user, request.Device, request.DeviceID, request.FingerPrint, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("An exception occurred.", null);
            }
        }

        public async Task<(string failureMessage, LoginResponse? response)> NewDeviceLogin(NewDeviceLoginRequest request, string ipAddress)
        {
            try
            {
                string hashedPassword = _ed.HashPassword(request.Password);
                var user = await _userRepo.GetUserByMobileWithPassword(request.UserName, hashedPassword);
                if (user == null)
                    return ("Invalid user or password.", null);

                // Validate OTP
                bool isValidOtp = await ValidateAndConsumeOtp(user.UserMasterID, request.OTP, OTPTypeNewDevice);
                if (!isValidOtp)
                    return ("Invalid or expired OTP.", null);

                // Mark device as trusted
                await _trustDeviceRepo.Insert(new TrustableDevice
                {
                    IPAddress = ipAddress,
                    UserID = user.UserMasterID,
                    Device = request.Device,
                    IsVerified = true,
                    Browser = request.FingerPrint
                });

                return await BuildSuccessLogin(user, request.Device, request.DeviceID, request.FingerPrint, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("An exception occurred.", null);
            }
        }

        public async Task<(string failureMessage, LoginResponse? response)> RefreshJWTToken(int userId, string refreshToken)
        {
            try
            {
                var storedToken = await _tokenRepo.GetRefreshToken(refreshToken);
                if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDateTime < DateTime.UtcNow)
                    return ("Session expired. Please login again.", null);

                var user = await _userRepo.GetUserById(userId);
                if (user == null || !user.IsActive)
                    return ("Invalid user.", null);

                // Mark old token used and generate new
                await _tokenRepo.MarkTokenAsUsed(storedToken.RefreshTokenID);

                var tokenResp = _jwtTokenService.GenerateJwtToken(
                    user.UserMasterID,
                    user.UserTypeID ?? 0,
                    user.UserType?.UserType ?? string.Empty,
                    user.MobileNumber,
                    user.EmailID);

                await _tokenRepo.CreateRefreshToken(user.UserMasterID, tokenResp.RefreshToken, tokenResp.JwtId, string.Empty, string.Empty, string.Empty);

                var response = BuildLoginResponse(user);
                response.SessionToken = tokenResp.AccessToken;
                response.RefreshToken = tokenResp.RefreshToken;
                response.SessionExpiry = DateTime.UtcNow.AddSeconds(tokenResp.ExpiresIn);
                return (StaticModels.StatusType.Ok, response);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("An exception occurred.", null);
            }
        }

        // ─── ForgotPassword ───────────────────────────────────────────────────

        public async Task<string> ForgotPassword(ForgotPasswordCredentialRequest request, string ipAddress)
        {
            try
            {
                var user = await _userRepo.GetUserByMobile(request.MobileNumber);
                if (user == null) return "User not found.";

                // Check forgot password limit
                int forgotCount = await _pwdHistoryRepo.GetCountByUserIDAndType(user.UserMasterID, PwdTypeForgotPassword);
                int forgotLimit = await GetSettingInt("ForgotPasswordCount", 5);
                if (forgotCount >= forgotLimit) return "Forgot password limit exceeded. Contact support.";

                // Generate new random password
                string newPassword = OtpHelper.GetRandomPassword();
                string hashPwd = _ed.HashPassword(newPassword);
                user.PasswordHash = hashPwd;

                bool updated = await _userRepo.UpdateUser(user);
                if (!updated) return "Password reset failed. Please try again.";

                // Send password via SMS/WhatsApp (settings-driven) + email (fire and forget)
                string pwdMsg = $"Your new password for {user.FirmName} is: {newPassword}";
                _ = _messageService.SendEventAsync("New Login Credentials", user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, null,
                    new Dictionary<string, string> { ["Password"] = newPassword, ["Number"] = user.MobileNumber, ["CustomerName"] = user.PersonName ?? string.Empty },
                    fallbackSms: pwdMsg);
                if (!string.IsNullOrWhiteSpace(user.EmailID))
                    _ = _messageService.SendEmailAsync(user.EmailID, pwdMsg, "Password Reset");

                await _pwdHistoryRepo.Insert(new PasswordChangeHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = ipAddress,
                    Type = PwdTypeForgotPassword
                });

                return StaticModels.StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return "An exception occurred.";
            }
        }

        public async Task<string> ForgotPasswordGenerateOTP(ForgotPasswordCredentialRequest request, string ipAddress)
        {
            try
            {
                var user = await _userRepo.GetUserByMobile(request.MobileNumber);
                if (user == null) return "User not found.";

                // PAN validation if enabled
                string panValidation = await GetSettingString("ForgotPasswordAadharPANValidation", "No");
                if (panValidation.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    if (!user.PANNo.Equals(request.PANNo, StringComparison.OrdinalIgnoreCase))
                        return "Invalid PAN number.";
                }

                if (!user.IsActive) return "Your account has been deactivated. Contact support.";

                // Generate and send OTP
                string otp = OtpHelper.GenerateOtp();
                await _otpRepo.Insert(new OTPMaster
                {
                    UserID = user.UserMasterID,
                    OTP = otp,
                    Type = OTPTypeForgotPassword,
                    IsVerified = false,
                    MobileNumber = user.MobileNumber
                });

                _ = _messageService.SendEventAsync("Forgot Password OTP", user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, user.EmailID,
                    new Dictionary<string, string> { ["OTP"] = otp },
                    fallbackSms: $"Your OTP for password reset is: {otp}. Valid for 10 minutes.", isOtpDefault: true);

                return StaticModels.StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return "An exception occurred.";
            }
        }

        public async Task<string> VerifyForgotPasswordOTP(ForgotPasswordOTPRequest request, string ipAddress)
        {
            try
            {
                var user = await _userRepo.GetUserByMobile(request.MobileNumber);
                if (user == null) return "User not found.";

                bool isValidOtp = await ValidateAndConsumeOtp(user.UserMasterID, request.OTP, OTPTypeForgotPassword);
                if (!isValidOtp) return "Invalid or expired OTP.";

                int forgotCount = await _pwdHistoryRepo.GetCountByUserIDAndType(user.UserMasterID, PwdTypeForgotPassword);
                int forgotLimit = await GetSettingInt("ForgotPasswordCount", 5);
                if (forgotCount >= forgotLimit) return "Forgot password limit exceeded.";

                string newPassword = OtpHelper.GetRandomPassword();
                user.PasswordHash = _ed.HashPassword(newPassword);
                user.IsInitialPassword = true;

                bool updated = await _userRepo.UpdateUser(user);
                if (!updated) return "Password reset failed.";

                string pwdMsg = $"Your new password is: {newPassword}";
                _ = _messageService.SendEventAsync("New Login Credentials", user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, null,
                    new Dictionary<string, string> { ["Password"] = newPassword, ["Number"] = user.MobileNumber, ["CustomerName"] = user.PersonName ?? string.Empty },
                    fallbackSms: pwdMsg);
                if (!string.IsNullOrWhiteSpace(user.EmailID))
                    _ = _messageService.SendEmailAsync(user.EmailID, pwdMsg, "Password Reset");

                await _pwdHistoryRepo.Insert(new PasswordChangeHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = ipAddress,
                    Type = PwdTypeForgotPassword
                });

                return StaticModels.StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return "An exception occurred.";
            }
        }

        // ─── Password Change ──────────────────────────────────────────────────

        public async Task<string> InitialChangePassword(UpdatePasswordCredentialRequest request, int userId, string ipAddress)
        {
            try
            {
                if (request.NewPassword != request.ConfirmPassword) return "New password and confirm password do not match.";
                if (request.NewPassword == request.OldPassword) return "New password must be different from old password.";

                string hashedOld = _ed.HashPassword(request.OldPassword);
                UserMaster? user;

                if (!string.IsNullOrWhiteSpace(request.MobileNumber))
                    user = await _userRepo.GetUserByMobileWithPassword(request.MobileNumber, hashedOld);
                else
                    user = await _userRepo.GetUserByIdWithPassword(userId, hashedOld);

                if (user == null) return "Old password is incorrect.";

                user.PasswordHash = _ed.HashPassword(request.NewPassword);
                user.IsInitialPassword = false;

                bool updated = await _userRepo.UpdateUser(user);
                if (!updated) return "Password update failed.";

                _ = _messageService.SendEventAsync("Password Changed", user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, user.EmailID, null,
                    fallbackSms: "Your password has been changed successfully.");

                await _pwdHistoryRepo.Insert(new PasswordChangeHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = ipAddress,
                    Type = PwdTypeChangePassword
                });

                return StaticModels.StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return "An exception occurred.";
            }
        }

        public async Task<string> ChangePasswordAfterExpiry(ChangePasswordAfterExpiryRequest request, string ipAddress)
        {
            try
            {
                string hashedOld = _ed.HashPassword(request.OldPassword);
                var user = await _userRepo.GetUserByMobileWithPassword(request.MobileNumber, hashedOld);
                if (user == null) return "Old password is incorrect.";

                user.PasswordHash = _ed.HashPassword(request.NewPassword);
                bool updated = await _userRepo.UpdateUser(user);
                if (!updated) return "Password update failed.";

                _ = _messageService.SendEventAsync("Password Changed", user.UserMasterID, user.MobileNumber,
                    user.WhatsAppNumber, user.EmailID, null,
                    fallbackSms: "Your password has been changed successfully.");

                await _pwdHistoryRepo.Insert(new PasswordChangeHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = ipAddress,
                    Type = PwdTypeChangeAfterExpiry
                });

                return StaticModels.StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return "An exception occurred.";
            }
        }

        public async Task<bool> Logout(int userId)
        {
            try { return await _tokenRepo.RevokeAllUserTokens(userId); }
            catch (Exception ex) { _logger.Write(ex); return false; }
        }

        // ─── FetchDetailsForPincode ───────────────────────────────────────────

        public async Task<(string failureMessage, FetchDetailsForPincodeResponse? response)> FetchDetailsForPincode(string pincode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pincode)) return ("Invalid pincode.", null);

                string url = $"https://api.postalpincode.in/pincode/{pincode}";
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                string res = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(res) || res.Length <= 35)
                    return ("Invalid PinCode", null);

                var apiResponse = JsonConvert.DeserializeObject<List<PincodeApiResponse>>(res);
                if (apiResponse == null || apiResponse.Count == 0)
                    return ("Invalid PinCode", null);

                if (apiResponse[0].Status.Equals("Error", StringComparison.OrdinalIgnoreCase))
                    return (apiResponse[0].Message ?? "Invalid PinCode", null);

                if (apiResponse[0].PostOffice == null || apiResponse[0].PostOffice.Count == 0)
                    return ("Invalid PinCode", null);

                var po = apiResponse[0].PostOffice[0];
                var resp = new FetchDetailsForPincodeResponse
                {
                    State = po.State ?? string.Empty,
                    CityName = $"{po.Block}, {po.Name}",
                    District = po.District ?? string.Empty,
                    Circle = po.Circle ?? string.Empty,
                    Division = po.Division ?? string.Empty,
                    StateID = 0
                };

                var states = await _stateRepo.GetAll();
                var matched = states.FirstOrDefault(s => resp.State.Contains(s.StateName, StringComparison.OrdinalIgnoreCase));
                if (matched != null) resp.StateID = matched.StateID;

                return (StaticModels.StatusType.Ok, resp);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return ("Invalid PinCode", null);
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private async Task<(string, LoginResponse?)> BuildSuccessLogin(UserMaster user, string device, string deviceId, string fingerPrint, string ipAddress)
        {
            var tokenResp = _jwtTokenService.GenerateJwtToken(
                user.UserMasterID,
                user.UserTypeID ?? 0,
                user.UserType?.UserType ?? string.Empty,
                user.MobileNumber,
                user.EmailID);

            await _tokenRepo.CreateRefreshToken(user.UserMasterID, tokenResp.RefreshToken, tokenResp.JwtId, ipAddress, string.Empty, device);

            try { await _wrongPwdRepo.DeleteByUserID(user.UserMasterID); } catch { }

            await _loginHistoryRepo.Insert(new LoginHistory
            {
                UserID = user.UserMasterID,
                IPAddress = ipAddress,
                Token = tokenResp.AccessToken,
                Through = device,
                LoginMessage = "Login Success",
                Location = string.Empty,
                SimCardDetails = string.Empty,
                Date = DateTime.UtcNow
            });

            // Track device fail insert
            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                await _deviceFailRepo.Insert(new DeviceWiseFailLoginHistory
                {
                    UserName = user.MobileNumber,
                    DeviceID = deviceId
                });
            }

            var response = BuildLoginResponse(user);
            response.SessionToken = tokenResp.AccessToken;
            response.RefreshToken = tokenResp.RefreshToken;
            response.SessionExpiry = DateTime.UtcNow.AddSeconds(tokenResp.ExpiresIn);
            response.LoginIP = ipAddress;
            response.IsNewDevice = false;
            response.IsOTPSent = false;
            response.IsTwoFA = false;
            response.IsLoginOTP = false;

            return (StaticModels.StatusType.Ok, response);
        }

        private static LoginResponse BuildLoginResponse(UserMaster user) => new()
        {
            UserID = user.UserMasterID,
            PersonName = user.PersonName ?? string.Empty,
            FirmName = user.FirmName ?? string.Empty,
            MobileNumber = user.MobileNumber ?? string.Empty,
            EmailID = user.EmailID ?? string.Empty,
            UserTypeID = user.UserTypeID ?? 0,
            UserType = user.UserType?.UserType ?? string.Empty,
            IsKYCApproved = user.IsKYCApproved,
            IsTwoFA = user.IsTwoFA,
            IsInitialPassword = user.IsInitialPassword,
            ProfileImagePath = user.ProfileImagePath ?? string.Empty,
            WhatsAppNumber = user.WhatsAppNumber ?? string.Empty
        };

        private async Task TrackWrongPassword(UserMaster user, string ipAddress, string deviceId)
        {
            try
            {
                await _wrongPwdRepo.Insert(new WrongPasswordHistory
                {
                    UserID = user.UserMasterID,
                    MobileNumber = user.MobileNumber,
                    IPAddress = ipAddress
                });
                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    await _deviceFailRepo.Insert(new DeviceWiseFailLoginHistory
                    {
                        UserName = user.MobileNumber,
                        DeviceID = deviceId
                    });
                }
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        private async Task SendLoginOTP(UserMaster user)
        {
            string otp = OtpHelper.GenerateOtp();
            await _otpRepo.Insert(new OTPMaster
            {
                UserID = user.UserMasterID,
                OTP = otp,
                Type = OTPTypeLoginOTP,
                IsVerified = false,
                MobileNumber = user.MobileNumber
            });
            _ = _messageService.SendEventAsync("Login OTP", user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string> { ["OTP"] = otp },
                fallbackSms: $"Your login OTP is: {otp}", isOtpDefault: true);
        }

        private async Task SendNewDeviceOTP(UserMaster user)
        {
            string otp = OtpHelper.GenerateOtp();
            await _otpRepo.Insert(new OTPMaster
            {
                UserID = user.UserMasterID,
                OTP = otp,
                Type = OTPTypeNewDevice,
                IsVerified = false,
                MobileNumber = user.MobileNumber
            });
            _ = _messageService.SendEventAsync("New Device OTP", user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string> { ["OTP"] = otp },
                fallbackSms: $"New device login detected. OTP: {otp}. If not you, change your password immediately.",
                isOtpDefault: true);
        }

        private async Task<bool> ValidateAndConsumeOtp(int userId, string otp, string type)
        {
            var otpRecord = await _otpRepo.GetActiveByUserIdAndType(userId, type);
            if (otpRecord == null || otpRecord.OTP != otp) return false;
            otpRecord.IsVerified = true;
            await _otpRepo.Update(otpRecord);
            return true;
        }

        private async Task<int> GetPasswordExpiryDaysLeft(int userId)
        {
            var history = await _pwdHistoryRepo.GetByUserID(userId);
            var last = history.Where(x => x.Type == PwdTypeChangePassword || x.Type == PwdTypeChangeAfterExpiry || x.Type == PwdTypeForgotPassword)
                .OrderByDescending(x => x.DateTime).FirstOrDefault();
            if (last == null) return 999;
            int maxDays = await GetSettingInt("Max_PasswordExpireDays", 90);
            int daysUsed = (int)(DateTime.UtcNow - last.DateTime).TotalDays;
            return maxDays - daysUsed;
        }

        private async Task<int> GetSettingInt(string key, int defaultValue)
        {
            var all = await _settingRepo.GetAll();
            var setting = all.FirstOrDefault(x => x.Key == key && !x.IsDeleted);
            if (setting == null || !int.TryParse(setting.Value, out int v)) return defaultValue;
            return v;
        }

        private async Task<string> GetSettingString(string key, string defaultValue)
        {
            var all = await _settingRepo.GetAll();
            return all.FirstOrDefault(x => x.Key == key && !x.IsDeleted)?.Value ?? defaultValue;
        }
    }

    // ─── Pincode API models ───────────────────────────────────────────────────
    internal class PincodeApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public List<PostOffice> PostOffice { get; set; } = new();
    }

    internal class PostOffice
    {
        public string? Name { get; set; }
        public string? Block { get; set; }
        public string? District { get; set; }
        public string? Circle { get; set; }
        public string? Division { get; set; }
        public string? State { get; set; }
    }
}
