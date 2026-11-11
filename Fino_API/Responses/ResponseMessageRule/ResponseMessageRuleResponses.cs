namespace FintechApp.Responses.ResponseMessageRule
{
    public class ResponseMessageRuleResponse
    {
        public int ResponseMessageRuleID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string MatchType { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Note { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }

    /// <summary>What the Test box answers: did a rule catch this message, and what will be sent.</summary>
    public class TestResponseMessageResponse
    {
        public bool Matched { get; set; }
        public int MatchedRuleID { get; set; }
        public string MatchedPattern { get; set; } = string.Empty;
        public string MatchType { get; set; } = string.Empty;
        public string Original { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        /// <summary>Plain-language outcome for the screen to show under the box.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
