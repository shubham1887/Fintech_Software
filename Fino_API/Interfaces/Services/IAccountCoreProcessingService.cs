namespace FintechApp.Interfaces.Services
{
    // Settlement engine (ported from old Processing/AccountCoreProcessing.cs) built on top of
    // IAccountProcessingService. Handles transaction success finalisation and refund/reversal
    // including commission reversal across retailer/distributor/super-distributor wallets.
    public interface IAccountCoreProcessingService
    {
        // Finalises a transaction as SUCCESS (the user wallet was debited up-front at initiation).
        // Returns rows affected by the status update.
        Task<int> SuccessTransaction(int transactionMasterID, string referenceNo, string remark);

        // Refunds a failed transaction: marks it FAILED, credits the debited amount back to the
        // user wallet (except AEPS/MATM cash-in services), and reverses retailer + upline commissions.
        Task<string> RefundTransaction(int transactionMasterID, string message, string docType);

        // Posts the chain commissions at initiation (the moment the txn goes to PROCESS). For the
        // recharge family this is proper double entry: each downline commission is CREDITED to the
        // downline and DEBITED from the admin (admin funds it), and a retailer surcharge is CREDITED
        // to the admin. For other services the legacy behaviour (credit downlines only) is preserved.
        // Reversed symmetrically by RefundTransaction on failure.
        Task PostInitiationCommissions(int transactionMasterID);

        // Credits the admin's income for a transaction at initiation (non-failed / PROCESS or
        // SUCCESS). Recharge family: credits the admin the provider commission P — combined with the
        // step-7 admin debits this nets to the admin profit P-(R+D+S). Other services: credits the
        // single net admin commission (legacy). Reversed by RefundTransaction if the txn later fails.
        Task CreditAdminOutcome(int transactionMasterID);
    }
}
