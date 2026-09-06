using FluentValidation;
namespace FintechApp.Requests.PasswordChangeHistory
{
    public class DeletePasswordChangeHistoryRequest { public int PasswordChangeHistoryID { get; set; } }
    public class DeletePasswordChangeHistoryRequestValidator : AbstractValidator<DeletePasswordChangeHistoryRequest> { public DeletePasswordChangeHistoryRequestValidator() { RuleFor(x => x.PasswordChangeHistoryID).GreaterThan(0); } }
}
