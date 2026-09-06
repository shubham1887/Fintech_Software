using FluentValidation;
namespace FintechApp.Requests.RedirectUrlMaster
{
    public class DeleteRedirectUrlMasterRequest { public int RedirectUrlMasterID { get; set; } }
    public class DeleteRedirectUrlMasterRequestValidator : AbstractValidator<DeleteRedirectUrlMasterRequest> { public DeleteRedirectUrlMasterRequestValidator() { RuleFor(x => x.RedirectUrlMasterID).GreaterThan(0); } }
}
