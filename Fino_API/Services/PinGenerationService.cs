using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PinGeneration;
using FintechApp.Responses.PinGeneration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PinGenerationService : IPinGenerationService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMPinHistoryRepository _mpinHistoryRepo;
        private readonly IOTPMasterRepository _otpRepo;
        private readonly IOTPService _otpService;
        private readonly EncryptDecrypt _encryptDecrypt;
        private readonly Logger _logger;

        private const string PinOTPType = "PinOTP";
        private const string ForgotMpinOTPType = "ForgotMpinOTP";

        public PinGenerationService(
            IUserRepository userRepo,
            IMPinHistoryRepository mpinHistoryRepo,
            IOTPMasterRepository otpRepo,
            IOTPService otpService,
            IConfiguration configuration)
        {
            _userRepo = userRepo;
            _mpinHistoryRepo = mpinHistoryRepo;
            _otpRepo = otpRepo;
            _otpService = otpService;
            _logger = new Logger();
            _encryptDecrypt = new EncryptDecrypt(
                configuration["Encryption:Key"] ?? string.Empty,
                configuration["Encryption:IV"] ?? string.Empty);
        }

        public async Task<string> GeneratePinOTP(PinGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return Messages.Common.Message.InvalidRequest;

            string hashedPassword = _encryptDecrypt.HashPassword(request.Password);
            var user = await _userRepo.GetUserByMobileWithPassword(request.UserName, hashedPassword);
            if (user == null) return "Invalid username or password.";

            if (request.Pin != request.ConfirmPin) return "PIN and Confirm PIN do not match.";
            if (IsInvalidPin(request.Pin)) return "PIN must not be repetitive or sequential.";

            // Check daily pin change limit and reuse
            var history = await _mpinHistoryRepo.GetByUserID(user.UserMasterID);
            string pinCheck = ValidatePinChange(history, request.Pin, request.DeviceID);
            if (!pinCheck.EqualsIgnoreCase(StatusType.Ok)) return pinCheck;

            await _mpinHistoryRepo.Insert(new MPinHistory
            {
                UserID = user.UserMasterID,
                Mpin = request.Pin,
                DeviceID = request.DeviceID,
                Device = request.Device
            });

            if (!user.IsActive) return "User account is inactive.";

            _ = Task.Run(() => _otpService.SendOtp(user.UserMasterID, PinOTPType));
            return StatusType.Ok;
        }

        public async Task<string> ResendPinOTP(PinGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return Messages.Common.Message.InvalidRequest;

            string hashedPassword = _encryptDecrypt.HashPassword(request.Password);
            var user = await _userRepo.GetUserByMobileWithPassword(request.UserName, hashedPassword);
            if (user == null) return "Invalid username or password.";

            if (request.Pin != request.ConfirmPin) return "PIN and Confirm PIN do not match.";
            if (IsInvalidPin(request.Pin)) return "PIN must not be repetitive or sequential.";

            if (!user.IsActive) return "User account is inactive.";

            _ = Task.Run(() => _otpService.SendOtp(user.UserMasterID, PinOTPType));
            return StatusType.Ok;
        }

        public async Task<(string failureMessage, VerifyPinOTPResponse? response)> VerifyPinOTP(PinVerifyRequest request)
        {
            var user = await _userRepo.GetUserByMobile(request.MobileNumber);
            if (user == null) return ("User not found.", null);

            var otpRecord = await _otpRepo.GetActiveByUserIdAndType(user.UserMasterID, PinOTPType);
            if (otpRecord == null || otpRecord.OTP != request.OTP)
                return ("Invalid or expired OTP.", null);

            otpRecord.IsVerified = true;
            await _otpRepo.Update(otpRecord);

            user.Mpin = request.Pin;
            await _userRepo.UpdateUser(user);

            return (StatusType.Ok, new VerifyPinOTPResponse { FirmName = user.FirmName });
        }

        public async Task<string> ForgotMpinGenerateOTP(ForgotMpinRequest request)
        {
            var user = await _userRepo.GetUserByMobile(request.MobileNumber);
            if (user == null) return "User not found.";

            // PAN must match for identity verification
            if (!user.PANNo.Equals(request.PANNo, StringComparison.OrdinalIgnoreCase))
                return "PAN verification failed.";

            _ = Task.Run(() => _otpService.SendOtp(user.UserMasterID, ForgotMpinOTPType));
            return StatusType.Ok;
        }

        private static string ValidatePinChange(List<MPinHistory> history, int newPin, string deviceId)
        {
            int today = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
            int changesToday = history.Count(h =>
                int.Parse(h.DateTime.ToString("yyyyMMdd")) == today && h.DeviceID == deviceId);
            if (changesToday >= 3) return "Daily PIN change limit reached (max 3).";
            if (history.Any(h => h.Mpin == newPin && h.DeviceID == deviceId)) return "This PIN has been used before.";
            return StatusType.Ok;
        }

        private static bool IsInvalidPin(int pin)
        {
            string s = pin.ToString();
            if (s.Length < 4) return false;
            bool repetitive = s.All(c => c == s[0]);
            bool successive = s.Zip(s.Skip(1), (a, b) => b - a).All(diff => diff == 1);
            return repetitive || successive;
        }
    }
}
