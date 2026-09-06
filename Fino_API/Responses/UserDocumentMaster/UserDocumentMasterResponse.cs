namespace FintechApp.Responses.UserDocumentMaster
{
    public class UserDocumentMasterResponse
    {
        public int UserDocumentID { get; set; }
        public int UserID { get; set; }
        public string DocmentType { get; set; } = string.Empty;
        public string DocumentNo { get; set; } = string.Empty;
        public string ImageURL { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string? Status { get; set; }
        public DateTime DateTime { get; set; }
    }
}
