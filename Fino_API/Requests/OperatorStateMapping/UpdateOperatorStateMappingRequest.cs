using FluentValidation;
namespace FintechApp.Requests.OperatorStateMapping
{
    public class UpdateOperatorStateMappingRequest { public int OperatorStateMappingID { get; set; } public int OperatorID { get; set; } public int ZoneMasterID { get; set; } public string? HLR { get; set; } public bool IsActive { get; set; } }
    public class UpdateOperatorStateMappingRequestValidator : AbstractValidator<UpdateOperatorStateMappingRequest> { public UpdateOperatorStateMappingRequestValidator() { RuleFor(x => x.OperatorStateMappingID).GreaterThan(0); } }
}
