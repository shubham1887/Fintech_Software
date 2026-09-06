namespace FintechApp.Responses.CallMeRequest
{
    public class CallMeRequestResponse
    {
        public int CallMeRequestID { get; set; }
        public int CallMeCategoryMasterID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UplineUserID { get; set; }
        public string UplineRemark { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
