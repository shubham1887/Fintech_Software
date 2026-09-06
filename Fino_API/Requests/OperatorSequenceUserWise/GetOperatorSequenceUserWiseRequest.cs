using FluentValidation;
namespace FintechApp.Requests.OperatorSequenceUserWise
{
    public class GetOperatorSequenceUserWiseRequest { public int OperatorSequenceUserWiseID { get; set; } public int UserID { get; set; } }
    public class GetOperatorSequenceUserWiseRequestValidator : AbstractValidator<GetOperatorSequenceUserWiseRequest> { public GetOperatorSequenceUserWiseRequestValidator() { RuleFor(x => x.OperatorSequenceUserWiseID).GreaterThan(0); } }
}
