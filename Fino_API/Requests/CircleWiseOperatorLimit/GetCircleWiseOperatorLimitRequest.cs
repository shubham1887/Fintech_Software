using FluentValidation;
namespace FintechApp.Requests.CircleWiseOperatorLimit
{
    public class GetCircleWiseOperatorLimitRequest { public int CircleWiseOperatorLimitID { get; set; }
    public int UserID { get; set; } }
    public class GetCircleWiseOperatorLimitRequestValidator : AbstractValidator<GetCircleWiseOperatorLimitRequest> { public GetCircleWiseOperatorLimitRequestValidator() { RuleFor(x => x.CircleWiseOperatorLimitID).GreaterThan(0); } }
}
