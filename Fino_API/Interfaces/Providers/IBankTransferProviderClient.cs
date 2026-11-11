using FintechApp.Requests.Transaction;

namespace FintechApp.Interfaces.Providers
{
    // Context passed to the upstream bank-transfer provider (DMT / payout / vendor-pay aggregator).
    public class BankTransferContext
    {
        public TransferKind Kind { get; set; }
        public int TransactionMasterID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        // What the transfer is for, in the retailer's words. Aggregators that put a narration on the
        // bank credit (Nexmint makes `remark` mandatory) send this; the rest ignore it.
        public string Remark { get; set; } = string.Empty;
        // DMT sender/beneficiary context, needed by aggregators that transact against a pre-registered
        // sender + beneficiary (e.g. JaiJui). Empty for payout/vendor-pay flows that don't use them.
        public string SenderMobile { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;
        public string BeneficiaryID { get; set; } = string.Empty;
        public string ProviderRequestID { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }

    public class BankTransferProviderResult
    {
        public string Status { get; set; } = "FAILED"; // SUCCESS / FAILED / PENDING
        public string ProviderRequestID { get; set; } = string.Empty;
        public string ProviderRefNo { get; set; } = string.Empty; // UTR / bank reference
        public string Message { get; set; } = string.Empty;
        public bool IsConfigured { get; set; } = true;

        // Id of the "Add API" provider that handled this transfer (0 = sandbox / none). Stamped onto the
        // DMT log row so the Provider Log shows the provider name and can filter by it.
        public int ProviderID { get; set; }

        /// <summary>
        /// What the provider itself said this transfer earned or cost us, when its reply carries the
        /// figure (JaiJui returns "commission": -5.90 on a 500 transfer). Null = the reply said
        /// nothing, and the configured amount band is used instead. Signed: negative = we paid.
        /// </summary>
        public decimal? ProviderCommission { get; set; }

        // Raw upstream request/response captured for the DMT provider log (the "Log" action on the DMT
        // report). Populated by the provider client; empty for providers that make no external call.
        public string RawRequest { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
    }

    // What a status enquiry needs: the reference the transfer was SENT with. JaiJui (and every other
    // aggregator on this rail) answers a status call on the request id we generated, not on our
    // transaction id, so ProviderRequestID is the key field here.
    public class BankTransferStatusContext
    {
        public int TransactionMasterID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public string ProviderRequestID { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }

    public class BankTransferStatusResult
    {
        public string Status { get; set; } = "PENDING"; // SUCCESS / FAILED / PENDING
        public string ProviderRefNo { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        // False = this rail cannot answer (no gateway active, sandbox, legacy row). The caller then
        // falls back to the template-driven STATUS endpoint and the transfer stays as it was.
        public bool IsConfigured { get; set; } = true;
        public int ProviderID { get; set; }
        public string RawRequest { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
    }

    // Seam for the external bank-transfer gateway. The pipeline (validation, wallet debit, transaction
    // record, settlement) is provider-agnostic; only this call talks to the upstream DMT/payout API.
    public interface IBankTransferProviderClient
    {
        Task<BankTransferProviderResult> SendTransfer(BankTransferContext context);

        // Ask the same gateway what became of a transfer it accepted. A DMT/payout transfer is sent
        // down this rail, NOT through the recharge gateway's endpoint templates, so without this the
        // status check had nowhere to go: the enquiry resolved no endpoint and every PENDING transfer
        // sat there until someone settled it by hand.
        Task<BankTransferStatusResult> CheckTransferStatus(BankTransferStatusContext context);
    }
}
