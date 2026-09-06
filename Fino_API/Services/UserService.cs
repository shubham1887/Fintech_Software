using System.Security.Cryptography;
using System.Text;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.User;
using FintechApp.Responses.User;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserWalletMasterRepository _userWalletMasterRepository;
        private readonly IWalletMasterRepository _walletMasterRepository;
        private readonly IStateMasterRepository _stateMasterRepository;
        private readonly IOTPMasterRepository _otpMasterRepository;
        private readonly IMessageService _messageService;
        private readonly IUserSlabMappingRepository _userSlabMappingRepository;
        private readonly ISlabMasterRepository _slabMasterRepository;
        private readonly IUserServiceMappingRepository _userServiceMappingRepository;
        private readonly IVerificationDocumentConfigRepository _verificationDocumentConfigRepository;
        private readonly IUserDocumentMasterRepository _userDocumentMasterRepository;
        private readonly IUserCodeService _userCodeService;

        private const string OTPType2FA = "2FADeactivate";

        public UserService(
            IUserRepository userRepository,
            IUserWalletMasterRepository userWalletMasterRepository,
            IWalletMasterRepository walletMasterRepository,
            IStateMasterRepository stateMasterRepository,
            IOTPMasterRepository otpMasterRepository,
            IMessageService messageService,
            IUserSlabMappingRepository userSlabMappingRepository,
            ISlabMasterRepository slabMasterRepository,
            IUserServiceMappingRepository userServiceMappingRepository,
            IVerificationDocumentConfigRepository verificationDocumentConfigRepository,
            IUserDocumentMasterRepository userDocumentMasterRepository,
            IUserCodeService userCodeService)
        {
            _userRepository = userRepository;
            _userWalletMasterRepository = userWalletMasterRepository;
            _walletMasterRepository = walletMasterRepository;
            _stateMasterRepository = stateMasterRepository;
            _otpMasterRepository = otpMasterRepository;
            _messageService = messageService;
            _userSlabMappingRepository = userSlabMappingRepository;
            _slabMasterRepository = slabMasterRepository;
            _userServiceMappingRepository = userServiceMappingRepository;
            _verificationDocumentConfigRepository = verificationDocumentConfigRepository;
            _userDocumentMasterRepository = userDocumentMasterRepository;
            _userCodeService = userCodeService;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static string GenerateUniqueReferenceNo()
        {
            return "USR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private static UserDetailViewModel MapToDetail(UserMaster u, string parentName = "", string parentMobile = "") => new()
        {
            UserMasterID = u.UserMasterID,
            FirmName = u.FirmName ?? string.Empty,
            PersonName = u.PersonName ?? string.Empty,
            MobileNumber = u.MobileNumber ?? string.Empty,
            EmailID = u.EmailID ?? string.Empty,
            UserType = u.UserType?.UserType ?? string.Empty,
            UserTypeID = u.UserTypeID ?? 0,
            ParentID = u.ParentID,
            ParentName = parentName,
            ParentMobile = parentMobile,
            Address = u.Address ?? string.Empty,
            CityName = u.CityName ?? string.Empty,
            Pincode = u.Pincode ?? string.Empty,
            StateName = u.State?.StateName ?? string.Empty,
            StateID = u.StateID,
            GSTNo = u.GSTNo ?? string.Empty,
            PANNo = u.PANNo ?? string.Empty,
            AadharNo = u.AadharNo ?? string.Empty,
            WhatsAppNumber = u.WhatsAppNumber ?? string.Empty,
            IsActive = u.IsActive,
            IsDeleted = u.IsDeleted,
            IsKYCApproved = u.IsKYCApproved,
            IseKYC = u.IseKYC,
            IsCommissionHold = u.IsCommissionHold,
            IsTwoFA = u.IsTwoFA,
            IsInvoiceApplicable = u.IsInvoiceApplicable,
            IsLoginOTP = u.IsLoginOTP,
            IsPin = u.IsPin,
            AliasName = u.AliasName ?? string.Empty,
            UniqueReferenceNo = u.UniqueReferenceNo,
            ProfileImagePath = u.ProfileImagePath,
            ShopAddress = u.ShopAddress ?? string.Empty,
            NPCIAgentID = u.NPCIAgentID,
            WalletIDforEcollection = u.WalletIDforEcollection,
            DOC = u.DateTime,
            LastUpdated = u.UpdatedateTime
        };

        public async Task<UserResponse?> GetUserById(int userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return null;
            return new UserResponse
            {
                UserMasterID = user.UserMasterID,
                PersonName = user.PersonName,
                FirmName = user.FirmName,
                MobileNumber = user.MobileNumber,
                EmailID = user.EmailID,
                UserTypeID = user.UserTypeID ?? 0,
                UserType = user.UserType?.UserType ?? string.Empty,
                IsActive = user.IsActive,
                IsKYCApproved = user.IsKYCApproved,
                StateName = user.State?.StateName ?? string.Empty,
                DateTime = user.DateTime
            };
        }

        public async Task<List<UserResponse>> GetUsers(Requests.User.GetUserRequest request)
        {
            var users = await _userRepository.GetUsers(request);
            return users.Select(user => new UserResponse
            {
                UserMasterID = user.UserMasterID,
                PersonName = user.PersonName,
                FirmName = user.FirmName,
                MobileNumber = user.MobileNumber,
                EmailID = user.EmailID,
                UserTypeID = user.UserTypeID ?? 0,
                UserType = user.UserType?.UserType ?? string.Empty,
                IsActive = user.IsActive,
                IsKYCApproved = user.IsKYCApproved,
                StateName = user.State?.StateName ?? string.Empty,
                DateTime = user.DateTime
            }).ToList();
        }

        public async Task<bool> UpdateUser(int userId, string fieldName, string oldValue, string newValue)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return false;
            switch (fieldName.ToLower())
            {
                case "personname": user.PersonName = newValue; break;
                case "firmname": user.FirmName = newValue; break;
                case "emailid": user.EmailID = newValue; break;
                default: return false;
            }
            user.UpdatedateTime = DateTime.UtcNow;
            return await _userRepository.UpdateUser(user);
        }

        public async Task<string> Insert(UserInsertRequest request, int currentUserID, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber) || request.MobileNumber.Length != 10)
                return "Please enter a valid 10-digit mobile number.";
            if (string.IsNullOrWhiteSpace(request.FirmName))
                return "Firm name is required.";
            if (request.UserTypeID <= 0)
                return "Please select a valid user type.";

            // Password is system-generated for admin-created users and texted to the new user
            // (no password field on the Add User form). An explicit password is still honoured if
            // one is supplied by an older client.
            bool passwordGenerated = string.IsNullOrWhiteSpace(request.Password);
            string effectivePassword = passwordGenerated ? OtpHelper.GetRandomPassword() : request.Password;

            var existingByMobile = await _userRepository.GetUserByMobile(request.MobileNumber);
            if (existingByMobile != null)
                return Messages.UserMessages.MobileNumberExists;

            if (!string.IsNullOrWhiteSpace(request.EmailID))
            {
                var existingByEmail = await _userRepository.GetUserByEmail(request.EmailID);
                if (existingByEmail != null)
                    return "Email address already registered.";
            }

            int parentID = request.ParentID ?? currentUserID;

            // The User Code (stored as AliasName) is generated from the user type, never typed in:
            // each type runs its own sequence (RD000001, RD000002 ...) and the generator skips any
            // code already in use, so two users can never end up sharing one.
            string userCode = await _userCodeService.GenerateUserCode(request.UserTypeID);

            var newUser = new UserMaster
            {
                FirmName = request.FirmName,
                PersonName = request.PersonName,
                MobileNumber = request.MobileNumber,
                EmailID = string.IsNullOrWhiteSpace(request.EmailID) ? $"{request.MobileNumber}@user.app" : request.EmailID,
                PasswordHash = HashPassword(effectivePassword),
                UserTypeID = request.UserTypeID,
                ParentID = parentID,
                Address = request.Address,
                CityName = request.CityName,
                Pincode = request.Pincode,
                StateID = request.StateID,
                GSTNo = request.GSTNo,
                PANNo = request.PANNo,
                AadharNo = request.AadharNo,
                WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber) ? request.MobileNumber : request.WhatsAppNumber,
                Mpin = request.Mpin > 0 ? request.Mpin : 1234,
                AliasName = userCode,
                ShopAddress = request.ShopAddress,
                APIToken = Guid.NewGuid().ToString("N"),
                UniqueReferenceNo = GenerateUniqueReferenceNo(),
                IsActive = true,
                JoiningByUserID = currentUserID,
                RegistrationType = "Inside",
                IsInvoiceApplicable = true,
                IsInitialPassword = true,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            bool created = await _userRepository.CreateUser(newUser);
            if (!created) return "User registration failed. Please try again.";

            // Create default wallet entries for all active wallets
            var wallets = await _walletMasterRepository.Select();
            foreach (var wallet in wallets.Where(w => w.IsActive))
            {
                await _userWalletMasterRepository.Insert(new UserWalletMaster
                {
                    UserID = newUser.UserMasterID,
                    WalletID = wallet.WalletID,
                    Balance = 0
                });
            }

            // Assign the commission slab chosen by the admin (if any).
            if (request.SlabMasterID.HasValue && request.SlabMasterID.Value > 0)
            {
                await _userSlabMappingRepository.Insert(new UserSlabMapping
                {
                    UserID = newUser.UserMasterID,
                    SlabMasterID = request.SlabMasterID.Value
                });
            }

            // Grant the selected services — IsActive true = enabled, false = assigned but locked.
            if (request.Services != null)
            {
                foreach (var svc in request.Services.Where(s => s.ServiceID > 0)
                             .GroupBy(s => s.ServiceID).Select(g => g.First()))
                {
                    await _userServiceMappingRepository.Insert(new UserServiceMapping
                    {
                        UserID = newUser.UserMasterID,
                        ServiceID = svc.ServiceID,
                        IsGranted = svc.IsGranted,
                        IsActive = svc.IsGranted && svc.IsActive
                    });
                }
            }

            // Attach any KYC / verification documents captured on the Add User form. Files were
            // already uploaded (Upload/Document); we store one UserDocumentMaster row per document,
            // keyed by the config's DocumentCode, in PROCESS so it flows into the admin KYC queue.
            if (request.Documents != null && request.Documents.Any(d => !string.IsNullOrWhiteSpace(d.ImageURL)))
            {
                foreach (var doc in request.Documents.Where(d => d.VerificationDocumentConfigID > 0
                             && !string.IsNullOrWhiteSpace(d.ImageURL)))
                {
                    var config = await _verificationDocumentConfigRepository.GetByID(doc.VerificationDocumentConfigID);
                    if (config == null) continue;
                    await _userDocumentMasterRepository.Insert(new UserDocumentMaster
                    {
                        UserID = newUser.UserMasterID,
                        DocmentType = config.DocumentCode,
                        DocumentNo = doc.DocumentNo?.Trim() ?? string.Empty,
                        ImageURL = doc.ImageURL,
                        Status = "PROCESS",
                        DateTime = DateTime.UtcNow,
                    });
                }
            }

            // Text the new user their login credentials (login ID = mobile, system-generated password).
            // Mirrors SendPassword; fire-and-forget so a gateway hiccup never fails the creation.
            string firm = !string.IsNullOrWhiteSpace(newUser.FirmName) ? newUser.FirmName : newUser.PersonName;
            string credMsg = $"Welcome to {firm}. Login ID: {newUser.MobileNumber}, Password: {effectivePassword}";
            _ = _messageService.SendEventAsync("New Login Credentials", newUser.UserMasterID, newUser.MobileNumber,
                newUser.WhatsAppNumber, null,
                new Dictionary<string, string> { ["Password"] = effectivePassword, ["Number"] = newUser.MobileNumber, ["CustomerName"] = newUser.PersonName ?? string.Empty },
                fallbackSms: credMsg);
            if (!string.IsNullOrWhiteSpace(request.EmailID))
                _ = _messageService.SendEmailAsync(newUser.EmailID, credMsg, "Your Login Credentials");

            return StatusType.Ok;
        }

        /// <summary>
        /// Which tiers a user of a given role may map under. Mirrors the Add / Edit User form so the
        /// upline picked there can never place a user under an illegal tier.
        /// </summary>
        private static readonly Dictionary<int, int[]> AllowedParentTiers = new()
        {
            [UserTypeCode.SuperDistributor] = new[] { UserTypeCode.Admin },
            [UserTypeCode.Distributor] = new[] { UserTypeCode.Admin, UserTypeCode.SuperDistributor },
            [UserTypeCode.Retailer] = new[] { UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor },
            [UserTypeCode.ApiUser] = new[] { UserTypeCode.Admin },
        };

        public async Task<string> Update(UserUpdateRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;

            // Editing is limited to the caller's own profile and their downline - the same scope the
            // Edit User screen is reachable from. Admin / support are unrestricted.
            if (!await IsWithinCallerScope(currentUserID, request.TargetUserID))
                return Messages.Common.Message.NoPermission;

            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            // ---- Role / upline -------------------------------------------------------------
            // Both are validated together: the upline's tier has to be legal for the role the user
            // ends up with, whether that is the existing role or the one being set now.
            int currentRole = user.UserTypeID ?? 0;
            int targetRole = currentRole;
            var downline = FintechApp.Helpers.UserHierarchy.GetDownlineIds(
                await _userRepository.GetAllUsers(), user.UserMasterID);

            if (request.UserTypeID.HasValue && request.UserTypeID.Value > 0 && request.UserTypeID.Value != currentRole)
            {
                if (!AllowedParentTiers.ContainsKey(request.UserTypeID.Value))
                    return "This role cannot be assigned from the user form.";
                if (downline.Count > 0)
                    return "Role cannot be changed while users are mapped under this user. Move them first.";
                targetRole = request.UserTypeID.Value;
            }

            if (request.ParentID.HasValue && request.ParentID.Value != (user.ParentID ?? 0))
            {
                int newParentID = request.ParentID.Value;
                if (newParentID <= 0) return "Please select the upline this user maps under.";
                if (newParentID == user.UserMasterID) return "A user cannot be mapped under themselves.";
                if (downline.Contains(newParentID)) return "A user cannot be mapped under their own downline.";

                var newParent = await _userRepository.GetUserById(newParentID);
                if (newParent == null || newParent.IsDeleted) return "The selected upline user was not found.";

                if (AllowedParentTiers.TryGetValue(targetRole, out var tiers)
                    && !tiers.Contains(newParent.UserTypeID ?? 0))
                    return "The selected upline is not a valid tier for this user's role.";

                user.ParentID = newParentID;
            }

            if (targetRole != currentRole) user.UserTypeID = targetRole;

            if (!string.IsNullOrWhiteSpace(request.FirmName)) user.FirmName = request.FirmName;
            if (!string.IsNullOrWhiteSpace(request.PersonName)) user.PersonName = request.PersonName;
            if (!string.IsNullOrWhiteSpace(request.EmailID) && request.EmailID != user.EmailID)
            {
                // Email is the alternate login identifier, so it stays unique across users.
                var emailOwner = await _userRepository.GetUserByEmail(request.EmailID);
                if (emailOwner != null && emailOwner.UserMasterID != user.UserMasterID)
                    return "Email address already registered.";
                user.EmailID = request.EmailID;
            }
            if (!string.IsNullOrWhiteSpace(request.Address)) user.Address = request.Address;
            if (!string.IsNullOrWhiteSpace(request.CityName)) user.CityName = request.CityName;
            if (!string.IsNullOrWhiteSpace(request.Pincode)) user.Pincode = request.Pincode;
            if (request.StateID.HasValue) user.StateID = request.StateID;
            if (!string.IsNullOrWhiteSpace(request.GSTNo)) user.GSTNo = request.GSTNo;
            if (!string.IsNullOrWhiteSpace(request.PANNo)) user.PANNo = request.PANNo;
            if (!string.IsNullOrWhiteSpace(request.AadharNo)) user.AadharNo = request.AadharNo;
            if (!string.IsNullOrWhiteSpace(request.WhatsAppNumber)) user.WhatsAppNumber = request.WhatsAppNumber;
            // AliasName holds the auto-generated User Code and is deliberately NOT editable here.
            // A record that predates auto-generation (or had its role changed) is given the code
            // its type is due, so every user ends up carrying one.
            if (string.IsNullOrWhiteSpace(user.AliasName) && user.UserTypeID.HasValue)
                user.AliasName = await _userCodeService.GenerateUserCode(user.UserTypeID.Value);
            if (!string.IsNullOrWhiteSpace(request.ShopAddress)) user.ShopAddress = request.ShopAddress;
            if (request.WalletIDforEcollection.HasValue) user.WalletIDforEcollection = request.WalletIDforEcollection;

            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            if (!result) return "User update failed.";

            await SyncSlab(user.UserMasterID, request.SlabMasterID);
            await SyncServices(user.UserMasterID, request.Services);
            await SyncDocuments(user.UserMasterID, request.Documents);

            return StatusType.Ok;
        }

        /// <summary>Points the user at one commission slab. 0 clears the mapping; null leaves it alone.</summary>
        private async Task SyncSlab(int userID, int? slabMasterID)
        {
            if (!slabMasterID.HasValue) return;

            var existing = await _userSlabMappingRepository.GetByUserID(userID);
            if (slabMasterID.Value <= 0)
            {
                foreach (var map in existing)
                    await _userSlabMappingRepository.Delete(map.UserSlabMappingID);
                return;
            }

            var keep = existing.FirstOrDefault();
            foreach (var extra in existing.Skip(1))
                await _userSlabMappingRepository.Delete(extra.UserSlabMappingID);

            if (keep == null)
            {
                await _userSlabMappingRepository.Insert(new UserSlabMapping
                {
                    UserID = userID,
                    SlabMasterID = slabMasterID.Value
                });
            }
            else if (keep.SlabMasterID != slabMasterID.Value)
            {
                keep.SlabMasterID = slabMasterID.Value;
                keep.UpdatedateTime = DateTime.UtcNow;
                await _userSlabMappingRepository.Update(keep);
            }
        }

/// <summary>
        /// Replaces the user's service grants with the supplied set. The client sends a row for every
        /// service in the grid, "Off" ones included (IsGranted false), so that turning the whole grid
        /// off is stored as real rows - deleting them instead would read back as "never configured",
        /// which means unrestricted. A null list (field omitted by an older client) changes nothing;
        /// a service missing from a non-null list is dropped.
        /// </summary>
        private async Task SyncServices(int userID, List<UserServiceSelection>? services)
        {
            if (services == null) return;

            var wanted = services.Where(s => s.ServiceID > 0)
                .GroupBy(s => s.ServiceID)
                .ToDictionary(g => g.Key, g => g.First());
            var existing = await _userServiceMappingRepository.GetByUserID(userID);

            foreach (var map in existing)
            {
                if (wanted.TryGetValue(map.ServiceID, out var want))
                {
                    bool isGranted = want.IsGranted;
                    bool isActive = want.IsGranted && want.IsActive;
                    if (map.IsGranted != isGranted || map.IsActive != isActive)
                    {
                        map.IsGranted = isGranted;
                        map.IsActive = isActive;
                        await _userServiceMappingRepository.Update(map);
                    }
                }
                else
                {
                    await _userServiceMappingRepository.Delete(map.UserServiceMappingID);
                }
            }

            var existingIDs = existing.Select(m => m.ServiceID).ToHashSet();
            foreach (var want in wanted.Values.Where(w => !existingIDs.Contains(w.ServiceID)))
            {
                await _userServiceMappingRepository.Insert(new UserServiceMapping
                {
                    UserID = userID,
                    ServiceID = want.ServiceID,
                    IsGranted = want.IsGranted,
                    IsActive = want.IsGranted && want.IsActive
                });
            }
        }

        /// <summary>
        /// Stores KYC documents submitted from the Edit User form. One row is kept per document type:
        /// re-submitting a type overwrites the stored number / file and puts it back in PROCESS so it
        /// re-enters the admin KYC queue.
        /// </summary>
        private async Task SyncDocuments(int userID, List<UserDocumentSubmission>? documents)
        {
            if (documents == null) return;

            var submitted = documents.Where(d => d.VerificationDocumentConfigID > 0
                && (!string.IsNullOrWhiteSpace(d.ImageURL) || !string.IsNullOrWhiteSpace(d.DocumentNo)))
                .ToList();
            if (submitted.Count == 0) return;

            var existing = (await _userDocumentMasterRepository.GetByUserID(userID)).Where(d => !d.IsDeleted).ToList();

            foreach (var doc in submitted)
            {
                var config = await _verificationDocumentConfigRepository.GetByID(doc.VerificationDocumentConfigID);
                if (config == null) continue;

                string docNo = doc.DocumentNo?.Trim() ?? string.Empty;
                var row = existing.FirstOrDefault(d => string.Equals(d.DocmentType, config.DocumentCode,
                    StringComparison.OrdinalIgnoreCase));

                if (row == null)
                {
                    await _userDocumentMasterRepository.Insert(new UserDocumentMaster
                    {
                        UserID = userID,
                        DocmentType = config.DocumentCode,
                        DocumentNo = docNo,
                        ImageURL = doc.ImageURL ?? string.Empty,
                        Status = "PROCESS",
                        DateTime = DateTime.UtcNow,
                    });
                    continue;
                }

                bool fileChanged = !string.IsNullOrWhiteSpace(doc.ImageURL) && doc.ImageURL != row.ImageURL;
                bool numberChanged = docNo != (row.DocumentNo ?? string.Empty);
                if (!fileChanged && !numberChanged) continue;

                if (fileChanged) row.ImageURL = doc.ImageURL;
                row.DocumentNo = docNo;
                // A re-uploaded file has to be looked at again; a typo fix on the number does not.
                if (fileChanged) row.Status = "PROCESS";
                row.UpdatedateTime = DateTime.UtcNow;
                await _userDocumentMasterRepository.Update(row);
            }
        }

        /// <summary>
        /// Switch a user on or off. The request carries the state to move TO, so this is the one
        /// endpoint behind both the Activate and Deactivate buttons — it must not hard-code
        /// IsActive = true, or "Deactivate" silently reactivates the user instead.
        /// </summary>
        public async Task<string> Activate(UserActivateRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.IsDeleted) return "This user is deleted. Restore the user before changing their status.";
            if (user.UserMasterID == currentUserID) return "You cannot change your own account status.";

            if (user.IsActive == request.IsActive)
                return request.IsActive ? "This user is already active." : "This user is already inactive.";

            user.IsActive = request.IsActive;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            if (!result) return request.IsActive ? "Activation failed." : "Deactivation failed.";

            await NotifyStatusChange(user, request.IsActive);
            return StatusType.Ok;
        }

        /// <summary>Tell the user their account was switched on or off (SMS / WhatsApp / email
        /// per the Message Settings template). Fire-and-forget — never fails the action.</summary>
        private async Task NotifyStatusChange(UserMaster user, bool isActive)
        {
            string name = string.IsNullOrWhiteSpace(user.PersonName) ? user.FirmName : user.PersonName;
            await _messageService.SendEventAsync(
                isActive ? "Account Activated" : "Account Deactivated",
                user.UserMasterID, user.MobileNumber, user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string> { ["CustomerName"] = name ?? string.Empty },
                isActive
                    ? $"Dear {name}, your account has been ACTIVATED. You can sign in and transact again."
                    : $"Dear {name}, your account has been DEACTIVATED. Please contact your service provider.");
        }

        /// <summary>
        /// Soft-delete a user. The row is kept (IsDeleted = 1) so their transaction history, ledger
        /// and the Deleted Users screen still resolve — nothing is ever physically removed.
        ///
        /// Refused when the account still has something attached to it: money in a wallet, or users
        /// mapped beneath it. Deleting either of those would orphan real balances / a whole branch
        /// of the network, so the admin has to move them first.
        /// </summary>
        public async Task<string> Delete(UserDeleteRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.TargetUserID == currentUserID) return "You cannot delete your own account.";

            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.IsDeleted) return "This user is already deleted.";
            if (user.UserTypeID == UserTypeCode.Admin) return "An administrator account cannot be deleted.";

            var allUsers = await _userRepository.GetAllUsers();
            if (allUsers.Any(u => u.ParentID == user.UserMasterID && !u.IsDeleted))
                return "This user still has users mapped beneath them. Transfer the downline first.";

            var wallets = (await _userWalletMasterRepository.GetAll())
                .Where(w => w.UserID == user.UserMasterID).ToList();
            decimal held = wallets.Sum(w => w.Balance + w.HoldBalance);
            if (held != 0m)
                return $"This user still holds a balance of {held:0.00}. Settle the wallet before deleting.";

            bool result = await _userRepository.DeleteUser(user.UserMasterID);
            return result ? StatusType.Ok : "Delete failed.";
        }

        /// <summary>
        /// Bring a soft-deleted user back. They return DEACTIVATED whatever they were before, so
        /// restoring never silently puts a live, transacting account back on the network — the
        /// admin activates them as a separate, deliberate step.
        /// </summary>
        public async Task<string> Restore(UserDeleteRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;

            var user = (await _userRepository.GetAllUsersWithDeleted())
                .FirstOrDefault(u => u.UserMasterID == request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (!user.IsDeleted) return "This user is not deleted.";

            bool result = await _userRepository.RestoreUser(user.UserMasterID);
            return result ? StatusType.Ok : "Restore failed.";
        }

        public async Task<string> Deactivate(int targetUserID, int currentUserID)
        {
            if (targetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(targetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            user.IsActive = false;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "Deactivation failed.";
        }

        public async Task<string> CommissionHoldActivate(UserCommissionHoldRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            user.IsCommissionHold = request.IsCommissionHold;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "Commission hold update failed.";
        }

        public async Task<string> IsAppActivate(UserBooleanToggleRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "App activation update failed.";
        }

        public async Task<string> IsWebActivate(UserBooleanToggleRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "Web activation update failed.";
        }

        public async Task<string> IsInvoiceActivate(UserBooleanToggleRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            user.IsInvoiceApplicable = request.IsActive;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "Invoice activation update failed.";
        }

        public async Task<string> BulkUpdate(UserBulkUpdateRequest request, int currentUserID)
        {
            if (request.UserIDs == null || request.UserIDs.Count == 0)
                return Messages.Common.Message.InvalidRequest;

            var allUsers = await _userRepository.GetAllUsers();
            var targets = allUsers.Where(u => request.UserIDs.Contains(u.UserMasterID)).ToList();
            if (targets.Count == 0) return Messages.Common.Message.DataNotFound;

            int successCount = 0;
            foreach (var user in targets)
            {
                user.IsActive = request.IsActive;
                user.IsCommissionHold = request.IsCommissionHold;
                user.UpdatedateTime = DateTime.UtcNow;
                if (await _userRepository.UpdateUser(user)) successCount++;
            }

            return successCount > 0 ? StatusType.Ok : "Bulk update failed.";
        }

        // "Forgot password" from the All Users screen: reset the target user to a fresh random
        // password and text it to their registered mobile through the configured SMS gateway (and
        // email, if present). Mirrors CredentialService.ForgotPassword; the SHA-256 hash matches the
        // login verifier, so the new password works immediately.
        public async Task<string> SendPassword(UserSendPasswordRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (string.IsNullOrWhiteSpace(user.MobileNumber)) return "User has no registered mobile number.";

            string newPassword = OtpHelper.GetRandomPassword();
            user.PasswordHash = HashPassword(newPassword);

            bool updated = await _userRepository.UpdateUser(user);
            if (!updated) return "Password reset failed. Please try again.";

            string firm = !string.IsNullOrWhiteSpace(user.FirmName) ? user.FirmName : user.PersonName;
            string msg = $"Your new password for {firm} is: {newPassword}";
            _ = _messageService.SendEventAsync("New Login Credentials", user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, null,
                new Dictionary<string, string> { ["Password"] = newPassword, ["Number"] = user.MobileNumber, ["CustomerName"] = user.PersonName ?? string.Empty },
                fallbackSms: msg);
            if (!string.IsNullOrWhiteSpace(user.EmailID))
                _ = _messageService.SendEmailAsync(user.EmailID, msg, "Password Reset");

            return StatusType.Ok;
        }

        // Place (or top up) a hold on the target user's wallet. The held amount is frozen: it stays in
        // Balance but AccountProcessingService will not let a debit eat into it (spendable = Balance -
        // HoldBalance). Admin-driven from the All Users screen; replaces the old Balance Capping page.
        public async Task<string> AddHoldBalance(UserAddHoldBalanceRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return Messages.Common.Message.InvalidRequest;
            if (request.Amount <= 0) return "Hold amount must be greater than zero.";

            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            var wallets = (await _userWalletMasterRepository.GetByUserID(request.TargetUserID)).ToList();
            if (wallets.Count == 0) return "User has no wallet.";

            // Target a specific wallet when given, else the user's primary (first) wallet.
            var wallet = request.WalletID > 0
                ? wallets.FirstOrDefault(w => w.WalletID == request.WalletID)
                : wallets.OrderBy(w => w.WalletID).First();
            if (wallet == null) return "Wallet not found for this user.";

            // Atomically bump HoldBalance only — never rewrites Balance, so a hold placed while a
            // recharge/transfer is debiting the same wallet cannot clobber that balance change.
            int rows = await _userWalletMasterRepository.ApplyHoldDelta(wallet.UserID, wallet.WalletID, request.Amount);
            return rows > 0 ? StatusType.Ok : Messages.Common.Message.ExceptionOccured;
        }

        public async Task<string> Deactivate2FAWithOTP(UserDeactivate2FARequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0 || string.IsNullOrWhiteSpace(request.OTP))
                return Messages.Common.Message.InvalidRequest;

            var otpRecord = await _otpMasterRepository.GetActiveByUserIdAndType(request.TargetUserID, OTPType2FA);
            if (otpRecord == null || otpRecord.OTP != request.OTP)
                return Messages.Common.Message.InvalidOTP;

            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            otpRecord.IsVerified = true;
            otpRecord.UpdatedateTime = DateTime.UtcNow;
            await _otpMasterRepository.Update(otpRecord);

            user.IsTwoFA = false;
            user.UpdatedateTime = DateTime.UtcNow;
            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "2FA deactivation failed.";
        }

        /// <summary>
        /// True when <paramref name="targetUserID"/> is the caller themselves, someone in their
        /// downline, or the caller is an admin/support operator. The JwtAuthorization filter applies
        /// the same rule centrally to every request body; this keeps the service correct on its own
        /// for the endpoints that hand back full KYC / bank / balance detail.
        /// </summary>
        private async Task<bool> IsWithinCallerScope(int currentUserID, int targetUserID)
        {
            if (targetUserID <= 0 || targetUserID == currentUserID) return true;

            var caller = await _userRepository.GetUserById(currentUserID);
            if (caller == null) return false;
            if (FintechApp.StaticModels.HierarchyScopePolicy.IsUnrestrictedRole(caller.UserTypeID ?? 0)) return true;

            var allUsers = await _userRepository.GetAllUsers();
            return FintechApp.Helpers.UserHierarchy.GetDownlineIds(allUsers, currentUserID).Contains(targetUserID);
        }

        public async Task<(string failureMessage, UserDetailViewModel? data)> SelectByID(UserSelectByIDRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return (Messages.Common.Message.InvalidRequest, null);

            // A non-admin may open only their own profile or a user beneath them — this endpoint
            // returns full KYC / bank / balance detail.
            if (!await IsWithinCallerScope(currentUserID, request.TargetUserID))
                return (Messages.Common.Message.NoPermission, null);

            return await BuildDetail(request.TargetUserID);
        }

        /// <summary>
        /// The caller's own record, for the profile page. No target id is accepted, so this is safe
        /// for every authenticated role — nobody can point it at another user.
        /// </summary>
        public async Task<(string failureMessage, UserDetailViewModel? data)> SelectProfile(int currentUserID)
        {
            if (currentUserID <= 0) return (Messages.Common.Message.InvalidRequest, null);
            return await BuildDetail(currentUserID);
        }

        /// <summary>
        /// Self-service profile edit. Contact and address details only: firm name, mobile and the
        /// statutory identifiers are what the KYC is issued against, so they stay with the admin.
        /// </summary>
        public async Task<string> UpdateProfile(UserProfileUpdateRequest request, int currentUserID)
        {
            if (currentUserID <= 0) return Messages.Common.Message.InvalidRequest;
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;

            if (string.IsNullOrWhiteSpace(request.PersonName)) return "Person name is required.";

            string email = request.EmailID?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(email) && email != user.EmailID)
            {
                var owner = await _userRepository.GetUserByEmail(email);
                if (owner != null && owner.UserMasterID != user.UserMasterID)
                    return "Email address already registered.";
                user.EmailID = email;
            }

            user.PersonName = request.PersonName.Trim();
            // AliasName is the system-generated User Code — self-service profile edits leave it alone.
            user.WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber)
                ? user.MobileNumber
                : request.WhatsAppNumber.Trim();
            user.Address = request.Address?.Trim() ?? string.Empty;
            user.CityName = request.CityName?.Trim() ?? string.Empty;
            user.Pincode = request.Pincode?.Trim() ?? string.Empty;
            if (request.StateID.HasValue && request.StateID.Value > 0) user.StateID = request.StateID;
            user.ShopAddress = request.ShopAddress?.Trim() ?? string.Empty;
            user.UpdatedateTime = DateTime.UtcNow;

            bool result = await _userRepository.UpdateUser(user);
            return result ? StatusType.Ok : "Profile update failed.";
        }

        /// <summary>Full detail view for one user — shared by SelectByID and SelectProfile.</summary>
        private async Task<(string failureMessage, UserDetailViewModel? data)> BuildDetail(int targetUserID)
        {
            var user = await _userRepository.GetUserById(targetUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);

            string parentName = string.Empty, parentMobile = string.Empty;
            int parentUserTypeID = 0;
            if (user.ParentID.HasValue)
            {
                var parent = await _userRepository.GetUserById(user.ParentID.Value);
                parentName = parent?.FirmName ?? string.Empty;
                parentMobile = parent?.MobileNumber ?? string.Empty;
                parentUserTypeID = parent?.UserTypeID ?? 0;
            }

            var detail = MapToDetail(user, parentName, parentMobile);
            detail.ParentUserTypeID = parentUserTypeID;

            // Slab, service grants and KYC documents — the Edit User form prefills from these, so the
            // whole record comes back in one call (Add User builds the same three from the masters).
            var slabMap = (await _userSlabMappingRepository.GetByUserID(user.UserMasterID)).FirstOrDefault();
            if (slabMap != null)
            {
                detail.SlabMasterID = slabMap.SlabMasterID;
                var slab = (await _slabMasterRepository.GetAll()).FirstOrDefault(x => x.SlabMasterID == slabMap.SlabMasterID);
                detail.SlabName = slab?.SlabName ?? string.Empty;
            }

            detail.Services = (await _userServiceMappingRepository.GetByUserID(user.UserMasterID))
                .Select(m => new UserServiceGrant
                {
                    ServiceID = m.ServiceID,
                    ServiceName = m.ServiceMaster?.Name ?? string.Empty,
                    IsGranted = m.IsGranted,
                    IsActive = m.IsActive,
                })
                .ToList();

            var ownDocuments = (await _userDocumentMasterRepository.GetByUserID(user.UserMasterID))
                .Where(d => !d.IsDeleted).ToList();
            detail.KYCStatus = KycStatus.Resolve(user, ownDocuments);

            detail.Documents = ownDocuments
                .Select(d => new UserDocumentView
                {
                    UserDocumentID = d.UserDocumentID,
                    DocumentCode = d.DocmentType ?? string.Empty,
                    DocumentNo = d.DocumentNo ?? string.Empty,
                    ImageURL = d.ImageURL ?? string.Empty,
                    Status = d.Status ?? string.Empty,
                    Remark = d.Remark ?? string.Empty,
                })
                .ToList();

            return (StatusType.Ok, detail);
        }

        public async Task<(string failureMessage, UserListWrapper data)> Select(UserSelectRequest request, int currentUserID, int currentUserTypeID = 0)
        {
            var wrapper = new UserListWrapper();
            var allUsers = await _userRepository.GetAllUsers();

            // Deleted rows are excluded unless explicitly requested. GetAllUsers() already drops
            // soft-deleted rows, so ShowDeleted must pull from the with-deleted set to surface them
            // (otherwise the admin list can never show a Deleted user). Parent/downline lookups still
            // use the non-deleted `allUsers`.
            var source = request.ShowDeleted || request.OnlyDeleted
                ? await _userRepository.GetAllUsersWithDeleted()
                : allUsers;
            var query = source.AsEnumerable();

            // The Deleted Users screen shows deleted rows and nothing else — a live user must never
            // appear there, so this filter comes first and is not overridable by ShowDeleted.
            if (request.OnlyDeleted)
                query = query.Where(u => u.IsDeleted);

            bool isAdmin = currentUserTypeID == UserTypeCode.Admin || currentUserTypeID == UserTypeCode.SupportUser;

            // Non-admins are scoped to their FULL downline (every retailer / distributor /
            // super-distributor beneath them, however deep) — never their upline or peers.
            HashSet<int>? downlineIds = isAdmin
                ? null
                : FintechApp.Helpers.UserHierarchy.GetDownlineIds(allUsers, currentUserID);

            if (request.UserID > 0)
            {
                // A parent filter may only name someone the caller can already reach. Without this
                // any role could list another branch's network just by passing its id.
                if (!isAdmin && request.UserID != currentUserID && !downlineIds!.Contains(request.UserID))
                    return (Messages.Common.Message.NoPermission, wrapper);

                query = query.Where(u => u.ParentID == request.UserID || u.UserMasterID == request.UserID);
            }

            // The downline filter applies whether or not a parent filter was supplied, so a
            // narrowing filter can never widen the result set beyond the caller's own branch.
            if (!isAdmin)
                query = query.Where(u => downlineIds!.Contains(u.UserMasterID));

            // Admin is a network operator, not a transacting entity (admin can't recharge / be funded),
            // so admin rows never belong in a user list. The caller themselves is also excluded — a user
            // can't fund or select themselves. This keeps every user picker (reports, fund transfer …)
            // to real, selectable downline entities only.
            query = query.Where(u => u.UserMasterID != currentUserID && u.UserTypeID != UserTypeCode.Admin);

            if (request.UserTypeID > 0)
                query = query.Where(u => u.UserTypeID == request.UserTypeID);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                bool isActive = request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(u => u.IsActive == isActive);
            }

            // Tri-state: the Inactive Users screen passes false and must get deactivated users only.
            // Deleted rows are a separate state — they are neither active nor inactive here, so this
            // never doubles as a "deleted" filter.
            if (request.IsActive.HasValue)
                query = query.Where(u => u.IsActive == request.IsActive.Value && !u.IsDeleted);

            if (DateTime.TryParse(request.FromDate, out DateTime fromDate))
                query = query.Where(u => u.DateTime >= fromDate);
            if (DateTime.TryParse(request.ToDate, out DateTime toDate))
                query = query.Where(u => u.DateTime < toDate.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.ToLower();
                query = query.Where(u => (u.FirmName ?? "").ToLower().Contains(s) ||
                    (u.MobileNumber ?? "").Contains(s) || (u.EmailID ?? "").ToLower().Contains(s) ||
                    (u.UniqueReferenceNo ?? "").ToLower().Contains(s));
            }

            // KYC state per user, resolved once from the documents on file (KycStatus is the single
            // source of truth every screen shares). Loaded here rather than per row so the list
            // costs one document read no matter how many users come back.
            var docsByUser = (await _userDocumentMasterRepository.GetAll())
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.UserID)
                .ToDictionary(g => g.Key, g => g.ToList());
            var noDocs = new List<Entities.UserDocumentMaster>();
            string KycOf(UserMaster u) =>
                KycStatus.Resolve(u, docsByUser.TryGetValue(u.UserMasterID, out var d) ? d : noDocs);

            if (!string.IsNullOrWhiteSpace(request.KYCStatus))
            {
                string wanted = request.KYCStatus.Trim().Replace(" ", string.Empty);
                query = query.Where(u => KycOf(u).Replace(" ", string.Empty)
                    .Equals(wanted, StringComparison.OrdinalIgnoreCase));
            }

            var list = query.OrderByDescending(u => u.DateTime).ToList();
            wrapper.Count = list.Count;

            IEnumerable<UserMaster> paged = list;
            if (request.TakeCount > 0)
                paged = list.Skip(request.SkipCount).Take(request.TakeCount);

            // Resolve each user's parent (upline) name/mobile so the list shows the mapping chain,
            // and their total on-hold wallet balance (summed across the user's wallets).
            var byId = source.ToDictionary(u => u.UserMasterID);
            var wallets = await _userWalletMasterRepository.GetAll();
            var holdByUser = wallets.GroupBy(w => w.UserID)
                .ToDictionary(g => g.Key, g => g.Sum(w => w.HoldBalance));
            var balanceByUser = wallets.GroupBy(w => w.UserID)
                .ToDictionary(g => g.Key, g => g.Sum(w => w.Balance));

            // Wallets are dynamic (WalletMaster) — name each user-wallet row and keep the WalletMaster
            // display order so the UI can render one stable column per wallet.
            var walletMasters = await _walletMasterRepository.Select();
            var walletName = walletMasters.ToDictionary(w => w.WalletID, w => w.Name);
            var walletOrder = walletMasters
                .OrderBy(w => w.UISequence).ThenBy(w => w.WalletID)
                .Select((w, i) => new { w.WalletID, i })
                .ToDictionary(x => x.WalletID, x => x.i);
            var walletsByUser = wallets.GroupBy(w => w.UserID).ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(w => walletOrder.TryGetValue(w.WalletID, out var o) ? o : int.MaxValue)
                    .Select(w => new UserWalletBalance
                    {
                        WalletID = w.WalletID,
                        WalletName = walletName.TryGetValue(w.WalletID, out var n) ? n : $"Wallet {w.WalletID}",
                        Balance = w.Balance,
                        HoldBalance = w.HoldBalance,
                    })
                    .ToList());

            // Slab(s) mapped to each user (usually one; comma-join if more than one).
            var slabName = (await _slabMasterRepository.GetAll())
                .GroupBy(s => s.SlabMasterID).ToDictionary(g => g.Key, g => g.First().SlabName);
            var slabsByUser = (await _userSlabMappingRepository.GetAll())
                .GroupBy(m => m.UserID)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g
                        .Select(m => slabName.TryGetValue(m.SlabMasterID, out var sn) ? sn : null)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()));

            wrapper.data = paged.Select(u =>
            {
                string parentName = string.Empty, parentMobile = string.Empty;
                if (u.ParentID.HasValue && byId.TryGetValue(u.ParentID.Value, out var parent))
                {
                    parentName = !string.IsNullOrWhiteSpace(parent.FirmName) ? parent.FirmName : parent.PersonName;
                    parentMobile = parent.MobileNumber ?? string.Empty;
                }
                var vm = MapToDetail(u, parentName, parentMobile);
                vm.KYCStatus = KycOf(u);
                vm.HoldBalance = holdByUser.TryGetValue(u.UserMasterID, out var hb) ? hb : 0m;
                vm.Balance = balanceByUser.TryGetValue(u.UserMasterID, out var bal) ? bal : 0m;
                vm.WalletBalances = walletsByUser.TryGetValue(u.UserMasterID, out var wb) ? wb : new();
                vm.SlabName = slabsByUser.TryGetValue(u.UserMasterID, out var sl) ? sl : string.Empty;
                return vm;
            }).ToList();
            if (wrapper.Count == 0) return (Messages.Common.Message.DataNotFound, wrapper);
            return (StatusType.Ok, wrapper);
        }

        public async Task<(string failureMessage, List<UserDDLResponse> data)> SelectForDDL(UserDDLRequest request, int currentUserID)
        {
            var allUsers = await _userRepository.GetAllUsers();
            var query = allUsers.Where(u => !u.IsDeleted && u.IsActive);

            query = query.Where(u => u.ParentID == currentUserID || u.UserMasterID == currentUserID);

            if (request.UserTypeID > 0)
                query = query.Where(u => u.UserTypeID == request.UserTypeID);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.ToLower();
                query = query.Where(u => (u.FirmName ?? "").ToLower().Contains(s) || (u.MobileNumber ?? "").Contains(s));
            }

            var list = query.Select(u => new UserDDLResponse
            {
                UserMasterID = u.UserMasterID,
                FirmName = u.FirmName ?? string.Empty,
                MobileNumber = u.MobileNumber ?? string.Empty,
                UserType = u.UserType?.UserType ?? string.Empty,
                AliasName = u.AliasName ?? string.Empty,
                UniqueReferenceNo = u.UniqueReferenceNo
            }).ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, list);
            return (StatusType.Ok, list);
        }

        public async Task<(string failureMessage, List<UserDDLResponse> data)> SelectForDDLWithUserType(UserDDLRequest request, int currentUserID)
        {
            var allUsers = await _userRepository.GetAllUsers();
            var query = allUsers.Where(u => !u.IsDeleted && u.IsActive);

            if (request.UserTypeID > 0)
                query = query.Where(u => u.UserTypeID == request.UserTypeID);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string s = request.Search.ToLower();
                query = query.Where(u => (u.FirmName ?? "").ToLower().Contains(s) || (u.MobileNumber ?? "").Contains(s));
            }

            var list = query.Select(u => new UserDDLResponse
            {
                UserMasterID = u.UserMasterID,
                FirmName = u.FirmName ?? string.Empty,
                MobileNumber = u.MobileNumber ?? string.Empty,
                UserType = u.UserType?.UserType ?? string.Empty,
                AliasName = u.AliasName ?? string.Empty,
                UniqueReferenceNo = u.UniqueReferenceNo
            }).ToList();

            if (list.Count == 0) return (Messages.Common.Message.DataNotFound, list);
            return (StatusType.Ok, list);
        }

        public async Task<(string failureMessage, UserGSTViewModel? data)> GSTView(UserGSTRequest request)
        {
            if (request.TargetUserID <= 0) return (Messages.Common.Message.InvalidRequest, null);
            var user = await _userRepository.GetUserById(request.TargetUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);

            return (StatusType.Ok, new UserGSTViewModel
            {
                UserMasterID = user.UserMasterID,
                FirmName = user.FirmName ?? string.Empty,
                GSTNo = user.GSTNo ?? string.Empty,
                PANNo = user.PANNo ?? string.Empty,
                CIN = user.CIN ?? string.Empty,
                Address = user.Address ?? string.Empty,
                StateName = user.State?.StateName ?? string.Empty,
                BusinessGSTStateCode = user.BusinessGSTStateCode ?? string.Empty,
                IsInvoiceApplicable = user.IsInvoiceApplicable
            });
        }

        public async Task<(string failureMessage, PincodeDetailsResponse? data)> FetchDetailsForPincode(string pincode)
        {
            if (string.IsNullOrWhiteSpace(pincode) || pincode.Length != 6)
                return ("Invalid pincode. Must be 6 digits.", null);

            // External India Post API integration required.
            // When ported: call https://api.postalpincode.in/pincode/{pincode} and parse response
            // For now return partial info based on pincode prefix mapping to state

            var response = new PincodeDetailsResponse
            {
                Pincode = pincode,
                City = string.Empty,
                District = string.Empty,
                State = string.Empty,
                StateID = 0
            };

            return (StatusType.Ok, response);
        }

        public async Task<(string failureMessage, string token)> LoginAsUser(UserLoginAsRequest request, int currentUserID)
        {
            if (request.TargetUserID <= 0) return (Messages.Common.Message.InvalidRequest, string.Empty);

            var currentUser = await _userRepository.GetUserById(currentUserID);
            if (currentUser == null) return (Messages.UserMessages.UserNotFound, string.Empty);

            if (currentUser.UserType?.UserType != UserTypeConstants.Admin &&
                currentUser.UserType?.UserType != UserTypeConstants.SuperAdmin)
                return (Messages.Common.Message.NoPermission, string.Empty);

            var targetUser = await _userRepository.GetUserById(request.TargetUserID);
            if (targetUser == null) return (Messages.UserMessages.UserNotFound, string.Empty);
            if (!targetUser.IsActive) return (Messages.Common.Message.AccountInactive, string.Empty);

            // JWT token generation integration required.
            // When ported: call JwtHelper.GenerateToken(targetUser) and return the token.

            return (StatusType.Ok, $"LOGIN_AS_USER_{request.TargetUserID}_{DateTime.UtcNow.Ticks}");
        }

        // ─── API-user self-service: API token + IP security (OTP-gated) ──────────────
        // These act on the logged-in user (currentUserID). Only API-user accounts qualify.
        // Changing the token or IP settings requires an OTP delivered to the user's mobile.
        private const string OTPTypeAPIToken = "API Token";
        private const string OTPTypeSecureIP = "Secure IP";

        // Issue a fresh OTP of the given type, invalidating any still-pending one first.
        private async Task SendOtpFor(UserMaster user, string type)
        {
            var existing = await _otpMasterRepository.GetActiveByUserIdAndType(user.UserMasterID, type);
            if (existing != null)
            {
                existing.IsVerified = true;
                existing.UpdatedateTime = DateTime.UtcNow;
                await _otpMasterRepository.Update(existing);
            }

            string otp = OtpHelper.GenerateOtp();
            await _otpMasterRepository.Insert(new OTPMaster
            {
                OTP = otp,
                UserID = user.UserMasterID,
                Type = type,
                IsVerified = false,
                MobileNumber = user.MobileNumber,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            });

            _ = _messageService.SendEventAsync(type, user.UserMasterID, user.MobileNumber,
                user.WhatsAppNumber, user.EmailID,
                new Dictionary<string, string>
                {
                    ["OTP"] = otp,
                    ["FirmName"] = user.FirmName ?? string.Empty,
                    ["CustomerName"] = user.PersonName ?? string.Empty,
                },
                fallbackSms: $"Your OTP is {otp}. Do not share it with anyone.", isOtpDefault: true);
        }

        // Verify the supplied OTP against the pending record and consume it on success.
        private async Task<bool> ValidateOtp(int userID, string type, string otp)
        {
            if (string.IsNullOrWhiteSpace(otp)) return false;
            var record = await _otpMasterRepository.GetActiveByUserIdAndType(userID, type);
            if (record == null || record.OTP != otp) return false;
            record.IsVerified = true;
            record.UpdatedateTime = DateTime.UtcNow;
            await _otpMasterRepository.Update(record);
            return true;
        }

        public async Task<(string failureMessage, APITokenResponse? data)> SelectAPIToken(int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);
            if (user.UserTypeID != UserTypeCode.ApiUser) return (Messages.UserMessages.InvalidUserType, null);
            return (StatusType.Ok, new APITokenResponse { APIToken = user.APIToken ?? string.Empty });
        }

        public async Task<string> SendAPITokenOTP(int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.UserTypeID != UserTypeCode.ApiUser) return Messages.UserMessages.InvalidUserType;
            if (string.IsNullOrWhiteSpace(user.MobileNumber)) return "User has no registered mobile number.";
            await SendOtpFor(user, OTPTypeAPIToken);
            return StatusType.Ok;
        }

        public async Task<(string failureMessage, APITokenResponse? data)> TokenGeneration(TokenGenerationRequest request, int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);
            if (user.UserTypeID != UserTypeCode.ApiUser) return (Messages.UserMessages.InvalidUserType, null);
            if (!await ValidateOtp(currentUserID, OTPTypeAPIToken, request.OTP))
                return (Messages.Common.Message.InvalidOTP, null);

            user.APIToken = Guid.NewGuid().ToString("N").Substring(0, 20);
            if (!await _userRepository.UpdateUser(user)) return ("Token generation failed. Please try again.", null);
            return (StatusType.Ok, new APITokenResponse { APIToken = user.APIToken });
        }

        public async Task<(string failureMessage, SecureIPResponse? data)> SelectIsSecureIP(int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, null);
            if (user.UserTypeID != UserTypeCode.ApiUser) return (Messages.UserMessages.InvalidUserType, null);
            return (StatusType.Ok, new SecureIPResponse { IsIPSecure = user.IsIPSecure, WhitelistedIPs = user.WhitelistedIPs ?? string.Empty });
        }

        public async Task<string> SendSecureIPOTP(int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.UserTypeID != UserTypeCode.ApiUser) return Messages.UserMessages.InvalidUserType;
            if (string.IsNullOrWhiteSpace(user.MobileNumber)) return "User has no registered mobile number.";
            await SendOtpFor(user, OTPTypeSecureIP);
            return StatusType.Ok;
        }

        public async Task<string> UpdateIsSecureIP(UpdateSecureIPRequest request, int currentUserID)
        {
            var user = await _userRepository.GetUserById(currentUserID);
            if (user == null) return Messages.UserMessages.UserNotFound;
            if (user.UserTypeID != UserTypeCode.ApiUser) return Messages.UserMessages.InvalidUserType;
            if (!await ValidateOtp(currentUserID, OTPTypeSecureIP, request.OTP))
                return Messages.Common.Message.InvalidOTP;

            user.IsIPSecure = request.IsIPSecure;
            user.WhitelistedIPs = (request.WhitelistedIPs ?? string.Empty).Trim();
            if (!await _userRepository.UpdateUser(user)) return "Update failed. Please try again.";
            return StatusType.Ok;
        }
    }
}
