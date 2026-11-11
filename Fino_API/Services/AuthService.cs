using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Login;
using FintechApp.Requests.TwoFactor;
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
        private readonly ITwoFactorService _twoFactorService;

        private const string ForgotOtpType = "ForgotPassword";

        public AuthService(
            IAuthRepository authRepository,
            ITokenRepository tokenRepository,
            IConfiguration configuration,
            JwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IOTPMasterRepository otpRepository,
            IMessageService messageService,
            ILoginHistoryRepository loginHistoryRepository,
            ITwoFactorService twoFactorService)
        {
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
            _jwtTokenService = jwtTokenService;
            _userRepository = userRepository;
            _otpRepository = otpRepository;
            _messageService = messageService;
            _loginHistoryRepository = loginHistoryRepository;
            _twoFactorService = twoFactorService;

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

            // Two-step verification, before any token exists.
            //
            // The flag is checked against the actual enrolment, not on its own: a flag set with no
            // CONFIRMED authenticator behind it would demand a code nothing can generate, locking
            // the account permanently. In that case the flag is cleared and sign-in continues.
            if (user.IsTwoFA)
            {
                var twoFactorStatus = await _twoFactorService.Status(user.UserMasterID);
                if (twoFactorStatus.IsEnabled)
                {
                    // No tokens, and deliberately no profile either — a correct password alone must
                    // not reveal the person's name, firm or email. The client reads IsTwoFA and
                    // collects the code, then calls LoginWithTwoFactor.
                    return (new LoginResponse
                    {
                        IsTwoFA = true,
                        MobileNumber = user.MobileNumber,
                    }, null);
                }

                await _userRepository.SetTwoFactorEnabled(user.UserMasterID, false);
            }

            return (await BuildLoginResponseWithProfiles(user), await IssueSession(user, request.DeviceType, request.IPAddress, request.Latitude, request.Longitude, request.Location));
        }

        /// <summary>
        /// The second step of signing in. The password is re-presented alongside the code rather
        /// than the server holding a half-built session, so there is no pending-login state to
        /// steal and nothing to expire.
        /// </summary>
        public async Task<(LoginResponse?, TokenResponse?)> LoginWithTwoFactor(TwoFactorLoginRequest request)
        {
            var user = await _authRepository.GetUserByMobileNumber(request.MobileNumber);
            if (user == null)
                return (null, null);

            if (!_encryptDecrypt.VerifyPassword(request.Password, user.PasswordHash))
            {
                await _authRepository.LogWrongPassword(user.UserMasterID, request.Password, request.IPAddress, request.MobileNumber);
                return (null, null);
            }

            if (!user.IsActive)
                return (null, null);

            // Accepts an authenticator code or an unused recovery code; a replayed authenticator
            // code and a spent recovery code are both refused inside the service.
            if (!await _twoFactorService.VerifyForLogin(user.UserMasterID, request.Code))
                return (null, null);

            return (await BuildLoginResponseWithProfiles(user), await IssueSession(user, request.DeviceType, request.IPAddress, request.Latitude, request.Longitude, request.Location));
        }

        /// <summary>
        /// Continue as the other role this mobile number holds.
        ///
        /// Sign-in always authenticates and issues for the PRIMARY account, so this is not a
        /// second authentication — it is the already-authenticated person choosing which of their
        /// own logins to work as. The only thing that has to be true is that the target really is
        /// one of theirs: it is resolved from the explicit link (UserMaster.LinkedPrimaryUserID),
        /// never from the shared mobile number, so two accounts that happen to carry the same
        /// number cannot be hopped between.
        ///
        /// The old session's refresh tokens are left alone deliberately — switching back is the
        /// same call in the other direction, and revoking here would sign the person out of the
        /// role they just came from on their other device.
        /// </summary>
        public async Task<(LoginResponse?, TokenResponse?, string failure)> SwitchProfile(
            int currentUserID, int targetUserID, string deviceType, string ipAddress)
        {
            if (targetUserID <= 0) return (null, null, Messages.Common.Message.InvalidRequest);

            var users = await _userRepository.GetAllUsers();
            var me = users.FirstOrDefault(u => u.UserMasterID == currentUserID);
            if (me == null) return (null, null, Messages.UserMessages.UserNotFound);

            if (targetUserID == currentUserID)
                return (null, null, "You are already signed in as this account.");

            var primary = UserService.PrimaryOf(users, me);
            var target = UserService.GroupOf(users, primary)
                .FirstOrDefault(u => u.UserMasterID == targetUserID);

            if (target == null) return (null, null, Messages.Common.Message.NoPermission);
            if (!target.IsActive) return (null, null, "That login is deactivated. Ask your service provider to activate it.");

            return (await BuildLoginResponseWithProfiles(target),
                    await IssueSession(target, deviceType, ipAddress),
                    string.Empty);
        }

        /// <summary>
        /// Mint the JWT pair, store the refresh token and record the login. Shared by the
        /// single-step and two-step paths so there is exactly one place a session is created —
        /// a second copy is how one path ends up skipping a check the other makes.
        /// </summary>
        // latitude / longitude / location are whatever the signing-in client could tell us about
        // where this was. Optional everywhere: the profile switch path has no request to read them
        // from and passes none, which is exactly what an unknown location should look like.
        private async Task<TokenResponse> IssueSession(UserMaster user, string deviceType, string ipAddress,
            string latitude = "", string longitude = "", string location = "")
        {
            // Generate JWT tokens
            var tokenResponse = _jwtTokenService.GenerateJwtToken(
                user.UserMasterID,
                user.UserTypeID ?? 0,
                user.UserType?.UserType ?? string.Empty,
                user.MobileNumber,
                user.EmailID,
                user.SessionEpoch
            );

            // Store refresh token in database
            await _tokenRepository.CreateRefreshToken(
                user.UserMasterID,
                tokenResponse.RefreshToken,
                tokenResponse.JwtId,
                ipAddress,
                string.Empty, // UserAgent can be passed from request
                deviceType
            );

            // Record a successful-login entry in login history (best-effort; never blocks login).
            try
            {
                await _loginHistoryRepository.Insert(new LoginHistory
                {
                    UserID = user.UserMasterID,
                    IPAddress = ipAddress ?? string.Empty,
                    Token = tokenResponse.JwtId ?? string.Empty, // JWT id (access token itself can exceed the 500-char column)
                    Through = string.IsNullOrWhiteSpace(deviceType) ? "Web" : deviceType,
                    Date = DateTime.Now,   // local time, like every other timestamp in this DB — a UTC Date here put logins 5.5h in the past and broke the report's date filter
                    LoginMessage = "Login successful",
                    // Whatever the client could tell us about where this was. Previously hardcoded
                    // empty, so the Login History report's Location column was blank on every row
                    // ever written — a report about where people sign in from, with the one field
                    // that answers that never populated.
                    Location = string.IsNullOrWhiteSpace(location)
                        ? (string.IsNullOrWhiteSpace(latitude) ? string.Empty : $"{latitude},{longitude}")
                        : location.Trim(),
                    Latitude = (latitude ?? string.Empty).Trim(),
                    Longitude = (longitude ?? string.Empty).Trim(),
                    SimCardDetails = string.Empty,
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now,
                    IsDeleted = false
                });
            }
            catch
            {
                // Login history is non-critical; swallow so a logging failure never blocks sign-in.
            }

            return tokenResponse;
        }

        /// <summary>
        /// The sign-in payload, plus the other logins this person holds when there are any.
        ///
        /// The list is empty for a normal single-role account, and the client reads an empty list
        /// exactly as it read the old response: straight to the dashboard. Only a person who has
        /// actually set up a second role is ever asked to choose.
        /// </summary>
        private async Task<LoginResponse> BuildLoginResponseWithProfiles(UserMaster user)
        {
            var response = BuildLoginResponse(user);

            var users = await _userRepository.GetAllUsers();
            var primary = UserService.PrimaryOf(users, user);
            var group = UserService.GroupOf(users, primary);
            if (group.Count <= 1) return response;

            response.Profiles = group
                .Where(u => u.IsActive)
                .Select(u => new LoginProfileOption
                {
                    UserID = u.UserMasterID,
                    UserTypeID = u.UserTypeID ?? 0,
                    UserType = u.UserType?.UserType ?? string.Empty,
                    FirmName = u.FirmName ?? string.Empty,
                    UserCode = u.AliasName ?? string.Empty,
                    IsPrimary = u.UserMasterID == primary.UserMasterID,
                })
                .ToList();

            // One usable login is not a choice — don't put a chooser in front of someone whose
            // second account has been deactivated.
            if (response.Profiles.Count <= 1) response.Profiles.Clear();
            return response;
        }

        private static LoginResponse BuildLoginResponse(UserMaster user) => new()
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

        public async Task<bool> Logout(int userId)
        {
            bool revoked = await _tokenRepository.RevokeAllUserTokens(userId);
            // Revoking refresh tokens only stops the session being EXTENDED. The access token
            // already issued is a signed JWT that keeps working until it expires, so the session
            // generation has to move too - that is what actually ends the session, in the tab that
            // asked and in every other tab or device holding a copy of the token.
            await _userRepository.BumpSessionEpoch(userId);
            return revoked;
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

            // The message event is named for the template the admin edits, not for the OTP record
            // type: ForgotOtpType is "ForgotPassword", which matches no row, so this path was
            // sending the hard-coded fallback while the "Forgot Password OTP" template the admin
            // had written sat unused — and the same OTP arrived worded differently depending on
            // which screen asked for it.
            _ = _messageService.SendEventAsync("Forgot Password OTP", user.UserMasterID, mobile,
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
