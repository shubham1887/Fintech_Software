using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DMTSender;
using FintechApp.Responses.DMTSender;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class DMTSenderFlowService : IDMTSenderFlowService
    {
        private readonly ISenderRepository _senderRepo;
        private readonly ISenderOTPRepository _senderOtpRepo;
        private readonly IBeneficiaryRepository _beneficiaryRepo;
        private readonly IDMTSettingRepository _dmtSettingRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMessageService _messageService;
        private readonly IMessageSettingRepository _messageSettingRepo;
        private readonly Logger _logger;

        private const string SenderRegistrationMsgType = "SenderRegistration";
        private const string DmtOtpSettingKey = "DMTOTP";
        private const string DmtSenderLimitKey = "DMTSenderLimit";
        // Admin-configured display / per-transfer cap shown on the DMT page (Key Setting).
        private const string ShowLimitInDmtKey = "ShowLimitInDMT";
        private const decimal DefaultSenderLimit = 25000m;

        public DMTSenderFlowService(
            ISenderRepository senderRepo,
            ISenderOTPRepository senderOtpRepo,
            IBeneficiaryRepository beneficiaryRepo,
            IDMTSettingRepository dmtSettingRepo,
            ISettingMasterRepository settingRepo,
            IUserRepository userRepo,
            IMessageService messageService,
            IMessageSettingRepository messageSettingRepo)
        {
            _senderRepo = senderRepo;
            _senderOtpRepo = senderOtpRepo;
            _beneficiaryRepo = beneficiaryRepo;
            _dmtSettingRepo = dmtSettingRepo;
            _settingRepo = settingRepo;
            _userRepo = userRepo;
            _messageService = messageService;
            _messageSettingRepo = messageSettingRepo;
            _logger = new Logger();
        }

        public async Task<(string result, string failureMessage)> AddSender(AddSenderRequest request, int userId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Mobile))
                return (string.Empty, Messages.Common.Message.InvalidRequest);

            try
            {
                var all = await _senderRepo.GetAll();
                var existing = all.FirstOrDefault(s => s.Mobile == request.Mobile);

                if (existing != null)
                {
                    if (!existing.IsOTPVerified) return ("VERIFY_OTP", StatusType.Ok);
                    return (string.Empty, "Sender mobile number already registered.");
                }

                bool requireOtp = await IsSenderOTPRequired();

                var sender = new Sender
                {
                    Name = request.Name,
                    Mobile = request.Mobile,
                    Address = request.Address,
                    UserID = userId,
                    GUID = Guid.NewGuid().ToString(),
                    IsActive = true,
                    IsOTPVerified = !requireOtp,
                    IsKYC = false,
                    IPAddress = ipAddress,
                    KYCDocumentURL = string.Empty,
                    Pincode = request.PinCode,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow
                };

                int rows = await _senderRepo.Insert(sender);
                if (rows <= 0) return (string.Empty, "Failed to register sender.");

                if (requireOtp)
                {
                    await SendSenderOTP(userId, sender);
                    return ("VERIFY_OTP", StatusType.Ok);
                }

                return ("Sender registered successfully.", StatusType.Ok);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (string.Empty, Messages.Common.Message.ExceptionOccured);
            }
        }

        public async Task<string> VerifyOTP(VerifySenderOTPRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.Mobile) || string.IsNullOrWhiteSpace(request.OTP))
                return Messages.Common.Message.InvalidRequest;

            try
            {
                var all = await _senderRepo.GetAll();
                var sender = all.FirstOrDefault(s => s.Mobile == request.Mobile);
                if (sender == null) return "Sender not found.";
                if (sender.IsOTPVerified) return "Sender already verified.";

                var otpList = await _senderOtpRepo.GetByUserID(userId);
                var senderOtp = otpList
                    .Where(o => o.SenderID == sender.SenderID && !o.IsOTPVerified)
                    .OrderByDescending(o => o.DateTime)
                    .FirstOrDefault();

                if (senderOtp == null) return "OTP not found. Please request a new OTP.";
                if (senderOtp.OTP != request.OTP) return "Invalid OTP.";

                senderOtp.IsOTPVerified = true;
                await _senderOtpRepo.Update(senderOtp);

                sender.IsOTPVerified = true;
                await _senderRepo.Update(sender);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        public async Task<string> ResendOTP(ResendSenderOTPRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.Mobile)) return Messages.Common.Message.InvalidRequest;
            try
            {
                var all = await _senderRepo.GetAll();
                var sender = all.FirstOrDefault(s => s.Mobile == request.Mobile);
                if (sender == null) return "Sender not found.";
                await SendSenderOTP(userId, sender);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        public async Task<(string failureMessage, SenderDetailsResponse? response)> GetSender(SelectSenderRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.Mobile)) return (Messages.Common.Message.InvalidRequest, null);
            try
            {
                var user = await _userRepo.GetUserById(userId);
                if (user == null) return ("User not found.", null);
                if (!user.IsKYCApproved) return ("KYC not approved.", null);

                var all = await _senderRepo.GetAll();
                var sender = all.FirstOrDefault(s => s.Mobile == request.Mobile);
                if (sender == null) return ("Sender not registered.", null);
                if (!sender.IsOTPVerified) return ("Sender OTP not verified.", null);

                decimal senderLimit = await GetSenderLimit(request.ProviderID);
                decimal displayLimit = await GetShowLimit();

                return (StatusType.Ok, new SenderDetailsResponse
                {
                    SenderGUID = sender.GUID,
                    Name = sender.Name,
                    Mobile = sender.Mobile,
                    Address = sender.Address,
                    Consumed = 0,
                    RemainingLimit = senderLimit,
                    DisplayLimit = displayLimit
                });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<(string failureMessage, List<SenderByAccountResponse> response)> GetByAccountNumber(SelectSenderByAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return (Messages.Common.Message.InvalidRequest, new());
            try
            {
                var all = await _beneficiaryRepo.GetAll();
                var matches = all
                    .Where(b => b.AccountNo == request.AccountNumber && b.IsActive)
                    .Select(b => new SenderByAccountResponse
                    {
                        Name = b.Name,
                        AccountNo = b.AccountNo,
                        IFSC = b.IFSC,
                        BankName = b.BankName,
                        SenderMobile = b.SenderMobile,
                        GUID = b.GUID
                    }).ToList();

                if (!matches.Any()) return ("No beneficiary found for this account number.", new());
                return (StatusType.Ok, matches);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, new());
            }
        }

        private async Task SendSenderOTP(int userId, Sender sender)
        {
            string otp = OtpHelper.GenerateOtp();
            var otpEntity = new SenderOTP
            {
                Mobile = sender.Mobile,
                OTP = otp,
                SenderID = sender.SenderID,
                UserID = userId,
                IsOTPVerified = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            await _senderOtpRepo.Insert(otpEntity);

            var setting = await _messageSettingRepo.GetByType(SenderRegistrationMsgType);

            _ = Task.Run(async () =>
            {
                try
                {
                    string msg = (setting?.SmsMessage ?? "Your OTP for sender registration is [OTP].")
                        .Replace("[OTP]", otp).Replace("[FirmName]", sender.Name);
                    if (setting == null || setting.IsSMS)
                        await _messageService.SendOtpSmsAsync(userId, sender.Mobile, msg, setting?.TemplateID ?? string.Empty, SenderRegistrationMsgType);

                    if (setting?.IsWhatsUp == true)
                    {
                        string wpMsg = (string.IsNullOrWhiteSpace(setting.WhatsUpMessage) ? setting.SmsMessage : setting.WhatsUpMessage)
                            .Replace("[OTP]", otp).Replace("[FirmName]", sender.Name);
                        await _messageService.SendWhatsAppAsync(userId, sender.Mobile, sender.Mobile, wpMsg, setting.TemplateID, SenderRegistrationMsgType);
                    }
                }
                catch (Exception ex) { _logger.Write(ex); }
            });
        }

        private async Task<bool> IsSenderOTPRequired()
        {
            var setting = await _settingRepo.GetByKey(DmtOtpSettingKey);
            return setting == null || !setting.Value.Equals("BeneOnly", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<decimal> GetSenderLimit(int providerId)
        {
            if (providerId > 0)
            {
                var dmtSettings = await _dmtSettingRepo.GetAll();
                var activeSetting = dmtSettings.FirstOrDefault(d => d.IsActive);
                if (activeSetting != null && activeSetting.SenderLimit >= 1000)
                    return activeSetting.SenderLimit;
            }

            var globalSetting = await _settingRepo.GetByKey(DmtSenderLimitKey);
            if (globalSetting != null && decimal.TryParse(globalSetting.Value, out decimal limit))
                return limit;

            return DefaultSenderLimit;
        }

        // 'ShowLimitInDMT' (Key Setting) — a numeric per-transfer cap the DMT page shows & enforces.
        // Honoured only when the row is On and holds a positive number; 0 ⇒ feature off (no extra cap).
        private async Task<decimal> GetShowLimit()
        {
            var setting = await _settingRepo.GetByKey(ShowLimitInDmtKey);
            if (setting != null && setting.IsOn && decimal.TryParse(setting.Value, out decimal cap) && cap > 0)
                return cap;
            return 0m;
        }
    }
}
