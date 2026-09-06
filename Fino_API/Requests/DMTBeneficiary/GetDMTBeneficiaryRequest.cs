namespace FintechApp.Requests.DMTBeneficiary
{
    public class GetDMTBeneficiaryRequest
    {
        public int BeneficiaryID { get; set; }
        public int SenderID { get; set; }
        public int UserID { get; set; }
        public string AccountNo { get; set; } = string.Empty;
    }
}
