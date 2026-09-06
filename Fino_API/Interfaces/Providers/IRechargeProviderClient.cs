namespace FintechApp.Interfaces.Providers
{
    // Context passed to the upstream operator/aggregator when dispatching a recharge.
    public class RechargeDispatchContext
    {
        public int TransactionMasterID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int UserID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;

        // Extra biller inputs for a bill payment (BBPS): whatever the biller asks for beyond the
        // consumer number - billing unit, cycle, sub-division. Bill FETCH already carried these;
        // the payment needs them too, or a biller that requires one rejects the payment.
        public string Param1 { get; set; } = string.Empty;
        public string Param2 { get; set; } = string.Empty;
        public string Param3 { get; set; } = string.Empty;

        // Set by an admin "shift" to pin the dispatch to one provider. The switching sequence only
        // expresses default priority, so an explicitly named provider bypasses it — but the provider
        // must still have an active route for the operator, and denomination rules still apply.
        // 0 = normal sequence-driven routing.
        public int ForceProviderID { get; set; }
    }

    // Result returned by the operator/aggregator dispatch.
    public class RechargeProviderResult
    {
        public string Status { get; set; } = "FAILED"; // SUCCESS / FAILED / PENDING
        public string ProviderRequestID { get; set; } = string.Empty;
        public string ProviderRefNo { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Which provider actually served the call + what it replied — persisted on the
        // transaction so reports show the real upstream outcome.
        public int ProviderID { get; set; }
        public string RawResponse { get; set; } = string.Empty;
        public decimal? ProviderOpeningBalance { get; set; }
        public decimal? ProviderClosingBalance { get; set; }

        // Commission earned from the provider for this transaction: the value captured from the
        // provider reply when present, otherwise the configured route commission (AdminComm).
        public decimal? ProviderCommission { get; set; }

        // False when no real provider is wired (used by the pipeline to auto-refund safely).
        public bool IsConfigured { get; set; } = true;
    }

    // Seam for the external recharge operator/aggregator API. The recharge pipeline (validation,
    // wallet debit, transaction record, settlement) is provider-agnostic; only this call talks to
    // the upstream gateway. Register a real implementation when operator credentials are available.
    public interface IRechargeProviderClient
    {
        Task<RechargeProviderResult> SendRecharge(RechargeDispatchContext context);
    }
}
