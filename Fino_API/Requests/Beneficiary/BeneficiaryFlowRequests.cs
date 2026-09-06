namespace FintechApp.Requests.Beneficiary
{
    public class AddBeneficiaryRequest
    {
        public string SenderGUID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public bool IsNameVerified { get; set; }
        public int ProviderID { get; set; }
    }

    // Inputs for a beneficiary account verification (penny-drop name enquiry before saving).
    public class VerifyAccountRequest
    {
        public string SenderGUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "IMPS";
    }

    public class VerifyBeneficiaryOTPRequest
    {
        public string BeneficiaryGUID { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    public class ResendBeneficiaryOTPRequest
    {
        public string BeneficiaryGUID { get; set; } = string.Empty;
    }

    public class SelectBeneficiaryRequest
    {
        public string SenderGUID { get; set; } = string.Empty;
        public int ProviderID { get; set; }
    }

    public class SelectBeneficiaryByGUIDRequest
    {
        public string BeneficiaryGUID { get; set; } = string.Empty;
    }

    public class DeleteBeneficiaryByGUIDRequest
    {
        public string BeneficiaryGUID { get; set; } = string.Empty;
    }

    public class CheckBranchRequest
    {
        public string IFSC { get; set; } = string.Empty;
    }
}
