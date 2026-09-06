using MplanCore.Controller;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace MplanCore.Response
{
    internal class PlanResponse
    {
        [JsonProperty("records")]
        public Records Records { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        public PlanResponse()
        {
            Records = new Records();
        }
    }

    public class Records
    {
        [JsonProperty("FULLTT")]
        public The2G[] FULLTT { get; set; }

        [JsonProperty("TOPUP")]
        public The2G[] TOPUP { get; set; }
        

        [JsonProperty("3G/4G")]

        [Column("3G/4G")]
        public The2G[] _3G4G { get; set; }

        [JsonProperty("RATE CUTTER")]
        public The2G[] RateCutter { get; set; }


        [JsonProperty("2G")]

        [Column("2G")]
        public The2G[] _2G { get; set; }

        [JsonProperty("SMS")]
        public The2G[] SMS { get; set; }

        [JsonProperty("Romaing")]
        public The2G[] Roaming { get; set; }

        [JsonProperty("COMBO")]
        public The2G[] COMBO { get; set; }

        [JsonProperty("JioPhone")]
        public The2G[] JioPhone { get; set; }

        [JsonProperty("FRC")]
        public The2G[] FRC { get; set; }

        public Records()
        {
            FULLTT = new The2G[0];
            TOPUP = new The2G[0];
            _3G4G = new The2G[0];
            RateCutter = new The2G[0];
            _2G = new The2G[0];
            SMS = new The2G[0];
            Roaming = new The2G[0];
            COMBO = new The2G[0];
            JioPhone = new The2G[0];
            FRC = new The2G[0];
        }

        private string _message;

        public string Message
        {
            get { return _message.TrimIfNotNull(); }
            set { _message = value.TrimIfNotNull(); }
        }
    }

    public class The2G
    {
        [JsonProperty("rs")]
        private string _Rs;

        public string Rs
        {
            get => _Rs.TrimIfNotNull();
            set => _Rs = value.TrimIfNotNull();
        }

        private string _Desc;

        [JsonProperty("desc")]
        public string Desc
        {
            get => _Desc.TrimIfNotNull();
            set => _Desc = value.TrimIfNotNull();
        }

        private string _Validity;

        [JsonProperty("validity")]
        public string Validity
        {
            get => _Validity.TrimIfNotNull();
            set => _Validity = value.TrimIfNotNull();
        }

        private string _LastUpdate;

        [JsonProperty("last_update")]
        public string LastUpdate
        {
            get => _LastUpdate.TrimIfNotNull();
            set => _LastUpdate = value.TrimIfNotNull();
        }
    }
}