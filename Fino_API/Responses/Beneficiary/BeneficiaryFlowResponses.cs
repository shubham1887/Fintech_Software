namespace FintechApp.Responses.Beneficiary
{
    public class AddBeneficiaryResponse
    {
        public string BeneficiaryGUID { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class BeneficiaryDetailResponse
    {
        public string Name { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string SenderGUID { get; set; } = string.Empty;
        public string BeneficiaryGUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    public class CheckBranchResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
    }

    // Result of a beneficiary account verification (penny-drop name enquiry via the active DMT gateway).
    public class VerifyAccountResponse
    {
        public string Status { get; set; } = string.Empty;       // SUCCESS / FAILED
        public string VerifiedName { get; set; } = string.Empty; // account-holder name returned by the bank
        public string BankName { get; set; } = string.Empty;
        public decimal Charge { get; set; }                      // fee debited for this verification
        public decimal ClosingBalance { get; set; }              // DMT wallet balance after the charge
        public string Message { get; set; } = string.Empty;
    }
}
