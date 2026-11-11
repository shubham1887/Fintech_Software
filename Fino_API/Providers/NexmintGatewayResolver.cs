using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;

namespace FintechApp.Providers
{
    // See INexmintGatewayResolver.
    //   An active DMTSetting row whose ProviderCode names an active Nexmint provider in "Add API"
    //   -> credentials come from that ProviderMaster row (LoginID/Token/ProviderUniqueID/ProviderUrl)
    //   via NexmintProviderResolver.
    //
    // There is deliberately no legacy "credentials on the DMT row itself" path here, the way JaiJui
    // has one. JaiJui predates the "Add API" screen and its rows exist in live databases; Nexmint
    // does not, so every Nexmint gateway is a provider row from the start and there is no second
    // shape to keep working.
    public class NexmintGatewayResolver : INexmintGatewayResolver
    {
        private readonly IDMTSettingRepository _dmtRepo;
        private readonly IProviderMasterRepository _providerRepo;

        public NexmintGatewayResolver(IDMTSettingRepository dmtRepo, IProviderMasterRepository providerRepo)
        {
            _dmtRepo = dmtRepo;
            _providerRepo = providerRepo;
        }

        public async Task<NexmintGatewayInfo?> ResolveActiveGateway()
        {
            var active = (await _dmtRepo.GetAll())
                .Where(s => s.IsActive)
                .OrderBy(s => s.Sequence)
                .ToList();

            foreach (var row in active)
            {
                if (string.IsNullOrWhiteSpace(row.ProviderCode)) continue;

                var provider = await _providerRepo.GetByName(row.ProviderCode);
                if (provider != null && provider.IsActive && !provider.IsDelete &&
                    NexmintProviderResolver.IsNexmint(provider.ProviderName))
                    return new NexmintGatewayInfo
                    {
                        Credentials = NexmintProviderResolver.CredentialsFrom(provider),
                        ProviderID = provider.ProviderID,
                        ProviderName = provider.ProviderName,
                        Sequence = row.Sequence
                    };
            }

            return null;
        }
    }
}
