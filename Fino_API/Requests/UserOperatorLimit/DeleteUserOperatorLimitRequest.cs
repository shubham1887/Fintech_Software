using FluentValidation;
namespace FintechApp.Requests.UserOperatorLimit
{
    public class DeleteUserOperatorLimitRequest { public int UserOperatorLimitID { get; set; } }
    public class DeleteUserOperatorLimitRequestValidator : AbstractValidator<DeleteUserOperatorLimitRequest> { public DeleteUserOperatorLimitRequestValidator() { RuleFor(x => x.UserOperatorLimitID).GreaterThan(0); } }
}
