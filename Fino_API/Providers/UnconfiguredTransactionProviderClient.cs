using FintechApp.Interfaces.Providers;
using FintechApp.Messages;

namespace FintechApp.Providers
{
    // Default provider client used until a real provider status/resend integration is registered.
    // Reports IsConfigured=false so callers leave the transaction unchanged (no false settlement).
    // Returns a user-facing message (no internal details); developers identify the missing provider via IsConfigured.
    public class UnconfiguredTransactionProviderClient : ITransactionProviderClient
    {
        public Task<ProviderStatusResult> CheckStatus(ProviderStatusContext context)
            => Task.FromResult(NotConfigured(context));

        public Task<ProviderStatusResult> Resend(ProviderStatusContext context)
            => Task.FromResult(NotConfigured(context));

        private static ProviderStatusResult NotConfigured(ProviderStatusContext context) => new()
        {
            Status = "PENDING",
            IsConfigured = false,
            ProviderRefNo = context.ProviderRequestID,
            Message = Common.Message.ServiceUnavailable
        };
    }
}
