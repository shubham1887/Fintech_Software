using FluentValidation;
namespace FintechApp.Requests.OperatorUserwise
{
    public class UpdateOperatorUserwiseRequest { public int OperatorUserwiseID { get; set; } public int UserID { get; set; } public int OperatorID { get; set; } public bool IsActive { get; set; } public int UsageLimit { get; set; } public int ServiceID { get; set; } public string? BlockedAmount { get; set; } }
    public class UpdateOperatorUserwiseRequestValidator : AbstractValidator<UpdateOperatorUserwiseRequest> { public UpdateOperatorUserwiseRequestValidator() { RuleFor(x => x.OperatorUserwiseID).GreaterThan(0); } }
}
