using FluentValidation;
namespace FintechApp.Requests.CircleWiseOperatorLimit
{
    public class CreateCircleWiseOperatorLimitRequest { public int UserID { get; set; } public int StateID { get; set; } public int OperatorID { get; set; } public decimal AmountLimit { get; set; } }
    public class CreateCircleWiseOperatorLimitRequestValidator : AbstractValidator<CreateCircleWiseOperatorLimitRequest> { public CreateCircleWiseOperatorLimitRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); } }
}
