using FluentValidation;
namespace FintechApp.Requests.RedirectUrlMaster
{
    public class GetRedirectUrlMasterRequest { public int RedirectUrlMasterID { get; set; } }
    public class GetRedirectUrlMasterRequestValidator : AbstractValidator<GetRedirectUrlMasterRequest> { public GetRedirectUrlMasterRequestValidator() { RuleFor(x => x.RedirectUrlMasterID).GreaterThan(0); } }
}
