using FluentValidation;
namespace FintechApp.Requests.RealProviderMapping
{
    public class UpdateRealProviderMappingRequest { public int RealProviderMappingID { get; set; } public int ProviderID { get; set; } public int OperatorID { get; set; } }
    public class UpdateRealProviderMappingRequestValidator : AbstractValidator<UpdateRealProviderMappingRequest> { public UpdateRealProviderMappingRequestValidator() { RuleFor(x => x.RealProviderMappingID).GreaterThan(0); } }
}
