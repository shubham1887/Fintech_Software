using FintechApp.Requests.ApiUser;
using FintechApp.Responses.ApiUser;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The core of the B2B (API-user) gateway. Everything an integrator's request passes through
    /// lives here — authentication, validation, idempotency, execution, audit logging and the
    /// outbound callback — so the controller is nothing but parameter binding.
    ///
    /// Every method returns a fully-formed contract object and never throws: an unhandled fault
    /// comes back as a SYS_ERROR envelope with the caller's requestid echoed, because an
    /// integrator must always be able to correlate a reply, even a broken one.
    /// </summary>
    public interface IApiUserGatewayService
    {
        /// <summary>
        /// Execute a recharge / bill payment. The caller's requestid is the idempotency key:
        /// a requestid already seen for this user replays the original transaction's current
        /// result (<c>duplicate: true</c>) and never charges the wallet twice.
        /// </summary>
        Task<ApiUserTransactionResponse> Recharge(ApiUserRechargeRequest request);

        /// <summary>
        /// Report the current state of a transaction, resolved by the caller's requestid or by
        /// our txnid. A still-pending transaction is re-enquired with the upstream provider
        /// first (rate-limited), so the answer reflects the operator, not just our record.
        /// </summary>
        Task<ApiUserTransactionResponse> Status(ApiUserStatusRequest request);

        /// <summary>The API user's wallet balances.</summary>
        Task<ApiUserBalanceResponse> Balance(ApiUserQueryRequest request);

        /// <summary>The live operator catalogue with the codes accepted by /recharge.</summary>
        Task<ApiUserOperatorListResponse> Operators(ApiUserQueryRequest request);
    }
}
