using FluentValidation;
namespace FintechApp.Requests.BrowserUserAgentMaster
{
    public class CreateBrowserUserAgentMasterRequest { public string UserAgent { get; set; } = string.Empty; }
    public class CreateBrowserUserAgentMasterRequestValidator : AbstractValidator<CreateBrowserUserAgentMasterRequest> { public CreateBrowserUserAgentMasterRequestValidator() {  } }
}
