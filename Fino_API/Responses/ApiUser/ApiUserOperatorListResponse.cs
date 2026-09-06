using Newtonsoft.Json;

namespace FintechApp.Responses.ApiUser
{
    /// <summary>One operator row in the operator catalogue.</summary>
    public class ApiUserOperator
    {
        /// <summary>Operator code to send as the "operator" parameter, e.g. "AIRT".</summary>
        [JsonProperty("code", Order = 1)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Operator display name, e.g. "Airtel".</summary>
        [JsonProperty("name", Order = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Numeric service id this operator belongs to.</summary>
        [JsonProperty("serviceid", Order = 3)]
        public int ServiceId { get; set; }

        /// <summary>Service name, e.g. "Prepaid Recharge".</summary>
        [JsonProperty("service", Order = 4)]
        public string Service { get; set; } = string.Empty;
    }

    /// <summary>Operator catalogue payload — the shared envelope plus the operator list.</summary>
    public class ApiUserOperatorListResponse : ApiUserEnvelope
    {
        /// <summary>Number of operators returned.</summary>
        [JsonProperty("count", Order = 6)]
        public int Count { get; set; }

        /// <summary>The live, active operator list.</summary>
        [JsonProperty("operators", Order = 7)]
        public List<ApiUserOperator> Operators { get; set; } = new();
    }
}
