using FluentValidation;
namespace FintechApp.Requests.BrowserUserAgentMaster
{
    public class GetBrowserUserAgentMasterRequest { public int BrowserUserAgentMasterID { get; set; } }
    public class GetBrowserUserAgentMasterRequestValidator : AbstractValidator<GetBrowserUserAgentMasterRequest> { public GetBrowserUserAgentMasterRequestValidator() { RuleFor(x => x.BrowserUserAgentMasterID).GreaterThan(0); } }
}
