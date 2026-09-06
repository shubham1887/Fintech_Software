namespace FintechApp.Requests.SenderBeneficiary
{
    public class SenderActivateRequest
    {
        public int SenderID { get; set; }
        public bool IsActive { get; set; }
    }

    public class BeneficiaryActivateRequest
    {
        public int BeneficiaryID { get; set; }
        public bool IsActive { get; set; }
    }

}
