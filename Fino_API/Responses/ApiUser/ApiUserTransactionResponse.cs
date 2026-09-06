using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>Where a transaction payload came from. Same shape, three origins.</summary>
    public static class ApiUserEventType
    {
        /// <summary>Synchronous reply to the initial transaction request.</summary>
        public const string Initial = "INITIAL";
        /// <summary>Reply to an explicit status enquiry.</summary>
        public const string Status = "STATUS";
        /// <summary>Server-initiated push once the transaction reached a final status.</summary>
        public const string Callback = "CALLBACK";
    }

    /// <summary>
    /// THE common transaction payload. Returned verbatim by:
    ///   • POST/GET /api/v1/recharge  (event = INITIAL)
    ///   • POST/GET /api/v1/status    (event = STATUS)
    ///   • the callback we POST to the integrator's URL (event = CALLBACK)
    ///
    /// Every field means the same thing in all three, so a single handler can process the
    /// initial reply, a poll and a push without branching on the source.
    /// </summary>
    public class ApiUserTransactionResponse : ApiUserEnvelope
    {
        /// <summary>INITIAL | STATUS | CALLBACK — which touch-point produced this payload.</summary>
        [JsonProperty("event", Order = 6)]
        public string Event { get; set; } = ApiUserEventType.Initial;

        /// <summary>True once <see cref="ApiUserEnvelope.Status"/> can no longer change.</summary>
        [JsonProperty("final", Order = 7)]
        public bool Final { get; set; }

        /// <summary>
        /// True when this requestid had already been used and we are replaying the original
        /// transaction instead of creating a new one. The wallet was NOT debited again.
        /// </summary>
        [JsonProperty("duplicate", Order = 8)]
        public bool Duplicate { get; set; }

        /// <summary>Our transaction id. 0 when no transaction was created.</summary>
        [JsonProperty("txnid", Order = 9)]
        public int TxnId { get; set; }

        /// <summary>Operator / aggregator reference number, when the upstream returned one.</summary>
        [JsonProperty("providerref", Order = 10)]
        public string ProviderRef { get; set; } = string.Empty;

        /// <summary>Service name, e.g. "Prepaid Recharge".</summary>
        [JsonProperty("service", Order = 11)]
        public string Service { get; set; } = string.Empty;

        /// <summary>Numeric service id, stable across renames.</summary>
        [JsonProperty("serviceid", Order = 12)]
        public int ServiceId { get; set; }

        /// <summary>Operator name, e.g. "Airtel".</summary>
        [JsonProperty("operator", Order = 13)]
        public string Operator { get; set; } = string.Empty;

        /// <summary>Operator code as used in the request, e.g. "AIRT".</summary>
        [JsonProperty("operatorcode", Order = 14)]
        public string OperatorCode { get; set; } = string.Empty;

        /// <summary>Recharged number / DTH subscriber id / consumer number.</summary>
        [JsonProperty("number", Order = 15)]
        public string Number { get; set; } = string.Empty;

        /// <summary>Transaction amount.</summary>
        [JsonProperty("amount", Order = 16)]
        public decimal Amount { get; set; }

        /// <summary>Amount actually taken from the wallet (amount + surcharge - discount).</summary>
        [JsonProperty("debitamount", Order = 17)]
        public decimal DebitAmount { get; set; }

        /// <summary>The API user's wallet balance after this transaction settled.</summary>
        [JsonProperty("balance", Order = 18)]
        public decimal Balance { get; set; }

        /// <summary>Transaction date-time, always "yyyy-MM-dd HH:mm:ss" (server local time).</summary>
        [JsonProperty("date", Order = 19)]
        public string Date { get; set; } = string.Empty;
    }
}
