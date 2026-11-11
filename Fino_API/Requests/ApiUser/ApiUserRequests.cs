using FintechApp.Interfaces.Services;

namespace FintechApp.Requests.ApiUser
{
    /// <summary>
    /// Implemented by every request on a SIGNED contract (/dmt, /payout). It is what lets one
    /// controller base fill the signed material for all of them — see
    /// <c>ApiUserSignedControllerBase.Bind</c> — so the signing scheme has a single
    /// implementation instead of one copy per contract.
    /// </summary>
    public interface IApiUserSecuredRequest
    {
        /// <summary>Timestamp, nonce, signature and the raw body bytes they were computed over.</summary>
        ApiUserSignedRequest Security { get; set; }
    }

    /// <summary>
    /// Fields every /api/v1 call carries. Values arrive as raw strings straight off the query
    /// string or form body — parsing and validation belong to the gateway service, not the
    /// controller, so that every endpoint rejects bad input with the same code and wording.
    /// </summary>
    public abstract class ApiUserRequestBase
    {
        /// <summary>
        /// The caller's REGISTERED MOBILE NUMBER — the account identifier on every /api/v1 call,
        /// sent as <c>mobile</c> (or the <c>X-Api-Mobile</c> header), unparsed.
        ///
        /// It is a mobile and not an internal account id because the mobile is the number the
        /// retailer already knows, logs in with, and can read off their own profile, while the
        /// numeric id exists only inside our database. It is unique (IX_UserMasters_MobileNumber)
        /// so it identifies exactly one account. The legacy <c>userid</c> parameter still resolves
        /// for integrations built before this change.
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>The API token issued on the API Access page.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>The caller's own reference. Echoed on every reply. Idempotency key on /recharge.</summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>Caller IP, resolved by the controller. Checked against the IP whitelist.</summary>
        public string ClientIp { get; set; } = string.Empty;

        /// <summary>
        /// The inbound request line as received, with the token masked, for the audit log.
        /// Never used for business logic.
        /// </summary>
        public string RawRequest { get; set; } = string.Empty;
    }

    /// <summary>A transaction request on /api/v1/recharge.</summary>
    public class ApiUserRechargeRequest : ApiUserRequestBase
    {
        /// <summary>Operator code, e.g. AIRT — see /api/v1/operators.</summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>Mobile / DTH subscriber id / consumer number to recharge.</summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>Amount, unparsed.</summary>
        public string Amount { get; set; } = string.Empty;

        /// <summary>Customer mobile for the confirmation SMS. Defaults to <see cref="Number"/>.</summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>The API user's mPIN, when PIN protection is enabled on the account.</summary>
        public string Pin { get; set; } = string.Empty;

        /// <summary>Extra biller field 1 (bill payments).</summary>
        public string Param1 { get; set; } = string.Empty;

        /// <summary>Extra biller field 2 (bill payments).</summary>
        public string Param2 { get; set; } = string.Empty;

        /// <summary>Extra biller field 3 (bill payments).</summary>
        public string Param3 { get; set; } = string.Empty;

        /// <summary>Customer name (bill payments).</summary>
        public string CustomerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// A status enquiry on /api/v1/status. Resolved by the caller's own
    /// <see cref="ApiUserRequestBase.RequestId"/>, or by our <see cref="TxnId"/> when they kept it.
    /// </summary>
    public class ApiUserStatusRequest : ApiUserRequestBase
    {
        /// <summary>Our transaction id, unparsed. Optional when requestid is supplied.</summary>
        public string TxnId { get; set; } = string.Empty;
    }

    /// <summary>A read-only enquiry: /api/v1/balance, /api/v1/operators.</summary>
    public class ApiUserQueryRequest : ApiUserRequestBase
    {
    }
}
