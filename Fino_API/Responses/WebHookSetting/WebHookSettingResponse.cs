namespace FintechApp.Responses.WebHookSetting
{
    public class WebHookSettingResponse
    {
        public int WebHookSettingID { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string MethodType { get; set; } = string.Empty;
        public string HeaderJson { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

