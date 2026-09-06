namespace FintechApp.Responses.SupportUserAccessDetail {
    public class SupportUserAccessDetailResponse {
        public int SupportUserAccessDetailID { get; set; }
        public int UserID { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string MenuGUID { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
