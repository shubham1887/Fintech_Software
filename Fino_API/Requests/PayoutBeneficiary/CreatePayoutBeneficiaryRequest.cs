using FluentValidation;

namespace FintechApp.Requests.PayoutBeneficiary
{
    public class CreatePayoutBeneficiaryRequest
    {
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
    public class CreatePayoutBeneficiaryRequestValidator : AbstractValidator<CreatePayoutBeneficiaryRequest>
    {
        public CreatePayoutBeneficiaryRequestValidator() { RuleFor(x => x.AccountHolderName).NotEmpty(); RuleFor(x => x.AccountNo).NotEmpty(); RuleFor(x => x.IFSC).NotEmpty(); RuleFor(x => x.UserID).GreaterThan(0); }
    }
}
