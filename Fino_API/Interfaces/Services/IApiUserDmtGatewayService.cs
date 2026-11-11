using FintechApp.Requests.ApiUser;
using FintechApp.Responses.ApiUser;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The DMT (Domestic Money Transfer) half of the B2B gateway — the /api/v1/dmt contract.
    ///
    /// It is a sibling of <see cref="IApiUserGatewayService"/> and follows the same rules:
    /// every request passes through authentication, request-signature verification, validation,
    /// idempotency, execution, audit logging and the outbound callback, so the controller does
    /// nothing but bind parameters. No method ever throws — an unhandled fault comes back as a
    /// SYS_ERROR envelope with the caller's requestid echoed.
    ///
    /// Two things separate DMT from recharge and shape this interface:
    ///
    ///   1. DMT is a MULTI-STEP flow, not a single call. A remitter is registered and OTP-verified,
    ///      a beneficiary is registered and OTP-verified, and only then may money move. Each step
    ///      is its own endpoint, and the ones that create state return the reference the next step
    ///      needs.
    ///
    ///   2. The tickets are large. Everything except the balance read therefore moves on POST with
    ///      a signed body — see <see cref="IApiUserRequestSecurityService"/> — so a captured
    ///      request cannot be replayed, re-pointed at another endpoint or altered in flight.
    /// </summary>
    public interface IApiUserDmtGatewayService
    {
        // ── Remitter onboarding ──────────────────────────────────────────

        /// <summary>
        /// Register a remitter against their mobile number. Replies with
        /// <c>otprequired: true</c> when the platform has sent an OTP that
        /// <see cref="VerifySenderOtp"/> must clear before the remitter can transact.
        /// </summary>
        Task<ApiUserDmtAckResponse> RegisterSender(ApiUserDmtSenderRegisterRequest request);

        /// <summary>Clear the OTP sent to a remitter, activating them for transfers.</summary>
        Task<ApiUserDmtAckResponse> VerifySenderOtp(ApiUserDmtSenderOtpRequest request);

        /// <summary>Send the remitter a fresh OTP.</summary>
        Task<ApiUserDmtAckResponse> ResendSenderOtp(ApiUserDmtSenderOtpRequest request);

        /// <summary>
        /// The remitter's profile and the headroom left on their limit. Call this before quoting
        /// a transfer to the customer: it is the only way to know what they may still send.
        /// </summary>
        Task<ApiUserDmtSenderResponse> FetchSender(ApiUserDmtSenderFetchRequest request);

        // ── Beneficiary management ───────────────────────────────────────

        /// <summary>
        /// Attach a bank account to a verified remitter. Returns the beneficiary reference that
        /// <see cref="Transfer"/> quotes, and whether an OTP still has to be cleared.
        /// </summary>
        Task<ApiUserDmtBeneficiaryResponse> RegisterBeneficiary(ApiUserDmtBeneficiaryRegisterRequest request);

        /// <summary>Clear the OTP for a beneficiary, making it eligible to receive transfers.</summary>
        Task<ApiUserDmtAckResponse> VerifyBeneficiaryOtp(ApiUserDmtBeneficiaryOtpRequest request);

        /// <summary>Send a fresh beneficiary OTP.</summary>
        Task<ApiUserDmtAckResponse> ResendBeneficiaryOtp(ApiUserDmtBeneficiaryOtpRequest request);

        /// <summary>Every beneficiary registered under one remitter by THIS API user.</summary>
        Task<ApiUserDmtBeneficiaryListResponse> ListBeneficiaries(ApiUserDmtBeneficiaryListRequest request);

        /// <summary>Detach a beneficiary from a remitter.</summary>
        Task<ApiUserDmtAckResponse> DeleteBeneficiary(ApiUserDmtBeneficiaryRefRequest request);

        /// <summary>
        /// Penny-drop name enquiry against the destination account. Chargeable — a fee is debited
        /// from the DMT wallet — so it is a separate, explicit call and never implied by a
        /// registration.
        /// </summary>
        Task<ApiUserDmtVerifyAccountResponse> VerifyBeneficiaryAccount(ApiUserDmtBeneficiaryVerifyRequest request);

        // ── Money movement ───────────────────────────────────────────────

        /// <summary>
        /// Send money. The caller's requestid is the idempotency key: a requestid already seen
        /// for this user replays the original transfer's current result (<c>duplicate: true</c>)
        /// and never debits the wallet twice.
        /// </summary>
        Task<ApiUserDmtTransactionResponse> Transfer(ApiUserDmtTransferRequest request);

        /// <summary>
        /// Current state of a transfer, by the caller's requestid or our txnid. A still-pending
        /// transfer is re-enquired with the upstream provider first, so the answer reflects the
        /// bank rather than only our record.
        /// </summary>
        Task<ApiUserDmtTransactionResponse> Status(ApiUserDmtStatusRequest request);

        // ── Reference data ───────────────────────────────────────────────

        /// <summary>The API user's wallet balances — the only GET in the DMT contract.</summary>
        Task<ApiUserBalanceResponse> Balance(ApiUserDmtQueryRequest request);

        /// <summary>The DMT bank catalogue.</summary>
        Task<ApiUserDmtBankListResponse> Banks(ApiUserDmtQueryRequest request);

        /// <summary>Branch details behind an IFSC.</summary>
        Task<ApiUserDmtIfscResponse> Ifsc(ApiUserDmtIfscRequest request);
    }
}
