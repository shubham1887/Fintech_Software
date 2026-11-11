using System.Globalization;
using FintechApp.Entities;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;

namespace FintechApp.Providers.Coded
{
    /// <summary>
    /// Nexmint payout, as a compiled integration.
    ///
    /// Only the wallet enquiry runs through here. Nexmint's money movement goes down the bank
    /// transfer pipeline (IBankTransferProviderClient, selected by NexmintGatewayResolver), not
    /// through this gateway's recharge path — so Dispatch / FetchBill / CheckStatus are left alone
    /// and the gateway treats them as "not handled", which is exactly right.
    ///
    /// Family is DMT rather than Payout for the same reason JaiJui's is: the money-out rail an
    /// admin switches a provider onto is the one configured in DMT Settings, and a provider that
    /// declared Payout here would be configured, credentialed, active, and visible in no routing
    /// grid at all.
    /// </summary>
    public class NexmintCodedProvider : CodedProviderBase
    {
        private readonly Fino.NexmintApi.Interfaces.INexmintApiService _nexmint;
        private readonly IProviderMasterRepository _providerRepo;

        public NexmintCodedProvider(
            Fino.NexmintApi.Interfaces.INexmintApiService nexmint,
            IProviderMasterRepository providerRepo,
            IProviderRequestResponseLogRepository logRepo) : base(logRepo)
        {
            _nexmint = nexmint;
            _providerRepo = providerRepo;
        }

        public override bool CanHandle(ProviderMaster provider) =>
            NexmintProviderResolver.IsNexmint(provider.ProviderName);

        public override string Family => ServiceKinds.DMT;

        public override async Task<CodedBalanceResult?> CheckBalance(ProviderMaster provider)
        {
            var creds = NexmintProviderResolver.CredentialsFrom(provider);
            var resp = await _nexmint.GetBalance(creds);

            var result = new ProviderBalanceResponse
            {
                ProviderID = provider.ProviderID,
                ProviderName = provider.ProviderName,
                ExtractedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RawResponse = Truncate(resp.Data ?? string.Empty, 4000)
            };

            if (!resp.Success || !resp.ApiSuccess)
            {
                result.Status = StatusType.Failed;
                result.Message = string.IsNullOrWhiteSpace(resp.Message)
                    ? Messages.Provider.Message.ProviderUnreachable
                    : resp.Message;
                return new CodedBalanceResult(StatusType.Ok, result);
            }

            result.Status = StatusType.Success;
            result.DMTBalance = resp.Balance;
            result.Message = string.IsNullOrWhiteSpace(resp.Message)
                ? Messages.Provider.Message.BalanceFetched
                : resp.Message;
            result.ExtractedValues["DMTBalance"] = resp.Balance.ToString(CultureInfo.InvariantCulture);
            // What is locked against payouts still in flight. It is not spendable, so it must never
            // be added to the balance — but an operator reconciling a short wallet needs to see it.
            result.ExtractedValues["HoldBalance"] = resp.HoldBalance.ToString(CultureInfo.InvariantCulture);

            provider.DMTBalance = resp.Balance;
            await _providerRepo.Update(provider);

            return new CodedBalanceResult(StatusType.Ok, result);
        }
    }
}
