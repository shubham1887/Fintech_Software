using FluentValidation;
namespace FintechApp.Requests.OperatorSequenceUserWise
{
    public class DeleteOperatorSequenceUserWiseRequest { public int OperatorSequenceUserWiseID { get; set; } }
    public class DeleteOperatorSequenceUserWiseRequestValidator : AbstractValidator<DeleteOperatorSequenceUserWiseRequest> { public DeleteOperatorSequenceUserWiseRequestValidator() { RuleFor(x => x.OperatorSequenceUserWiseID).GreaterThan(0); } }
}
