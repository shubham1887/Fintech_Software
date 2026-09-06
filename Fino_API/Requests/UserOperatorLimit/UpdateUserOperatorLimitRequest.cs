using FluentValidation;
namespace FintechApp.Requests.UserOperatorLimit
{
    public class UpdateUserOperatorLimitRequest { public int UserOperatorLimitID { get; set; } public int UserID { get; set; } public int OperatorID { get; set; } public int ServiceID { get; set; } public decimal LimitAmount { get; set; } public bool IsActive { get; set; } }
    public class UpdateUserOperatorLimitRequestValidator : AbstractValidator<UpdateUserOperatorLimitRequest> { public UpdateUserOperatorLimitRequestValidator() { RuleFor(x => x.UserOperatorLimitID).GreaterThan(0); } }
}
