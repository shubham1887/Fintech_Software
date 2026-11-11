using FintechApp.Interfaces.Services;

namespace FintechApp.Requests.ApiUser
{
    /// <summary>
    /// Fields every /api/v1/payout call carries on top of the shared credential block.
    ///
    /// Payout is NOT DMT. DMT moves a walk-in customer's cash, so the regulator requires a
    /// registered, OTP-verified remitter and a registered, OTP-verified beneficiary — which is
    /// why /dmt/transfer quotes a <c>beneficiaryid</c> that only exists after two prior calls.
    /// Payout moves the API user's OWN money out of their own wallet to a bank account they
    /// name on the call. There is no remitter and nothing to pre-register, so this contract
    /// takes the destination account inline and completes in a single request.
    ///
    /// Everything else is identical to DMT: POST only, HMAC-signed body, same envelope, same
    /// status/callback semantics.
    /// </summary>
    public abstract class ApiUserPayoutRequestBase : ApiUserRequestBase, IApiUserSecuredRequest
    {
        /// <summary>
        /// Timestamp, nonce, signature and the raw body bytes they were computed over. Populated
        /// by the controller straight from the HTTP request; never by the caller's JSON.
        /// </summary>
        public ApiUserSignedRequest Security { get; set; } = new();
    }

    /// <summary>A read-only payout enquiry: /payout/balance.</summary>
    public class ApiUserPayoutQueryRequest : ApiUserPayoutRequestBase
    {
    }

    /// <summary>
    /// /payout/transfer — the money-moving call, and the whole contract in ONE request.
    /// <see cref="ApiUserRequestBase.RequestId"/> is MANDATORY here and is the idempotency key.
    /// </summary>
    public class ApiUserPayoutTransferRequest : ApiUserPayoutRequestBase
    {
        /// <summary>Destination account number. The payout goes here — nothing is pre-registered.</summary>
        public string AccountNo { get; set; } = string.Empty;

        /// <summary>Destination IFSC.</summary>
        public string Ifsc { get; set; } = string.Empty;

        /// <summary>Account holder's name, as it should appear on the bank's records.</summary>
        public string BeneficiaryName { get; set; } = string.Empty;

        /// <summary>Destination bank name. Resolved from the IFSC when omitted.</summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>Amount in rupees, unparsed.</summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>IMPS, NEFT or RTGS. Defaults to IMPS.</summary>
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>Free-text narration carried to the beneficiary's statement where the bank supports it.</summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// A contact mobile for the payout, stored against the transaction for your own
        /// reconciliation and shown in reports. Optional — payout has no remitter.
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>Outlet latitude, when the integrator captures it. Stored for audit.</summary>
        public string Latitude { get; set; } = string.Empty;

        /// <summary>Outlet longitude, when the integrator captures it. Stored for audit.</summary>
        public string Longitude { get; set; } = string.Empty;
    }

    /// <summary>
    /// /payout/status — current state of a payout, resolved by the caller's own
    /// <see cref="ApiUserRequestBase.RequestId"/> or by our <see cref="TxnId"/>.
    /// </summary>
    public class ApiUserPayoutStatusRequest : ApiUserPayoutRequestBase
    {
        /// <summary>Our transaction id, unparsed. Optional when requestid is supplied.</summary>
        public string TxnId { get; set; } = string.Empty;
    }

    /// <summary>/payout/ifsc — branch lookup for an IFSC.</summary>
    public class ApiUserPayoutIfscRequest : ApiUserPayoutRequestBase
    {
        public string Ifsc { get; set; } = string.Empty;
    }
}
