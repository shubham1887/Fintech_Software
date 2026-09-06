using Newtonsoft.Json;
using Project.Processing;
using System;

namespace Project.Response.CallbackMaster
{
    public class CashFreePayoutWebhookResponse
    {
        [JsonProperty("event")]
        private string _Event;

        public string Event
        {
            get => _Event.TrimIfNotNull();
            set => _Event = value.TrimIfNotNull();
        }

        [JsonProperty("currentBalance")]
        private string _CurrentBalance;

        public string CurrentBalance
        {
            get => _CurrentBalance.TrimIfNotNull();
            set => _CurrentBalance = value.TrimIfNotNull();
        }

        [JsonProperty("transferId")]
        private string _TransferId;

        public string TransferId
        {
            get => _TransferId.TrimIfNotNull();
            set => _TransferId = value.TrimIfNotNull();
        }

        [JsonProperty("referenceId")]
        private string _ReferenceId;

        public string ReferenceId
        {
            get => _ReferenceId.TrimIfNotNull();
            set => _ReferenceId = value.TrimIfNotNull();
        }

        [JsonProperty("utr")]
        private string _Utr;

        public string Utr
        {
            get => _Utr.TrimIfNotNull();
            set => _Utr = value.TrimIfNotNull();
        }

        [JsonProperty("reason")]
        private string _Reason;

        public string Reason
        {
            get => _Reason.TrimIfNotNull();
            set => _Reason = value.TrimIfNotNull();
        }

        [JsonProperty("eventTime")]
        public DateTimeOffset EventTime { get; set; }

        [JsonProperty("alertTime")]
        public DateTimeOffset AlertTime { get; set; }

        [JsonProperty("signature")]
        private string _Signature;

        public string Signature
        {
            get => _Signature.TrimIfNotNull();
            set => _Signature = value.TrimIfNotNull();
        }
    }
}