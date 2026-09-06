namespace FintechApp.Responses.OperatorwiseCallbackSetting
{
    public class OperatorwiseCallbackSettingResponse
    {
        public int OperatorwiseCallbackSettingID { get; set; }
        public int OperatorID { get; set; }
        public string? Url { get; set; }
        public string MethodType { get; set; } = string.Empty;
        public string HeaderJson { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

