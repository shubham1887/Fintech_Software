namespace FintechApp.Responses.PasswordChangeHistory
{
    public class PasswordChangeHistoryResponse
    {
        public int PasswordChangeHistoryID { get; set; }
        public int UserID { get; set; }
        public string? Type { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }

    }
}

