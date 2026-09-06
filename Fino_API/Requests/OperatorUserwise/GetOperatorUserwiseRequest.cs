using FluentValidation;
namespace FintechApp.Requests.OperatorUserwise
{
    public class GetOperatorUserwiseRequest { public int OperatorUserwiseID { get; set; } public int UserID { get; set; } }
    public class GetOperatorUserwiseRequestValidator : AbstractValidator<GetOperatorUserwiseRequest> { public GetOperatorUserwiseRequestValidator() { RuleFor(x => x.OperatorUserwiseID).GreaterThan(0); } }
}
