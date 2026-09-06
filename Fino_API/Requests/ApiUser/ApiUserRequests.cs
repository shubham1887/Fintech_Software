namespace FintechApp.Requests.ApiUser
{
    /// <summary>
    /// Fields every /api/v1 call carries. Values arrive as raw strings straight off the query
    /// string or form body — parsing and validation belong to the gateway service, not the
    /// controller, so that every endpoint rejects bad input with the same code and wording.
    /// </summary>
    public abstract class ApiUserRequestBase
    {
        /// <summary>The API user's numeric account id, unparsed.</summary>
        public string UserId { get; set; } = string.Empty;

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
