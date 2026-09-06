using FintechApp.Interfaces.Providers;
using FintechApp.Messages;

namespace FintechApp.Providers
{
    // Default recharge provider used until a real operator/aggregator integration is registered.
    // Reports FAILED + IsConfigured=false so the recharge pipeline auto-refunds (no money is held).
    // Returns a user-facing message (no internal details); developers identify the missing provider via IsConfigured.
    public class UnconfiguredRechargeProviderClient : IRechargeProviderClient
    {
        public Task<RechargeProviderResult> SendRecharge(RechargeDispatchContext context)
        {
            return Task.FromResult(new RechargeProviderResult
            {
                Status = "FAILED",
                IsConfigured = false,
                ProviderRequestID = context.ProviderRequestID,
                Message = Common.Message.ServiceUnavailable
            });
        }
    }
}
