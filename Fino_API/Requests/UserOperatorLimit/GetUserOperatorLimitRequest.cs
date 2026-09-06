using FluentValidation;
namespace FintechApp.Requests.UserOperatorLimit
{
    public class GetUserOperatorLimitRequest { public int UserOperatorLimitID { get; set; } public int UserID { get; set; } }
    public class GetUserOperatorLimitRequestValidator : AbstractValidator<GetUserOperatorLimitRequest> { public GetUserOperatorLimitRequestValidator() { RuleFor(x => x.UserOperatorLimitID).GreaterThan(0); } }
}
