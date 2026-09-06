using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project.Response.RazorPay
{
    internal class RazorPayPayoutResponse
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("contains")]
        public List<string> Contains { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }

    internal class Payload
    {
        [JsonProperty("payout")]
        public Payout Payout { get; set; }
    }

    internal class Payout
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    internal class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("entity")]
        public string EntityEntity { get; set; }

        [JsonProperty("fund_account_id")]
        public string FundAccountId { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("notes")]
        public Notes Notes { get; set; }

        [JsonProperty("fees")]
        public string Fees { get; set; }

        [JsonProperty("tax")]
        public string Tax { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("utr")]
        public string Utr { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("narration")]
        public string Narration { get; set; }

        [JsonProperty("batch_id")]
        public string BatchId { get; set; }

        [JsonProperty("status_details")]
        public StatusDetails StatusDetails { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("fee_type")]
        public string FeeType { get; set; }
    }

    internal class Notes
    {
        [JsonProperty("note_key 1")]
        public string NoteKey1 { get; set; }

        [JsonProperty("note_key 2")]
        public string NoteKey2 { get; set; }
    }

    internal class StatusDetails
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
