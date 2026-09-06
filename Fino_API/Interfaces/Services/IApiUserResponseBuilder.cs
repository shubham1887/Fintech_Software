using FintechApp.Entities;
using FintechApp.Responses.ApiUser;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Builds the API-user payloads that need database context. This is the ONLY place a
    /// transaction is turned into the public contract, which is what makes the initial reply,
    /// the status enquiry and the pushed callback identical in shape — they all call
    /// <see cref="FromTransaction"/> and differ only in the <c>event</c> field.
    /// </summary>
    public interface IApiUserResponseBuilder
    {
        /// <summary>
        /// The common transaction payload, resolved from the stored transaction (single source
        /// of truth — never from an in-flight DTO, so a replay can never disagree with the original).
        /// </summary>
        /// <param name="txn">The transaction to describe.</param>
        /// <param name="eventType">INITIAL, STATUS or CALLBACK — see <see cref="ApiUserEventType"/>.</param>
        /// <param name="balanceOverride">Wallet balance to report; looked up when null.</param>
        /// <param name="messageOverride">Message to report; the transaction's own remark when null.</param>
        /// <param name="duplicate">True when replaying an already-used requestid.</param>
        /// <param name="requestIdOverride">Echo this requestid instead of the stored one.</param>
        Task<ApiUserTransactionResponse> FromTransaction(
            TransactionMaster txn,
            string eventType,
            decimal? balanceOverride = null,
            string? messageOverride = null,
            bool duplicate = false,
            string? requestIdOverride = null);

        /// <summary>Wallet balances for one API user.</summary>
        Task<ApiUserBalanceResponse> Balance(int userId, string requestId);

        /// <summary>The live operator catalogue.</summary>
        Task<ApiUserOperatorListResponse> Operators(string requestId);
    }
}
