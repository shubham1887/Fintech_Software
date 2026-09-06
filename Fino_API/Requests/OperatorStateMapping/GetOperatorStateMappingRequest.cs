using FluentValidation;
namespace FintechApp.Requests.OperatorStateMapping
{
    public class GetOperatorStateMappingRequest { public int OperatorStateMappingID { get; set; } public int OperatorID { get; set; } }
    public class GetOperatorStateMappingRequestValidator : AbstractValidator<GetOperatorStateMappingRequest> { public GetOperatorStateMappingRequestValidator() { RuleFor(x => x.OperatorStateMappingID).GreaterThan(0); } }
}
