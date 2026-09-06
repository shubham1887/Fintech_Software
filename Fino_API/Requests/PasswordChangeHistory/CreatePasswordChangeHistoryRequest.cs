using FluentValidation;
namespace FintechApp.Requests.PasswordChangeHistory
{
    public class CreatePasswordChangeHistoryRequest { public int UserID { get; set; } public string? Type { get; set; } public string IPAddress { get; set; } = string.Empty; }
    public class CreatePasswordChangeHistoryRequestValidator : AbstractValidator<CreatePasswordChangeHistoryRequest> { public CreatePasswordChangeHistoryRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.IPAddress).NotEmpty(); } }
}
