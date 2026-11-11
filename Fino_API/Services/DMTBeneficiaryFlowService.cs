using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    public class DMTBeneficiaryFlowService : IDMTBeneficiaryFlowService
    {
        private readonly ISenderRepository _senderRepo;
        private readonly IBeneficiaryRepository _beneficiaryRepo;
        private readonly IBeneficiaryOTPRepository _beneficiaryOtpRepo;
        private readonly IBlackListBankMasterRepository _blackListRepo;
        private readonly IDMTBankRepository _dmtBankRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IMessageService _messageService;
        private readonly IMessageSettingRepository _messageSettingRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDmtGatewayModeService _mode;
        private readonly Logger _logger;

        private const string BeneficiaryRegMsgType = "BeneficiaryRegistration";
        private const string DmtOtpSettingKey = "DMTOTP";
        // Explicit admin switch (Settings → Key Setting). true ⇒ require OTP when adding a beneficiary.
        private const string IsBeneficiaryOtpSettingKey = "IsBeneficiaryOTP";

        public DMTBeneficiaryFlowService(
            ISenderRepository senderRepo,
            IBeneficiaryRepository beneficiaryRepo,
            IBeneficiaryOTPRepository beneficiaryOtpRepo,
            IBlackListBankMasterRepository blackListRepo,
            IDMTBankRepository dmtBankRepo,
            ISettingMasterRepository settingRepo,
            IMessageService messageService,
            IMessageSettingRepository messageSettingRepo,
            IHttpClientFactory httpClientFactory,
            IDmtGatewayModeService mode)
        {
            _mode = mode;
            _senderRepo = senderRepo;
            _beneficiaryRepo = beneficiaryRepo;
            _beneficiaryOtpRepo = beneficiaryOtpRepo;
            _blackListRepo = blackListRepo;
            _dmtBankRepo = dmtBankRepo;
            _settingRepo = settingRepo;
            _messageService = messageService;
            _messageSettingRepo = messageSettingRepo;
            _httpClientFactory = httpClientFactory;
            _logger = new Logger();
        }

        public async Task<(string failureMessage, AddBeneficiaryResponse? response)> AddBeneficiary(AddBeneficiaryRequest request, int userId, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.SenderGUID) || string.IsNullOrWhiteSpace(request.AccountNo) || string.IsNullOrWhiteSpace(request.IFSC))
                return (Messages.Common.Message.InvalidRequest, null);

            try
            {
                // Blacklist check
                var blackList = await _blackListRepo.GetAll();
                bool isBlackListed = blackList.Any(b =>
                    b.IFSC.Equals(request.IFSC, StringComparison.OrdinalIgnoreCase) ||
                    b.BankName.Equals(request.BankName, StringComparison.OrdinalIgnoreCase));
                if (isBlackListed) return ("This bank/IFSC is blacklisted.", null);

                // Get sender by GUID
                var allSenders = await _senderRepo.GetAll();
                var sender = allSenders.FirstOrDefault(s => s.GUID == request.SenderGUID && !s.IsDeleted);
                if (sender == null) return ("Invalid sender.", null);
                if (!sender.IsOTPVerified && !await _mode.IsPayoutDmt()) return ("Sender OTP not verified. Please verify sender first.", null);

                // Check if beneficiary already exists
                var allBeneficiaries = await _beneficiaryRepo.GetAll();
                var existing = allBeneficiaries.FirstOrDefault(b =>
                    b.AccountNo == request.AccountNo &&
                    b.IFSC.Equals(request.IFSC, StringComparison.OrdinalIgnoreCase) &&
                    b.SenderID == sender.SenderID &&
                    !b.IsDeleted);

                if (existing != null)
                {
                    if (!existing.IsOTPVerified)
                    {
                        // Re-adding an unverified beneficiary → resend a fresh OTP so the user can
                        // finish verification (previously it returned "verify OTP" without sending one).
                        if (await IsOTPRequiredForBeneficiary())
                            await SendBeneficiaryOTP(existing, sender, userId);
                        return (StatusType.Ok, new AddBeneficiaryResponse { BeneficiaryGUID = existing.GUID, Message = "Verify OTP to activate beneficiary." });
                    }
                    return ("Beneficiary already registered.", null);
                }

                bool requireOtp = await IsOTPRequiredForBeneficiary();

                var bene = new Beneficiary
                {
                    Name = request.Name,
                    BankName = request.BankName,
                    AccountNo = request.AccountNo,
                    IFSC = request.IFSC,
                    SenderMobile = request.SenderMobile,
                    SenderID = sender.SenderID,
                    UserID = userId,
                    GUID = Guid.NewGuid().ToString(),
                    IPAddress = ipAddress,
                    IsActive = true,
                    IsOTPVerified = !requireOtp,
                    IsNameVerified = request.IsNameVerified,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow
                };

                int rows = await _beneficiaryRepo.Insert(bene);
                if (rows <= 0) return ("Failed to add beneficiary.", null);
                // EF populates the real identity onto bene.BeneficiaryID after Insert. Do NOT
                // overwrite it with the rows-affected count — that gave the OTP row a wrong
                // BeneficiaryID (and corrupted the tracked PK), so verification never matched.

                if (requireOtp)
                {
                    await SendBeneficiaryOTP(bene, sender, userId);
                    return (StatusType.Ok, new AddBeneficiaryResponse { BeneficiaryGUID = bene.GUID, Message = "Verify OTP to activate beneficiary." });
                }

                return (StatusType.Ok, new AddBeneficiaryResponse { BeneficiaryGUID = bene.GUID, Message = "Beneficiary added successfully." });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<string> VerifyOTP(VerifyBeneficiaryOTPRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.BeneficiaryGUID) || string.IsNullOrWhiteSpace(request.OTP))
                return Messages.Common.Message.InvalidRequest;
            try
            {
                var all = await _beneficiaryRepo.GetAll();
                var bene = all.FirstOrDefault(b => b.GUID == request.BeneficiaryGUID);
                if (bene == null) return "Beneficiary not found.";
                if (bene.IsOTPVerified) return "Beneficiary already verified.";

                var otpList = await _beneficiaryOtpRepo.GetByUserID(userId);
                var otpRecord = otpList
                    .Where(o => o.BeneficiaryID == bene.BeneficiaryID && !o.IsOTPVerified)
                    .OrderByDescending(o => o.DateTime)
                    .FirstOrDefault();

                if (otpRecord == null) return "OTP not found. Please request a new OTP.";
                if (otpRecord.OTP != request.OTP) return "Invalid OTP.";

                otpRecord.IsOTPVerified = true;
                await _beneficiaryOtpRepo.Update(otpRecord);

                bene.IsOTPVerified = true;
                await _beneficiaryRepo.Update(bene);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        public async Task<string> ResendOTP(ResendBeneficiaryOTPRequest request, int userId)
        {
            if (string.IsNullOrWhiteSpace(request.BeneficiaryGUID)) return Messages.Common.Message.InvalidRequest;
            try
            {
                var all = await _beneficiaryRepo.GetAll();
                var bene = all.FirstOrDefault(b => b.GUID == request.BeneficiaryGUID);
                if (bene == null) return "Beneficiary not found.";

                var allSenders = await _senderRepo.GetAll();
                var sender = allSenders.FirstOrDefault(s => s.SenderID == bene.SenderID);
                if (sender == null) return "Sender not found.";

                await SendBeneficiaryOTP(bene, sender, userId);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        public async Task<(string failureMessage, List<BeneficiaryDetailResponse> response)> GetBySender(SelectBeneficiaryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SenderGUID)) return (Messages.Common.Message.InvalidRequest, new());
            try
            {
                var allSenders = await _senderRepo.GetAll();
                var sender = allSenders.FirstOrDefault(s => s.GUID == request.SenderGUID);
                if (sender == null) return ("Invalid sender.", new());
                if (!sender.IsOTPVerified && !await _mode.IsPayoutDmt()) return ("Sender OTP not verified.", new());

                var all = await _beneficiaryRepo.GetAll();
                var list = all.Where(b => b.SenderID == sender.SenderID && b.IsActive && !b.IsDeleted).ToList();
                if (!list.Any()) return ("No beneficiaries found.", new());

                var result = list.Select(b => new BeneficiaryDetailResponse
                {
                    Name = b.Name,
                    BankName = b.BankName,
                    AccountNo = b.AccountNo,
                    IFSC = b.IFSC,
                    SenderGUID = sender.GUID,
                    BeneficiaryGUID = b.GUID,
                    SenderMobile = b.SenderMobile,
                    // "Verified" in the retailer UI means the beneficiary completed OTP activation
                    // (IsOTPVerified) — that is what gates money-out. IsNameVerified is a separate
                    // penny-drop name-match flag that this flow never sets.
                    IsVerified = b.IsOTPVerified
                }).ToList();

                return (StatusType.Ok, result);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, new());
            }
        }

        public async Task<(string failureMessage, BeneficiaryDetailResponse? response)> GetByGUID(SelectBeneficiaryByGUIDRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BeneficiaryGUID)) return (Messages.Common.Message.InvalidRequest, null);
            try
            {
                var all = await _beneficiaryRepo.GetAll();
                var bene = all.FirstOrDefault(b => b.GUID == request.BeneficiaryGUID);
                if (bene == null) return ("Beneficiary not found.", null);

                var allSenders = await _senderRepo.GetAll();
                var sender = allSenders.FirstOrDefault(s => s.SenderID == bene.SenderID);
                if (sender == null) return ("Sender not found.", null);
                if (!sender.IsOTPVerified && !await _mode.IsPayoutDmt()) return ("Sender OTP not verified.", null);

                return (StatusType.Ok, new BeneficiaryDetailResponse
                {
                    Name = bene.Name,
                    BankName = bene.BankName,
                    AccountNo = bene.AccountNo,
                    IFSC = bene.IFSC,
                    SenderGUID = sender.GUID,
                    BeneficiaryGUID = bene.GUID,
                    SenderMobile = bene.SenderMobile,
                    IsVerified = bene.IsOTPVerified
                });
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, null);
            }
        }

        public async Task<string> Delete(DeleteBeneficiaryByGUIDRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BeneficiaryGUID)) return Messages.Common.Message.InvalidRequest;
            try
            {
                var all = await _beneficiaryRepo.GetAll();
                var bene = all.FirstOrDefault(b => b.GUID == request.BeneficiaryGUID);
                if (bene == null) return "Beneficiary not found.";

                int result = await _beneficiaryRepo.Delete(bene.BeneficiaryID);
                return result > 0 ? StatusType.Ok : "Failed to delete beneficiary.";
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        public async Task<(string failureMessage, CheckBranchResponse response)> CheckBranch(CheckBranchRequest request)
        {
            var result = new CheckBranchResponse { Status = "Failure" };
            if (string.IsNullOrWhiteSpace(request.IFSC)) return (Messages.Common.Message.InvalidRequest, result);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var httpResponse = await client.GetAsync($"https://ifsc.razorpay.com/{request.IFSC}");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    result.Status = StatusType.Ok;
                    return (StatusType.Ok, result);
                }

                string json = await httpResponse.Content.ReadAsStringAsync();
                dynamic apiResponse = JsonConvert.DeserializeObject(json)!;

                result.Branch = (string?)apiResponse.BRANCH ?? string.Empty;
                result.Address = (string?)apiResponse.ADDRESS ?? string.Empty;
                result.BankName = (string?)apiResponse.BANK ?? string.Empty;

                // Match against known DMT banks
                var banks = await _dmtBankRepo.GetAll();
                foreach (var bank in banks)
                {
                    if (bank.Name.Contains(result.BankName, StringComparison.OrdinalIgnoreCase) ||
                        result.BankName.Contains(bank.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.IFSC = bank.DefaultIFSC;
                        result.BankName = bank.Name;
                        break;
                    }
                }

                result.Status = StatusType.Ok;
                return (StatusType.Ok, result);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, result);
            }
        }

        private async Task SendBeneficiaryOTP(Beneficiary bene, Sender sender, int userId)
        {
            string otp = OtpHelper.GenerateOtp();
            await _beneficiaryOtpRepo.Insert(new BeneficiaryOTP
            {
                SenderMobile = bene.SenderMobile,
                OTP = otp,
                SenderID = bene.SenderID,
                BeneficiaryID = bene.BeneficiaryID,
                UserID = userId,
                IsOTPVerified = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            });

            var setting = await _messageSettingRepo.GetByType(BeneficiaryRegMsgType);

            _ = Task.Run(async () =>
            {
                try
                {
                    string msg = FillOtpTokens(setting?.SmsMessage ?? "Your OTP for beneficiary registration is {OTP}.", otp, sender.Name);
                    if (setting == null || setting.IsSMS)
                        await _messageService.SendOtpSmsAsync(userId, bene.SenderMobile, msg, setting?.TemplateID ?? string.Empty, BeneficiaryRegMsgType);

                    if (setting?.IsWhatsUp == true)
                    {
                        string wpMsg = FillOtpTokens(
                            string.IsNullOrWhiteSpace(setting.WhatsUpMessage) ? setting.SmsMessage : setting.WhatsUpMessage,
                            otp, sender.Name);
                        await _messageService.SendWhatsAppAsync(userId, bene.SenderMobile, bene.SenderMobile, wpMsg, setting.TemplateID, BeneficiaryRegMsgType);
                    }
                }
                catch (Exception ex) { _logger.Write(ex); }
            });
        }

        /// <summary>
        /// Fill the two event tokens this template has, in either spelling — the Message Settings
        /// screen's placeholder chips insert {OTP} / {FirmName} while only the bracket spelling was
        /// ever replaced here. See the same helper in DMTSenderFlowService.
        /// </summary>
        private static string FillOtpTokens(string template, string otp, string firmName) =>
            (template ?? string.Empty)
                .Replace("[OTP]", otp).Replace("{OTP}", otp)
                .Replace("[FirmName]", firmName).Replace("{FirmName}", firmName);

        private async Task<bool> IsOTPRequiredForBeneficiary()
        {
            // A payout rail keeps no beneficiary registration, so there is nothing for an OTP to
            // activate — it would only block a transfer the rail would have accepted. This check
            // comes first and overrides the Key Settings below.
            if (await _mode.IsPayoutDmt()) return false;

            // Preferred explicit switch (Key Setting): 'IsBeneficiaryOTP' — true ⇒ require OTP, false ⇒ skip.
            var beneOtp = await _settingRepo.GetByKey(IsBeneficiaryOtpSettingKey);
            if (beneOtp != null) return ParseBool(beneOtp.Value, beneOtp.IsOn);

            // Legacy fallback: shared 'DMTOTP' key ('SenderOnly' ⇒ skip beneficiary OTP).
            var setting = await _settingRepo.GetByKey(DmtOtpSettingKey);
            return setting == null || !setting.Value.Equals("SenderOnly", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            value = value.Trim();
            if (bool.TryParse(value, out bool parsed)) return parsed;
            return value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("on", StringComparison.OrdinalIgnoreCase)
                || value.Equals("y", StringComparison.OrdinalIgnoreCase);
        }
    }
}
