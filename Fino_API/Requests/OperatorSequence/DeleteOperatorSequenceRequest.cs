using FluentValidation;
namespace FintechApp.Requests.OperatorSequence
{
    public class DeleteOperatorSequenceRequest { public int OperatorSequenceID { get; set; } }
    public class DeleteOperatorSequenceRequestValidator : AbstractValidator<DeleteOperatorSequenceRequest> { public DeleteOperatorSequenceRequestValidator() { RuleFor(x => x.OperatorSequenceID).GreaterThan(0); } }
}
