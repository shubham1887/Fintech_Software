using FluentValidation;

namespace FintechApp.Requests.CompanyMaster
{
    public class DeleteCompanyMasterRequest
    {
        public int CompanyMasterID { get; set; }
    }

    public class DeleteCompanyMasterRequestValidator : AbstractValidator<DeleteCompanyMasterRequest>
    {
        public DeleteCompanyMasterRequestValidator()
        {
            RuleFor(x => x.CompanyMasterID).GreaterThan(0);
        }
    }
}
