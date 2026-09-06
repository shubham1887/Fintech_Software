using FluentValidation;
namespace FintechApp.Requests.UserOperatorLimit
{
    public class CreateUserOperatorLimitRequest { public int UserID { get; set; } public int OperatorID { get; set; } public int ServiceID { get; set; } public decimal LimitAmount { get; set; } public bool IsActive { get; set; } }
    public class CreateUserOperatorLimitRequestValidator : AbstractValidator<CreateUserOperatorLimitRequest> { public CreateUserOperatorLimitRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
