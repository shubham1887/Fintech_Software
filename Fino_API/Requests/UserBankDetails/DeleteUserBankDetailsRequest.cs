using FluentValidation;
namespace FintechApp.Requests.UserBankDetails
{
    public class DeleteUserBankDetailsRequest { public int UserBankDetailsID { get; set; } }
    public class DeleteUserBankDetailsRequestValidator : AbstractValidator<DeleteUserBankDetailsRequest> { public DeleteUserBankDetailsRequestValidator() { RuleFor(x => x.UserBankDetailsID).GreaterThan(0); } }
}
