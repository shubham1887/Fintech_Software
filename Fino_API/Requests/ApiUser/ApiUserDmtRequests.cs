using FintechApp.Interfaces.Services;

namespace FintechApp.Requests.ApiUser
{
    /// <summary>
    /// Fields every /api/v1/dmt call carries on top of the shared credential block.
    ///
    /// The DMT endpoints move real money in large tickets, so each one additionally carries the
    /// signed material (<see cref="Security"/>) the controller lifted off the request. Values
    /// still arrive as raw strings — parsing and validation belong to the gateway service, so
    /// every endpoint rejects bad input with the same code and wording.
    /// </summary>
    public abstract class ApiUserDmtRequestBase : ApiUserRequestBase, IApiUserSecuredRequest
    {
        /// <summary>
        /// Timestamp, nonce, signature and the raw body bytes they were computed over. Populated
        /// by the controller straight from the HTTP request; never by the caller's JSON.
        /// </summary>
        public ApiUserSignedRequest Security { get; set; } = new();
    }

    /// <summary>A read-only DMT enquiry: /dmt/balance, /dmt/banks.</summary>
    public class ApiUserDmtQueryRequest : ApiUserDmtRequestBase
    {
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Sender (the remitter — the walk-in customer sending the money)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>/dmt/sender/register — onboard a remitter against their mobile number.</summary>
    public class ApiUserDmtSenderRegisterRequest : ApiUserDmtRequestBase
    {
        /// <summary>Remitter's 10-digit mobile. This is the sender's identity across the platform.</summary>
        public string SenderMobile { get; set; } = string.Empty;

        /// <summary>Remitter's full name as per their KYC.</summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Remitter's address. Optional.</summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>Remitter's 6-digit pincode. Optional.</summary>
        public string Pincode { get; set; } = string.Empty;
    }

    /// <summary>/dmt/sender/otp/verify and /dmt/sender/otp/resend.</summary>
    public class ApiUserDmtSenderOtpRequest : ApiUserDmtRequestBase
    {
        public string SenderMobile { get; set; } = string.Empty;

        /// <summary>The OTP the remitter received. Not read by the resend endpoint.</summary>
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>/dmt/sender/fetch — profile and remaining monthly limit for a registered remitter.</summary>
    public class ApiUserDmtSenderFetchRequest : ApiUserDmtRequestBase
    {
        public string SenderMobile { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Beneficiary (who the money goes to)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>/dmt/beneficiary/register — attach a bank account to a verified remitter.</summary>
    public class ApiUserDmtBeneficiaryRegisterRequest : ApiUserDmtRequestBase
    {
        /// <summary>The remitter this beneficiary belongs to.</summary>
        public string SenderMobile { get; set; } = string.Empty;

        /// <summary>Account holder's name.</summary>
        public string BeneficiaryName { get; set; } = string.Empty;

        /// <summary>Destination account number.</summary>
        public string AccountNo { get; set; } = string.Empty;

        /// <summary>Destination IFSC.</summary>
        public string Ifsc { get; set; } = string.Empty;

        /// <summary>Destination bank name. Resolved from the IFSC when omitted.</summary>
        public string BankName { get; set; } = string.Empty;
    }

    /// <summary>/dmt/beneficiary/otp/verify and /dmt/beneficiary/otp/resend.</summary>
    public class ApiUserDmtBeneficiaryOtpRequest : ApiUserDmtRequestBase
    {
        /// <summary>The beneficiary reference returned by /dmt/beneficiary/register.</summary>
        public string BeneficiaryId { get; set; } = string.Empty;

        /// <summary>The OTP the remitter received. Not read by the resend endpoint.</summary>
        public string Otp { get; set; } = string.Empty;
    }

    /// <summary>/dmt/beneficiary/list — every beneficiary registered under one remitter.</summary>
    public class ApiUserDmtBeneficiaryListRequest : ApiUserDmtRequestBase
    {
        public string SenderMobile { get; set; } = string.Empty;
    }

    /// <summary>/dmt/beneficiary/delete — detach a beneficiary from a remitter.</summary>
    public class ApiUserDmtBeneficiaryRefRequest : ApiUserDmtRequestBase
    {
        public string BeneficiaryId { get; set; } = string.Empty;
    }

    /// <summary>
    /// /dmt/beneficiary/verify — penny-drop name enquiry. Chargeable: a fee is debited from the
    /// DMT wallet whether or not the bank returns a name.
    /// </summary>
    public class ApiUserDmtBeneficiaryVerifyRequest : ApiUserDmtRequestBase
    {
        public string SenderMobile { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string Ifsc { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Money movement
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// /dmt/transfer — the money-moving call.
    /// <see cref="ApiUserRequestBase.RequestId"/> is MANDATORY here and is the idempotency key.
    /// </summary>
    public class ApiUserDmtTransferRequest : ApiUserDmtRequestBase
    {
        /// <summary>
        /// The customer's 10-digit mobile. REQUIRED on both rails.
        ///
        /// On a pure DMT platform it must be a registered, OTP-verified remitter. On a payout
        /// rail it need not be registered anywhere — any valid number is accepted — but it is
        /// still required: it is the contact number filed against the transfer, and the only
        /// customer-side identifier a later dispute can be traced by.
        /// </summary>
        public string SenderMobile { get; set; } = string.Empty;

        /// <summary>
        /// The beneficiary reference from /dmt/beneficiary/register or /list. Required on a PURE
        /// DMT platform. On a payout rail it is optional: send it if you registered one, or name
        /// the destination inline with <see cref="AccountNo"/>, <see cref="Ifsc"/> and
        /// <see cref="BeneficiaryName"/> and skip registration entirely.
        /// </summary>
        public string BeneficiaryId { get; set; } = string.Empty;

        /// <summary>
        /// Destination account number, named inline instead of pre-registered. PAYOUT RAIL ONLY —
        /// a pure DMT rail will not accept a destination it has not registered, so this is
        /// refused there rather than silently ignored.
        /// </summary>
        public string AccountNo { get; set; } = string.Empty;

        /// <summary>Destination IFSC, with <see cref="AccountNo"/>. Payout rail only.</summary>
        public string Ifsc { get; set; } = string.Empty;

        /// <summary>Account holder's name, with <see cref="AccountNo"/>. Payout rail only.</summary>
        public string BeneficiaryName { get; set; } = string.Empty;

        /// <summary>Destination bank name. Resolved from the IFSC when omitted. Payout rail only.</summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>Amount in rupees, unparsed.</summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>IMPS or NEFT. Defaults to IMPS.</summary>
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>Free-text narration carried to the beneficiary's statement where the bank supports it.</summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>Retail outlet latitude, when the integrator captures it. Stored for audit.</summary>
        public string Latitude { get; set; } = string.Empty;

        /// <summary>Retail outlet longitude, when the integrator captures it. Stored for audit.</summary>
        public string Longitude { get; set; } = string.Empty;
    }

    /// <summary>
    /// /dmt/status — current state of a transfer, resolved by the caller's own
    /// <see cref="ApiUserRequestBase.RequestId"/> or by our <see cref="TxnId"/>.
    /// </summary>
    public class ApiUserDmtStatusRequest : ApiUserDmtRequestBase
    {
        /// <summary>Our transaction id, unparsed. Optional when requestid is supplied.</summary>
        public string TxnId { get; set; } = string.Empty;
    }

    /// <summary>/dmt/ifsc — branch lookup for an IFSC.</summary>
    public class ApiUserDmtIfscRequest : ApiUserDmtRequestBase
    {
        public string Ifsc { get; set; } = string.Empty;
    }
}
