using System.Text;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ApiUser;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FintechApp.Controllers
{
    /// <summary>
    /// The HTTP plumbing every SIGNED B2B contract shares — /api/v1/dmt/* and /api/v1/payout/*.
    ///
    /// It exists so the signing scheme has exactly one implementation on the server side. The
    /// signature is an HMAC over the RAW body bytes (see <see cref="ApiUserSignature"/>), so the
    /// body must be buffered verbatim and the method, path, timestamp and nonce lifted off the
    /// wire untouched. Two copies of that would drift, and a drifted copy means either every
    /// correctly-signed request on one route is rejected, or a secret leaks into the audit table.
    ///
    /// Subclasses declare the routes and bind their own fields with <see cref="P"/>; everything
    /// below — buffering, credential precedence, signed material, audit masking — is fixed.
    /// </summary>
    public abstract class ApiUserSignedControllerBase : ControllerBase
    {
        /// <summary>Header names for credentials and signed material. Body/query fallbacks use the same names, lowercased.</summary>
        protected const string HeaderTimestamp = "X-Api-Timestamp";
        protected const string HeaderNonce = "X-Api-Nonce";
        protected const string HeaderSignature = "X-Api-Signature";
        protected const string HeaderMobile = "X-Api-Mobile";
        /// <summary>Legacy header for the numeric account id. Still read, never documented.</summary>
        protected const string HeaderUserId = "X-Api-UserId";
        protected const string HeaderToken = "X-Api-Token";

        /// <summary>
        /// A body larger than this is refused before it is buffered. Every signed payload is a
        /// handful of short fields, so anything bigger is a mistake or an attempt to exhaust
        /// memory — and the signature is computed over the buffered bytes, so the buffer must be
        /// bounded.
        /// </summary>
        private const int MaxBodyBytes = 64 * 1024;

        /// <summary>The parsed JSON body, read once per request and reused by every lookup.</summary>
        private JObject? _body;

        /// <summary>The raw body bytes exactly as received — the signature is computed over these.</summary>
        private byte[] _rawBody = Array.Empty<byte>();

        /// <summary>
        /// Reads and buffers the request body ONCE. Must be awaited before any call to
        /// <see cref="P"/> or <see cref="Bind"/>, because both read from the buffer.
        ///
        /// The bytes are kept verbatim: the signature is an HMAC over exactly what the caller put
        /// on the wire, so re-serialising the parsed JSON — with our key order, our number
        /// formatting, our whitespace — would produce a different string and reject every
        /// correctly-signed request.
        /// </summary>
        protected async Task Load()
        {
            try
            {
                if (HttpMethods.IsGet(Request.Method)) return;
                if (Request.ContentLength is > MaxBodyBytes) return;

                Request.EnableBuffering();
                using var buffer = new MemoryStream();
                await Request.Body.CopyToAsync(buffer);
                Request.Body.Position = 0;

                if (buffer.Length == 0 || buffer.Length > MaxBodyBytes) return;
                _rawBody = buffer.ToArray();

                // A form post is bound through Request.Form instead; only JSON is parsed here.
                if (Request.HasFormContentType) return;

                string text = Encoding.UTF8.GetString(_rawBody).TrimStart();
                if (text.StartsWith('{')) _body = JObject.Parse(text);
            }
            catch
            {
                // Malformed JSON is not an exception the caller should see as a 500. Leaving _body
                // null makes every parameter read as empty, and the gateway's own validation then
                // returns the precise contract error for whatever was actually missing.
                _body = null;
            }
        }

        /// <summary>Fills the parameters every signed endpoint shares, including the signed material.</summary>
        protected T Bind<T>(T request) where T : ApiUserRequestBase, IApiUserSecuredRequest
        {
            request.Mobile = Credential("mobile", HeaderMobile);
            // Legacy fallback: an integration built before the credential became the mobile
            // number sends the numeric account id. The auth service resolves either.
            if (request.Mobile.Length == 0) request.Mobile = Credential("userid", HeaderUserId);
            request.Token = Credential("token", HeaderToken);
            request.RequestId = P("requestid");
            request.ClientIp = IPAddressHelper.GetClientIPAddress(HttpContext);
            request.RawRequest = RawRequest();

            request.Security = new ApiUserSignedRequest
            {
                Method = Request.Method,
                Path = Request.Path.Value ?? string.Empty,
                Timestamp = Signed("timestamp", HeaderTimestamp),
                Nonce = Signed("nonce", HeaderNonce),
                Signature = Signed("signature", HeaderSignature),
                Body = _rawBody,
            };

            return request;
        }

        /// <summary>
        /// Reads a parameter from the JSON body, falling back to POST form fields and then the
        /// query string. Body first because that is where a correct integration puts it; the
        /// fallbacks keep simple form-posting stacks working.
        /// </summary>
        protected string P(string key)
        {
            if (_body != null)
            {
                var token = _body.GetValue(key, StringComparison.OrdinalIgnoreCase);
                if (token != null && token.Type != JTokenType.Null)
                {
                    string? v = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
                    if (!string.IsNullOrEmpty(v)) return v.Trim();
                }
            }

            if (Request.HasFormContentType)
            {
                string? form = Request.Form[key].FirstOrDefault();
                if (!string.IsNullOrEmpty(form)) return form.Trim();
            }

            return (Request.Query[key].FirstOrDefault() ?? string.Empty).Trim();
        }

        /// <summary>The requestid as sent, for echoing on a reply built before any binding happened.</summary>
        protected string QueryRequestId() => Request.Query["requestid"].FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Credentials, preferring the header. A header keeps the token out of the query string on
        /// the one GET these APIs expose, and out of any body an integrator might log wholesale.
        /// </summary>
        private string Credential(string key, string header)
        {
            string? fromHeader = Request.Headers[header].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(fromHeader) ? fromHeader.Trim() : P(key);
        }

        /// <summary>
        /// Signed material, preferring the header. The body fallback exists only for stacks that
        /// cannot set custom headers — and note it changes nothing about the signature itself,
        /// which is always computed over the raw body plus the method, path, timestamp and nonce.
        /// </summary>
        private string Signed(string key, string header)
        {
            string? fromHeader = Request.Headers[header].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(fromHeader) ? fromHeader.Trim() : P(key);
        }

        /// <summary>
        /// The inbound call rendered for the audit log, with every secret masked. Support staff
        /// read these rows, so the token and the signature must never be stored in full — but
        /// enough of each is kept to tell a stale credential from a missing one.
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

            if (_body != null)
            {
                foreach (var property in _body.Properties())
                    parts.Add($"{property.Name}={Mask(property.Name, property.Value?.ToString())}");
            }

            foreach (var header in new[] { HeaderMobile, HeaderUserId, HeaderToken, HeaderTimestamp, HeaderNonce, HeaderSignature })
            {
                string? value = Request.Headers[header].FirstOrDefault();
                if (!string.IsNullOrEmpty(value)) parts.Add($"{header}={Mask(header, value)}");
            }

            var sb = new StringBuilder();
            sb.Append(Request.Method).Append(' ').Append(Request.Path);
            if (parts.Count > 0) sb.Append('?').Append(string.Join('&', parts));
            return sb.ToString();
        }

        /// <summary>
        /// Keys whose values must never reach the audit table in full. Matched exactly rather than
        /// by substring, so a legitimate field is never mangled — "pincode" is a postal code and
        /// belongs in the log, while "pin" is a credential and does not.
        /// </summary>
        private static readonly HashSet<string> SecretKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "token", "signature", "otp", "pin", "mpin",
            HeaderToken, HeaderSignature,
        };

        /// <summary>
        /// Masks a value when its key names a secret. Keeps the first four characters so a support
        /// engineer can still match a request against the credential that was in use.
        /// </summary>
        private static string Mask(string key, string? value)
        {
            string v = value ?? string.Empty;
            if (v.Length == 0 || !SecretKeys.Contains(key)) return v;

            return v.Length <= 4 ? new string('*', v.Length) : $"{v[..4]}{new string('*', Math.Min(v.Length - 4, 8))}";
        }
    }
}
