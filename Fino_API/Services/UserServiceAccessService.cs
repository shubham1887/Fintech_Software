using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IUserServiceAccessService"/>
    public class UserServiceAccessService : IUserServiceAccessService
    {
        /// <summary>Key Setting master switch for the KYC gate (curated feature toggle).</summary>
        public const string KycEnforcedKey = "IsKYCEnforced";

        private readonly IUserServiceMappingRepository _mappingRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISettingMasterRepository _settingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IVerificationDocumentConfigRepository _documentConfigRepository;
        private readonly ISystemDowntimeService _downtime;

        public UserServiceAccessService(
            IUserServiceMappingRepository mappingRepository,
            IUserRepository userRepository,
            ISettingMasterRepository settingRepository,
            IServiceRepository serviceRepository,
            IVerificationDocumentConfigRepository documentConfigRepository,
            ISystemDowntimeService downtime)
        {
            _mappingRepository = mappingRepository;
            _userRepository = userRepository;
            _settingRepository = settingRepository;
            _serviceRepository = serviceRepository;
            _documentConfigRepository = documentConfigRepository;
            _downtime = downtime;
        }

        public async Task<Dictionary<int, bool>?> GetGrants(int userID)
        {
            if (userID <= 0) return null;

            var user = await _userRepository.GetUserById(userID);
            int role = user?.UserTypeID ?? 0;
            // Admin and support run the platform; their access is never narrowed by a grant list.
            if (role == UserTypeCode.Admin || role == UserTypeCode.SupportUser) return null;

            var mappings = await _mappingRepository.GetByUserID(userID);
            // No rows at all = an account created before services became assignable. Treating that
            // as "nothing granted" would lock the user out of the whole app, so it means unrestricted;
            // access only narrows once an admin has actually saved a grid for them.
            if (mappings.Count == 0) return null;

            // Off rows are simply left out of the map, so a withheld service reads the same as one
            // that was never listed at all: NotGranted.
            var grants = new Dictionary<int, bool>();
            foreach (var m in mappings.Where(x => x.IsGranted))
                grants[m.ServiceID] = m.IsActive || (grants.TryGetValue(m.ServiceID, out bool on) && on);
            return grants;
        }

        public async Task<ServiceAccessContext> GetContext(int userID)
        {
            if (userID <= 0) return ServiceAccessContext.Unrestricted;

            var user = await _userRepository.GetUserById(userID);
            int role = user?.UserTypeID ?? 0;
            if (user == null || role == UserTypeCode.Admin || role == UserTypeCode.SupportUser)
                return ServiceAccessContext.Unrestricted;

            return new ServiceAccessContext
            {
                Grants = await GetGrants(userID),
                KycBlocked = await IsKycBlocked(user),
                KycExemptServiceIDs = await KycExemptServiceIDs(),
                // Admin and support returned above, so the window never closes on the two roles
                // that have to be able to settle and repair things while the platform is down.
                SystemDown = await _downtime.GetState(),
            };
        }

        /// <summary>
        /// Whether this user is currently held back by the KYC gate. Three things must all be true:
        /// the platform enforces KYC, the user's KYC is not approved, and there is actually a way
        /// for them to get approved — an E-KYC flow, or at least one required document configured.
        /// That last check is the safety valve: on a deployment where no verification documents
        /// have been set up yet, enforcing KYC would lock every user out of every service with no
        /// route back in, so the gate stays open until the admin configures what to verify.
        /// </summary>
        private async Task<bool> IsKycBlocked(Entities.UserMaster user)
        {
            if (user.IsKYCApproved) return false;
            if (!await IsEnforced()) return false;

            var settings = await _settingRepository.GetAll();
            bool eVerification = IsSettingOn(settings, "eVerification");
            if (eVerification) return true; // E-KYC is the route out.

            bool anyRequiredDocument = (await _documentConfigRepository.GetAll())
                .Any(d => d.IsActive && d.IsRequired);
            return anyRequiredDocument;
        }

        private async Task<bool> IsEnforced()
        {
            var settings = await _settingRepository.GetAll();
            return IsSettingOn(settings, KycEnforcedKey);
        }

        private async Task<HashSet<int>> KycExemptServiceIDs()
        {
            var services = await _serviceRepository.GetAllServices();
            return services.Where(s => s.IsAllowedWithoutKYC).Select(s => s.ServiceID).ToHashSet();
        }

        /// <summary>Reads a curated Key Setting the same way the Key Setting screen writes it.</summary>
        private static bool IsSettingOn(IEnumerable<Entities.SettingMaster> settings, string key)
        {
            var row = settings.FirstOrDefault(s =>
                (s.Key ?? string.Empty).Equals(key, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted);
            if (row == null) return false; // key never seeded ⇒ feature off (never break an old DB).
            string v = (row.Value ?? string.Empty).Trim().ToLowerInvariant();
            if (v.Length == 0) return row.IsOn;
            if (v is "true" or "false") return v == "true";
            return v is "1" or "yes" or "on" or "y";
        }

        public async Task<ServiceAccess> GetAccess(int userID, int serviceID)
        {
            return Resolve(await GetContext(userID), serviceID);
        }

        public async Task<string> EnsureUsable(int userID, int serviceID)
        {
            // Resolved from the context rather than from GetAccess alone, so a maintenance refusal
            // can quote the operator's own wording ("back at 02:00, DMT still running") instead of
            // a generic sentence. Every other outcome has nothing configurable to say.
            var context = await GetContext(userID);
            var access = Resolve(context, serviceID);
            return access == ServiceAccess.SystemDown
                ? context.SystemDown.Message
                : Describe(access);
        }

        /// <summary>Where a service stands given an already-loaded grant map (null = unrestricted).</summary>
        public static ServiceAccess Resolve(Dictionary<int, bool>? grants, int serviceID)
        {
            if (grants == null) return ServiceAccess.Granted;
            if (!grants.TryGetValue(serviceID, out bool isActive)) return ServiceAccess.NotGranted;
            return isActive ? ServiceAccess.Granted : ServiceAccess.Locked;
        }

        /// <summary>
        /// Where a service stands for a user whose whole access picture is already loaded. The
        /// admin's grant decision comes first — a service the user was never given stays hidden
        /// whatever their KYC says — and the KYC gate is applied to what's left.
        /// </summary>
        public static ServiceAccess Resolve(ServiceAccessContext context, int serviceID)
        {
            var access = Resolve(context.Grants, serviceID);
            if (access != ServiceAccess.Granted) return access;
            if (context.KycBlocked && !context.KycExemptServiceIDs.Contains(serviceID))
                return ServiceAccess.KycLocked;
            // Last, because it is the most temporary of the three and the least about this user:
            // a service they were never granted should not report itself as "down for
            // maintenance", which would imply it is coming back.
            if (context.SystemDown.Blocks(serviceID))
                return ServiceAccess.SystemDown;
            return ServiceAccess.Granted;
        }

        /// <summary>StatusType.Ok, or the message explaining why the service can't be used.</summary>
        public static string Describe(ServiceAccess access) => access switch
        {
            ServiceAccess.Locked => "This service is locked for your account. Please contact your service provider.",
            ServiceAccess.NotGranted => "This service is not enabled for your account. Please contact your service provider.",
            ServiceAccess.KycLocked => "Your KYC is not approved yet. Complete your KYC from your profile to use this service.",
            // Fallback wording only. Callers that can reach the resolved context (EnsureUsable)
            // use the operator's configured message instead, which names the window.
            ServiceAccess.SystemDown => StaticModels.SystemDownPolicy.DefaultMessage,
            _ => StatusType.Ok,
        };

        /// <summary>Short reason shown on a locked service tile ("" when the service is usable).</summary>
        public static string LockReason(ServiceAccess access) => access switch
        {
            ServiceAccess.Locked => "Locked by your service provider",
            ServiceAccess.KycLocked => "KYC pending",
            ServiceAccess.SystemDown => "Under maintenance",
            _ => string.Empty,
        };
    }
}
