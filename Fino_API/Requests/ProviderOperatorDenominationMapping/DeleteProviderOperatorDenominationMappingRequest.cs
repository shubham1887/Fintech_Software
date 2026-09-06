using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorDenominationMapping
{
    public class DeleteProviderOperatorDenominationMappingRequest { public int ProviderOperatorDenominationMappingID { get; set; } }
    public class DeleteProviderOperatorDenominationMappingRequestValidator : AbstractValidator<DeleteProviderOperatorDenominationMappingRequest> { public DeleteProviderOperatorDenominationMappingRequestValidator() { RuleFor(x => x.ProviderOperatorDenominationMappingID).GreaterThan(0); } }
}
