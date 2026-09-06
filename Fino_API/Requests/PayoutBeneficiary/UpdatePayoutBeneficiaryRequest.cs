using FluentValidation;

namespace FintechApp.Requests.PayoutBeneficiary
{
    public class UpdatePayoutBeneficiaryRequest
    {
        public int PayoutBeneficaryID { get; set; }
        public string AccountHolderName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string? Document { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public bool IsNameVerified { get; set; }
    }
    public class UpdatePayoutBeneficiaryRequestValidator : AbstractValidator<UpdatePayoutBeneficiaryRequest>
    {
        public UpdatePayoutBeneficiaryRequestValidator() { RuleFor(x => x.PayoutBeneficaryID).GreaterThan(0); RuleFor(x => x.AccountHolderName).NotEmpty(); }
    }
}
