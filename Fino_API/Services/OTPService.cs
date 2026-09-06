using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OTPService : IOTPService
    {
        private readonly IUserRepository _userRepo;
        private readonly IOTPMasterRepository _otpRepo;
        private readonly IMessageService _messageService;

        public OTPService(
            IUserRepository userRepo,
            IOTPMasterRepository otpRepo,
            IMessageService messageService)
        {
            _userRepo = userRepo;
            _otpRepo = otpRepo;
            _messageService = messageService;
        }

        public async Task<string> SendOtp(int userId, string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "OTP type is required.";

            var user = await _userRepo.GetUserById(userId);
            if (user == null)
                return "User not found.";

            string otp = OtpHelper.GenerateOtp();
            await _otpRepo.Insert(new OTPMaster
            {
                OTP = otp,
                UserID = userId,
                Type = type
            });

            _ = _messageService.SendEventAsync(type, userId, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string>
                {
                    ["OTP"] = otp,
                    ["FirmName"] = user.FirmName ?? string.Empty,
                    ["CustomerName"] = user.PersonName ?? string.Empty,
                },
                fallbackSms: $"Your OTP is {otp}. Do not share it with anyone.", isOtpDefault: true);

            return StatusType.Ok;
        }
    }
}
