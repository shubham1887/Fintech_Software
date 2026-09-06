using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.OutsideRegistration;
using FintechApp.Requests.User;
using FintechApp.Responses.PendingRegistration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PendingRegistrationService : IPendingRegistrationService
    {
        private const string OtpType = "RegistrationOTP";

        private readonly IPendingRegistrationRepository _repo;
        private readonly IOTPMasterRepository _otpRepo;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly IStateMasterService _stateService;
        private readonly IUserTypeMasterService _userTypeService;

        public PendingRegistrationService(
            IPendingRegistrationRepository repo,
            IOTPMasterRepository otpRepo,
            IMessageService messageService,
            IUserService userService,
            IUserRepository userRepo,
            IStateMasterService stateService,
            IUserTypeMasterService userTypeService)
        {
            _repo = repo;
            _otpRepo = otpRepo;
            _messageService = messageService;
            _userService = userService;
            _userRepo = userRepo;
            _stateService = stateService;
            _userTypeService = userTypeService;
        }

        public async Task<string> GenerateOtp(string mobile)
        {
            mobile = (mobile ?? string.Empty).Trim();
            if (mobile.Length != 10 || !mobile.All(char.IsDigit))
                return "Please enter a valid 10-digit mobile number.";

            // A mobile that already belongs to an account can't self-register.
            var existing = await _userRepo.GetUserByMobile(mobile);
            if (existing != null)
                return "This mobile number is already registered. Please sign in or use Forgot Password.";

            string otp = OtpHelper.GenerateOtp();
            await _otpRepo.Insert(new OTPMaster
            {
                OTP = otp,
                UserID = 0,
                Type = OtpType,
                MobileNumber = mobile,
                IsVerified = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            });

            _ = _messageService.SendEventAsync(OtpType, 0, mobile, mobile, null,
                new Dictionary<string, string> { ["OTP"] = otp },
                fallbackSms: $"Your OTP for registration is {otp}. Do not share it with anyone.",
                isOtpDefault: true);

            return StatusType.Ok;
        }

        public async Task<string> VerifyOtp(string mobile, string otp)
        {
            mobile = (mobile ?? string.Empty).Trim();
            otp = (otp ?? string.Empty).Trim();
            if (mobile.Length == 0 || otp.Length == 0)
                return Messages.Common.Message.InvalidRequest;

            var record = await _otpRepo.GetActiveByMobileAndType(mobile, OtpType);
            if (record == null)
                return "OTP has expired or was not requested. Please resend.";
            if (record.OTP != otp)
                return "Incorrect OTP. Please try again.";

            record.IsVerified = true;
            record.UpdatedateTime = DateTime.UtcNow;
            await _otpRepo.Update(record);
            return StatusType.Ok;
        }

        public async Task<string> Register(OutsideRegisterRequest request)
        {
            if (request == null) return Messages.Common.Message.InvalidRequest;

            string mobile = (request.MobileNumber ?? string.Empty).Trim();
            if (mobile.Length != 10 || !mobile.All(char.IsDigit))
                return "Please enter a valid 10-digit mobile number.";
            if (string.IsNullOrWhiteSpace(request.PersonName))
                return "Please enter your name.";
            if (request.UserTypeID <= 0)
                return "Please select a role.";

            // Mobile must have a verified OTP on record for this sign-up.
            var otpRows = await _otpRepo.GetAll();
            bool mobileVerified = otpRows.Any(x => x.MobileNumber == mobile && x.Type == OtpType && x.IsVerified);
            if (!mobileVerified)
                return "Please verify your mobile number with OTP before submitting.";

            // Duplicate checks — existing users first, then the pending queue.
            string? dupUser = await _repo.FindDuplicateInUsers(mobile, request.EmailID, request.PANNo, request.AadharNo, request.GSTNo);
            if (dupUser != null)
                return $"{dupUser} already exists — an account with these details is already registered.";

            string? dupPending = await _repo.FindDuplicateInPending(mobile, request.EmailID, request.PANNo, request.AadharNo, request.GSTNo);
            if (dupPending != null)
                return $"{dupPending} already exists — a registration with these details is already awaiting approval.";

            var entity = new PendingRegistration
            {
                PersonName = request.PersonName.Trim(),
                FirmName = (request.FirmName ?? string.Empty).Trim(),
                MobileNumber = mobile,
                WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber) ? mobile : request.WhatsAppNumber.Trim(),
                EmailID = (request.EmailID ?? string.Empty).Trim(),
                PANNo = (request.PANNo ?? string.Empty).Trim(),
                AadharNo = (request.AadharNo ?? string.Empty).Trim(),
                GSTNo = (request.GSTNo ?? string.Empty).Trim(),
                Address = (request.Address ?? string.Empty).Trim(),
                CityName = (request.CityName ?? string.Empty).Trim(),
                District = (request.District ?? string.Empty).Trim(),
                Pincode = (request.Pincode ?? string.Empty).Trim(),
                StateID = request.StateID,
                UserTypeID = request.UserTypeID,
                ReferralCode = (request.ReferralCode ?? string.Empty).Trim(),
                Status = "Pending",
                IsMobileVerified = true,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repo.Insert(entity);
            return result > 0 ? StatusType.Ok : "Registration could not be saved. Please try again.";
        }

        public async Task<List<PendingRegistrationResponse>> GetAll()
        {
            var list = await _repo.GetAll();
            return await MapMany(list);
        }

        public async Task<PendingRegistrationResponse?> GetByID(int id)
        {
            var e = await _repo.GetByID(id);
            if (e == null) return null;
            return (await MapMany(new List<PendingRegistration> { e })).FirstOrDefault();
        }

        public async Task<string> Approve(int id, int currentUserId, string ipAddress)
        {
            var entity = await _repo.GetByID(id);
            if (entity == null) return "Registration not found.";
            if (!entity.Status.EqualsIgnoreCase("Pending"))
                return $"This registration is already {entity.Status.ToLower()}.";

            string tempPassword = OtpHelper.GetRandomPassword();

            var insertRequest = new UserInsertRequest
            {
                FirmName = string.IsNullOrWhiteSpace(entity.FirmName) ? entity.PersonName : entity.FirmName,
                PersonName = entity.PersonName,
                MobileNumber = entity.MobileNumber,
                EmailID = entity.EmailID,
                Password = tempPassword,
                UserTypeID = entity.UserTypeID,
                ParentID = currentUserId,
                Address = entity.Address,
                CityName = entity.CityName,
                Pincode = entity.Pincode,
                StateID = entity.StateID,
                GSTNo = entity.GSTNo,
                PANNo = entity.PANNo,
                AadharNo = entity.AadharNo,
                WhatsAppNumber = entity.WhatsAppNumber
            };

            string createResult = await _userService.Insert(insertRequest, currentUserId, ipAddress ?? string.Empty);
            if (!createResult.EqualsIgnoreCase(StatusType.Ok))
                return createResult; // e.g. "Mobile number already registered." — surfaces the reason.

            var createdUser = await _userRepo.GetUserByMobile(entity.MobileNumber);

            entity.Status = "Approved";
            entity.CreatedUserID = createdUser?.UserMasterID;
            entity.ActionByUserID = currentUserId;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repo.Update(entity);

            // Text the login credentials to the new user (email too, if present).
            string firm = string.IsNullOrWhiteSpace(entity.FirmName) ? entity.PersonName : entity.FirmName;
            string msg = $"Welcome to {firm}. Your login mobile is {entity.MobileNumber} and temporary password is {tempPassword}. Please change it after first login.";
            _ = _messageService.SendEventAsync("New Login Credentials", createdUser?.UserMasterID ?? 0,
                entity.MobileNumber, entity.WhatsAppNumber, entity.EmailID,
                new Dictionary<string, string>
                {
                    ["Password"] = tempPassword,
                    ["Number"] = entity.MobileNumber,
                    ["CustomerName"] = entity.PersonName
                },
                fallbackSms: msg);
            if (!string.IsNullOrWhiteSpace(entity.EmailID))
                _ = _messageService.SendEmailAsync(entity.EmailID, msg, "Your account is approved");

            return StatusType.Ok;
        }

        public async Task<string> Reject(int id, string reason, int currentUserId)
        {
            var entity = await _repo.GetByID(id);
            if (entity == null) return "Registration not found.";
            if (!entity.Status.EqualsIgnoreCase("Pending"))
                return $"This registration is already {entity.Status.ToLower()}.";

            entity.Status = "Rejected";
            entity.RejectReason = (reason ?? string.Empty).Trim();
            entity.ActionByUserID = currentUserId;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repo.Update(entity);
            return StatusType.Ok;
        }

        private async Task<List<PendingRegistrationResponse>> MapMany(List<PendingRegistration> list)
        {
            if (list.Count == 0) return new List<PendingRegistrationResponse>();

            var states = await _stateService.GetAll();
            var types = await _userTypeService.GetAll();
            var stateMap = states.ToDictionary(s => s.StateID, s => s.StateName);
            var typeMap = types.ToDictionary(t => t.UserTypeID, t => t.UserType);

            return list.Select(e => new PendingRegistrationResponse
            {
                PendingRegistrationID = e.PendingRegistrationID,
                PersonName = e.PersonName,
                FirmName = e.FirmName,
                MobileNumber = e.MobileNumber,
                WhatsAppNumber = e.WhatsAppNumber,
                EmailID = e.EmailID,
                PANNo = e.PANNo,
                AadharNo = e.AadharNo,
                GSTNo = e.GSTNo,
                Address = e.Address,
                CityName = e.CityName,
                District = e.District,
                Pincode = e.Pincode,
                StateID = e.StateID,
                StateName = e.StateID.HasValue && stateMap.TryGetValue(e.StateID.Value, out var sn) ? sn : string.Empty,
                UserTypeID = e.UserTypeID,
                UserTypeName = typeMap.TryGetValue(e.UserTypeID, out var tn) ? tn : string.Empty,
                ReferralCode = e.ReferralCode,
                Status = e.Status,
                RejectReason = e.RejectReason,
                IsMobileVerified = e.IsMobileVerified,
                CreatedUserID = e.CreatedUserID,
                DateTime = e.DateTime.ToString("yyyy-MM-dd HH:mm")
            }).ToList();
        }
    }
}
