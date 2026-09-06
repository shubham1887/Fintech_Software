using FluentValidation;
namespace FintechApp.Requests.UserBankDetails
{
    public class GetUserBankDetailsRequest { public int UserBankDetailsID { get; set; } public int UserID { get; set; } }
    public class GetUserBankDetailsRequestValidator : AbstractValidator<GetUserBankDetailsRequest> { public GetUserBankDetailsRequestValidator() { RuleFor(x => x.UserBankDetailsID).GreaterThan(0); } }
}
