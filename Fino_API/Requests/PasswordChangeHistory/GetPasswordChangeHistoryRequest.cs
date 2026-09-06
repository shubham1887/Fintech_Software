using FluentValidation;
namespace FintechApp.Requests.PasswordChangeHistory
{
    public class GetPasswordChangeHistoryRequest { public int PasswordChangeHistoryID { get; set; } public int UserID { get; set; } }
    public class GetPasswordChangeHistoryRequestValidator : AbstractValidator<GetPasswordChangeHistoryRequest> { public GetPasswordChangeHistoryRequestValidator() { RuleFor(x => x.PasswordChangeHistoryID).GreaterThan(0); } }
}
