using FintechApp.Requests.ApiUser;
using FintechApp.Responses.ApiUser;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The PAYOUT half of the B2B gateway — the /api/v1/payout contract.
    ///
    /// Sibling of <see cref="IApiUserDmtGatewayService"/>, with the same gates (credentials, then
    /// HMAC signature), the same envelope, the same idempotency rule and the same callback. One
    /// thing is deliberately different, and it is the whole point of this contract:
    ///
    ///   PAYOUT IS A SINGLE CALL. There is no remitter to register, no beneficiary to register,
    ///   no OTP and therefore no <c>beneficiaryid</c>. The money is the API user's own, leaving
    ///   their own wallet, and the destination account travels on the transfer request itself.
    ///   DMT needs those steps because it moves a walk-in customer's cash and the regulator
    ///   requires a verified remitter; payout does not, and forcing a DMT-shaped flow onto it
    ///   would make an integrator register a "sender" that does not exist.
    ///
    /// No method ever throws — an unhandled fault comes back as a SYS_ERROR envelope with the
    /// caller's requestid echoed.
    /// </summary>
    public interface IApiUserPayoutGatewayService
    {
        /// <summary>
        /// Send money to a bank account in one call. The caller's requestid is the idempotency
        /// key: a requestid already seen for this user replays the original payout's current
        /// result (<c>duplicate: true</c>) and never debits the wallet twice.
        /// </summary>
        Task<ApiUserPayoutTransactionResponse> Transfer(ApiUserPayoutTransferRequest request);

        /// <summary>
        /// Current state of a payout, by the caller's requestid or our txnid. A still-pending
        /// payout is re-enquired with the upstream provider first, so the answer reflects the
        /// bank rather than only our record.
        /// </summary>
        Task<ApiUserPayoutTransactionResponse> Status(ApiUserPayoutStatusRequest request);

        /// <summary>The API user's wallet balances — the only GET in the payout contract.</summary>
        Task<ApiUserBalanceResponse> Balance(ApiUserPayoutQueryRequest request);

        /// <summary>
        /// Branch details behind an IFSC. Free, and worth calling before a transfer: it is how an
        /// integrator fills <c>bankname</c> and catches a mistyped IFSC before money moves.
        /// </summary>
        Task<ApiUserPayoutIfscResponse> Ifsc(ApiUserPayoutIfscRequest request);
    }
}
