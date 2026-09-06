using FluentValidation;
namespace FintechApp.Requests.OperatorSequence
{
    public class GetOperatorSequenceRequest { public int OperatorSequenceID { get; set; } public int OperatorID { get; set; } }
    public class GetOperatorSequenceRequestValidator : AbstractValidator<GetOperatorSequenceRequest> { public GetOperatorSequenceRequestValidator() { RuleFor(x => x.OperatorSequenceID).GreaterThan(0); } }
}
