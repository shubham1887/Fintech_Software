using FluentValidation;
namespace FintechApp.Requests.ProviderOperatorDenominationMapping
{
    public class UpdateProviderOperatorDenominationMappingRequest { public int ProviderOperatorDenominationMappingID { get; set; } public int ProviderID { get; set; } public int OperatorID { get; set; } public int DenominationMasterID { get; set; } }
    public class UpdateProviderOperatorDenominationMappingRequestValidator : AbstractValidator<UpdateProviderOperatorDenominationMappingRequest> { public UpdateProviderOperatorDenominationMappingRequestValidator() { RuleFor(x => x.ProviderOperatorDenominationMappingID).GreaterThan(0); } }
}
