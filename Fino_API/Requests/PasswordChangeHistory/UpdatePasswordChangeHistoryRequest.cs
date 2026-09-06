using FluentValidation;
namespace FintechApp.Requests.PasswordChangeHistory
{
    public class UpdatePasswordChangeHistoryRequest { public int PasswordChangeHistoryID { get; set; } public int UserID { get; set; } public string? Type { get; set; } public string IPAddress { get; set; } = string.Empty; }
    public class UpdatePasswordChangeHistoryRequestValidator : AbstractValidator<UpdatePasswordChangeHistoryRequest> { public UpdatePasswordChangeHistoryRequestValidator() { RuleFor(x => x.PasswordChangeHistoryID).GreaterThan(0); } }
}
