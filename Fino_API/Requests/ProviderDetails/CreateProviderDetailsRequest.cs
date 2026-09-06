using FluentValidation;

namespace FintechApp.Requests.ProviderDetails
{
    public class CreateProviderDetailsRequest
    {
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public string UrlType { get; set; } = string.Empty;
        public string ProviderURL { get; set; } = string.Empty;
        public string MethodType { get; set; } = "POST";
        public decimal Balance { get; set; }
        public string HeaderJson { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string TransactionIDPrefix { get; set; } = string.Empty;
    }

    public class CreateProviderDetailsRequestValidator : AbstractValidator<CreateProviderDetailsRequest>
    {
        public CreateProviderDetailsRequestValidator()
        {
            RuleFor(x => x.ProviderID).GreaterThan(0);
            RuleFor(x => x.ServiceID).GreaterThan(0);
            RuleFor(x => x.UrlType).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ProviderURL).NotEmpty();
            RuleFor(x => x.MethodType).NotEmpty().MaximumLength(30);
        }
    }
}
