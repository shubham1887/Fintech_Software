using FintechApp.Requests.AccountProcessing;

namespace FintechApp.Interfaces.Services
{
    // Low-level wallet ledger engine (ported from old Processing/AccountProcessing.cs).
    // Atomically updates a user's wallet balance and writes a TransactionDetails ledger row.
    public interface IAccountProcessingService
    {
        // Returns StatusType.Ok when the wallet can fund (amount) given (commission) and the
        // user's minimum-balance capping; otherwise an "Insufficient Balance..." message.
        Task<string> BalanceValidationBeforeTransaction(int userID, int walletID, decimal amount, decimal commission);

        // Posts a debit (balance decreases by AmountWithCommission). Returns rows affected (>0 success, <=0 failure).
        Task<int> DebitBalance(AccountPostingRequest request);

        // Posts a credit (balance increases by AmountWithCommission). Returns rows affected.
        Task<int> CreditBalance(AccountPostingRequest request);
    }
}
