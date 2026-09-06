using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>
    /// The outer envelope shared by EVERY API-user response — transaction, status check,
    /// callback, balance and operator list alike. An integrator can deserialise any reply into
    /// this type first, branch on <see cref="Status"/>/<see cref="ErrorCode"/>, and only then
    /// look at the payload block of the specific endpoint.
    ///
    /// Field order is fixed (JsonProperty Order) so the JSON always reads the same way.
    /// </summary>
    public abstract class ApiUserEnvelope
    {
        /// <summary>SUCCESS | PENDING | FAILED | ERROR. See <see cref="ApiUserStatus"/>.</summary>
        [JsonProperty("status", Order = 1)]
        public string Status { get; set; } = ApiUserStatus.Error;

        /// <summary>Numeric mirror of <see cref="Status"/>. See <see cref="ApiUserStatusCode"/>.</summary>
        [JsonProperty("statuscode", Order = 2)]
        public int StatusCode { get; set; } = ApiUserStatusCode.SystemError;

        /// <summary>Stable machine reason code; empty when there is no error.</summary>
        [JsonProperty("errorcode", Order = 3)]
        public string ErrorCode { get; set; } = ApiUserErrorCode.None;

        /// <summary>Human-readable description. Never parse this — parse <see cref="ErrorCode"/>.</summary>
        [JsonProperty("message", Order = 4)]
        public string Message { get; set; } = string.Empty;

        /// <summary>The caller's own reference, echoed on every reply including errors.</summary>
        [JsonProperty("requestid", Order = 5)]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>Server time this response was produced (ISO-8601, with offset).</summary>
        [JsonProperty("timestamp", Order = 98)]
        public string Timestamp { get; set; } = DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

        /// <summary>Contract version this payload conforms to.</summary>
        [JsonProperty("version", Order = 99)]
        public string Version { get; set; } = ApiUserContract.Version;
    }
}
