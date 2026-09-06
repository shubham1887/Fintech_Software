using FluentValidation;
namespace FintechApp.Requests.BrowserUserAgentMaster
{
    public class DeleteBrowserUserAgentMasterRequest { public int BrowserUserAgentMasterID { get; set; } }
    public class DeleteBrowserUserAgentMasterRequestValidator : AbstractValidator<DeleteBrowserUserAgentMasterRequest> { public DeleteBrowserUserAgentMasterRequestValidator() { RuleFor(x => x.BrowserUserAgentMasterID).GreaterThan(0); } }
}
