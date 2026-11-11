using FintechApp.Entities;
using FintechApp.Requests.User;
using FintechApp.Responses.User;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// The "one mobile number, two roles" feature — see <see cref="LinkedProfilePolicy"/> for the
    /// rules and why it exists. This part owns reading a person's logins and creating the second
    /// one; <c>AuthService</c> owns choosing between them at sign-in and switching afterwards.
    /// </summary>
    public partial class UserService
    {
        /// <summary>
        /// Every login this person has, primary first.
        ///
        /// Resolved from the group, never from the mobile number alone: two accounts sharing a
        /// number by accident (a legacy row, an admin who reused one) must not become a pair of
        /// identities someone can hop between. The link is explicit and it is the only thing read.
        /// </summary>
        public async Task<(string failureMessage, LinkedProfilesResponse data)> GetLinkedProfiles(int currentUserID)
        {
            var response = new LinkedProfilesResponse();
            var users = await _userRepository.GetAllUsers();

            var me = users.FirstOrDefault(u => u.UserMasterID == currentUserID);
            if (me == null) return (Messages.UserMessages.UserNotFound, response);

            var primary = PrimaryOf(users, me);
            var group = GroupOf(users, primary);

            response.PrimaryUserID = primary.UserMasterID;
            response.CurrentUserID = currentUserID;
            response.Profiles = group.Select(u => new LinkedProfileItem
            {
                UserID = u.UserMasterID,
                UserTypeID = u.UserTypeID ?? 0,
                RoleName = RoleName(u.UserTypeID ?? 0),
                FirmName = u.FirmName ?? string.Empty,
                PersonName = u.PersonName ?? string.Empty,
                UserCode = u.AliasName ?? string.Empty,
                MobileNumber = u.MobileNumber ?? string.Empty,
                IsPrimary = u.UserMasterID == primary.UserMasterID,
                IsCurrent = u.UserMasterID == currentUserID,
                IsActive = u.IsActive,
            }).ToList();

            // Only the primary may add one, and only while it has none — the switcher is a pair,
            // not a tree. A secondary sees the list (it needs the switcher) and no Add option.
            bool isPrimary = currentUserID == primary.UserMasterID;
            int primaryRole = primary.UserTypeID ?? 0;
            response.CanSetUp = isPrimary
                               && LinkedProfilePolicy.CanSetUp(primaryRole)
                               && group.Count == 1
                               && primary.IsActive;
            response.AllowedRoles = response.CanSetUp
                ? LinkedProfilePolicy.AllowedFor(primaryRole)
                      .Select(r => new LinkedProfileRoleOption { UserTypeID = r, RoleName = RoleName(r) })
                      .ToList()
                : new List<LinkedProfileRoleOption>();

            // Why the option is absent, in the words of the rule that withheld it. A blank card
            // reads as a broken feature; this is what the profile screen prints instead.
            if (!response.CanSetUp)
            {
                response.Reason = !isPrimary
                    ? "A second login is set up from the main account."
                    : !LinkedProfilePolicy.CanSetUp(primaryRole)
                        ? "Your account type does not use a second login."
                        : group.Count > 1
                            ? "You already have a second login."
                            : "Your account must be active to add a second login.";
            }

            return (string.Empty, response);
        }

        /// <summary>
        /// Create this person's second login.
        ///
        /// A real account, bootstrapped the same way Add User bootstraps one — wallets, user code,
        /// optional slab — but it shares the primary's mobile number and password hash, which is
        /// the whole point. Nothing is texted: the person already knows their own credentials.
        /// </summary>
        public async Task<(string failureMessage, LinkedProfilesResponse data)> CreateLinkedProfile(
            CreateLinkedProfileRequest request, int currentUserID)
        {
            var empty = new LinkedProfilesResponse();
            var users = await _userRepository.GetAllUsers();

            var primary = users.FirstOrDefault(u => u.UserMasterID == currentUserID);
            if (primary == null) return (Messages.UserMessages.UserNotFound, empty);
            if (primary.LinkedPrimaryUserID.HasValue)
                return ("A second login is set up from the main account.", empty);
            if (!primary.IsActive)
                return ("Your account must be active to add a second login.", empty);

            int primaryRole = primary.UserTypeID ?? 0;
            if (!LinkedProfilePolicy.CanSetUp(primaryRole))
                return ("Your account type does not use a second login.", empty);
            if (!LinkedProfilePolicy.IsAllowed(primaryRole, request.UserTypeID))
                return ("That role is not available as a second login for your account type.", empty);
            if (GroupOf(users, primary).Count > 1)
                return ("You already have a second login.", empty);

            // Where it sits in the network. The secondary belongs under the primary when the
            // primary is a legal upline for that role (a distributor's own counter maps under the
            // distributor, which is also what makes the commission split come out right). An API
            // user is not a legal upline for anyone, so its retailer login maps where the API user
            // itself maps — alongside it, under the same admin.
            int parentID = AllowedParentTiers.TryGetValue(request.UserTypeID, out var tiers)
                           && tiers.Contains(primaryRole)
                ? primary.UserMasterID
                : (primary.ParentID ?? 0);
            if (parentID <= 0) return ("Your account has no upline to place a second login under.", empty);

            string firmName = (request.FirmName ?? string.Empty).Trim();
            if (firmName.Length == 0)
                firmName = $"{primary.FirmName} — {LinkedProfilePolicy.DefaultFirmSuffix(request.UserTypeID)}";
            if (firmName.Length > 100) firmName = firmName[..100];

            string userCode = await _userCodeService.GenerateUserCode(request.UserTypeID);

            var secondary = new UserMaster
            {
                LinkedPrimaryUserID = primary.UserMasterID,
                FirmName = firmName,
                PersonName = primary.PersonName,
                MobileNumber = primary.MobileNumber,
                // EmailID is the alternate login identifier and has to stay unique, so the second
                // account gets its own derived address rather than a copy of the primary's.
                EmailID = $"{primary.MobileNumber}.{userCode}@user.app".ToLowerInvariant(),
                // The same password, because it is the same person. Changing it on the primary
                // carries across (see SyncLinkedCredentials) — one login, two roles.
                PasswordHash = primary.PasswordHash,
                UserTypeID = request.UserTypeID,
                ParentID = parentID,
                Address = primary.Address,
                CityName = primary.CityName,
                Pincode = primary.Pincode,
                StateID = primary.StateID,
                GSTNo = primary.GSTNo,
                PANNo = primary.PANNo,
                AadharNo = primary.AadharNo,
                WhatsAppNumber = primary.WhatsAppNumber,
                Mpin = primary.Mpin,
                MpinHash = primary.MpinHash,
                IsMpinEnabled = primary.IsMpinEnabled,
                AliasName = userCode,
                ShopAddress = primary.ShopAddress,
                APIToken = Guid.NewGuid().ToString("N"),
                UniqueReferenceNo = GenerateUniqueReferenceNo(),
                // KYC is held against the person, and the person is already verified — making them
                // re-submit the same Aadhaar for their own second counter is pure friction.
                IsKYCApproved = primary.IsKYCApproved,
                IseKYC = primary.IseKYC,
                IsActive = true,
                JoiningByUserID = currentUserID,
                RegistrationType = "Linked",
                IsInvoiceApplicable = primary.IsInvoiceApplicable,
                IsInitialPassword = false,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false,
            };

            if (!await _userRepository.CreateUser(secondary))
                return ("Could not create the second login. Please try again.", empty);

            var wallets = await _walletMasterRepository.Select();
            foreach (var wallet in wallets.Where(w => w.IsActive))
                await _userWalletMasterRepository.Insert(new UserWalletMaster
                {
                    UserID = secondary.UserMasterID,
                    WalletID = wallet.WalletID,
                    Balance = 0,
                });

            // The primary's own slab is the sensible starting point — it is the same business.
            // An admin can re-assign it from Assign Slab like any other account.
            var primarySlab = (await _userSlabMappingRepository.GetByUserID(primary.UserMasterID))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.UserSlabMappingID)
                .FirstOrDefault();
            if (primarySlab != null)
                await _userSlabMappingRepository.Insert(new UserSlabMapping
                {
                    UserID = secondary.UserMasterID,
                    SlabMasterID = primarySlab.SlabMasterID,
                });

            // Same service grants the primary holds, so the second login is usable immediately
            // rather than being an account that can sign in and do nothing.
            foreach (var grant in (await _userServiceMappingRepository.GetByUserID(primary.UserMasterID))
                         .Where(m => m.IsGranted)
                         .GroupBy(m => m.ServiceID).Select(g => g.First()))
                await _userServiceMappingRepository.Insert(new UserServiceMapping
                {
                    UserID = secondary.UserMasterID,
                    ServiceID = grant.ServiceID,
                    IsGranted = true,
                    IsActive = grant.IsActive,
                });

            await LogChange(secondary.UserMasterID, currentUserID, ChangeAction.Create, "Second login",
                string.Empty,
                $"{RoleName(request.UserTypeID)} {firmName}, code {userCode}, linked to {primary.FirmName} ({primary.MobileNumber})",
                "Second login created from profile");
            await LogChange(primary.UserMasterID, currentUserID, ChangeAction.Update, "Second login",
                "None", $"{RoleName(request.UserTypeID)} ({userCode})", "Second login created from profile");

            return await GetLinkedProfiles(currentUserID);
        }

        // The password itself is kept in step by UserRepository.UpdateUser, which mirrors a
        // changed hash across the group — every path that can change a password goes through it,
        // so none of them has to know this feature exists.

        // ---- group resolution ------------------------------------------------------------

        /// <summary>The account that owns the group — itself, when it is not a linked second login.</summary>
        internal static UserMaster PrimaryOf(IEnumerable<UserMaster> users, UserMaster user)
        {
            if (user.LinkedPrimaryUserID is not int primaryID || primaryID <= 0) return user;
            return users.FirstOrDefault(u => u.UserMasterID == primaryID) ?? user;
        }

        /// <summary>The primary plus every login linked to it, primary first.</summary>
        internal static List<UserMaster> GroupOf(IEnumerable<UserMaster> users, UserMaster primary)
        {
            var all = users as IList<UserMaster> ?? users.ToList();
            var group = new List<UserMaster> { primary };
            group.AddRange(all
                .Where(u => u.LinkedPrimaryUserID == primary.UserMasterID && u.UserMasterID != primary.UserMasterID)
                .OrderBy(u => u.UserMasterID));
            return group;
        }
    }
}
