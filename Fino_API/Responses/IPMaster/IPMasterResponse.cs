namespace FintechApp.Responses.IPMaster
{
    public class IPMasterResponse
    {
        public int IpMasterID { get; set; }
        public string IP { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
