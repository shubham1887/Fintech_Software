using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Kyc;
using FintechApp.Responses.Kyc;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// The admin KYC desk. It reads the SAME two tables the user's own profile writes to —
    /// VerificationDocumentConfig (what must be verified) and UserDocumentMaster (what they
    /// submitted) — so the admin sees exactly what the user submitted, and the user sees exactly
    /// what the admin decided. There is no second copy of the truth anywhere.
    ///
    /// Approval has two routes into it:
    ///   Manual — the admin approves each document (or the user in one action) from this service.
    ///   E-KYC  — a user who cleared the electronic flow (UserMaster.IseKYC) is approved
    ///            automatically; the admin still SEES them here, marked E-KYC, but has nothing
    ///            to do. See <see cref="AutoApproveEkyc"/>.
    /// </summary>
    public class KycApprovalService : IKycApprovalService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserDocumentMasterRepository _userDocumentRepository;
        private readonly IVerificationDocumentConfigRepository _documentConfigRepository;
        private readonly IKYCApprovedDetailsRepository _approvedRepository;
        private readonly IeKYCLogRepository _ekycLogRepository;
        private readonly ISettingMasterRepository _settingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMessageService _messageService;

        public KycApprovalService(
            IUserRepository userRepository,
            IUserDocumentMasterRepository userDocumentRepository,
            IVerificationDocumentConfigRepository documentConfigRepository,
            IKYCApprovedDetailsRepository approvedRepository,
            IeKYCLogRepository ekycLogRepository,
            ISettingMasterRepository settingRepository,
            IServiceRepository serviceRepository,
            IMessageService messageService)
        {
            _userRepository = userRepository;
            _userDocumentRepository = userDocumentRepository;
            _documentConfigRepository = documentConfigRepository;
            _approvedRepository = approvedRepository;
            _ekycLogRepository = ekycLogRepository;
            _settingRepository = settingRepository;
            _serviceRepository = serviceRepository;
            _messageService = messageService;
        }

        // ── Queue ────────────────────────────────────────────────────────────────────

        public async Task<KycUserListResponse> SelectUsers(KycUserListRequest request)
        {
            await ReconcileEkycApprovals();

            var users = (await _userRepository.GetAllUsers())
                .Where(u => u.UserTypeID != UserTypeCode.Admin)
                .ToList();
            var docsByUser = await DocumentsByUser();
            var required = await RequiredCodes();
            var approvals = (await _approvedRepository.GetAll())
                .GroupBy(a => a.UserID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.DateTime).First());
            var userNames = users.ToDictionary(u => u.UserMasterID, u => u.FirmName ?? u.PersonName ?? string.Empty);
            var configCount = (await _documentConfigRepository.GetAll()).Count(c => c.IsActive);

            var rows = users.Select(u =>
            {
                var docs = docsByUser.TryGetValue(u.UserMasterID, out var d) ? d : new List<UserDocumentMaster>();
                approvals.TryGetValue(u.UserMasterID, out var approval);
                string approverName = approval != null && userNames.TryGetValue(approval.ApporvedUserID, out var an)
                    ? an : (approval != null ? "Administrator" : string.Empty);

                return new KycUserRow
                {
                    UserID = u.UserMasterID,
                    PersonName = u.PersonName ?? string.Empty,
                    FirmName = u.FirmName ?? string.Empty,
                    MobileNumber = u.MobileNumber ?? string.Empty,
                    WhatsAppNumber = u.WhatsAppNumber ?? string.Empty,
                    EmailID = u.EmailID ?? string.Empty,
                    UserTypeID = u.UserTypeID ?? 0,
                    UserType = u.UserType?.UserType ?? string.Empty,
                    ParentName = ParentNameOf(u, users),
                    CityName = u.CityName ?? string.Empty,
                    StateName = u.State?.StateName ?? string.Empty,
                    Status = KycStatus.Resolve(u, docs),
                    Mode = u.IseKYC ? "E-KYC" : "Manual",
                    IseKYC = u.IseKYC,
                    IsKYCApproved = u.IsKYCApproved,
                    IsActive = u.IsActive,
                    RequiredCount = required.Count > 0 ? required.Count : configCount,
                    SubmittedCount = docs.Count,
                    ApprovedCount = docs.Count(x => KycStatus.IsDocApproved(x.Status)),
                    RejectedCount = docs.Count(x => KycStatus.IsDocRejected(x.Status)),
                    InReviewCount = docs.Count(x => KycStatus.IsDocInReview(x.Status)),
                    RegisteredOn = u.DateTime,
                    LastSubmittedOn = docs.Count == 0 ? null : docs.Max(x => x.UpdatedateTime > x.DateTime ? x.UpdatedateTime : x.DateTime),
                    ApprovedOn = approval?.DateTime,
                    ApprovedBy = approverName,
                    Remark = approval?.Remark ?? docs.FirstOrDefault(x => KycStatus.IsDocRejected(x.Status))?.Remark ?? string.Empty,
                };
            }).ToList();

            var summary = new KycSummary
            {
                Total = rows.Count,
                Approved = rows.Count(r => r.Status == KycStatus.Approved),
                Pending = rows.Count(r => r.Status == KycStatus.Pending),
                Rejected = rows.Count(r => r.Status == KycStatus.Rejected),
                NotSubmitted = rows.Count(r => r.Status == KycStatus.NotSubmitted),
                EkycVerified = rows.Count(r => r.IseKYC),
            };

            // Filters apply to the rows, not to the summary — the KPI strip always describes the
            // whole network, so switching tabs doesn't make the totals move around.
            var filtered = rows.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.Status) && !request.Status.EqualsIgnoreCase("All"))
            {
                string wanted = request.Status.Replace(" ", string.Empty);
                filtered = filtered.Where(r => r.Status.Replace(" ", string.Empty).EqualsIgnoreCase(wanted));
            }

            if (request.UserTypeID > 0)
                filtered = filtered.Where(r => r.UserTypeID == request.UserTypeID);

            if (DateTime.TryParse(request.FromDate, out var from))
                filtered = filtered.Where(r => r.RegisteredOn >= from.Date);
            if (DateTime.TryParse(request.ToDate, out var to))
                filtered = filtered.Where(r => r.RegisteredOn < to.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.Trim();
                filtered = filtered.Where(r =>
                    r.PersonName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.FirmName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.MobileNumber.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    r.EmailID.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            return new KycUserListResponse
            {
                Summary = summary,
                // Waiting-on-the-admin first, then whatever changed most recently.
                Users = filtered
                    .OrderBy(r => r.Status == KycStatus.Pending ? 0 : r.Status == KycStatus.Rejected ? 1 : 2)
                    .ThenByDescending(r => r.LastSubmittedOn ?? r.RegisteredOn)
                    .ToList(),
            };
        }

        public async Task<(string failureMessage, KycUserDetail? data)> SelectUser(int userID)
        {
            if (userID <= 0) return (Messages.Common.Message.InvalidRequest, null);
            var users = await _userRepository.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserMasterID == userID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);

            var docs = (await _userDocumentRepository.GetByUserID(userID)).Where(d => !d.IsDeleted).ToList();
            var configs = (await _documentConfigRepository.GetAll())
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder).ThenBy(c => c.VerificationDocumentConfigID)
                .ToList();
            var approval = (await _approvedRepository.GetByUserID(userID))
                .OrderByDescending(a => a.DateTime).FirstOrDefault();

            // Every configured document is listed whether or not it was submitted, so the admin can
            // see what is still outstanding — not just what arrived.
            var rows = configs.Select(c =>
            {
                var mine = docs.FirstOrDefault(d => (d.DocmentType ?? string.Empty)
                    .Equals(c.DocumentCode, StringComparison.OrdinalIgnoreCase));
                return new KycDocumentRow
                {
                    UserDocumentID = mine?.UserDocumentID ?? 0,
                    VerificationDocumentConfigID = c.VerificationDocumentConfigID,
                    DocumentName = c.DocumentName,
                    DocumentCode = c.DocumentCode,
                    DocumentType = c.DocumentType,
                    IsRequired = c.IsRequired,
                    DisplayOrder = c.DisplayOrder,
                    DocumentNo = mine?.DocumentNo ?? string.Empty,
                    ImageURL = mine?.ImageURL ?? string.Empty,
                    Status = mine?.Status ?? string.Empty,
                    Remark = mine?.Remark ?? string.Empty,
                    SubmittedOn = mine?.DateTime,
                };
            }).ToList();

            // A document uploaded against a code that is no longer configured still has to be shown
            // — it may be the very thing holding the user's KYC up.
            var configCodes = configs.Select(c => c.DocumentCode.ToUpperInvariant()).ToHashSet();
            rows.AddRange(docs
                .Where(d => !configCodes.Contains((d.DocmentType ?? string.Empty).ToUpperInvariant()))
                .Select(d => new KycDocumentRow
                {
                    UserDocumentID = d.UserDocumentID,
                    DocumentName = d.DocmentType ?? "Document",
                    DocumentCode = d.DocmentType ?? string.Empty,
                    DocumentType = "Other",
                    DisplayOrder = 999,
                    DocumentNo = d.DocumentNo ?? string.Empty,
                    ImageURL = d.ImageURL ?? string.Empty,
                    Status = d.Status ?? string.Empty,
                    Remark = d.Remark ?? string.Empty,
                    SubmittedOn = d.DateTime,
                }));

            var ekyc = (await _ekycLogRepository.GetByUserID(userID))
                .OrderByDescending(l => l.DateTime)
                .Select(l => new KycEkycEntry
                {
                    DocumentType = l.DocumentType ?? string.Empty,
                    DocumentNo = l.DocumentNo ?? string.Empty,
                    DateTime = l.DateTime,
                })
                .ToList();

            var detail = new KycUserDetail
            {
                UserID = user.UserMasterID,
                PersonName = user.PersonName ?? string.Empty,
                FirmName = user.FirmName ?? string.Empty,
                MobileNumber = user.MobileNumber ?? string.Empty,
                WhatsAppNumber = user.WhatsAppNumber ?? string.Empty,
                EmailID = user.EmailID ?? string.Empty,
                UserTypeID = user.UserTypeID ?? 0,
                UserType = user.UserType?.UserType ?? string.Empty,
                ParentName = ParentNameOf(user, users),
                CityName = user.CityName ?? string.Empty,
                StateName = user.State?.StateName ?? string.Empty,
                Status = KycStatus.Resolve(user, docs),
                Mode = user.IseKYC ? "E-KYC" : "Manual",
                IseKYC = user.IseKYC,
                IsKYCApproved = user.IsKYCApproved,
                IsActive = user.IsActive,
                RequiredCount = configs.Count(c => c.IsRequired),
                SubmittedCount = docs.Count,
                ApprovedCount = docs.Count(d => KycStatus.IsDocApproved(d.Status)),
                RejectedCount = docs.Count(d => KycStatus.IsDocRejected(d.Status)),
                InReviewCount = docs.Count(d => KycStatus.IsDocInReview(d.Status)),
                RegisteredOn = user.DateTime,
                LastSubmittedOn = docs.Count == 0 ? null : docs.Max(d => d.UpdatedateTime > d.DateTime ? d.UpdatedateTime : d.DateTime),
                ApprovedOn = approval?.DateTime,
                ApprovedBy = approval == null ? string.Empty : NameOf(users, approval.ApporvedUserID),
                Remark = approval?.Remark ?? string.Empty,
                PANNo = user.PANNo ?? string.Empty,
                AadharNo = user.AadharNo ?? string.Empty,
                GSTNo = user.GSTNo ?? string.Empty,
                Address = user.Address ?? string.Empty,
                ShopAddress = user.ShopAddress ?? string.Empty,
                Pincode = user.Pincode ?? string.Empty,
                UniqueReferenceNo = user.UniqueReferenceNo ?? string.Empty,
                Documents = rows.OrderBy(r => r.DisplayOrder).ThenBy(r => r.DocumentName).ToList(),
                EkycHistory = ekyc,
            };
            return (StatusType.Ok, detail);
        }

        // ── Decisions ────────────────────────────────────────────────────────────────

        public async Task<string> ReviewDocument(KycReviewDocumentRequest request, int adminUserID)
        {
            if (request.UserDocumentID <= 0) return Messages.Common.Message.InvalidRequest;
            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Remark))
                return "A reason is required when rejecting a document.";

            var doc = await _userDocumentRepository.GetByID(request.UserDocumentID);
            if (doc == null) return "Document not found.";

            var user = await _userRepository.GetUserById(doc.UserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            doc.Status = request.IsApproved ? KycStatus.DocApproved : KycStatus.DocRejected;
            doc.Remark = Trim(request.Remark, 100);
            doc.UpdatedateTime = DateTime.UtcNow;
            if (await _userDocumentRepository.Update(doc) <= 0) return "Could not save the decision.";

            if (request.IsApproved)
            {
                await CopyIdentifierToUser(user, doc);
                // Approving the last outstanding requirement finishes the user's KYC — the admin
                // shouldn't have to approve the documents AND then the user.
                var docs = (await _userDocumentRepository.GetByUserID(user.UserMasterID)).Where(d => !d.IsDeleted).ToList();
                if (!user.IsKYCApproved && KycStatus.AllRequiredApproved(docs, await RequiredCodes()))
                    await MarkApproved(user, adminUserID, request.Remark);
            }
            else if (user.IsKYCApproved)
            {
                // Rejecting a document on an already-approved user pulls the approval back — the
                // account must not keep transacting on a KYC that no longer holds.
                user.IsKYCApproved = false;
                await _userRepository.UpdateUser(user);
                await NotifyRejected(user, request.Remark);
            }
            else
            {
                await NotifyRejected(user, request.Remark);
            }

            return StatusType.Ok;
        }

        public async Task<string> ReviewUser(KycReviewUserRequest request, int adminUserID)
        {
            if (request.UserID <= 0) return Messages.Common.Message.InvalidRequest;
            if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Remark))
                return "A reason is required when rejecting a KYC.";

            var user = await _userRepository.GetUserById(request.UserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.UserTypeID == UserTypeCode.Admin) return "An administrator does not go through KYC.";

            var docs = (await _userDocumentRepository.GetByUserID(user.UserMasterID)).Where(d => !d.IsDeleted).ToList();

            if (request.IsApproved)
            {
                if (docs.Count == 0 && !user.IseKYC)
                    return "This user has not submitted any documents yet.";

                foreach (var doc in docs.Where(d => !KycStatus.IsDocApproved(d.Status)))
                {
                    doc.Status = KycStatus.DocApproved;
                    doc.Remark = Trim(request.Remark, 100);
                    doc.UpdatedateTime = DateTime.UtcNow;
                    await _userDocumentRepository.Update(doc);
                    await CopyIdentifierToUser(user, doc);
                }
                await MarkApproved(user, adminUserID, request.Remark);
                return StatusType.Ok;
            }

            // Reject: everything not already approved carries the reason back to the user.
            foreach (var doc in docs.Where(d => !KycStatus.IsDocRejected(d.Status)))
            {
                doc.Status = KycStatus.DocRejected;
                doc.Remark = Trim(request.Remark, 100);
                doc.UpdatedateTime = DateTime.UtcNow;
                await _userDocumentRepository.Update(doc);
            }

            if (user.IsKYCApproved || user.IseKYC)
            {
                user.IsKYCApproved = false;
                user.IseKYC = false;
                user.UpdatedateTime = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
            }

            await _approvedRepository.Insert(new KYCApprovedDetails
            {
                UserID = user.UserMasterID,
                ApporvedUserID = adminUserID,
                Remark = Trim($"REJECTED: {request.Remark}", 200),
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });

            await NotifyRejected(user, request.Remark);
            return StatusType.Ok;
        }

        /// <summary>
        /// Catch up any user who cleared E-KYC but is not marked approved yet, so the rule
        /// "E-KYC verified ⇒ approved" holds no matter which code path set the E-KYC flag — the
        /// provider callback, a data migration, or an older build. Runs when the admin opens the
        /// queue, which is exactly when a stale row would otherwise be visible as "Pending".
        /// </summary>
        private async Task ReconcileEkycApprovals()
        {
            var stale = (await _userRepository.GetAllUsers())
                .Where(u => u.IseKYC && !u.IsKYCApproved && u.UserTypeID != UserTypeCode.Admin)
                .ToList();
            foreach (var user in stale) await AutoApproveEkyc(user.UserMasterID);
        }

        /// <summary>
        /// E-KYC needs no admin: clearing the electronic flow IS the verification. Call this from
        /// the E-KYC provider flow the moment it reports success (see eKYCService); it is
        /// idempotent, so a retry can't double-write the audit row.
        /// </summary>
        public async Task<string> AutoApproveEkyc(int userID)
        {
            var user = await _userRepository.GetUserById(userID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.IsKYCApproved && user.IseKYC) return StatusType.Ok;

            user.IseKYC = true;
            await MarkApproved(user, 0, "Auto-approved — E-KYC verified.");
            return StatusType.Ok;
        }

        // ── Policy ───────────────────────────────────────────────────────────────────

        public async Task<KycPolicyResponse> SelectPolicy()
        {
            var settings = await _settingRepository.GetAll();
            var services = (await _serviceRepository.GetAllServices()).Where(s => !s.IsDelete).ToList();
            var configs = await _documentConfigRepository.GetAll();

            return new KycPolicyResponse
            {
                IsKYCEnforced = IsSettingOn(settings, UserServiceAccessService.KycEnforcedKey),
                EVerification = IsSettingOn(settings, "eVerification"),
                HasRequiredDocuments = configs.Any(c => c.IsActive && c.IsRequired),
                Services = services
                    .OrderBy(s => s.ServiceCategory).ThenBy(s => s.UISequence).ThenBy(s => s.Name)
                    .Select(s => new KycPolicyService
                    {
                        ServiceID = s.ServiceID,
                        Name = s.Name,
                        ServiceCategory = string.IsNullOrWhiteSpace(s.ServiceCategory) ? "Other Services" : s.ServiceCategory,
                        IsActive = s.IsActive,
                        IsAllowedWithoutKYC = s.IsAllowedWithoutKYC,
                    })
                    .ToList(),
            };
        }

        public async Task<string> SavePolicy(KycPolicySaveRequest request, int adminUserID)
        {
            await SetSetting(UserServiceAccessService.KycEnforcedKey, request.IsKYCEnforced,
                "When ON, a user whose KYC is not approved cannot use any service except the ones marked as allowed without KYC.");

            // Only the services whose exemption actually changed are written — saving the policy
            // must not stamp UpdatedateTime across the whole service master.
            var allowed = request.ServicesAllowedWithoutKYC.ToHashSet();
            var services = (await _serviceRepository.GetAllServices()).Where(s => !s.IsDelete).ToList();
            foreach (var service in services)
            {
                bool next = allowed.Contains(service.ServiceID);
                if (service.IsAllowedWithoutKYC == next) continue;
                service.IsAllowedWithoutKYC = next;
                service.UpdatedateTime = DateTime.UtcNow;
                await _serviceRepository.Update(service);
            }

            return StatusType.Ok;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private async Task<Dictionary<int, List<UserDocumentMaster>>> DocumentsByUser() =>
            (await _userDocumentRepository.GetAll())
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());

        /// <summary>Document codes the admin has marked ACTIVE + REQUIRED.</summary>
        private async Task<List<string>> RequiredCodes() =>
            (await _documentConfigRepository.GetAll())
                .Where(c => c.IsActive && c.IsRequired)
                .Select(c => c.DocumentCode)
                .ToList();

        private static string ParentNameOf(UserMaster user, List<UserMaster> all)
        {
            if (!user.ParentID.HasValue) return string.Empty;
            var parent = all.FirstOrDefault(u => u.UserMasterID == user.ParentID.Value);
            if (parent == null) return string.Empty;
            return string.IsNullOrWhiteSpace(parent.FirmName) ? parent.PersonName ?? string.Empty : parent.FirmName;
        }

        private static string NameOf(List<UserMaster> all, int userID)
        {
            if (userID <= 0) return "System (E-KYC)";
            var u = all.FirstOrDefault(x => x.UserMasterID == userID);
            if (u == null) return "Administrator";
            return string.IsNullOrWhiteSpace(u.FirmName) ? u.PersonName ?? "Administrator" : u.FirmName;
        }

        private static string Trim(string? value, int max)
        {
            string v = (value ?? string.Empty).Trim();
            return v.Length <= max ? v : v[..max];
        }

        /// <summary>
        /// Flip the user to approved, record who did it, and tell them. Shared by the manual and
        /// E-KYC routes so both leave the same audit trail.
        /// </summary>
        private async Task MarkApproved(UserMaster user, int adminUserID, string remark)
        {
            user.IsKYCApproved = true;
            user.UpdatedateTime = DateTime.UtcNow;
            await _userRepository.UpdateUser(user);

            await _approvedRepository.Insert(new KYCApprovedDetails
            {
                UserID = user.UserMasterID,
                ApporvedUserID = adminUserID,
                Remark = Trim(remark, 200),
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });

            string name = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName : user.PersonName;
            await _messageService.SendEventAsync("KYC Approved", user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string> { ["CustomerName"] = name ?? string.Empty },
                $"Congratulations {name}! Your KYC is APPROVED. All services are now active.");
        }

        private async Task NotifyRejected(UserMaster user, string remark)
        {
            string name = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName : user.PersonName;
            await _messageService.SendEventAsync("KYC Rejected", user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string> { ["CustomerName"] = name ?? string.Empty, ["Remark"] = remark ?? string.Empty },
                $"Your KYC was REJECTED. Reason: {remark}. Please re-submit your documents.");
        }

        /// <summary>
        /// An approved PAN / Aadhaar / GST document is the authoritative source for that number on
        /// the user record, so approving it fills a blank column in. An already-populated column is
        /// left alone — the admin set it deliberately.
        /// </summary>
        private async Task CopyIdentifierToUser(UserMaster user, UserDocumentMaster doc)
        {
            string code = (doc.DocmentType ?? string.Empty).ToUpperInvariant();
            string number = (doc.DocumentNo ?? string.Empty).Trim();
            if (number.Length == 0) return;

            bool changed = false;
            if (code.Contains("AADHAAR") || code.Contains("AADHAR"))
            {
                if (string.IsNullOrWhiteSpace(user.AadharNo)) { user.AadharNo = Trim(number, 16); changed = true; }
            }
            else if (code.Contains("PAN"))
            {
                if (string.IsNullOrWhiteSpace(user.PANNo)) { user.PANNo = Trim(number, 10); changed = true; }
            }
            else if (code.Contains("GST"))
            {
                if (string.IsNullOrWhiteSpace(user.GSTNo)) { user.GSTNo = Trim(number, 15); changed = true; }
            }

            if (changed)
            {
                user.UpdatedateTime = DateTime.UtcNow;
                await _userRepository.UpdateUser(user);
            }
        }

        private static bool IsSettingOn(IEnumerable<SettingMaster> settings, string key)
        {
            var row = settings.FirstOrDefault(s =>
                (s.Key ?? string.Empty).Equals(key, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted);
            if (row == null) return false;
            string v = (row.Value ?? string.Empty).Trim().ToLowerInvariant();
            if (v.Length == 0) return row.IsOn;
            if (v is "true" or "false") return v == "true";
            return v is "1" or "yes" or "on" or "y";
        }

        /// <summary>Write a curated Key Setting in the shape the Key Setting screen reads.</summary>
        private async Task SetSetting(string key, bool on, string description)
        {
            var existing = await _settingRepository.GetByKey(key);
            if (existing == null)
            {
                await _settingRepository.Insert(new SettingMaster
                {
                    Key = key,
                    Value = on ? "true" : "false",
                    IsOn = on,
                    Description = description,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
                return;
            }

            existing.Value = on ? "true" : "false";
            existing.IsOn = on;
            if (string.IsNullOrWhiteSpace(existing.Description)) existing.Description = description;
            existing.UpdatedateTime = DateTime.UtcNow;
            await _settingRepository.Update(existing);
        }
    }
}
