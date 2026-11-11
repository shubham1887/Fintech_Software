using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.StaticModels;

namespace FintechApp.Providers
{
    // Live status-check / resend client backed by the config-driven gateway engine (the same
    // ProviderDetails STATUS endpoint + ProviderResponseMaster STATUS rules the admin panel edits).
    // The provider's raw reply and reference number are persisted onto the transaction so reports
    // show what the upstream actually said.
    public class DynamicTransactionProviderClient : ITransactionProviderClient
    {
        private readonly IProviderGatewayService _gateway;
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IBankTransferProviderClient _bankTransfer;
        private readonly IServiceRepository _serviceRepo;
        private readonly IProviderRequestResponseLogRepository _logRepo;
        private readonly Logger _logger = new();

        // Through-marker on the provider log row, so a DMT status enquiry is distinguishable from
        // the transfer dispatch it is asking about.
        private const string StatusThrough = "STATUS";

        public DynamicTransactionProviderClient(
            IProviderGatewayService gateway,
            ITransactionMasterRepository txnRepo,
            IBankTransferProviderClient bankTransfer,
            IServiceRepository serviceRepo,
            IProviderRequestResponseLogRepository logRepo)
        {
            _gateway = gateway;
            _txnRepo = txnRepo;
            _bankTransfer = bankTransfer;
            _serviceRepo = serviceRepo;
            _logRepo = logRepo;
        }

        public async Task<ProviderStatusResult> CheckStatus(ProviderStatusContext context)
        {
            var txn = await _txnRepo.GetByID(context.TransactionMasterID);
            if (txn == null) return NotConfigured(context);

            // Old rows may not carry ProviderID in the context — the transaction is authoritative.
            if (context.ProviderID <= 0) context.ProviderID = txn.ProviderID;
            if (string.IsNullOrWhiteSpace(context.ProviderRequestID)) context.ProviderRequestID = txn.ProviderRequestID;
            if (context.ServiceID <= 0) context.ServiceID = txn.ServiceID;

            // DMT and payout transfers never went out through the recharge gateway's endpoint
            // templates - they went down the bank-transfer rail (JaiJui et al). So that is where
            // their status has to be asked, and asking the template path instead is why a DMT
            // status check did nothing at all: it resolved no STATUS endpoint and answered
            // "service unavailable" every time, leaving every pending transfer to be settled by
            // hand. When the rail cannot answer we still fall through to the template path, so a
            // provider that IS configured with endpoints keeps working exactly as before.
            // A transfer parked by the "Hold DMT" switch was never sent upstream, so there is
            // nothing to enquire about: the gateway would answer "unknown reference", which reads
            // as FAILED and would refund a transfer the admin is still holding. It stays held.
            if (!TransactionHold.IsHeld(txn) && await IsBankTransferRail(txn, context.ServiceType))
            {
                var rail = await CheckOnBankRail(txn, context);
                if (rail != null) return rail;
            }

            var result = await _gateway.CheckTransactionStatus(context, txn.DateTime);

            // Record the enquiry reply on the transaction only when it settles something —
            // a PENDING/NotFound probe must not overwrite the original dispatch response.
            if (result.IsConfigured && (result.Status == "SUCCESS" || result.Status == "FAILED"))
                await _txnRepo.UpdateProviderReply(txn.TransactionMasterID, context.ProviderID,
                    result.ProviderRefNo, result.RawResponse, null, null);

            return result;
        }

        /// <summary>
        /// Is this transaction one the bank-transfer rail sent? The caller's service-type string is
        /// taken first (the queues and the API gateways pass "DMT" / "PAYOUT"), and when it says
        /// nothing useful the transaction's own service decides, so a sweep that passes a blank or
        /// an unexpected label still routes correctly.
        /// </summary>
        private async Task<bool> IsBankTransferRail(TransactionMaster txn, string serviceType)
        {
            string kind = (serviceType ?? string.Empty).Trim().ToUpperInvariant();
            if (kind.StartsWith(ServiceKinds.DMT, StringComparison.Ordinal)) return true;
            if (kind == ServiceKinds.Payout) return true;
            if (kind.Length > 0 && kind != ServiceKinds.Recharge) return false;

            try
            {
                var svc = await _serviceRepo.GetServiceById(txn.ServiceID);
                string resolved = ServiceKinds.KindOf(svc);
                return resolved == ServiceKinds.DMT || resolved == ServiceKinds.Payout;
            }
            catch (Exception ex) { _logger.Write(ex); return false; }
        }

        /// <summary>
        /// Run the enquiry on the bank-transfer rail. Null = the rail could not answer (no active
        /// gateway, sandbox), which lets the caller fall back to the template path. The exchange is
        /// written to the provider log so the status-check log shows the enquiry, and a decisive
        /// answer is stamped onto the transaction exactly as the template path stamps its own.
        /// </summary>
        private async Task<ProviderStatusResult?> CheckOnBankRail(TransactionMaster txn, ProviderStatusContext context)
        {
            var rail = await _bankTransfer.CheckTransferStatus(new BankTransferStatusContext
            {
                TransactionMasterID = txn.TransactionMasterID,
                UserID = context.UserID,
                ServiceID = txn.ServiceID,
                ProviderRequestID = string.IsNullOrWhiteSpace(context.ProviderRequestID)
                    ? txn.ProviderRequestID
                    : context.ProviderRequestID,
                IPAddress = context.IPAddress
            });

            if (!rail.IsConfigured) return null;

            await WriteStatusLog(txn, context, rail);

            if (rail.Status == StatusType.Success || rail.Status == StatusType.Failed)
                await _txnRepo.UpdateProviderReply(txn.TransactionMasterID,
                    rail.ProviderID > 0 ? rail.ProviderID : txn.ProviderID,
                    rail.ProviderRefNo, rail.RawResponse, null, null);

            return new ProviderStatusResult
            {
                Status = rail.Status,
                ProviderRefNo = string.IsNullOrWhiteSpace(rail.ProviderRefNo) ? txn.ProviderRefNo : rail.ProviderRefNo,
                Message = rail.Message,
                RawResponse = Truncate(rail.RawResponse, 4000),
                IsConfigured = true
            };
        }

        // The enquiry belongs in the provider log next to the dispatch it asks about - otherwise a
        // status check that changed nothing leaves no trace to read. Never fatal.
        private async Task WriteStatusLog(TransactionMaster txn, ProviderStatusContext context, BankTransferStatusResult rail)
        {
            try
            {
                DateTime now = DateTime.Now;
                await _logRepo.Insert(new ProviderRequestResponseLog
                {
                    ProviderID = rail.ProviderID > 0 ? rail.ProviderID : txn.ProviderID,
                    UserID = context.UserID > 0 ? context.UserID : txn.UserID,
                    TransactionID = txn.TransactionMasterID,
                    ServiceID = txn.ServiceID,
                    IPAddress = context.IPAddress ?? string.Empty,
                    Through = StatusThrough,
                    Request = Truncate(rail.RawRequest, 8000),
                    Response = Truncate(rail.RawResponse, 8000),
                    DateTime = now,
                    UpdatedateTime = now
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        private static string Truncate(string value, int max) =>
            string.IsNullOrEmpty(value) || value.Length <= max ? value ?? string.Empty : value[..max];

        public async Task<ProviderStatusResult> Resend(ProviderStatusContext context)
        {
            var txn = await _txnRepo.GetByID(context.TransactionMasterID);
            if (txn == null) return NotConfigured(context);

            // Re-dispatch with the SAME ProviderRequestID — providers (incl. Payin) reject
            // duplicate request ids, so an already-processed transaction cannot double-charge.
            var dispatch = await _gateway.DispatchRecharge(new RechargeDispatchContext
            {
                TransactionMasterID = txn.TransactionMasterID,
                OperatorID = txn.OperatorID,
                ServiceID = txn.ServiceID,
                ConsumerNumber = txn.ConsumerNumber,
                Amount = txn.Amount,
                UserID = txn.UserID,
                ProviderRequestID = txn.ProviderRequestID,
                IPAddress = context.IPAddress,
                ForceProviderID = context.ForceProviderID
            });

            if (dispatch.IsConfigured)
                await _txnRepo.UpdateProviderReply(txn.TransactionMasterID, dispatch.ProviderID,
                    dispatch.ProviderRefNo, dispatch.RawResponse, dispatch.ProviderOpeningBalance, dispatch.ProviderClosingBalance);

            return new ProviderStatusResult
            {
                Status = dispatch.Status,
                ProviderRefNo = dispatch.ProviderRefNo,
                Message = dispatch.Message,
                RawResponse = dispatch.RawResponse,
                IsConfigured = dispatch.IsConfigured
            };
        }

        private static ProviderStatusResult NotConfigured(ProviderStatusContext context) => new()
        {
            Status = "PENDING",
            IsConfigured = false,
            ProviderRefNo = context.ProviderRequestID,
            Message = Common.Message.ServiceUnavailable
        };
    }
}
