using FluentValidation;

namespace FintechApp.Requests.CompanyMaster
{
    public class CreateCompanyMasterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CreateCompanyMasterRequestValidator : AbstractValidator<CreateCompanyMasterRequest>
    {
        public CreateCompanyMasterRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
