using FluentValidation;
namespace FintechApp.Requests.OperatorUserwise
{
    public class DeleteOperatorUserwiseRequest { public int OperatorUserwiseID { get; set; } }
    public class DeleteOperatorUserwiseRequestValidator : AbstractValidator<DeleteOperatorUserwiseRequest> { public DeleteOperatorUserwiseRequestValidator() { RuleFor(x => x.OperatorUserwiseID).GreaterThan(0); } }
}
