namespace FintechApp.Responses.KYCApprovedDetails
{
    public class KYCApprovedDetailsResponse
    {
        public int KYCApprovedDetailsID { get; set; }
        public int UserID { get; set; }
        public int ApporvedUserID { get; set; }
        public string? Remark { get; set; }
        public DateTime DateTime { get; set; }
    }
}
