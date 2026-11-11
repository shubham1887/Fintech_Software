using System.Globalization;
using FintechApp.Entities;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Responses.ProviderGateway;
using FintechApp.StaticModels;

namespace FintechApp.Providers.Coded
{
    /// <summary>
    /// JaiJui DMT, as a compiled integration.
    ///
    /// Only the balance enquiry runs through here. JaiJui's money movement goes down the bank
    /// transfer pipeline (IBankTransferProviderClient, selected by JaiJuiGatewayResolver), not
    /// through this gateway's recharge path — so Dispatch / FetchBill / CheckStatus are left
    /// alone and the gateway treats them as "not handled", which is exactly right.
    ///
    /// This is the shape a coded provider usually takes: implement what the library offers, and
    /// nothing else.
    /// </summary>
    public class JaiJuiCodedProvider : CodedProviderBase
    {
        private readonly Fino.JaiJuiApi.Interfaces.IJaiJuiApiService _jaijui;
        private readonly IProviderMasterRepository _providerRepo;

        public JaiJuiCodedProvider(
            Fino.JaiJuiApi.Interfaces.IJaiJuiApiService jaijui,
            IProviderMasterRepository providerRepo,
            IProviderRequestResponseLogRepository logRepo) : base(logRepo)
        {
            _jaijui = jaijui;
            _providerRepo = providerRepo;
        }

        public override bool CanHandle(ProviderMaster provider) =>
            JaiJuiProviderResolver.IsJaiJui(provider.ProviderName);

        public override string Family => ServiceKinds.DMT;

        public override async Task<CodedBalanceResult?> CheckBalance(ProviderMaster provider)
        {
            var creds = JaiJuiProviderResolver.CredentialsFrom(provider);
            var resp = await _jaijui.GetBalance(creds);

            var result = new ProviderBalanceResponse
            {
                ProviderID = provider.ProviderID,
                ProviderName = provider.ProviderName,
                ExtractedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RawResponse = Truncate(resp.Data ?? string.Empty, 4000)
            };

            // Provider returns HTTP 200 even on decline: transport failure OR an explicit FAILURE status
            // means we could not read a balance.
            string status = (resp.Status ?? string.Empty).Trim().ToUpperInvariant();
            if (!resp.Success || status == "FAILURE")
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

            provider.DMTBalance = resp.Balance;
            await _providerRepo.Update(provider);

            return new CodedBalanceResult(StatusType.Ok, result);
        }
    }
}
