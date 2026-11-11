using FintechApp.Helpers;
using FintechApp.Requests.User;
using FintechApp.Responses.User;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Network-shape operations on users, plus the change-history report every other part of
    /// <see cref="UserService"/> writes to.
    /// </summary>
    public partial class UserService
    {
        /// <summary>
        /// Move one user under a new parent — the User Transfer screen.
        ///
        /// The endpoint behind that screen used to answer "Feature under development.", which is
        /// why the page did nothing at all. This is the single-user form of the Shift action in
        /// <see cref="BulkUpdate"/> and applies the same tree rules, because a bad move corrupts
        /// the network:
        ///   - a user cannot become their own parent;
        ///   - the new parent cannot be inside that user's own downline (that detaches the branch
        ///     from the tree and points it at itself);
        ///   - the new parent must sit ABOVE the user in the role chain (admin may hold anyone);
        ///   - a non-admin may only move users already inside their own network, and only under
        ///     themselves or someone else inside it.
        ///
        /// Descendants travel with the user: they point at the user, not at the old parent, so
        /// re-parenting the user re-parents the whole branch in one write.
        /// </summary>
        public async Task<(string failureMessage, string summary)> TransferUsers(
            UserTransferRequest request, int currentUserID)
        {
            if (request.UserID <= 0) return ("Choose the user to move.", string.Empty);
            if (request.NewParentID <= 0) return ("Choose the user to move them under.", string.Empty);
            if (request.UserID == request.NewParentID) return ("A user cannot be mapped under themselves.", string.Empty);
            if (string.IsNullOrWhiteSpace(request.Remark))
                return ("Add a remark — a re-parent should be explainable afterwards.", string.Empty);

            var allUsers = await _userRepository.GetAllUsers();
            var user = allUsers.FirstOrDefault(u => u.UserMasterID == request.UserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, string.Empty);

            var newParent = allUsers.FirstOrDefault(u => u.UserMasterID == request.NewParentID);
            if (newParent == null) return ("The new parent was not found.", string.Empty);
            if (!newParent.IsActive) return ("The new parent is deactivated — activate it first.", string.Empty);

            var caller = allUsers.FirstOrDefault(u => u.UserMasterID == currentUserID);
            bool callerIsAdmin = caller != null
                && (caller.UserTypeID == UserTypeCode.Admin || caller.UserTypeID == UserTypeCode.SupportUser);
            if (!callerIsAdmin)
            {
                var callerDownline = UserHierarchy.GetDownlineIds(allUsers, currentUserID);
                if (!callerDownline.Contains(user.UserMasterID))
                    return ("That user is outside your network.", string.Empty);
                if (!callerDownline.Contains(newParent.UserMasterID) && newParent.UserMasterID != currentUserID)
                    return ("The new parent is outside your network.", string.Empty);
            }

            int parentWas = user.ParentID ?? 0;
            if (parentWas == newParent.UserMasterID)
                return ("That user is already mapped under this parent.", string.Empty);

            var userDownline = UserHierarchy.GetDownlineIds(allUsers, user.UserMasterID);
            if (userDownline.Contains(newParent.UserMasterID))
                return ("A user cannot be mapped under their own downline.", string.Empty);

            int parentType = newParent.UserTypeID ?? 0;
            int childType = user.UserTypeID ?? 0;
            if (parentType != UserTypeCode.Admin && childType > 0 && parentType >= childType)
                return ("The new parent must be a higher role than the user.", string.Empty);

            user.ParentID = newParent.UserMasterID;
            user.UpdatedateTime = DateTime.UtcNow;
            if (!await _userRepository.UpdateUser(user))
                return ("User transfer failed. Please try again.", string.Empty);

            await LogChange(user.UserMasterID, currentUserID, ChangeAction.Transfer, "Mapped under",
                await DescribeUser(parentWas), await DescribeUser(newParent.UserMasterID), request.Remark);

            string movedName = string.IsNullOrWhiteSpace(user.FirmName) ? user.PersonName : user.FirmName;
            string parentName = string.IsNullOrWhiteSpace(newParent.FirmName) ? newParent.PersonName : newParent.FirmName;
            // Worth saying out loud: moving one distributor moves every retailer beneath them.
            string branch = userDownline.Count > 0
                ? $" Their downline of {userDownline.Count} user(s) moved with them."
                : string.Empty;
            return (StatusType.Ok, $"{movedName} is now mapped under {parentName}.{branch}");
        }

        /// <summary>
        /// Who may legally become <paramref name="request"/>.TargetUserID's parent — the "New
        /// parent" picker on the User Transfer screen.
        ///
        /// The picker used to be a flat list of every user, which is how the screen offered moves
        /// the server then refused: a retailer under another retailer, a distributor under one of
        /// its own retailers, a user under itself. The eligible set is derived from the role of the
        /// user being moved, so the list only ever holds legal destinations:
        ///   - Retailer    → Admin, Super Distributor, Distributor
        ///   - Distributor → Admin, Super Distributor
        ///   - everyone else (SD / API user) → Admin
        /// then narrowed by the same tree and branch rules <see cref="TransferUsers"/> applies, so
        /// the user's own downline, their current parent and deactivated accounts are all absent.
        ///
        /// Admin is returned as a real selectable row (its own UserMaster id), because "map this
        /// user directly under admin" is a destination the tier rules allow.
        /// </summary>
        public async Task<(string failureMessage, List<UserDDLResponse> data)> SelectEligibleParents(
            UserSelectByIDRequest request, int currentUserID)
        {
            var empty = new List<UserDDLResponse>();
            if (request.TargetUserID <= 0) return ("Choose the user to move first.", empty);

            var allUsers = await _userRepository.GetAllUsers();
            var user = allUsers.FirstOrDefault(u => u.UserMasterID == request.TargetUserID);
            if (user == null) return (Messages.UserMessages.UserNotFound, empty);

            var caller = allUsers.FirstOrDefault(u => u.UserMasterID == currentUserID);
            bool callerIsAdmin = HierarchyScopePolicy.IsUnrestrictedRole(caller?.UserTypeID ?? 0);

            // A non-admin may only move users inside their own network.
            var callerDownline = callerIsAdmin ? null : UserHierarchy.GetDownlineIds(allUsers, currentUserID);
            if (callerDownline != null && !callerDownline.Contains(user.UserMasterID))
                return ("That user is outside your network.", empty);

            int childType = user.UserTypeID ?? 0;
            var legalTiers = AllowedParentTiers.TryGetValue(childType, out var tiers)
                ? tiers
                : new[] { UserTypeCode.Admin };

            // Everything beneath the user travels with it, so none of it can become its parent.
            var userDownline = UserHierarchy.GetDownlineIds(allUsers, user.UserMasterID);
            int currentParent = user.ParentID ?? 0;

            var rows = allUsers
                .Where(u => !u.IsDeleted && u.IsActive)
                .Where(u => legalTiers.Contains(u.UserTypeID ?? 0))
                .Where(u => u.UserMasterID != user.UserMasterID)
                .Where(u => !userDownline.Contains(u.UserMasterID))
                .Where(u => u.UserMasterID != currentParent)
                .Where(u => callerDownline == null
                            || u.UserMasterID == currentUserID
                            || callerDownline.Contains(u.UserMasterID))
                .OrderBy(u => u.UserTypeID)
                .ThenBy(u => u.FirmName)
                .Select(u => new UserDDLResponse
                {
                    UserMasterID = u.UserMasterID,
                    FirmName = string.IsNullOrWhiteSpace(u.FirmName) ? u.PersonName ?? string.Empty : u.FirmName,
                    MobileNumber = u.MobileNumber ?? string.Empty,
                    UserType = RoleName(u.UserTypeID ?? 0),
                    AliasName = u.AliasName ?? string.Empty,
                    UniqueReferenceNo = u.UniqueReferenceNo ?? string.Empty,
                })
                .ToList();

            if (rows.Count == 0)
                return ($"There is no other account a {RoleName(childType)} can be mapped under.", empty);
            return (StatusType.Ok, rows);
        }

        /// <summary>
        /// The user change report — every change made to a user record: what changed, from what to
        /// what, who did it, why, and from which IP / channel.
        ///
        /// Scoped like every other report: admin / support read the whole platform, an upline reads
        /// its own network, anyone else reads only their own account.
        /// </summary>
        public async Task<(string failureMessage, UserChangeHistoryWrapper data)> ChangeHistory(
            UserChangeHistoryRequest request, int callerUserID, int callerUserTypeID)
        {
            var wrapper = new UserChangeHistoryWrapper();

            List<int>? allowed = null;
            bool isAdmin = callerUserTypeID == UserTypeCode.Admin || callerUserTypeID == UserTypeCode.SupportUser;
            if (!isAdmin)
            {
                if (callerUserTypeID == UserTypeCode.Distributor || callerUserTypeID == UserTypeCode.SuperDistributor)
                {
                    var downline = UserHierarchy.GetDownlineIds(await _userRepository.GetAllUsers(), callerUserID);
                    downline.Add(callerUserID);
                    allowed = downline.ToList();
                }
                else
                {
                    allowed = new List<int> { callerUserID };
                }
            }

            DateTime? from = DateTime.TryParse(request.FromDate, out var f) ? f : null;
            DateTime? to = DateTime.TryParse(request.ToDate, out var t) ? t : null;

            // Free text is matched over the rendered row, so one box covers the user, the operator,
            // the field, both values and the reason. Pull a wider page when searching, then clip.
            bool searching = !string.IsNullOrWhiteSpace(request.Search);
            int take = request.TakeCount > 0 ? request.TakeCount : 200;
            var (count, rows) = await _changeHistoryRepository.Search(
                request.UserID, request.ChangedByUserID, from, to, request.Action, allowed,
                searching ? 0 : request.SkipCount, searching ? Math.Max(take, 2000) : take);

            wrapper.Count = count;
            if (rows.Count == 0) return (StatusType.Ok, wrapper);

            // A deleted user still appears in their own history, so read the with-deleted set.
            var everyUser = await _userRepository.GetAllUsersWithDeleted();
            var userById = everyUser.GroupBy(u => u.UserMasterID).ToDictionary(g => g.Key, g => g.First());
            string NameOf(int id) => userById.TryGetValue(id, out var u)
                ? (!string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : u.PersonName)
                : (id > 0 ? $"User {id}" : string.Empty);

            var list = rows.Select(h =>
            {
                userById.TryGetValue(h.UserID, out var target);
                userById.TryGetValue(h.ChangedByUserID, out var actor);
                return new UserChangeHistoryViewModel
                {
                    UserChangeHistoryID = h.UserChangeHistoryID,
                    DOC = h.DateTime,
                    UserID = h.UserID,
                    UserName = NameOf(h.UserID),
                    UserMobile = target?.MobileNumber ?? string.Empty,
                    UserCode = target?.AliasName ?? string.Empty,
                    UserType = RoleName(target?.UserTypeID ?? 0),
                    Action = h.Action,
                    FieldName = h.FieldName,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    Reason = h.Reason,
                    ChangedByUserID = h.ChangedByUserID,
                    ChangedByName = NameOf(h.ChangedByUserID),
                    ChangedByType = RoleName(actor?.UserTypeID ?? 0),
                    IPAddress = h.IPAddress,
                    Platform = h.Platform,
                };
            }).ToList();

            if (searching)
            {
                string s = request.Search.Trim().ToLowerInvariant();
                list = list.Where(x =>
                    x.UserName.ToLowerInvariant().Contains(s)
                    || x.UserMobile.Contains(s)
                    || x.UserCode.ToLowerInvariant().Contains(s)
                    || x.ChangedByName.ToLowerInvariant().Contains(s)
                    || x.Action.ToLowerInvariant().Contains(s)
                    || x.FieldName.ToLowerInvariant().Contains(s)
                    || x.OldValue.ToLowerInvariant().Contains(s)
                    || x.NewValue.ToLowerInvariant().Contains(s)
                    || x.Reason.ToLowerInvariant().Contains(s)
                    || x.IPAddress.Contains(s)).ToList();
                wrapper.Count = list.Count;
                list = list.Skip(Math.Max(request.SkipCount, 0)).Take(take).ToList();
            }

            wrapper.list = list;
            return (StatusType.Ok, wrapper);
        }
    }
}
