using FintechApp.Interfaces.Providers;
using FintechApp.Messages;

namespace FintechApp.Providers
{
    // Default bank-transfer provider used until a real DMT/payout aggregator is registered.
    // Reports FAILED + IsConfigured=false so the pipeline auto-refunds (no money is held).
    // Returns a user-facing message (no internal details); developers identify the missing provider via IsConfigured.
    public class UnconfiguredBankTransferProviderClient : IBankTransferProviderClient
    {
        public Task<BankTransferProviderResult> SendTransfer(BankTransferContext context)
        {
            return Task.FromResult(new BankTransferProviderResult
            {
                Status = "FAILED",
                IsConfigured = false,
                ProviderRequestID = context.ProviderRequestID,
                Message = Common.Message.ServiceUnavailable
            });
        }
        public Task<BankTransferStatusResult> CheckTransferStatus(BankTransferStatusContext context) =>
            Task.FromResult(new BankTransferStatusResult
            {
                Status = "PENDING",
                IsConfigured = false,
                Message = Common.Message.ServiceUnavailable
            });

    }
}