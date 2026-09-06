using FluentValidation;
namespace FintechApp.Requests.RedirectUrlMaster
{
    public class CreateRedirectUrlMasterRequest { public string Name { get; set; } = string.Empty; public string RedirectUrl { get; set; } = string.Empty; public bool IsActive { get; set; } }
    public class CreateRedirectUrlMasterRequestValidator : AbstractValidator<CreateRedirectUrlMasterRequest> { public CreateRedirectUrlMasterRequestValidator() {  } }
}
