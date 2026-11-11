using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SystemDown;
using FintechApp.Responses.SystemDown;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ISystemDowntimeService"/>
    public class SystemDowntimeService : ISystemDowntimeService
    {
        private readonly ISettingMasterRepository _settingRepository;
        private readonly IServiceRepository _serviceRepository;

        public SystemDowntimeService(
            ISettingMasterRepository settingRepository,
            IServiceRepository serviceRepository)
        {
            _settingRepository = settingRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<SystemDownState> GetState()
        {
            var settings = await _settingRepository.GetAll();
            if (!IsSettingOn(settings, SystemDownPolicy.EnabledKey))
                return SystemDownState.Open;

            string from = ValueOf(settings, SystemDownPolicy.FromKey);
            string to = ValueOf(settings, SystemDownPolicy.ToKey);
            string message = ValueOf(settings, SystemDownPolicy.MessageKey);
            if (message.Length == 0) message = SystemDownPolicy.DefaultMessage;

            // Local time, because the window is written by an operator reading a wall clock and
            // the rest of this system stamps transactions with DateTime.Now.
            bool within = SystemDownPolicy.IsWithin(
                DateTime.Now.TimeOfDay,
                SystemDownPolicy.ParseClock(from),
                SystemDownPolicy.ParseClock(to));

            // The exception list is only loaded when it can matter. Outside the window nothing is
            // blocked, so there is no decision for it to inform.
            var allowed = new HashSet<int>();
            if (within)
            {
                foreach (var svc in await _serviceRepository.GetAllServices())
                    if (svc.IsAllowedWhenSystemDown) allowed.Add(svc.ServiceID);
            }

            return new SystemDownState
            {
                IsDown = within,
                IsScheduled = true,
                From = from,
                To = to,
                Message = message,
                AllowedServiceIDs = allowed,
            };
        }

        public async Task<string> EnsureOpen(int serviceID)
        {
            var state = await GetState();
            return state.Blocks(serviceID) ? state.Message : StatusType.Ok;
        }

        public async Task<string> EnsureOpenForKind(string serviceKind)
        {
            var state = await GetState();
            if (!state.IsDown) return StatusType.Ok;

            var services = await _serviceRepository.GetAllServices();
            bool anyAllowed = services
                .Where(s => s.IsActive && !s.IsDelete && ServiceKinds.Matches(s, serviceKind))
                .Any(s => state.AllowedServiceIDs.Contains(s.ServiceID));

            return anyAllowed ? StatusType.Ok : state.Message;
        }

        public async Task<string> EnsureOpenForMoneyMovement()
        {
            var state = await GetState();
            return state.IsDown ? state.Message : StatusType.Ok;
        }

        // ── Admin policy ─────────────────────────────────────────────────────────────

        public async Task<SystemDownPolicyResponse> SelectPolicy()
        {
            var settings = await _settingRepository.GetAll();
            var services = (await _serviceRepository.GetAllServices()).Where(s => !s.IsDelete).ToList();

            bool enabled = IsSettingOn(settings, SystemDownPolicy.EnabledKey);
            string from = ValueOf(settings, SystemDownPolicy.FromKey);
            string to = ValueOf(settings, SystemDownPolicy.ToKey);

            return new SystemDownPolicyResponse
            {
                IsSystemDown = enabled,
                From = from,
                To = to,
                Message = ValueOf(settings, SystemDownPolicy.MessageKey),
                IsDownNow = enabled && SystemDownPolicy.IsWithin(
                    DateTime.Now.TimeOfDay,
                    SystemDownPolicy.ParseClock(from),
                    SystemDownPolicy.ParseClock(to)),
                Services = services
                    .OrderBy(s => s.ServiceCategory).ThenBy(s => s.UISequence).ThenBy(s => s.Name)
                    .Select(s => new SystemDownServiceItem
                    {
                        ServiceID = s.ServiceID,
                        Name = s.Name,
                        ServiceCategory = string.IsNullOrWhiteSpace(s.ServiceCategory) ? "Other Services" : s.ServiceCategory,
                        IsActive = s.IsActive,
                        IsAllowedWhenSystemDown = s.IsAllowedWhenSystemDown,
                    })
                    .ToList(),
            };
        }

        public async Task<string> SavePolicy(SystemDownSaveRequest request)
        {
            // Hours are validated before anything is written. A window stored as "10 pm" would
            // parse as nothing, and a window that parses as nothing with the switch ON reads as
            // "down all day" - the one outcome an operator setting a 2-hour window never intends.
            string from = (request.From ?? string.Empty).Trim();
            string to = (request.To ?? string.Empty).Trim();
            if (request.IsSystemDown)
            {
                if (from.Length == 0 || to.Length == 0)
                    return "Enter both a From and a To time for the system-down window.";
                if (SystemDownPolicy.ParseClock(from) == null || SystemDownPolicy.ParseClock(to) == null)
                    return "Enter the times as 24-hour HH:mm, for example 22:00.";
            }

            await SetSetting(SystemDownPolicy.EnabledKey, request.IsSystemDown ? "true" : "false", request.IsSystemDown);
            await SetSetting(SystemDownPolicy.FromKey, from, from.Length > 0);
            await SetSetting(SystemDownPolicy.ToKey, to, to.Length > 0);
            await SetSetting(SystemDownPolicy.MessageKey, (request.Message ?? string.Empty).Trim(), true);

            // Only the services whose exception actually changed are written, so saving the window
            // does not stamp UpdatedateTime across the whole service master.
            var allowed = (request.AllowedServiceIDs ?? new List<int>()).ToHashSet();
            foreach (var service in (await _serviceRepository.GetAllServices()).Where(s => !s.IsDelete))
            {
                bool next = allowed.Contains(service.ServiceID);
                if (service.IsAllowedWhenSystemDown == next) continue;
                service.IsAllowedWhenSystemDown = next;
                service.UpdatedateTime = DateTime.UtcNow;
                await _serviceRepository.Update(service);
            }

            return StatusType.Ok;
        }

        public async Task<SystemDownStatusResponse> SelectStatus()
        {
            var state = await GetState();
            var names = new List<string>();
            if (state.IsDown && state.AllowedServiceIDs.Count > 0)
            {
                names = (await _serviceRepository.GetAllServices())
                    .Where(s => state.AllowedServiceIDs.Contains(s.ServiceID) && s.IsActive && !s.IsDelete)
                    .OrderBy(s => s.UISequence).ThenBy(s => s.Name)
                    .Select(s => s.Name)
                    .ToList();
            }

            return new SystemDownStatusResponse
            {
                IsDownNow = state.IsDown,
                From = state.From,
                To = state.To,
                // Only while it is actually down. A banner quoting tonight's message all day is
                // noise, and the screens have nothing to say about a window that is not open.
                Message = state.IsDown ? state.Message : string.Empty,
                AllowedServices = names,
            };
        }

        /// <summary>
        /// Write one curated Key Setting row, creating it when the install predates the seed.
        /// Mirrors KycApprovalService.SetSetting, with a free-text Value rather than a bool -
        /// the window's hours and message live in the same table as its switch.
        /// </summary>
        private async Task SetSetting(string key, string value, bool isOn)
        {
            var existing = await _settingRepository.GetByKey(key);
            if (existing == null)
            {
                await _settingRepository.Insert(new SettingMaster
                {
                    Key = key,
                    Value = value,
                    IsOn = isOn,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
                return;
            }

            existing.Value = value;
            existing.IsOn = isOn;
            existing.UpdatedateTime = DateTime.UtcNow;
            await _settingRepository.Update(existing);
        }

        /// <summary>
        /// Reads a curated Key Setting exactly as the Key Setting screen writes it: the Value
        /// string decides for a pure on/off row, and IsOn decides when the Value is blank. The
        /// same reader lives in UserServiceAccessService for the KYC switch; both have to agree
        /// with the screen or a toggle reads as off after being turned on.
        /// </summary>
        private static bool IsSettingOn(IEnumerable<SettingMaster> settings, string key)
        {
            var row = Row(settings, key);
            if (row == null) return false; // never seeded => feature off (never break an old DB)
            string v = (row.Value ?? string.Empty).Trim().ToLowerInvariant();
            if (v.Length == 0) return row.IsOn;
            if (v is "true" or "false") return v == "true";
            return v is "1" or "yes" or "on" or "y";
        }

        private static string ValueOf(IEnumerable<SettingMaster> settings, string key)
            => (Row(settings, key)?.Value ?? string.Empty).Trim();

        private static SettingMaster? Row(IEnumerable<SettingMaster> settings, string key)
            => settings.FirstOrDefault(s =>
                (s.Key ?? string.Empty).Equals(key, StringComparison.OrdinalIgnoreCase) && !s.IsDeleted);
    }
}
