using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SubscriptionPlan;
using FintechApp.Responses.SubscriptionPlan;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ISubscriptionPlanService"/>
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IServiceSubscriptionCreatePlanMasterRepository _planRepo;
        private readonly IServiceSubscriptionPlanDetailsRepository _detailRepo;
        private readonly IUserSubscriptionRepository _subRepo;
        private readonly IUserRepository _userRepo;

        // Roles that can be required to hold a subscription (admin / support / back-office excluded).
        private static readonly HashSet<int> RequiredRoles = new()
        {
            (int)UserTypeCode.SuperDistributor, // 2
            (int)UserTypeCode.Distributor,      // 3
            (int)UserTypeCode.Retailer,         // 4
            (int)UserTypeCode.ApiUser,          // 5
            (int)UserTypeCode.SDFOS,            // 10
            (int)UserTypeCode.DFOS,             // 11
        };

        public SubscriptionPlanService(
            IServiceSubscriptionCreatePlanMasterRepository planRepo,
            IServiceSubscriptionPlanDetailsRepository detailRepo,
            IUserSubscriptionRepository subRepo,
            IUserRepository userRepo)
        {
            _planRepo = planRepo;
            _detailRepo = detailRepo;
            _subRepo = subRepo;
            _userRepo = userRepo;
        }

        // ── Plan management ───────────────────────────────────────────────────────────
        public async Task<SavePlanResult> SavePlan(SavePlanRequest request)
        {
            var now = DateTime.UtcNow;
            var desired = (request.ServiceIDs ?? new List<int>())
                .Where(id => id > 0).Distinct().ToList();

            int planId;
            if (request.PlanID <= 0)
            {
                var entity = new ServiceSubscriptionCreatePlanMaster
                {
                    PlanName = request.PlanName?.Trim() ?? string.Empty,
                    Amount = request.Amount,
                    ImageURL = request.ImageURL ?? string.Empty,
                    IsDefault = request.IsDefault,
                    ValidityDays = request.ValidityDays < 0 ? 0 : request.ValidityDays,
                    DateTime = now,
                    UpdatedateTime = now,
                };
                await _planRepo.Insert(entity);
                planId = entity.ServiceSubscriptionCreatePlanMasterID;
            }
            else
            {
                var entity = await _planRepo.GetByID(request.PlanID);
                if (entity == null) return new SavePlanResult { PlanID = 0, Status = Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound };
                entity.PlanName = request.PlanName?.Trim() ?? string.Empty;
                entity.Amount = request.Amount;
                entity.ImageURL = request.ImageURL ?? string.Empty;
                entity.IsDefault = request.IsDefault;
                entity.ValidityDays = request.ValidityDays < 0 ? 0 : request.ValidityDays;
                entity.UpdatedateTime = now;
                await _planRepo.Update(entity);
                planId = request.PlanID;
            }

            // Only one plan may be the default — clear it on every other plan.
            if (request.IsDefault)
            {
                var all = await _planRepo.GetAll();
                foreach (var other in all.Where(p => p.IsDefault && p.ServiceSubscriptionCreatePlanMasterID != planId))
                {
                    other.IsDefault = false;
                    other.UpdatedateTime = now;
                    await _planRepo.Update(other);
                }
            }

            await ReconcileServices(planId, desired, now);
            return new SavePlanResult { PlanID = planId, Status = StatusType.Ok };
        }

        // Add links for newly-selected services, soft-delete links for de-selected ones.
        private async Task ReconcileServices(int planId, List<int> desired, DateTime now)
        {
            var details = await _detailRepo.GetAll();
            var current = details.Where(d => d.ServiceSubscriptionCreatePlanMasterID == planId).ToList();
            var currentServiceIds = current.Select(d => d.ServiceID).ToHashSet();

            foreach (int serviceId in desired.Where(id => !currentServiceIds.Contains(id)))
            {
                await _detailRepo.Insert(new ServiceSubscriptionPlanDetails
                {
                    ServiceSubscriptionCreatePlanMasterID = planId,
                    ServiceID = serviceId,
                    DateTime = now,
                    UpdatedateTime = now,
                });
            }

            foreach (var stale in current.Where(d => !desired.Contains(d.ServiceID)))
            {
                await _detailRepo.Delete(stale.ServiceSubscriptionPlanDetailsID);
            }
        }

        public async Task<List<SubscriptionPlanDetailResponse>> GetPlansWithServices()
        {
            var plans = await _planRepo.GetAll();
            var details = await _detailRepo.GetAll();
            var byPlan = details
                .GroupBy(d => d.ServiceSubscriptionCreatePlanMasterID)
                .ToDictionary(g => g.Key, g => g.Select(d => d.ServiceID).Distinct().ToList());

            return plans
                .Select(p => Map(p, byPlan.TryGetValue(p.ServiceSubscriptionCreatePlanMasterID, out var ids) ? ids : new List<int>()))
                .OrderByDescending(p => p.IsDefault)
                .ThenBy(p => p.PlanName)
                .ToList();
        }

        public async Task<SubscriptionPlanDetailResponse?> GetPlanWithServices(int planId)
        {
            var plan = await _planRepo.GetByID(planId);
            if (plan == null) return null;
            return Map(plan, await GetServiceIdsForPlan(planId));
        }

        public async Task<string> DeletePlan(int planId)
        {
            var plan = await _planRepo.GetByID(planId);
            if (plan == null) return Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound;

            var details = await _detailRepo.GetAll();
            foreach (var d in details.Where(d => d.ServiceSubscriptionCreatePlanMasterID == planId))
            {
                await _detailRepo.Delete(d.ServiceSubscriptionPlanDetailsID);
            }
            int result = await _planRepo.Delete(planId);
            return result > 0 ? StatusType.Ok : Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound;
        }

        public async Task<List<int>> GetServiceIdsForPlan(int planId)
        {
            var details = await _detailRepo.GetAll();
            return details
                .Where(d => d.ServiceSubscriptionCreatePlanMasterID == planId)
                .Select(d => d.ServiceID)
                .Distinct()
                .ToList();
        }

        public async Task<List<int>> GetAllPlanServiceIds()
        {
            var details = await _detailRepo.GetAll();
            return details.Select(d => d.ServiceID).Distinct().ToList();
        }

        public async Task<decimal> GetPlanAmount(int planId)
        {
            var plan = await _planRepo.GetByID(planId);
            return plan?.Amount ?? 0m;
        }

        // ── Subscription lifecycle ────────────────────────────────────────────────────
        public async Task RecordSubscription(int userId, int planId)
        {
            var plan = await _planRepo.GetByID(planId);
            if (plan == null) return;
            var now = DateTime.UtcNow;
            var expiry = plan.ValidityDays > 0 ? now.AddDays(plan.ValidityDays) : now.AddYears(100);

            // Retire any currently-active rows for the user so the newest one is unambiguous.
            var existing = await _subRepo.GetByUserID(userId);
            foreach (var row in existing.Where(r => r.IsActive))
            {
                row.IsActive = false;
                row.UpdatedateTime = now;
                await _subRepo.Update(row);
            }

            await _subRepo.Insert(new UserSubscription
            {
                UserID = userId,
                PlanID = planId,
                Amount = plan.Amount,
                StartDate = now,
                ExpiryDate = expiry,
                IsActive = true,
                DateTime = now,
                UpdatedateTime = now,
            });
        }

        public async Task<UserSubscription?> GetActiveSubscription(int userId)
        {
            var now = DateTime.UtcNow;
            var rows = await _subRepo.GetByUserID(userId);
            return rows
                .Where(r => r.IsActive && r.ExpiryDate > now)
                .OrderByDescending(r => r.ExpiryDate)
                .FirstOrDefault();
        }

        public async Task<bool> IsEffectivelyExempt(int userId)
        {
            var seen = new HashSet<int>();
            int? cur = userId;
            while (cur.HasValue && cur.Value > 0 && seen.Add(cur.Value))
            {
                var u = await _userRepo.GetUserById(cur.Value);
                if (u == null) break;
                if (u.IsSubscriptionExempt) return true;
                cur = u.ParentID;
            }
            return false;
        }

        public async Task<MySubscriptionStatusResponse> GetMyStatus(int userId, bool enabled, int userTypeId)
        {
            var status = new MySubscriptionStatusResponse { Enabled = enabled };
            if (!enabled) return status;

            status.Exempt = await IsEffectivelyExempt(userId);
            status.Required = !status.Exempt && RequiredRoles.Contains(userTypeId);

            var active = await GetActiveSubscription(userId);
            if (active != null)
            {
                var plan = await _planRepo.GetByID(active.PlanID);
                status.Active = true;
                status.PlanID = active.PlanID;
                status.PlanName = plan?.PlanName ?? string.Empty;
                status.ExpiryDate = active.ExpiryDate;
                status.DaysRemaining = DaysLeft(active.ExpiryDate);
            }
            return status;
        }

        public async Task<List<UserSubscriptionStatusResponse>> GetUsersStatus()
        {
            var users = await _userRepo.GetAllUsers();
            var subs = await _subRepo.GetAll();
            var plans = await _planRepo.GetAll();
            var now = DateTime.UtcNow;
            var planName = plans.ToDictionary(p => p.ServiceSubscriptionCreatePlanMasterID, p => p.PlanName);

            var result = new List<UserSubscriptionStatusResponse>();
            foreach (var u in users.Where(u => RequiredRoles.Contains(u.UserTypeID ?? 0)))
            {
                var active = subs
                    .Where(s => s.UserID == u.UserMasterID && s.IsActive && s.ExpiryDate > now)
                    .OrderByDescending(s => s.ExpiryDate)
                    .FirstOrDefault();

                result.Add(new UserSubscriptionStatusResponse
                {
                    UserID = u.UserMasterID,
                    PersonName = u.PersonName ?? string.Empty,
                    FirmName = u.FirmName ?? string.Empty,
                    MobileNumber = u.MobileNumber ?? string.Empty,
                    UserTypeID = u.UserTypeID ?? 0,
                    Exempt = await IsEffectivelyExempt(u.UserMasterID),
                    Active = active != null,
                    PlanName = active != null && planName.TryGetValue(active.PlanID, out var n) ? n : string.Empty,
                    ExpiryDate = active?.ExpiryDate,
                    DaysRemaining = active != null ? DaysLeft(active.ExpiryDate) : 0,
                });
            }
            return result
                .OrderBy(r => r.Exempt)
                .ThenBy(r => r.Active)
                .ThenBy(r => r.DaysRemaining)
                .ToList();
        }

        public async Task<int> GetAdminUserId()
        {
            var admins = await _userRepo.GetUsersByUserType((int)UserTypeCode.Admin);
            var admin = admins.Where(a => a.IsActive).OrderBy(a => a.UserMasterID).FirstOrDefault()
                        ?? admins.OrderBy(a => a.UserMasterID).FirstOrDefault();
            return admin?.UserMasterID ?? 0;
        }

        public async Task<string> SetExempt(int userId, bool isExempt)
        {
            var user = await _userRepo.GetUserById(userId);
            if (user == null) return Messages.Common.Message.NotFound;
            user.IsSubscriptionExempt = isExempt;
            await _userRepo.UpdateUser(user);
            return StatusType.Ok;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────
        private static int DaysLeft(DateTime expiry)
        {
            int days = (int)Math.Ceiling((expiry - DateTime.UtcNow).TotalDays);
            return days < 0 ? 0 : days;
        }

        private static SubscriptionPlanDetailResponse Map(ServiceSubscriptionCreatePlanMaster p, List<int> serviceIds) => new()
        {
            ServiceSubscriptionCreatePlanMasterID = p.ServiceSubscriptionCreatePlanMasterID,
            PlanName = p.PlanName,
            Amount = p.Amount,
            ImageURL = p.ImageURL,
            IsDefault = p.IsDefault,
            ValidityDays = p.ValidityDays,
            ServiceIDs = serviceIds,
            ServiceCount = serviceIds.Count,
            DateTime = p.DateTime,
            UpdatedateTime = p.UpdatedateTime,
        };
    }
}
