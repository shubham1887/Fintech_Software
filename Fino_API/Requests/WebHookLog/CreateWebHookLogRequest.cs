using FluentValidation;
namespace FintechApp.Requests.WebHookLog
{
    public class CreateWebHookLogRequest { public string Type { get; set; } = string.Empty; public string RequestURL { get; set; } = string.Empty; public string Response { get; set; } = string.Empty; }
    public class CreateWebHookLogRequestValidator : AbstractValidator<CreateWebHookLogRequest> { public CreateWebHookLogRequestValidator() { RuleFor(x => x.Type).NotEmpty(); RuleFor(x => x.RequestURL).NotEmpty(); } }
}
