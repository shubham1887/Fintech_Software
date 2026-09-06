using FluentValidation;
namespace FintechApp.Requests.ProviderUserMapping
{
    public class GetProviderUserMappingRequest { public int ProviderUserMappingID { get; set; } public int UserID { get; set; } }
    public class GetProviderUserMappingRequestValidator : AbstractValidator<GetProviderUserMappingRequest> { public GetProviderUserMappingRequestValidator() { RuleFor(x => x.ProviderUserMappingID).GreaterThan(0); } }
}
