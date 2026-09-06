using FluentValidation;
namespace FintechApp.Requests.RealProviderMapping
{
    public class DeleteRealProviderMappingRequest { public int RealProviderMappingID { get; set; } }
    public class DeleteRealProviderMappingRequestValidator : AbstractValidator<DeleteRealProviderMappingRequest> { public DeleteRealProviderMappingRequestValidator() { RuleFor(x => x.RealProviderMappingID).GreaterThan(0); } }
}
