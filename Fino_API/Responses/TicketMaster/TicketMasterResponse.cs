namespace FintechApp.Responses.TicketMaster
{
    public class TicketMasterResponse
    {
        public int TicketMasterID { get; set; }
        public int TicketCategoryID { get; set; }
        public string TicketCategoryName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string UploadFilePath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public DateTime ResolutionDate { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ResolutionByUserID { get; set; }
        public int UserTypeID { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }
}