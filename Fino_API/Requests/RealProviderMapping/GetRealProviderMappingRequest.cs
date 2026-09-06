using FluentValidation;
namespace FintechApp.Requests.RealProviderMapping
{
    public class GetRealProviderMappingRequest { public int RealProviderMappingID { get; set; } }
    public class GetRealProviderMappingRequestValidator : AbstractValidator<GetRealProviderMappingRequest> { public GetRealProviderMappingRequestValidator() { RuleFor(x => x.RealProviderMappingID).GreaterThan(0); } }
}
