using FluentValidation;
namespace FintechApp.Requests.RedirectUrlMaster
{
    public class UpdateRedirectUrlMasterRequest { public int RedirectUrlMasterID { get; set; } public string Name { get; set; } = string.Empty; public string RedirectUrl { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class UpdateRedirectUrlMasterRequestValidator : AbstractValidator<UpdateRedirectUrlMasterRequest> { public UpdateRedirectUrlMasterRequestValidator() { RuleFor(x => x.RedirectUrlMasterID).GreaterThan(0); } }
}
