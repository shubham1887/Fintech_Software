using FluentValidation;
namespace FintechApp.Requests.BrowserUserAgentMaster
{
    public class UpdateBrowserUserAgentMasterRequest { public int BrowserUserAgentMasterID { get; set; } public string UserAgent { get; set; } = string.Empty; }
    public class UpdateBrowserUserAgentMasterRequestValidator : AbstractValidator<UpdateBrowserUserAgentMasterRequest> { public UpdateBrowserUserAgentMasterRequestValidator() { RuleFor(x => x.BrowserUserAgentMasterID).GreaterThan(0); } }
}
