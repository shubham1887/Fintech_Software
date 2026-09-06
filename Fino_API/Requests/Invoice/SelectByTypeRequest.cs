namespace FintechApp.Requests.Invoice
{
    public class SelectByTypeRequest
    {
        public string Type { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool IsDownload { get; set; }
        public bool IsSend { get; set; }
    }
}
