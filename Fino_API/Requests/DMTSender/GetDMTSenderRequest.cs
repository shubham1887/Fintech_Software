namespace FintechApp.Requests.DMTSender
{
    public class GetDMTSenderRequest
    {
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
