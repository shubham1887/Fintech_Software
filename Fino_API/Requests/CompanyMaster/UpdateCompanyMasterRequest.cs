using FluentValidation;

namespace FintechApp.Requests.CompanyMaster
{
    public class UpdateCompanyMasterRequest
    {
        public int CompanyMasterID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class UpdateCompanyMasterRequestValidator : AbstractValidator<UpdateCompanyMasterRequest>
    {
        public UpdateCompanyMasterRequestValidator()
        {
            RuleFor(x => x.CompanyMasterID).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
