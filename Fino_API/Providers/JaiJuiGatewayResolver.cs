using Fino.JaiJuiApi.Configuration;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;

namespace FintechApp.Providers
{
    // See IJaiJuiGatewayResolver.
    //   Preferred:  an active DMTSetting row whose ProviderCode names an active JaiJui provider in
    //               "Add API" → credentials come from that ProviderMaster row (LoginID/Token/
    //               ProviderUniqueID/ProviderUrl) via JaiJuiProviderResolver.
    //   Legacy:     the DMT row itself is named/coded JaiJui and carries its own credentials
    //               (UserName=Mobile, Token=APIToken, WalletID=AuthCode).
    public class JaiJuiGatewayResolver : IJaiJuiGatewayResolver
    {
        private readonly IDMTSettingRepository _dmtRepo;
        private readonly IProviderMasterRepository _providerRepo;

        public JaiJuiGatewayResolver(IDMTSettingRepository dmtRepo, IProviderMasterRepository providerRepo)
        {
            _dmtRepo = dmtRepo;
            _providerRepo = providerRepo;
        }

        public async Task<JaiJuiGatewayInfo?> ResolveActiveGateway()
        {
            var active = (await _dmtRepo.GetAll())
                .Where(s => s.IsActive)
                .OrderBy(s => s.Sequence)
                .ToList();

            foreach (var row in active)
            {
                if (!string.IsNullOrWhiteSpace(row.ProviderCode))
                {
                    var provider = await _providerRepo.GetByName(row.ProviderCode);
                    if (provider != null && provider.IsActive && !provider.IsDelete &&
                        JaiJuiProviderResolver.IsJaiJui(provider.ProviderName))
                        return new JaiJuiGatewayInfo
                        {
                            Credentials = JaiJuiProviderResolver.CredentialsFrom(provider),
                            ProviderID = provider.ProviderID,
                            ProviderName = provider.ProviderName
                        };
                }

                if (JaiJuiProviderResolver.IsJaiJui(row.ProviderCode) || JaiJuiProviderResolver.IsJaiJui(row.Name))
                    return new JaiJuiGatewayInfo
                    {
                        // Legacy: credentials on the DMT row itself, no linked ProviderMaster (ProviderID 0).
                        Credentials = new JaiJuiCredentials
                        {
                            Mobile = row.UserName,   // registered mobile (KeyData header)
                            ApiToken = row.Token,    // API token          (KeyData header)
                            AuthCode = row.WalletID  // auth code          (request body)
                        },
                        ProviderName = string.IsNullOrWhiteSpace(row.ProviderCode) ? row.Name : row.ProviderCode
                    };
            }

            return null;
        }
    }
}
