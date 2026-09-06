using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorMaster
{
    public class GetProviderOperatorMasterRequest { public int ProviderOperatorID { get; set; } public int ProviderID { get; set; } }
    public class GetProviderOperatorMasterRequestValidator : AbstractValidator<GetProviderOperatorMasterRequest> { public GetProviderOperatorMasterRequestValidator() { RuleFor(x => x.ProviderOperatorID).GreaterThan(0); } }
}
