using FluentValidation;
namespace FintechApp.Requests.OperatorUserwise
{
    public class CreateOperatorUserwiseRequest { public int UserID { get; set; } public int OperatorID { get; set; } public bool IsActive { get; set; } public int UsageLimit { get; set; } public int ServiceID { get; set; } public string? BlockedAmount { get; set; } }
    public class CreateOperatorUserwiseRequestValidator : AbstractValidator<CreateOperatorUserwiseRequest> { public CreateOperatorUserwiseRequestValidator() { RuleFor(x => x.UserID).GreaterThan(0); RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
