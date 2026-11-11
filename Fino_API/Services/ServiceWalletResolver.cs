using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IServiceWalletResolver" />
    public class ServiceWalletResolver : IServiceWalletResolver
    {
        private readonly IWalletServiceMappingRepository _mappingRepo;
        private readonly IWalletMasterRepository _walletRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly Logger _logger = new();

        public ServiceWalletResolver(
            IWalletServiceMappingRepository mappingRepo,
            IWalletMasterRepository walletRepo,
            IServiceRepository serviceRepo)
        {
            _mappingRepo = mappingRepo;
            _walletRepo = walletRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<int> Resolve(int serviceID, int fallbackWalletID)
        {
            if (serviceID <= 0) return fallbackWalletID;

            try
            {
                // The newest mapping for the service wins. The screen lets a service be mapped more
                // than once over time, and the Service Catalog already reads the latest row as the
                // live one — the pipeline has to agree with what the admin is looking at.
                var mapping = (await _mappingRepo.GetAll())
                    .Where(m => m.ServiceID == serviceID)
                    .OrderByDescending(m => m.WalletServiceMappingID)
                    .FirstOrDefault();

                if (mapping == null || mapping.WalletID <= 0) return fallbackWalletID;

                // A wallet the admin has switched off is not a place to take money from; the
                // service falls back to its built-in wallet rather than failing the transaction.
                var wallet = await _walletRepo.SelectByID(mapping.WalletID);
                if (wallet == null || !wallet.IsActive) return fallbackWalletID;

                return mapping.WalletID;
            }
            catch (Exception ex)
            {
                // Resolving the wallet must never be the reason a transaction cannot be attempted.
                _logger.Write(ex);
                return fallbackWalletID;
            }
        }

        public async Task<int> ResolveForKind(string serviceKind, int fallbackWalletID)
        {
            try
            {
                var services = await _serviceRepo.GetAllServices();
                // An active, undeleted service of this kind — the same matching the reports and
                // the menu use, so the admin's "Report" mapping on the Service Catalog decides
                // this too and nothing has an install's service ids compiled into it.
                var service = services
                    .Where(s => s.IsActive && !s.IsDelete && ServiceKinds.Matches(s, serviceKind))
                    .OrderBy(s => s.ServiceID)
                    .FirstOrDefault();

                return service == null ? fallbackWalletID : await Resolve(service.ServiceID, fallbackWalletID);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return fallbackWalletID;
            }
        }
    }
}
