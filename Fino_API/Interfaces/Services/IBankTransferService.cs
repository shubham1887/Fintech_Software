using FintechApp.Requests.Transaction;
using FintechApp.Responses.Transaction;

namespace FintechApp.Interfaces.Services
{
    // Unified money-out pipeline for DMT / Payout / VendorPay: validate → balance check →
    // create PENDING transaction → debit wallet → dispatch to provider → settle (success/refund).
    public interface IBankTransferService
    {
        Task<(string failureMessage, BankTransferResponse? data)> Transfer(BankTransferRequest request, TransferKind kind, int userID, string ipAddress);

        // Send a transfer that the global hold parked (debited and commissioned, never dispatched)
        // through the same provider client and settlement rules as a live one.
        Task<(string failureMessage, BankTransferResponse? data)> ReleaseHeldTransfer(int transactionMasterID, string ipAddress);
    }
}
