using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorDenominationMapping
{
    public class GetProviderOperatorDenominationMappingRequest { public int ProviderOperatorDenominationMappingID { get; set; }
    public int ProviderID { get; set; } }
    public class GetProviderOperatorDenominationMappingRequestValidator : AbstractValidator<GetProviderOperatorDenominationMappingRequest> { public GetProviderOperatorDenominationMappingRequestValidator() { RuleFor(x => x.ProviderOperatorDenominationMappingID).GreaterThan(0); } }
}
