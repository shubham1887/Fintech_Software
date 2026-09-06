using FluentValidation;

namespace FintechApp.Requests.CompanyMaster
{
    public class GetCompanyMasterRequest
    {
        public int CompanyMasterID { get; set; }
    }

    public class GetCompanyMasterRequestValidator : AbstractValidator<GetCompanyMasterRequest>
    {
        public GetCompanyMasterRequestValidator()
        {
            RuleFor(x => x.CompanyMasterID).GreaterThan(0);
        }
    }
}
