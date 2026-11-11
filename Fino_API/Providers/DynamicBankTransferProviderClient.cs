using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Messages;
using Fino.JaiJuiApi.Requests;
using Fino.NexmintApi.Interfaces;
using Fino.NexmintApi.Messages;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using NexmintRequests = Fino.NexmintApi.Requests;

namespace FintechApp.Providers
{
    // DB-driven bank-transfer router. Instead of picking a provider at DI time from appsettings, this
    // reads the DMT gateways configured in the "DMT Settings" screen (DMTSetting table) at call time and
    // dispatches through the active one. A gateway routes through a coded rail when it links (its
    // ProviderCode = the provider name) to an active provider registered in the "Add API"
    // (ProviderMaster) screen; the transfer is then sent through that provider's library
    // (Fino.JaiJuiApi, Fino.NexmintApi) using THAT provider row's credentials. When no real gateway is
    // active it falls back to the sandbox (config DMTProvider:Mode=Simulation) or the Unconfigured
    // client (fail-and-refund) — so existing dev/UAT behaviour is preserved until an admin activates a
    // real provider.
    public class DynamicBankTransferProviderClient : IBankTransferProviderClient
    {
        private readonly IJaiJuiGatewayResolver _gateway;
        private readonly IJaiJuiApiService _jaijui;
        private readonly INexmintGatewayResolver _nexmintGateway;
        private readonly INexmintApiService _nexmint;
        private readonly SimulationBankTransferProviderClient _simulation;
        private readonly UnconfiguredBankTransferProviderClient _unconfigured;
        private readonly IConfiguration _config;
        // Admin rewrites for the provider's own wording - see ResponseMessageRuleService.
        private readonly IResponseMessageRuleService _messageRules;
        private readonly Logger _logger;

        public DynamicBankTransferProviderClient(
            IJaiJuiGatewayResolver gateway,
            IJaiJuiApiService jaijui,
            INexmintGatewayResolver nexmintGateway,
            INexmintApiService nexmint,
            SimulationBankTransferProviderClient simulation,
            UnconfiguredBankTransferProviderClient unconfigured,
            IConfiguration config,
            IResponseMessageRuleService messageRules)
        {
            _messageRules = messageRules;
            _gateway = gateway;
            _jaijui = jaijui;
            _nexmintGateway = nexmintGateway;
            _nexmint = nexmint;
            _simulation = simulation;
            _unconfigured = unconfigured;
            _config = config;
            _logger = new Logger();
        }

        public async Task<BankTransferProviderResult> SendTransfer(BankTransferContext context)
        {
            try
            {
                var active = await ResolveActive();
                if (active.JaiJui != null) return await SendViaJaiJui(context, active.JaiJui);
                if (active.Nexmint != null) return await SendViaNexmint(context, active.Nexmint);
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

        /// <summary>
        /// Status enquiry for a transfer this rail sent — JaiJui's /GetTransctionStatus or Nexmint's
        /// /fund-transfer/status, both keyed on the reference we submitted with the transfer (our
        /// ProviderRequestID, which is JaiJui's APIUserRequestID and Nexmint's order_id).
        ///
        /// A transport failure or an unrecognised reply deliberately stays PENDING: the settlement
        /// pipeline refunds on FAILED, and a provider that simply did not answer must never be read
        /// as "the transfer failed".
        /// </summary>
        public async Task<BankTransferStatusResult> CheckTransferStatus(BankTransferStatusContext context)
        {
            if (string.IsNullOrWhiteSpace(context.ProviderRequestID))
                return NotAnswered(JaiJui.Message.RequestIDRequired);

            ActiveGateway active;
            try { active = await ResolveActive(); }
            catch (Exception ex) { _logger.Write(ex); return NotAnswered(JaiJui.Message.ServiceUnavailable); }

            if (active.JaiJui != null) return await StatusViaJaiJui(context, active.JaiJui);
            if (active.Nexmint != null) return await StatusViaNexmint(context, active.Nexmint);

            return NotAnswered(JaiJui.Message.ServiceUnavailable);
        }

        /// <summary>
        /// The gateway actually in force, as one answer rather than "whichever resolver was asked
        /// first". Both rails read the same DMT Settings list, so asking them in a fixed order would
        /// quietly ignore the Sequence an admin set: activate Nexmint above JaiJui and the transfers
        /// would still have gone to JaiJui. Lowest Sequence wins, and a tie keeps JaiJui — which is
        /// what every existing install already does.
        /// </summary>
        private async Task<ActiveGateway> ResolveActive()
        {
            var jaijui = await _gateway.ResolveActiveGateway();
            var nexmint = await _nexmintGateway.ResolveActiveGateway();

            if (jaijui != null && nexmint != null)
                return nexmint.Sequence < jaijui.Sequence
                    ? new ActiveGateway(null, nexmint)
                    : new ActiveGateway(jaijui, null);

            return new ActiveGateway(jaijui, nexmint);
        }

        private sealed record ActiveGateway(JaiJuiGatewayInfo? JaiJui, NexmintGatewayInfo? Nexmint);

        private static BankTransferStatusResult NotAnswered(string message) => new()
        {
            Status = StatusType.Pending,
            IsConfigured = false,
            Message = message
        };

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
                // JaiJui reports its own charge on every transfer ("commission": -5.90). Carried
                // through so the recorded cost is what actually happened, not what we expected.
                ProviderCommission = resp.Commission != 0m ? resp.Commission : null,
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

            result.Message = await Rewrite(result.Message, result.ProviderID);
            return result;
        }

        private async Task<BankTransferStatusResult> StatusViaJaiJui(BankTransferStatusContext context, JaiJuiGatewayInfo gateway)
        {
            var statusRequest = new StatusCheckRequest { ID = context.ProviderRequestID };
            var resp = await _jaijui.StatusCheck(statusRequest, gateway.Credentials);

            var result = new BankTransferStatusResult
            {
                IsConfigured = true,
                ProviderID = gateway.ProviderID,
                Message = resp.Message,
                RawRequest = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Endpoint = "JaiJui GetTransctionStatus",
                    ID = context.ProviderRequestID
                }),
                RawResponse = string.IsNullOrWhiteSpace(resp.Data)
                    ? System.Text.Json.JsonSerializer.Serialize(new { resp.Success, resp.Status, resp.Message })
                    : resp.Data
            };

            if (!resp.Success)
            {
                result.Status = StatusType.Pending;
                if (string.IsNullOrWhiteSpace(result.Message)) result.Message = JaiJui.Message.ServiceUnavailable;
                return result;
            }

            string reference = string.IsNullOrWhiteSpace(resp.TransactionID) ? resp.VendorID : resp.TransactionID;
            switch ((resp.Status ?? string.Empty).Trim().ToUpperInvariant())
            {
                case JaiJui.Status.Success:
                    result.Status = StatusType.Success;
                    result.ProviderRefNo = reference;
                    break;
                case JaiJui.Status.Failure:
                    result.Status = StatusType.Failed;
                    break;
                default: // PROCESS, or anything we do not recognise — leave it pending.
                    result.Status = StatusType.Pending;
                    break;
            }

            if (string.IsNullOrWhiteSpace(result.Message)) result.Message = resp.Description;
            result.Message = await Rewrite(result.Message, result.ProviderID);
            return result;
        }

        /// <summary>
        /// Nexmint payout. One call with the beneficiary on it — no sender to register, no
        /// beneficiary id — and the answer is ALWAYS "pending": the outcome arrives later on the
        /// webhook or from a status enquiry, both keyed on the order_id we send here.
        /// </summary>
        private async Task<BankTransferProviderResult> SendViaNexmint(BankTransferContext context, NexmintGatewayInfo gateway)
        {
            var payout = new NexmintRequests.PayoutRequest
            {
                OrderID = context.ProviderRequestID,
                PaymentMethod = context.PaymentType,
                AccountHolderName = context.BeneficiaryName,
                BankName = context.BankName,
                AccountNumber = context.AccountNo,
                IFSC = context.IFSC,
                // Nexmint wants the beneficiary's number; the pipeline carries the one the transfer
                // was keyed on, which for a payout is the number the retailer entered.
                Mobile = context.SenderMobile,
                Amount = context.Amount,
                Remark = context.Remark
            };
            var resp = await _nexmint.SendPayout(payout, gateway.Credentials);

            var result = new BankTransferProviderResult
            {
                IsConfigured = true,
                ProviderID = gateway.ProviderID,
                ProviderRequestID = context.ProviderRequestID,
                // Their transaction id until a UTR exists; the webhook/status reply replaces it.
                ProviderRefNo = resp.TransactionID,
                Message = resp.Message,
                RawRequest = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Endpoint = "Nexmint fund-transfer",
                    payout.OrderID,
                    payout.PaymentMethod,
                    payout.AccountHolderName,
                    payout.BankName,
                    payout.AccountNumber,
                    payout.IFSC,
                    payout.Mobile,
                    payout.Amount,
                    payout.Remark
                }),
                RawResponse = string.IsNullOrWhiteSpace(resp.Data)
                    ? System.Text.Json.JsonSerializer.Serialize(new { resp.Success, resp.ApiSuccess, resp.HttpStatus, resp.Status, resp.Message })
                    : resp.Data
            };

            // No answer (network, timeout, HTTP 5xx). The payout may already exist upstream, so this
            // must stay PENDING — failing it here would refund the retailer for a transfer the bank
            // may still be making, and the status enquiry can resolve it from the same order_id.
            if (resp.Retryable)
            {
                result.Status = StatusType.Pending;
                result.ProviderRefNo = string.Empty;
                if (string.IsNullOrWhiteSpace(result.Message)) result.Message = Nexmint.Message.ServiceUnavailable;
                result.Message = await Rewrite(result.Message, result.ProviderID);
                return result;
            }

            // A refusal (400/401/403) or a reply that was not a Nexmint envelope. Nothing was
            // accepted, so fail and let the pipeline refund.
            if (!resp.Success || !resp.ApiSuccess)
            {
                result.Status = StatusType.Failed;
                result.ProviderRefNo = string.Empty;
                if (string.IsNullOrWhiteSpace(result.Message)) result.Message = Nexmint.Message.InvalidResponse;
                result.Message = await Rewrite(result.Message, result.ProviderID);
                return result;
            }

            result.Status = MapNexmintStatus(resp.Status, StatusType.Pending);
            if (result.Status == StatusType.Failed) result.ProviderRefNo = string.Empty;
            result.Message = await Rewrite(result.Message, result.ProviderID);
            return result;
        }

        private async Task<BankTransferStatusResult> StatusViaNexmint(BankTransferStatusContext context, NexmintGatewayInfo gateway)
        {
            // Asked by order_id — our own reference, which we always have. Their transaction_id is
            // only on the initiation reply, and a transfer whose reply never arrived is exactly the
            // one most in need of an enquiry.
            var statusRequest = new NexmintRequests.PayoutStatusRequest { OrderID = context.ProviderRequestID };
            var resp = await _nexmint.GetPayoutStatus(statusRequest, gateway.Credentials);

            var result = new BankTransferStatusResult
            {
                IsConfigured = true,
                ProviderID = gateway.ProviderID,
                Message = resp.Message,
                RawRequest = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Endpoint = "Nexmint fund-transfer/status",
                    OrderID = context.ProviderRequestID
                }),
                RawResponse = string.IsNullOrWhiteSpace(resp.Data)
                    ? System.Text.Json.JsonSerializer.Serialize(new { resp.Success, resp.ApiSuccess, resp.HttpStatus, resp.Status, resp.Message })
                    : resp.Data
            };

            // Unreachable, unparseable, or an enquiry Nexmint refused — including "Invalid order_id",
            // which reads like proof the payout does not exist but is also what a merchant-mismatch
            // or a credential swap looks like. An enquiry is never allowed to be the thing that
            // refunds a transfer; only a status the provider actually stated is.
            if (resp.Retryable || !resp.Success || !resp.ApiSuccess)
            {
                result.Status = StatusType.Pending;
                if (string.IsNullOrWhiteSpace(result.Message)) result.Message = Nexmint.Message.ServiceUnavailable;
                result.Message = await Rewrite(result.Message, result.ProviderID);
                return result;
            }

            result.Status = MapNexmintStatus(resp.Status, StatusType.Pending);
            if (result.Status == StatusType.Success)
                result.ProviderRefNo = string.IsNullOrWhiteSpace(resp.UtrNumber) ? resp.TransactionID : resp.UtrNumber;

            result.Message = await Rewrite(result.Message, result.ProviderID);
            return result;
        }

        /// <summary>
        /// Nexmint's status word, as the settlement pipeline's. Anything unrecognised stays on the
        /// caller's default — pending — because an unknown word is not a failure.
        /// </summary>
        private static string MapNexmintStatus(string? status, string fallback) =>
            (status ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                Nexmint.Status.Success => StatusType.Success,
                Nexmint.Status.Failed => StatusType.Failed,
                Nexmint.Status.Pending => StatusType.Pending,
                _ => fallback
            };

        /// <summary>The provider's wording, as the admin wants it said. Never throws.</summary>
        private async Task<string> Rewrite(string message, int providerID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message)) return message;
                return await _messageRules.Apply(message, providerID, 0, MessageChannel.All);
            }
            catch (Exception ex) { _logger.Write(ex); return message; }
        }
    }
}
