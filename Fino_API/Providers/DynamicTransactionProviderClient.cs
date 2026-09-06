using FintechApp.Interfaces.Providers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;

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

        public DynamicTransactionProviderClient(IProviderGatewayService gateway, ITransactionMasterRepository txnRepo)
        {
            _gateway = gateway;
            _txnRepo = txnRepo;
        }

        public async Task<ProviderStatusResult> CheckStatus(ProviderStatusContext context)
        {
            var txn = await _txnRepo.GetByID(context.TransactionMasterID);
            if (txn == null) return NotConfigured(context);

            // Old rows may not carry ProviderID in the context — the transaction is authoritative.
            if (context.ProviderID <= 0) context.ProviderID = txn.ProviderID;
            if (string.IsNullOrWhiteSpace(context.ProviderRequestID)) context.ProviderRequestID = txn.ProviderRequestID;
            if (context.ServiceID <= 0) context.ServiceID = txn.ServiceID;

            var result = await _gateway.CheckTransactionStatus(context, txn.DateTime);

            // Record the enquiry reply on the transaction only when it settles something —
            // a PENDING/NotFound probe must not overwrite the original dispatch response.
            if (result.IsConfigured && (result.Status == "SUCCESS" || result.Status == "FAILED"))
                await _txnRepo.UpdateProviderReply(txn.TransactionMasterID, context.ProviderID,
                    result.ProviderRefNo, result.RawResponse, null, null);

            return result;
        }

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
