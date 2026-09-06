using FluentValidation;
namespace FintechApp.Requests.WebHookLog
{
    public class UpdateWebHookLogRequest { public int WebHookLogID { get; set; } public string Type { get; set; } = string.Empty; public string RequestURL { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; }
    public class UpdateWebHookLogRequestValidator : AbstractValidator<UpdateWebHookLogRequest> { public UpdateWebHookLogRequestValidator() { RuleFor(x => x.WebHookLogID).GreaterThan(0); } }
}
