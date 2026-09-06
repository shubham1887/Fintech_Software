using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Services;

namespace FintechApp.Providers
{
    // Live recharge provider backed by the config-driven gateway engine. The route, endpoint
    // template and response rules all come from the admin panel (ProviderConfiguration /
    // ProviderDetails / ProviderResponseMaster), so onboarding a new upstream provider is a
    // data change — no code deployment. When nothing is configured for an operator, the engine
    // reports IsConfigured=false and the recharge pipeline auto-refunds safely.
    public class DynamicRechargeProviderClient : IRechargeProviderClient
    {
        private readonly IProviderGatewayService _gateway;

        public DynamicRechargeProviderClient(IProviderGatewayService gateway)
        {
            _gateway = gateway;
        }

        public Task<RechargeProviderResult> SendRecharge(RechargeDispatchContext context)
            => _gateway.DispatchRecharge(context);
    }
}
