using Fino.JaiJuiApi.Configuration;
using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Messages;
using Fino.JaiJuiApi.Requests;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;

namespace FintechApp.Providers
{
    // DB-driven bank-transfer router. Instead of picking a provider at DI time from appsettings, this
    // reads the DMT gateways configured in the "DMT Settings" screen (DMTSetting table) at call time and
    // dispatches through the active one. A DMT gateway routes through JaiJui when it links (its
    // ProviderCode = the provider name) to an active JaiJui provider registered in the "Add API"
    // (ProviderMaster) screen; the transfer is then sent through the Fino.JaiJuiApi library using THAT
    // provider row's credentials. When no real gateway is active it falls back to the sandbox
    // (config DMTProvider:Mode=Simulation) or the Unconfigured client (fail-and-refund) — so existing
    // dev/UAT behaviour is preserved until an admin activates a real provider.
    public class DynamicBankTransferProviderClient : IBankTransferProviderClient
    {
        private readonly IJaiJuiGatewayResolver _gateway;
        private readonly IJaiJuiApiService _jaijui;
        private readonly SimulationBankTransferProviderClient _simulation;
        private readonly UnconfiguredBankTransferProviderClient _unconfigured;
        private readonly IConfiguration _config;
        private readonly Logger _logger;

        public DynamicBankTransferProviderClient(
            IJaiJuiGatewayResolver gateway,
            IJaiJuiApiService jaijui,
            SimulationBankTransferProviderClient simulation,
            UnconfiguredBankTransferProviderClient unconfigured,
            IConfiguration config)
        {
            _gateway = gateway;
            _jaijui = jaijui;
            _simulation = simulation;
            _unconfigured = unconfigured;
            _config = config;
            _logger = new Logger();
        }

        public async Task<BankTransferProviderResult> SendTransfer(BankTransferContext context)
        {
            try
            {
                var gateway = await _gateway.ResolveActiveGateway();
                if (gateway != null)
                    return await SendViaJaiJui(context, gateway);
            }
            catch (Exception ex)
            {
                // A configuration/lookup failure must not silently succeed a transfer — log and fall
                // back to the safe default below.
                _logger.Write(ex);
            }

            // No active real gateway configured — preserve the prior appsettings-driven behaviour.
            if (string.Equals(_config["DMTProvider:Mode"], "Simulation", StringComparison.OrdinalIgnoreCase))
                return await _simulation.SendTransfer(context);
            return await _unconfigured.SendTransfer(context);
        }

        private async Task<BankTransferProviderResult> SendViaJaiJui(BankTransferContext context, JaiJuiGatewayInfo gateway)
        {
            var moneyTransferRequest = new MoneyTransferRequest
            {
                Mobile = context.SenderMobile,
                BankName = context.BankName,
                AccountNo = context.AccountNo,
                IFSC = context.IFSC,
                BeneficiaryID = context.BeneficiaryID,
                SenderID = context.SenderID,
                PaymentType = context.PaymentType,
                Amount = context.Amount,
                Name = context.BeneficiaryName,
                APIUserRequestID = context.ProviderRequestID
            };
            var resp = await _jaijui.MoneyTransfer(moneyTransferRequest, gateway.Credentials);

            var result = new BankTransferProviderResult
            {
                IsConfigured = true,
                ProviderID = gateway.ProviderID,
                ProviderRequestID = context.ProviderRequestID,
                ProviderRefNo = string.IsNullOrWhiteSpace(resp.TransactionID) ? resp.VendorID : resp.TransactionID,
                Message = resp.Message,
                // Captured for the DMT provider log (raw response kept verbatim from JaiJui).
                RawRequest = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Endpoint = "JaiJui MoneyTransfer",
                    moneyTransferRequest.Mobile,
                    moneyTransferRequest.BankName,
                    moneyTransferRequest.AccountNo,
                    moneyTransferRequest.IFSC,
                    moneyTransferRequest.PaymentType,
                    moneyTransferRequest.Amount,
                    moneyTransferRequest.Name,
                    moneyTransferRequest.APIUserRequestID
                }),
                RawResponse = string.IsNullOrWhiteSpace(resp.Data)
                    ? System.Text.Json.JsonSerializer.Serialize(new { resp.Success, resp.Status, resp.Message })
                    : resp.Data
            };

            // Transport failure (network / non-200 / unparseable) → fail so the pipeline refunds.
            if (!resp.Success)
            {
                result.Status = StatusType.Failed;
                if (string.IsNullOrWhiteSpace(result.Message)) result.Message = JaiJui.Message.ServiceUnavailable;
                result.ProviderRefNo = string.Empty;
                return result;
            }

            // Branch on the provider's own outcome (HTTP 200 is returned even for declines).
            switch ((resp.Status ?? string.Empty).Trim().ToUpperInvariant())
            {
                case JaiJui.Status.Success:
                    result.Status = StatusType.Success;
                    break;
                case JaiJui.Status.Process:
                    result.Status = StatusType.Pending;
                    break;
                default: // FAILURE or anything unrecognised
                    result.Status = StatusType.Failed;
                    result.ProviderRefNo = string.Empty;
                    if (string.IsNullOrWhiteSpace(result.Message)) result.Message = JaiJui.Message.InvalidResponse;
                    break;
            }

            return result;
        }
    }
}
