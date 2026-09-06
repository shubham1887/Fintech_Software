using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorDenominationMapping
{
    public class CreateProviderOperatorDenominationMappingRequest { public int ProviderID { get; set; } public int OperatorID { get; set; } public int DenominationMasterID { get; set; } }
    public class CreateProviderOperatorDenominationMappingRequestValidator : AbstractValidator<CreateProviderOperatorDenominationMappingRequest> { public CreateProviderOperatorDenominationMappingRequestValidator() { RuleFor(x => x.ProviderID).GreaterThan(0); } }
}
