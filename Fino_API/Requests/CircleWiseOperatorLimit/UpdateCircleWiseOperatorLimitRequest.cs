using FluentValidation;
namespace FintechApp.Requests.CircleWiseOperatorLimit
{
    public class UpdateCircleWiseOperatorLimitRequest { public int CircleWiseOperatorLimitID { get; set; } public int UserID { get; set; } public int StateID { get; set; } public int OperatorID { get; set; } public decimal AmountLimit { get; set; } }
    public class UpdateCircleWiseOperatorLimitRequestValidator : AbstractValidator<UpdateCircleWiseOperatorLimitRequest> { public UpdateCircleWiseOperatorLimitRequestValidator() { RuleFor(x => x.CircleWiseOperatorLimitID).GreaterThan(0); } }
}
