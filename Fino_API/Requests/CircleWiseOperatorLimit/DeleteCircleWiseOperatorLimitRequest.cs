using FluentValidation;
namespace FintechApp.Requests.CircleWiseOperatorLimit
{
    public class DeleteCircleWiseOperatorLimitRequest { public int CircleWiseOperatorLimitID { get; set; } }
    public class DeleteCircleWiseOperatorLimitRequestValidator : AbstractValidator<DeleteCircleWiseOperatorLimitRequest> { public DeleteCircleWiseOperatorLimitRequestValidator() { RuleFor(x => x.CircleWiseOperatorLimitID).GreaterThan(0); } }
}
