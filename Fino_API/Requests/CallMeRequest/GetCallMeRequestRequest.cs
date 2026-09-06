namespace FintechApp.Requests.CallMeRequest
{
    public class GetCallMeRequestRequest
    {
        public int CallMeRequestID { get; set; }
        public int UserID { get; set; }
        public int UplineUserID { get; set; }
        public int CallMeCategoryMasterID { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
