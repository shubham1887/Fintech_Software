using FluentValidation;
namespace FintechApp.Requests.OperatorStateMapping
{
    public class DeleteOperatorStateMappingRequest { public int OperatorStateMappingID { get; set; } }
    public class DeleteOperatorStateMappingRequestValidator : AbstractValidator<DeleteOperatorStateMappingRequest> { public DeleteOperatorStateMappingRequestValidator() { RuleFor(x => x.OperatorStateMappingID).GreaterThan(0); } }
}
