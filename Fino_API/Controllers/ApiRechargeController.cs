using System.Text;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using Microsoft.AspNetCore.Mvc;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The public B2B API for API-user integrations (contract v1).
    ///
    /// Unlike the rest of the app there is no AES envelope and no JWT — the caller passes their
    /// userid and API token as plain parameters (GET query string or POST form) and receives
    /// plain JSON. Access is guarded by the API token and the optional IP whitelist configured
    /// on the API Access page.
    ///
    /// This class is deliberately thin: it binds parameters and returns JSON. Authentication,
    /// validation, idempotency, execution, audit logging and callbacks all live in
    /// <see cref="IApiUserGatewayService"/>, so every endpoint behaves identically.
    ///
    /// Every endpoint replies HTTP 200 and carries the real outcome in the body's
    /// <c>status</c> / <c>statuscode</c> / <c>errorcode</c> fields — integrators on simple HTTP
    /// stacks must never have to distinguish a transport failure from a business decline.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    public class ApiRechargeController : ControllerBase
    {
        private readonly IApiUserGatewayService _gateway;

        public ApiRechargeController(IApiUserGatewayService gateway) => _gateway = gateway;

        /// <summary>GET/POST /api/v1/recharge — mobile / DTH / bill payment.</summary>
        [HttpGet("recharge")]
        [HttpPost("recharge")]
        public async Task<IActionResult> Recharge()
        {
            var request = Bind(new ApiUserRechargeRequest());
            request.Operator = P("operator");
            request.Number = P("number");
            request.Amount = P("amount");
            request.Mobile = P("mobile");
            request.Pin = P("pin");
            request.Param1 = P("param1");
            request.Param2 = P("param2");
            request.Param3 = P("param3");
            request.CustomerName = P("customername");

            return Ok(await _gateway.Recharge(request));
        }

        /// <summary>
        /// GET/POST /api/v1/status — current state of a transaction, by your requestid or our txnid.
        /// Returns exactly the same payload shape as /recharge and as the callback.
        /// </summary>
        [HttpGet("status")]
        [HttpPost("status")]
        public async Task<IActionResult> Status()
        {
            var request = Bind(new ApiUserStatusRequest());
            request.TxnId = P("txnid");

            return Ok(await _gateway.Status(request));
        }

        /// <summary>GET/POST /api/v1/balance — the API user's wallet balances.</summary>
        [HttpGet("balance")]
        [HttpPost("balance")]
        public async Task<IActionResult> Balance() => Ok(await _gateway.Balance(Bind(new ApiUserQueryRequest())));

        /// <summary>GET/POST /api/v1/operators — the live operator catalogue.</summary>
        [HttpGet("operators")]
        [HttpPost("operators")]
        public async Task<IActionResult> Operators() => Ok(await _gateway.Operators(Bind(new ApiUserQueryRequest())));

        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Fills the parameters every endpoint shares.</summary>
        private T Bind<T>(T request) where T : ApiUserRequestBase
        {
            // The account's registered mobile. 'userid' is the legacy spelling, still accepted so
            // an integration built before this change keeps working; the auth service resolves
            // either value.
            request.Mobile = P("mobile");
            if (request.Mobile.Length == 0) request.Mobile = P("userid");
            request.Token = P("token");
            request.RequestId = P("requestid");
            request.ClientIp = IPAddressHelper.GetClientIPAddress(HttpContext);
            request.RawRequest = RawRequest();
            return request;
        }

        /// <summary>Reads a parameter from the query string, falling back to POST form fields.</summary>
        private string P(string key)
        {
            string? v = Request.Query[key].FirstOrDefault();
            if (string.IsNullOrEmpty(v) && Request.HasFormContentType) v = Request.Form[key].FirstOrDefault();
            return (v ?? string.Empty).Trim();
        }

        /// <summary>
        /// The inbound call rendered for the audit log, with the token masked. Support reads
        /// these rows, so the secret must never be stored — but enough of it is kept to tell a
        /// stale token from a missing one.
        /// </summary>
        private string RawRequest()
        {
            var parts = new List<string>();
            foreach (var kv in Request.Query)
                parts.Add($"{kv.Key}={Mask(kv.Key, kv.Value.FirstOrDefault())}");
            if (Request.HasFormContentType)
            {
                foreach (var kv in Request.Form)
                    parts.Add($"{kv.Key}={Mask(kv.Key, kv.Value.FirstOrDefault())}");
            }

            var sb = new StringBuilder();
            sb.Append(Request.Method).Append(' ').Append(Request.Path);
            if (parts.Count > 0) sb.Append('?').Append(string.Join('&', parts));
            return sb.ToString();
        }

        private static string Mask(string key, string? value)
        {
            string v = value ?? string.Empty;
            bool secret = key.Equals("token", StringComparison.OrdinalIgnoreCase)
                       || key.Equals("pin", StringComparison.OrdinalIgnoreCase);
            if (!secret) return v;
            return v.Length <= 4 ? "****" : $"****{v[^4..]}";
        }
    }
}
