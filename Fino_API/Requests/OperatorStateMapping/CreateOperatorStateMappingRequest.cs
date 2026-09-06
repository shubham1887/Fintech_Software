using FluentValidation;
namespace FintechApp.Requests.OperatorStateMapping
{
    public class CreateOperatorStateMappingRequest { public int OperatorID { get; set; } public int ZoneMasterID { get; set; } public string? HLR { get; set; } public bool IsActive { get; set; } }
    public class CreateOperatorStateMappingRequestValidator : AbstractValidator<CreateOperatorStateMappingRequest> { public CreateOperatorStateMappingRequestValidator() { RuleFor(x => x.OperatorID).GreaterThan(0); } }
}
