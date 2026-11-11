using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;

namespace FintechApp.Services
{
    // Classifies services by their ServiceMaster name/category (case-insensitive) so the money
    // engine never relies on hardcoded ServiceIDs. Loaded once per scope (one request).
    public class ServiceKindResolver : IServiceKindResolver
    {
        private readonly IServiceRepository _serviceRepo;
        private List<ServiceMaster>? _services;

        public ServiceKindResolver(IServiceRepository serviceRepo)
        {
            _serviceRepo = serviceRepo;
        }

        private async Task<List<ServiceMaster>> Load()
            => _services ??= await _serviceRepo.GetAllServices() ?? new List<ServiceMaster>();

        private static bool IsAEPS(ServiceMaster s) =>
            s.Name.Contains("AEPS", StringComparison.OrdinalIgnoreCase) ||
            s.ServiceCategory.Contains("AEPS", StringComparison.OrdinalIgnoreCase);

        private static bool IsMicroATM(ServiceMaster s) =>
            s.Name.Replace(" ", string.Empty).Contains("MICROATM", StringComparison.OrdinalIgnoreCase) ||
            s.Name.Contains("MATM", StringComparison.OrdinalIgnoreCase) ||
            s.ServiceCategory.Replace(" ", string.Empty).Contains("MICROATM", StringComparison.OrdinalIgnoreCase);

        private static bool IsDMT(ServiceMaster s) =>
            s.ServiceCategory.Contains("DMT", StringComparison.OrdinalIgnoreCase) ||
            s.Name.Contains("DMT", StringComparison.OrdinalIgnoreCase) ||
            s.Name.Contains("Money Transfer", StringComparison.OrdinalIgnoreCase);

        public async Task<bool> IsCashInService(int serviceID)
        {
            var service = (await Load()).FirstOrDefault(s => s.ServiceID == serviceID);
            return service != null && (IsAEPS(service) || IsMicroATM(service));
        }

        public async Task<bool> IsDMTService(int serviceID)
        {
            var service = (await Load()).FirstOrDefault(s => s.ServiceID == serviceID);
            return service != null && IsDMT(service);
        }

        public async Task<bool> IsRechargeService(int serviceID)
        {
            var service = (await Load()).FirstOrDefault(s => s.ServiceID == serviceID);
            return service != null && StaticModels.ServiceKinds.Matches(service, StaticModels.ServiceKinds.Recharge);
        }

        public async Task<bool> IsChainFundedService(int serviceID)
            => await IsRechargeService(serviceID) || await IsDMTService(serviceID);

        public async Task<HashSet<int>> GetAEPSServiceIDs()
            => (await Load()).Where(IsAEPS).Select(s => s.ServiceID).ToHashSet();

        public async Task<HashSet<int>> GetMicroATMServiceIDs()
            => (await Load()).Where(IsMicroATM).Select(s => s.ServiceID).ToHashSet();
    }
}
