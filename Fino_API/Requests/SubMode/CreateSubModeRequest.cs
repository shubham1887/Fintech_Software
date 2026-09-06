using FluentValidation;

namespace FintechApp.Requests.SubMode
{
    public class CreateSubModeRequest
    {
        public int ModeID { get; set; }
        public string SubModeName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImage { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }

    public class CreateSubModeRequestValidator : AbstractValidator<CreateSubModeRequest>
    {
        public CreateSubModeRequestValidator()
        {
            RuleFor(x => x.ModeID).GreaterThan(0);
            RuleFor(x => x.SubModeName).NotEmpty().MaximumLength(150);
        }
    }
}
