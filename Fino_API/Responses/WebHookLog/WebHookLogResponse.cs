namespace FintechApp.Responses.WebHookLog
{
    public class WebHookLogResponse
    {
        public int WebHookLogID { get; set; }
        public string Type { get; set; } = string.Empty;
        public string RequestURL { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }

    }
}

